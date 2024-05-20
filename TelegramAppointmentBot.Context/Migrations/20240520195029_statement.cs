using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    public partial class statement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Statement",
                table: "Hunters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Statement",
                table: "Hunters");
        }
    }
}
