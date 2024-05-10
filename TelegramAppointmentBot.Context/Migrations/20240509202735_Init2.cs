using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramAppointmentBot.Context.Migrations
{
    public partial class Init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Users_OwnerId",
                table: "Profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_OwnerId",
                table: "Profiles");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "SystemId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "OwnerSystemId",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_OwnerSystemId",
                table: "Profiles",
                column: "OwnerSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Users_OwnerSystemId",
                table: "Profiles",
                column: "OwnerSystemId",
                principalTable: "Users",
                principalColumn: "SystemId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Users_OwnerSystemId",
                table: "Profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_OwnerSystemId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "SystemId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerSystemId",
                table: "Profiles");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_OwnerId",
                table: "Profiles",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Users_OwnerId",
                table: "Profiles",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
