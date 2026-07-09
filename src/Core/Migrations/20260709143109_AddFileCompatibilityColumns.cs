using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaraW3B.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFileCompatibilityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AudioConversion",
                table: "Songs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InstrumentalConversion",
                table: "Songs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VideoConversion",
                table: "Songs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VocalsConversion",
                table: "Songs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioConversion",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "InstrumentalConversion",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "VideoConversion",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "VocalsConversion",
                table: "Songs");
        }
    }
}
