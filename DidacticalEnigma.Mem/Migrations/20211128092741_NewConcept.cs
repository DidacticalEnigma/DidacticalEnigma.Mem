using System;
using DidacticalEnigma.Mem.DatabaseModels;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace DidacticalEnigma.Mem.Migrations
{
    public partial class NewConcept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MediaType = table.Column<string>(type: "text", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NpgsqlQueries",
                columns: table => new
                {
                    Vec = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    ContentObjectId = table.Column<long>(type: "bigint", nullable: true),
                    MediaTypeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contexts_MediaTypes_MediaTypeId",
                        column: x => x.MediaTypeId,
                        principalTable: "MediaTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contexts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TranslationPairs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false),
                    Target = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<NotesCollection>(type: "jsonb", nullable: true),
                    AssociatedData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationPairs_Projects_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MediaTypes",
                columns: new[] { "Id", "Extension", "MediaType" },
                values: new object[,]
                {
                    { 1, "jpg", "image/jpeg" },
                    { 2, "png", "image/png" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contexts_MediaTypeId",
                table: "Contexts",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contexts_ProjectId_CorrelationId",
                table: "Contexts",
                columns: new[] { "ProjectId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaTypes_MediaType",
                table: "MediaTypes",
                column: "MediaType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_CorrelationId",
                table: "TranslationPairs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_ParentId_CorrelationId",
                table: "TranslationPairs",
                columns: new[] { "ParentId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_SearchVector",
                table: "TranslationPairs",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contexts");

            migrationBuilder.DropTable(
                name: "NpgsqlQueries");

            migrationBuilder.DropTable(
                name: "TranslationPairs");

            migrationBuilder.DropTable(
                name: "MediaTypes");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
