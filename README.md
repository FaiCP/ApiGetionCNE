# 🚀 GestorAdmi Core API

![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4.svg)
![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue.svg)
![Database](https://img.shields.io/badge/Database-SQL_Server-red.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

**GestorAdmi Core** es una API REST robusta diseñada para la gestión integral de activos tecnológicos. Permite el control de inventario, asignación de custodios, gestión de suministros y generación automatizada de documentos legales en PDF y Excel.

📍 **Producción:** [CNEAPI.somee.com](https://CNEAPI.somee.com)  
📖 **Swagger UI:** [Ver Documentación Interactiva](https://CNEAPI.somee.com/swagger)

---

## 🛠️ Stack Tecnológico

* **Framework:** .NET 10.0 (C#)
* **ORM:** Entity Framework Core 8 (SQL Server)
* **Patrones de Diseño:** CQRS, Mediator (MediatR), Repository Pattern, Unit of Work.
* **Seguridad:** Autenticación JWT Bearer & Hashing de contraseñas con BCrypt.
* **Reportes:** iTextSharp (PDF) & EPPlus (Excel).
* **Pruebas:** xUnit, Moq, FluentAssertions y WebApplicationFactory.
* **Logging:** Serilog estructurado.

---

## 🏛️ Arquitectura del Sistema

El proyecto implementa **Clean Architecture**, dividiendo la lógica en capas con dependencias unidireccionales para garantizar un código mantenible, testeable y desacoplado:

1.  **Domain:** Entidades de negocio, interfaces de repositorio y excepciones personalizadas.
2.  **Application:** Casos de uso (Commands/Queries), DTOs y lógica de validación (FluentValidation).
3.  **Infrastructure:** Implementación de persistencia, servicios de correo/reportes y seguridad.
4.  **API (Presentation):** Controladores REST, Middlewares de excepciones y configuración de DI.

---

## 📦 Módulos Principales

| Módulo | Descripción |
| :--- | :--- |
| 🔑 **Auth** | Gestión de acceso con tokens JWT expirables. |
| 💻 **Hardware** | Inventario de equipos con búsqueda paginada avanzada. |
| 👤 **Custodios** | Administración de responsables de activos. |
| 🔄 **Gestión de Activos** | Flujo completo de asignación y devolución con actas digitales. |
| 📈 **Reportes** | Estadísticas mensuales y dashboard de estado del inventario. |

---

## 🚀 Instalación y Uso Local

### Requisitos
* .NET 10 SDK
* SQL Server 2019+ (local o remoto)
* Git

### Setup Inicial

1. **Clonar repositorio:**
   ```bash
   git clone https://github.com/FaiCP/ApiGetionCNE.git
   cd GestorAdmi-master/GestorAdmi.Core
   ```

2. **Configurar Connection String:**
   - Copia `src/API/appsettings.Development.json` (crear si no existe)
   - Actualiza `ConnectionStrings:DefaultConnection` con credenciales SQL local:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=GestorAdmiDB;User Id=sa;Password=TuPassword123;"
     }
   }
   ```

3. **Ejecutar Migraciones:**
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/API
   ```

4. **Restaurar Dependencias y Ejecutar:**
   ```bash
   dotnet restore
   dotnet run --project src/API
   ```

   API disponible en: `http://localhost:5000`  
   Swagger en: `http://localhost:5000/swagger`

---

## 🧪 Estrategia de Pruebas

Sistema de dos niveles para garantizar integridad:

- **Unit Tests:** Validación aislada de Handlers y lógica de dominio
- **Integration Tests:** Endpoints HTTP con BD en memoria simulando escenarios reales

Ejecutar tests:
```bash
dotnet test
```

Ejecución con coverage:
```bash
dotnet test /p:CollectCoverage=true
```

---

## 🔧 Variables de Entorno

Configurables en `appsettings.{Environment}.json`:

| Variable | Descripción | Ejemplo |
|:---|:---|:---|
| `JwtSettings:SecretKey` | Clave para firmar tokens JWT | `super-secret-key-min-32-chars` |
| `JwtSettings:ExpirationMinutes` | Minutos de expiración token | `60` |
| `EmailSettings:Host` | SMTP host | `smtp.gmail.com` |
| `EmailSettings:Port` | Puerto SMTP | `587` |
| `EmailSettings:Username` | Usuario correo | `tu-email@gmail.com` |
| `EmailSettings:Password` | Contraseña correo | `app-password` |

---

## 🚢 Despliegue a Producción

Script PowerShell automatizado (`deploy.ps1`):

```powershell
.\deploy.ps1
```

Pasos ejecutados:
1. Restauración de dependencias
2. Build en modo Release
3. Ejecución de tests (detiene si fallan)
4. Publicación de artefactos
5. Carga vía FTP a Somee.com

**Requisitos deploy:**
- Credenciales FTP configuradas en `.env` (local, no subir)
- Tests pasando
- Cambios commiteados en git

---

## 📁 Estructura del Proyecto

```
GestorAdmi.Core/
├── src/
│   ├── API/                    # Presentation Layer (Controllers, Middlewares)
│   ├── Application/            # Use Cases (Commands, Queries, Validators)
│   ├── Domain/                 # Business Logic (Entities, Interfaces)
│   └── Infrastructure/         # Data Access, Services (EF, SMTP, Reports)
├── tests/
│   ├── GestorAdmi.Tests.Unit/
│   └── GestorAdmi.Tests.Integration/
└── publish/                    # Artefactos de despliegue
```

---

## 🤝 Contribución

1. Fork el repositorio
2. Crea rama: `git checkout -b feature/tu-feature`
3. Commit: `git commit -m "feat: descripción"`
4. Push: `git push origin feature/tu-feature`
5. PR a `main`

---

## 🐛 Troubleshooting

**Error de conexión BD:**
- Verifica `appsettings.Development.json`
- SQL Server corriendo: `SELECT @@VERSION;`

**Tokens JWT expirados:**
- Aumenta `JwtSettings:ExpirationMinutes`
- Regenera `SecretKey` en todas instancias

**Tests fallan en Integration:**
- Limpia BD: `dotnet ef database drop --force`
- Reaplica migraciones: `dotnet ef database update`

---

## 📝 Licencia

MIT © 2025 GestorAdmi
