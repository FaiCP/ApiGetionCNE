# Auditoria de Seguridad - GestorAdmi.Core

**Fecha:** 13 de mayo de 2026  
**Proyecto:** GestorAdmi.Core  
**Alcance:** Revision completa de seguridad de la API, autenticacion, autorizacion, configuracion y arquitectura.

---

## 1. Evaluacion de Arquitectura

La arquitectura es **buena** en general: sigue Clean Architecture con 4 capas (Domain, Application, Infrastructure, API), usa CQRS con MediatR, FluentValidation, y el patron repositorio. Sin embargo, hay huecos de seguridad importantes que deben remediarse.

| Aspecto | Estado |
|---------|--------|
| Clean Architecture | Bien implementada |
| CQRS + MediatR | Correcto |
| Validacion (FluentValidation) | Presente pero insuficiente |
| Autenticacion JWT | Funcional pero con key expuesta |
| Autorizacion por roles | Correcta (Admin/User) |
| Hashing contrasenas (BCrypt) | Correcto |
| Separacion de responsabilidades | Buena |
| Proteccion contra SQL Injection | Buena (usa EF Core con parametros) |
| Rate limiting | Parcial (solo produccion) |

---

## 2. Hallazgos Criticos

### CRITICO-01: Credenciales expuestas en el repositorio

**Archivo:** `src/API/appsettings.Production.json` (lineas 3, 6)

El archivo `appsettings.Production.json` contiene credenciales en texto plano que estan en el repositorio:

- Connection string con usuario y password: `user id=Fairez_SQLLogin_1;pwd=52lyp8m9d6`
- JWT Key en texto plano: `5b3af53ea2fc42babb209f91d50649d698191cff9a1244cbae50fc59754d5e80`

Aunque `.gitignore` lista `appsettings.Production.json`, el archivo ya fue commiteado antes de agregar la regla. Cualquiera con acceso al repositorio tiene las credenciales de produccion.

**Impacto:** Un atacante puede obtener acceso completo a la base de datos y falsificar tokens JWT.

**Referencia OWASP:** [A05:2021 - Security Misconfiguration](https://owasp.org/Top10/A05_2021-Security_Misconfiguration/)

---

### CRITICO-02: JWT Key hardcodeada como fallback

**Archivo:** `src/API/Program.cs` (linea 25)

```csharp
jwtKey = "fallback-dev-key-for-testing-only-min-32ch!!";
```

Existe un fallback hardcodeado. Si la variable de entorno no esta configurada correctamente, se usara esta clave debil y predecible que cualquiera puede ver en el codigo fuente. Permite la falsificacion de tokens JWT.

**Impacto:** Un atacante puede generar tokens JWT validos y obtener acceso como cualquier usuario, incluyendo administradores.

**Referencia OWASP:** [A07:2021 - Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)

---

### CRITICO-03: Contrasenas hardcodedas en el seeder

**Archivo:** `src/Infrastructure/Persistence/DataSeeder.cs` (lineas 68-69)

```csharp
Password = hasher.HashPassword("Admin123!"),
...
Password = hasher.HashPassword("User123!"),
```

Las contrasenas de usuarios por defecto estan en el codigo fuente. Un atacante que vea el repositorio puede probar esas credenciales directamente contra el endpoint de login.

**Impacto:** Acceso inicial garantizado si las credenciales no fueron cambiadas tras el despliegue.

**Referencia OWASP:** [A07:2021 - Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)

---

### CRITICO-04: Sin rate limiting en login en ambientes no-Testing

**Archivo:** `src/API/Program.cs` (lineas 147-161)

El rate limiting solo se aplica cuando el ambiente NO es "Testing". En Development no hay proteccion contra fuerza bruta en el endpoint `/login`.

```csharp
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options => { ... });
}
```

Y luego en el pipeline:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();
```

**Impacto:** Ataques de fuerza bruta o diccionario contra el login sin restriccion en ambientes de desarrollo.

**Referencia OWASP:** [A07:2021 - Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)

---

### CRITICO-05: AllowedHosts configurado como wildcard

**Archivo:** `src/API/appsettings.json` (linea 41) y `appsettings.Production.json` (linea 43)

```json
"AllowedHosts": "*"
```

Acepta cualquier Host header, lo que permite ataques de Host Header Injection.

**Impacto:** Redireccionamiento malicioso, envenenamiento de cache, o bypass de politiques de seguridad que dependen del Host header.

**Referencia OWASP:** [A05:2021 - Security Misconfiguration](https://owasp.org/Top10/A05_2021-Security_Misconfiguration/)

---

## 3. Hallazgos Altos

### ALTO-01: Sin HSTS configurado

No se llama `app.UseHsts()` en produccion. Sin HSTS, los navegadores pueden hacer solicitudes HTTP sin cifrar en futuras visitas, permitiendo ataques de downgrade.

**Impacto:** Posible intercepcion de trafico en futuras visitas tras un ataque MITM inicial.

**Referencia OWASP:** [A02:2021 - Cryptographic Failures](https://owasp.org/Top10/A02_2021-Cryptographic_Failures/)

---

### ALTO-02: Validacion de input insuficiente - Campo `busqueda`

Todos los endpoints `LeerTodo` aceptan un parametro `busqueda` sin limites de longitud ni sanitizacion:

```csharp
public async Task<IActionResult> LeerTodo(
    [FromQuery] int cantidad = 10,
    [FromQuery] int pagina = 0,
    [FromQuery] string busqueda = "")
```

Si este parametro se usa en consultas LINQ/SQL podria permitir ataques de inyeccion o DoS con cadenas muy largas.

**Impacto:** Posible DoS o inyeccion si los datos no se sanitizan adecuadamente en la capa de aplicacion.

**Referencia OWASP:** [A03:2021 - Injection](https://owasp.org/Top10/A03_2021-Injection/)

---

### ALTO-03: Campo `TempPass` almacenado en texto plano

**Archivo:** `src/Infrastructure/Persistence/ApplicationDbContext.cs` (linea 170)

```csharp
entity.Property(e => e.TempPass).HasColumnName("tempPass").HasMaxLength(255);
```

La entidad `Persona` almacena una contrasena temporal (`TempPass`) en texto plano en la base de datos. No tiene hashing ni cifrado. Cualquiera con acceso a la BD puede leer las credenciales temporales de todo el personal.

**Impacto:** Exposicion de credenciales si la base de datos es comprometida o accesible por personal no autorizado.

**Referencia OWASP:** [A02:2021 - Cryptographic Failures](https://owasp.org/Top10/A02_2021-Cryptographic_Failures/)

---

### ALTO-04: Ausencia de auditoria y logs de acceso

No hay log de quien accede a que datos. No hay tracking de operaciones sensibles (quien elimino un registro, quien asigno admin, etc.). El `AsignarAdminCommand` cambia roles sin dejar rastro de quien lo hizo ni cuando.

**Impacto:** Imposibilidad de determinar responsabilidades ante incidentes, no cumplimiento de requisitos de auditoria.

**Referencia OWASP:** [A09:2021 - Security Logging and Monitoring Failures](https://owasp.org/Top10/A09_2021-Security_Logging_and_Monitoring_Failures/)

---

### ALTO-05: Sin validacion de propiedad de recurso (IDOR)

Un usuario autenticado con rol "User" puede ver TODOS los datos de todas las entidades via los endpoints `LeerTodo`. No hay filtrado por usuario o departamento. Un usuario regular puede ver todo el inventario, personal, custodios, etc.

**Impacto:** Violacion de confidencialidad. Un usuario de bajo privilegio puede acceder a toda la informacion del sistema.

**Referencia OWASP:** [A01:2021 - Broken Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/)

---

## 4. Hallazgos Medios

### MEDIO-01: Sin refresh token ni revocacion de tokens

Los JWT tienen expiracion fija (8h produccion, 24h desarrollo) sin mecanismo de revocacion. Si un token es comprometido, no hay forma de invalidarlo hasta que expire naturalmente. Tampoco hay refresh token.

**Impacto:** Ventana extendida de explotacion ante tokens comprometidos.

---

### MEDIO-02: Swagger disponible sin restriccion en produccion

**Archivo:** `src/API/Program.cs` (lineas 169-174)

```csharp
app.UseSwagger();
app.UseSwaggerUI(options => { ... });
```

Swagger esta habilitado sin importar el ambiente. En produccion, esto expone toda la documentacion de la API a cualquiera.

**Impacto:** Informacion detallada de endpoints facilita ataques dirigidos.

---

### MEDIO-03: Campo `Password` con nombre engañoso en entidad `Usuario`

**Archivo:** `src/Domain/Entities/Usuario.cs` (linea 9)

```csharp
public string Password { get; set; } = string.Empty; // Se almacenara hasheada
```

El campo se llama `Password` pero almacena hashes BCrypt. El nombre es confuso y podria llevar a un desarrollador a tratarlo como texto plano en el futuro.

**Impacto:** Posible introduccion de bugs de seguridad por confusion de nombres.

---

### MEDIO-04: CORS con credenciales habilitadas

**Archivo:** `src/API/Program.cs` (lineas 104, 114)

```csharp
.AllowCredentials()
```

Esta habilitado en ambas politicas CORS. Combinado con origins amplios, esto puede ser explotado en ataques CSRF si el frontend tiene vulnerabilidades XSS.

**Impacto:** Posible exfiltracion de datos autenticados desde sitios maliciosos.

---

### MEDIO-05: Posible exposicion de `TempPass` en DTOs

Si el DTO de Personal incluye `TempPass` en la respuesta API, se estaria enviando la contrasena temporal al cliente Frontend.

**Impacto:** Exposicion de credenciales en trafico de red y almacenamiento del navegador.

---

## 5. Hallazgos Bajos

### BAJO-01: Sin MaxLength en parametros de busqueda

Los parametros `busqueda` en los endpoints `LeerTodo` no tienen restriccion de longitud. Strings extremadamente largos podrian causar problemas de rendimiento.

---

### BAJO-02: Sin validacion de lista vacia en eliminaciones masivas

Los endpoints `DELETE` aceptan `List<long> ids` via query string. Un array vacio podria causar excepciones no manejadas en el handler.

---

### BAJO-03: Sin headers de seguridad HTTP

No se configuran headers de seguridad recomendados:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Content-Security-Policy`
- `Referrer-Policy`

**Impacto:** Mayor superficie de ataque para ataques de clickjacking, MIME-type sniffing, etc.

---

## 6. Plan de Remediacion

### Fase 1: Acciones Inmediatas (1-2 dias)

| # | Tarea | Prioridad | Responsable |
|---|-------|-----------|-------------|
| 1 | **Rotar todas las credenciales expuestas**: Cambiar la password de BD y la JWT Key. Regenerar la clave JWT con al menos 256 bits de entropia. Cambiar las contrasenas de los usuarios admin/jperez en produccion. | CRITICO | DevOps / Backend |
| 2 | **Eliminar `appsettings.Production.json` del historial de git**: Usar `git filter-branch` o BFG Repo Cleaner para eliminar el archivo del historial. Verificar que `.gitignore` lo excluye correctamente. | CRITICO | DevOps |
| 3 | **Mover secrets a variables de entorno**: Configurar JWT:Key, ConnectionStrings y otros secrets como variables de entorno o usar Azure Key Vault / User Secrets. Eliminar el fallback hardcodeado en `Program.cs` linea 25. | CRITICO | Backend |
| 4 | **Eliminar contrasenas hardcodedas del seeder**: Cambiar el seeder para que lea las contrasenas de variable de entorno o generarlas aleatoriamente en primer arranque, forzando al admin a cambiarla en el primer login. | CRITICO | Backend |

### Fase 2: Correcciones de Seguridad (3-5 dias)

| # | Tarea | Prioridad | Responsable |
|---|-------|-----------|-------------|
| 5 | **Hash de `TempPass`**: Modificar la entidad `Persona` para almacenar `TempPass` hasheado con BCrypt, igual que `Password` en `Usuario`. Crear un metodo en el servicio para verificar la contrasena temporal. Renombrar el campo a `TempPassHash`. | ALTO | Backend |
| 6 | **Agregar rate limiting en todos los ambientes**: Eliminar la condicion `!builder.Environment.IsEnvironment("Testing")` para el rate limiting en login, o crear una politica especifica mas permisiva para desarrollo pero que igual proteja contra fuerza bruta. | ALTO | Backend |
| 7 | **Configurar `AllowedHosts`**: Cambiar `"AllowedHosts": "*"` por los hosts especificos de produccion (ej: `"AllowedHosts": "tudominio.com,www.tudominio.com"`). | ALTO | DevOps / Backend |
| 8 | **Agregar HSTS**: Incluir `app.UseHsts()` en el pipeline de produccion, despues de `app.UseHttpsRedirection()`. | ALTO | Backend |
| 9 | **Agregar headers de seguridad HTTP**: Crear middleware o usar `app.UseSecurityHeaders()` para agregar `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`, y `Referrer-Policy`. | BAJO | Backend |
| 10 | **Restringir Swagger en produccion**: Encerrar la configuracion de Swagger en una condicion `if (app.Environment.IsDevelopment())` o protegerlo con autorizacion. | MEDIO | Backend |

### Fase 3: Mejoras de Arquitectura (1-2 semanas)

| # | Tarea | Prioridad | Responsable |
|---|-------|-----------|-------------|
| 11 | **Implementar sistema de auditoria**: Crear un middleware o interceptor que registre quien (userId), que accion, sobre que entidad, y cuando. Crear tabla `AuditLog` en la BD. Registrar especialmente operaciones sensibles: eliminaciones, cambios de rol, login. | ALTO | Backend |
| 12 | **Implementar autorizacion a nivel de recursos**: Agregar filtrado por departamentos o usuarios en los endpoints `LeerTodo` para que un usuario regular solo vea los datos de su area. Crear claims adicionales en el JWT (departamento, sede). | ALTO | Backend |
| 13 | **Agregar refresh tokens**: Implementar un mecanismo de refresh tokens con almacenamiento en BD o cache, permitir revocacion de tokens individuales, y reducir la vida del token de acceso a 15-30 minutos. | MEDIO | Backend |
| 14 | **Validacion de longitud en parametros de busqueda**: Agregar `[MaxLength(200)]` en los parametros `busqueda` de los controladores, o validar en el validator del query correspondiente. Limitar `pagina` y `cantidad` a rangos razonables. | ALTO | Backend |
| 15 | **Validar `ids` vacios en eliminaciones masivas**: Agregar validacion en los handlers de DeleteCommands para verificar que la lista de IDs no este vacia y que no exceda un limite razonable (ej: max 100 IDs por solicitud). | BAJO | Backend |
| 16 | **Renombrar campo `Password` a `PasswordHash`** en la entidad `Usuario` y en la BD (migracion). | MEDIO | Backend |

### Fase 4: Endurecimiento Continuo

| # | Tarea | Prioridad | Responsable |
|---|-------|-----------|-------------|
| 17 | **Eliminar contrasenas temporales del DTO de respuesta**: Verificar que ningun DTO de Personal incluya `TempPass` en las respuestas API. Usar DTOs separados para entrada y salida. | MEDIO | Backend |
| 18 | **Configurar CORS mas restrictivo**: En produccion, limitar los origins a los dominios exactos del frontend. Considerar eliminar `.AllowCredentials()` si no es estrictamente necesario. | MEDIO | Backend |
| 19 | **Implementar account lockout**: Despues de N intentos fallidos de login, bloquear la cuenta temporalmente. Registrar intentos fallidos en logs. | MEDIO | Backend |
| 20 | **Agregar proteccion CSRF**: Si la API sera consumida por navegadores, implementar proteccion Anti-Forgery Tokens para operaciones destructivas (POST, PUT, DELETE). | BAJO | Backend |
| 21 | **Escaneo de dependencias**: Configurar `dotnet list package --vulnerable` o herramientas como Snyk para detectar vulnerabilidades en paquetes NuGet. | BAJO | DevOps |
| 22 | **Configurar HTTPS redirection estricta**: Verificar que `app.UseHttpsRedirection()` este correctamente posicionado en el pipeline y que en produccion se redirija todo trafico HTTP a HTTPS. | BAJO | Backend |

---

## 7. Detalle Tecnico de las Remediaciones Prioritarias

### Remediacion CRITICO-01 y CRITICO-02: Eliminar secrets del codigo

**Paso 1:** Eliminar el archivo del historial de git:
```bash
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch src/API/appsettings.Production.json" \
  --prune-empty -- --all
```

**Paso 2:** En `Program.cs`, eliminar el fallback hardcodeado:
```csharp
// ELIMINAR esta linea:
// jwtKey = "fallback-dev-key-for-testing-only-min-32ch!!";

// Mantener solo la validacion estricta:
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT:Key debe estar configurada.");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("JWT:Key debe tener al menos 32 caracteres.");
```

**Paso 3:** Configurar secrets via variables de entorno o User Secrets:
```bash
dotnet user-secrets set "Jwt:Key" "<nueva-clave-256-bits>" --project src/API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" --project src/API
```

**Paso 4:** Rotar TODAS las credenciales existentes en la base de datos somee.com.

### Remediacion CRITICO-03: Contrasenas del seeder

Reemplazar las contrasenas harcodeadas:
```csharp
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD")
    ?? throw new InvalidOperationException("ADMIN_DEFAULT_PASSWORD no configurada.");
var userPassword = Environment.GetEnvironmentVariable("USER_DEFAULT_PASSWORD")
    ?? throw new InvalidOperationException("USER_DEFAULT_PASSWORD no configurada.");

new Usuario
{
    Nombre = "admin",
    Password = hasher.HashPassword(adminPassword),
    ...
}
```

### Remediacion ALTO-05: Autorizacion por recurso

Ejemplo de implementacion en un controller:
```csharp
[HttpGet("LeerTodo")]
[Authorize] // Cualquier usuario autenticado
public async Task<IActionResult> LeerTodo(...)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

    // Si no es admin, filtrar solo los datos de su area
    var result = await _mediator.Send(new GetHardwareQuery(cantidad, pagina, busqueda, userRole, userId));
    return Ok(ApiResponse<...>.Ok(result));
}
```

---

## 8. Resumen de Severidad

| Severidad | Cantidad |
|-----------|----------|
| Critico   | 5        |
| Alto      | 5        |
| Medio     | 5        |
| Bajo      | 3        |
| **Total** | **18**   |

---

*Documento generado como parte de la auditoria de seguridad del proyecto GestorAdmi.Core.*