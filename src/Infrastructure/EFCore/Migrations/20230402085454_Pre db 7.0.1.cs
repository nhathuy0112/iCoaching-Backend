using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.EFCore.Migrations
{
    public partial class Predb701 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundJobId",
                table: "TrainingLogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundJobId",
                table: "TrainingLogs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
