using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddCalificacionAndIntentoLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Calificaciones",
                columns: table => new
                {
                    CalificacionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InscripcionID = table.Column<int>(type: "integer", nullable: false),
                    TipoEvaluacion = table.Column<string>(type: "text", nullable: false),
                    NumeroEvaluacion = table.Column<int>(type: "integer", nullable: true),
                    Puntos = table.Column<decimal>(type: "numeric", nullable: false),
                    PuntosMaximos = table.Column<decimal>(type: "numeric", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.CalificacionID);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Inscripciones_InscripcionID",
                        column: x => x.InscripcionID,
                        principalTable: "Inscripciones",
                        principalColumn: "InscripcionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntentosLogin",
                columns: table => new
                {
                    IntentoLoginID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    UsuarioID = table.Column<int>(type: "integer", nullable: true),
                    FechaIntento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoFallo = table.Column<string>(type: "text", nullable: true),
                    DireccionIP = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Pais = table.Column<string>(type: "text", nullable: true),
                    Ciudad = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosLogin", x => x.IntentoLoginID);
                    table.ForeignKey(
                        name: "FK_IntentosLogin_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_InscripcionID",
                table: "Calificaciones",
                column: "InscripcionID");

            migrationBuilder.CreateIndex(
                name: "IX_IntentosLogin_UsuarioID",
                table: "IntentosLogin",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calificaciones");

            migrationBuilder.DropTable(
                name: "IntentosLogin");
        }
    }
}
