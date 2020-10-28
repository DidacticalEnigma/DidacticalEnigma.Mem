using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace DidacticalEnigma.Mem.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MediaType = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NpgsqlQueries",
                columns: table => new
                {
                    Vec = table.Column<NpgsqlTsVector>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(maxLength: 512, nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    MediaTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contexts_MediaTypes_MediaTypeId",
                        column: x => x.MediaTypeId,
                        principalTable: "MediaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TranslationPairs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CorrelationId = table.Column<string>(maxLength: 256, nullable: false),
                    Source = table.Column<string>(maxLength: 4096, nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(maxLength: 8192, nullable: false),
                    Target = table.Column<string>(maxLength: 4096, nullable: true),
                    ContextId = table.Column<Guid>(nullable: true),
                    ParentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationPairs_Contexts_ContextId",
                        column: x => x.ContextId,
                        principalTable: "Contexts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranslationPairs_Projects_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MediaTypes",
                columns: new[] { "Id", "MediaType" },
                values: new object[,]
                {
                    { 1, "image/jpeg" },
                    { 2, "image/png" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contexts_MediaTypeId",
                table: "Contexts",
                column: "MediaTypeId");

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
                name: "IX_TranslationPairs_ContextId",
                table: "TranslationPairs",
                column: "ContextId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_CorrelationId",
                table: "TranslationPairs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_ParentId_CorrelationId",
                table: "TranslationPairs",
                columns: new[] { "ParentId", "CorrelationId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpgsqlQueries");

            migrationBuilder.DropTable(
                name: "TranslationPairs");

            migrationBuilder.DropTable(
                name: "Contexts");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "MediaTypes");
        }
    }
}
