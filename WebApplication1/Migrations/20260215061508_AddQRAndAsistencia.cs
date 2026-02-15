using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddQRAndAsistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Horarios",
                columns: table => new
                {
                    HorarioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoID = table.Column<int>(type: "integer", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Aula = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horarios", x => x.HorarioID);
                    table.ForeignKey(
                        name: "FK_Horarios_Grupos_GrupoID",
                        column: x => x.GrupoID,
                        principalTable: "Grupos",
                        principalColumn: "GrupoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SesionesClase",
                columns: table => new
                {
                    SesionClaseID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoID = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Aula = table.Column<string>(type: "text", nullable: true),
                    Tema = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesClase", x => x.SesionClaseID);
                    table.ForeignKey(
                        name: "FK_SesionesClase_Grupos_GrupoID",
                        column: x => x.GrupoID,
                        principalTable: "Grupos",
                        principalColumn: "GrupoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRsGenerados",
                columns: table => new
                {
                    QRGeneradoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SesionClaseID = table.Column<int>(type: "integer", nullable: false),
                    TokenUnico = table.Column<string>(type: "text", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: true),
                    Latitud = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitud = table.Column<decimal>(type: "numeric", nullable: true),
                    RadioMetros = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRsGenerados", x => x.QRGeneradoID);
                    table.ForeignKey(
                        name: "FK_QRsGenerados_SesionesClase_SesionClaseID",
                        column: x => x.SesionClaseID,
                        principalTable: "SesionesClase",
                        principalColumn: "SesionClaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    AsistenciaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SesionClaseID = table.Column<int>(type: "integer", nullable: false),
                    EstudianteID = table.Column<int>(type: "integer", nullable: false),
                    QRGeneradoID = table.Column<int>(type: "integer", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    MinutosTarde = table.Column<int>(type: "integer", nullable: true),
                    Latitud = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitud = table.Column<decimal>(type: "numeric", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.AsistenciaID);
                    table.ForeignKey(
                        name: "FK_Asistencias_Estudiantes_EstudianteID",
                        column: x => x.EstudianteID,
                        principalTable: "Estudiantes",
                        principalColumn: "EstudianteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asistencias_QRsGenerados_QRGeneradoID",
                        column: x => x.QRGeneradoID,
                        principalTable: "QRsGenerados",
                        principalColumn: "QRGeneradoID");
                    table.ForeignKey(
                        name: "FK_Asistencias_SesionesClase_SesionClaseID",
                        column: x => x.SesionClaseID,
                        principalTable: "SesionesClase",
                        principalColumn: "SesionClaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_EstudianteID",
                table: "Asistencias",
                column: "EstudianteID");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_QRGeneradoID",
                table: "Asistencias",
                column: "QRGeneradoID");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_SesionClaseID",
                table: "Asistencias",
                column: "SesionClaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Horarios_GrupoID",
                table: "Horarios",
                column: "GrupoID");

            migrationBuilder.CreateIndex(
                name: "IX_QRsGenerados_SesionClaseID",
                table: "QRsGenerados",
                column: "SesionClaseID");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesClase_GrupoID",
                table: "SesionesClase",
                column: "GrupoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "Horarios");

            migrationBuilder.DropTable(
                name: "QRsGenerados");

            migrationBuilder.DropTable(
                name: "SesionesClase");
        }
    }
}
