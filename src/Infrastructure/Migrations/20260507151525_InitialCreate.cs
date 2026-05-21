using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departamentos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gestion_hardware",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_equipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    marca = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    modelo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_adquisicion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ubicacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    codigo_cne = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    nombre_dispositivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    id_suministro = table.Column<long>(type: "bigint", nullable: true),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gestion_hardware", x => x.id);
                    table.UniqueConstraint("AK_gestion_hardware_id_equipo", x => x.id_equipo);
                });

            migrationBuilder.CreateTable(
                name: "historial_custodios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    timestamp_evento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    custodio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    id_departamento = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_custodios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Kits",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    INSUMO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CANTIDAD = table.Column<int>(type: "int", nullable: false),
                    ESTADO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MARCA = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MODELO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OBSERVACION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cedula = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cargo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    tempPass = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cargo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "User")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Custodios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cargo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cedula = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    id_departamento = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Custodios", x => x.id);
                    table.ForeignKey(
                        name: "FK_Custodios_departamentos_id_departamento",
                        column: x => x.id_departamento,
                        principalTable: "departamentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "caracteristicas_computadora",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_equipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    rom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    procesador = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caracteristicas_computadora", x => x.id);
                    table.ForeignKey(
                        name: "FK_caracteristicas_computadora_gestion_hardware_id_equipo",
                        column: x => x.id_equipo,
                        principalTable: "gestion_hardware",
                        principalColumn: "id_equipo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "control_activos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_equipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_auditoria = table.Column<DateTime>(type: "datetime2", nullable: true),
                    detalles_auditoria = table.Column<string>(type: "text", nullable: true),
                    custodio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_control_activos", x => x.id);
                    table.ForeignKey(
                        name: "FK_control_activos_gestion_hardware_id_equipo",
                        column: x => x.id_equipo,
                        principalTable: "gestion_hardware",
                        principalColumn: "id_equipo");
                });

            migrationBuilder.CreateTable(
                name: "suministros_remanufacturados",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_equipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tipo_suministro = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_retiro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_equipoAsignado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suministros_remanufacturados", x => x.id);
                    table.ForeignKey(
                        name: "FK_suministros_remanufacturados_gestion_hardware_id_equipo",
                        column: x => x.id_equipo,
                        principalTable: "gestion_hardware",
                        principalColumn: "id_equipo");
                });

            migrationBuilder.CreateTable(
                name: "gestion_activos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_equipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    id_custodio = table.Column<long>(type: "bigint", nullable: true),
                    fecha_asignacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fecha_devolucion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gestion_activos", x => x.id);
                    table.ForeignKey(
                        name: "FK_gestion_activos_Custodios_id_custodio",
                        column: x => x.id_custodio,
                        principalTable: "Custodios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gestion_activos_gestion_hardware_id_equipo",
                        column: x => x.id_equipo,
                        principalTable: "gestion_hardware",
                        principalColumn: "id_equipo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_caracteristicas_computadora_id_equipo",
                table: "caracteristicas_computadora",
                column: "id_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_control_activos_id_equipo",
                table: "control_activos",
                column: "id_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_Custodio_Cedula",
                table: "Custodios",
                column: "cedula");

            migrationBuilder.CreateIndex(
                name: "IX_Custodios_id_departamento",
                table: "Custodios",
                column: "id_departamento");

            migrationBuilder.CreateIndex(
                name: "IX_gestion_activos_id_custodio",
                table: "gestion_activos",
                column: "id_custodio");

            migrationBuilder.CreateIndex(
                name: "IX_gestion_activos_id_equipo",
                table: "gestion_activos",
                column: "id_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_Hardware_Estado",
                table: "gestion_hardware",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_Hardware_IdEquipo",
                table: "gestion_hardware",
                column: "id_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_suministros_remanufacturados_id_equipo",
                table: "suministros_remanufacturados",
                column: "id_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Nombre",
                table: "Usuarios",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caracteristicas_computadora");

            migrationBuilder.DropTable(
                name: "control_activos");

            migrationBuilder.DropTable(
                name: "gestion_activos");

            migrationBuilder.DropTable(
                name: "historial_custodios");

            migrationBuilder.DropTable(
                name: "Kits");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.DropTable(
                name: "suministros_remanufacturados");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Custodios");

            migrationBuilder.DropTable(
                name: "gestion_hardware");

            migrationBuilder.DropTable(
                name: "departamentos");
        }
    }
}
