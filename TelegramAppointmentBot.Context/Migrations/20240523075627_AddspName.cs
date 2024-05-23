using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    public partial class AddspName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpecialityName",
                table: "Hunters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialityName",
                table: "Hunters");
        }
    }
}
