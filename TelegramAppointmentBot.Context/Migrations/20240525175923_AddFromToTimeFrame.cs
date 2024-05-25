using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    public partial class AddFromToTimeFrame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DesiredTimeFrom",
                table: "Hunters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DesiredTimeTo",
                table: "Hunters",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesiredTimeFrom",
                table: "Hunters");

            migrationBuilder.DropColumn(
                name: "DesiredTimeTo",
                table: "Hunters");
        }
    }
}
