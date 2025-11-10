using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace geohunt.Migrations
{
    /// <inheritdoc />
    public partial class AddPanoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "panoId",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "panoId",
                table: "Locations");
        }
    }
}
