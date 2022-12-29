using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MangaNatoIndexer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mangas",
                columns: table => new
                {
                    MangaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChapterCount = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastChapterUpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mangas", x => x.MangaId);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "MangaAndTags",
                columns: table => new
                {
                    MangaAndTagsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MangaId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaAndTags", x => x.MangaAndTagsId);
                    table.ForeignKey(
                        name: "FK_MangaAndTags_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "MangaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaAndTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "TagId", "TagName" },
                values: new object[,]
                {
                    { 1, "Action" },
                    { 2, "Adult" },
                    { 3, "Adventure" },
                    { 4, "Comedy" },
                    { 5, "Cooking" },
                    { 6, "Doujinshi" },
                    { 7, "Drama" },
                    { 8, "Ecchi" },
                    { 9, "Erotica" },
                    { 10, "Fantasy" },
                    { 11, "Gender bender" },
                    { 12, "Harem" },
                    { 13, "Historical" },
                    { 14, "Horror" },
                    { 15, "Isekai" },
                    { 16, "Josei" },
                    { 17, "Manhua" },
                    { 18, "Manhwa" },
                    { 19, "Martial arts" },
                    { 20, "Mature" },
                    { 21, "Mecha" },
                    { 22, "Medical" },
                    { 23, "Mystery" },
                    { 24, "One shot" },
                    { 25, "Pornographic" },
                    { 26, "Psychological" },
                    { 27, "Romance" },
                    { 28, "School life" },
                    { 29, "Sci fi" },
                    { 30, "Seinen" },
                    { 31, "Shoujo" },
                    { 32, "Shoujo ai" },
                    { 33, "Shounen" },
                    { 34, "Shounen ai" },
                    { 35, "Slice of life" },
                    { 36, "Smut" },
                    { 37, "Sports" },
                    { 38, "Supernatural" },
                    { 39, "Tragedy" },
                    { 40, "Webtoons" },
                    { 41, "Yaoi" },
                    { 42, "Yuri" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MangaAndTags_MangaId",
                table: "MangaAndTags",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaAndTags_TagId",
                table: "MangaAndTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MangaAndTags");

            migrationBuilder.DropTable(
                name: "Mangas");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
