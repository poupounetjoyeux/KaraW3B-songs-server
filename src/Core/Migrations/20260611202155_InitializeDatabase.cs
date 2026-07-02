using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaraW3B.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitializeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnalyzing = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastAnalyzeMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Bpm = table.Column<decimal>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Artist = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Audio = table.Column<string>(type: "TEXT", nullable: true),
                    Gap = table.Column<double>(type: "REAL", nullable: true),
                    Start = table.Column<double>(type: "REAL", nullable: true),
                    End = table.Column<double>(type: "REAL", nullable: true),
                    Cover = table.Column<string>(type: "TEXT", nullable: true),
                    Background = table.Column<string>(type: "TEXT", nullable: true),
                    Video = table.Column<string>(type: "TEXT", nullable: true),
                    VideoGap = table.Column<double>(type: "REAL", nullable: true),
                    Vocals = table.Column<string>(type: "TEXT", nullable: true),
                    Instrumental = table.Column<string>(type: "TEXT", nullable: true),
                    PreviewStart = table.Column<double>(type: "REAL", nullable: true),
                    Medley = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Genres = table.Column<string>(type: "TEXT", nullable: true),
                    Languages = table.Column<string>(type: "TEXT", nullable: true),
                    Editions = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Creators = table.Column<string>(type: "TEXT", nullable: true),
                    ProvidedBy = table.Column<string>(type: "TEXT", nullable: true),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    AudioUrl = table.Column<string>(type: "TEXT", nullable: true),
                    VideoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CoverUrl = table.Column<string>(type: "TEXT", nullable: true),
                    BackgroundUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Rendition = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    NotManagedHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    SongFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    AnalyzedFileHash = table.Column<string>(type: "TEXT", nullable: false),
                    LastParseTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SongId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    FileLine = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongAlerts_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongNotes",
                columns: table => new
                {
                    FileLine = table.Column<int>(type: "INTEGER", nullable: false),
                    SongId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StartBeat = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    Pitch = table.Column<int>(type: "INTEGER", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongNotes", x => new { x.SongId, x.FileLine });
                    table.ForeignKey(
                        name: "FK_SongNotes_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayers",
                columns: table => new
                {
                    SongId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayers", x => new { x.SongId, x.Number });
                    table.ForeignKey(
                        name: "FK_SongPlayers_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongAlerts_SongId",
                table: "SongAlerts",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_SongNotes_PlayerNumber",
                table: "SongNotes",
                column: "PlayerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_LibraryId",
                table: "Songs",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongAlerts");

            migrationBuilder.DropTable(
                name: "SongNotes");

            migrationBuilder.DropTable(
                name: "SongPlayers");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
