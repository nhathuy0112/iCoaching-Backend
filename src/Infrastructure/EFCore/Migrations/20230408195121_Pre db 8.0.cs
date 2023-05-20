using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.EFCore.Migrations
{
    public partial class Predb80 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCert",
                table: "MediaAssets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCert",
                table: "MediaAssets");
        }
    }
}
