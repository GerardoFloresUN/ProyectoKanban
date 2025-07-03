using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoKanban.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAlerta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlertaEnviada",
                table: "Tareas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiasAntesAlerta",
                table: "Tareas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertaEnviada",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "DiasAntesAlerta",
                table: "Tareas");
        }
    }
}
