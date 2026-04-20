using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QRsGenerados_SesionClaseID",
                table: "QRsGenerados");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_EstudianteID",
                table: "Inscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_EstudianteID",
                table: "Asistencias");

            migrationBuilder.RenameIndex(
                name: "IX_SesionesClase_GrupoID",
                table: "SesionesClase",
                newName: "IX_SesionClase_GrupoID");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocadoPorIP = table.Column<string>(type: "text", nullable: true),
                    ReemplazadoPorToken = table.Column<string>(type: "text", nullable: true),
                    CreadoPorIP = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionClase_Fecha",
                table: "SesionesClase",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_QRGenerado_FechaExpiracion",
                table: "QRsGenerados",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_QRGenerado_Sesion_Activo",
                table: "QRsGenerados",
                columns: new[] { "SesionClaseID", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_QRGenerado_TokenUnico",
                table: "QRsGenerados",
                column: "TokenUnico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntentoLogin_Email_Fecha",
                table: "IntentosLogin",
                columns: new[] { "Email", "FechaIntento" });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripcion_Estudiante_Grupo",
                table: "Inscripciones",
                columns: new[] { "EstudianteID", "GrupoID" });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Estudiante_Sesion",
                table: "Asistencias",
                columns: new[] { "EstudianteID", "SesionClaseID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_FechaRegistro",
                table: "Asistencias",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UsuarioID",
                table: "RefreshTokens",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_SesionClase_Fecha",
                table: "SesionesClase");

            migrationBuilder.DropIndex(
                name: "IX_QRGenerado_FechaExpiracion",
                table: "QRsGenerados");

            migrationBuilder.DropIndex(
                name: "IX_QRGenerado_Sesion_Activo",
                table: "QRsGenerados");

            migrationBuilder.DropIndex(
                name: "IX_QRGenerado_TokenUnico",
                table: "QRsGenerados");

            migrationBuilder.DropIndex(
                name: "IX_IntentoLogin_Email_Fecha",
                table: "IntentosLogin");

            migrationBuilder.DropIndex(
                name: "IX_Inscripcion_Estudiante_Grupo",
                table: "Inscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Asistencia_Estudiante_Sesion",
                table: "Asistencias");

            migrationBuilder.DropIndex(
                name: "IX_Asistencia_FechaRegistro",
                table: "Asistencias");

            migrationBuilder.RenameIndex(
                name: "IX_SesionClase_GrupoID",
                table: "SesionesClase",
                newName: "IX_SesionesClase_GrupoID");

            migrationBuilder.CreateIndex(
                name: "IX_QRsGenerados_SesionClaseID",
                table: "QRsGenerados",
                column: "SesionClaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_EstudianteID",
                table: "Inscripciones",
                column: "EstudianteID");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_EstudianteID",
                table: "Asistencias",
                column: "EstudianteID");
        }
    }
}
