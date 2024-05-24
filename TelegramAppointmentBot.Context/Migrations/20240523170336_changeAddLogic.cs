using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    public partial class changeAddLogic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Hunters");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialityName",
                table: "Hunters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Specialities",
                columns: table => new
                {
                    SystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lpuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialities", x => x.SystemId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Specialities");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialityName",
                table: "Hunters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Hunters",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
