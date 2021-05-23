using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DidacticalEnigma.Mem.Migrations
{
    public partial class FixContextCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationPairs_Contexts_ContextId",
                table: "TranslationPairs");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "TranslationPairs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificationTime",
                table: "TranslationPairs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationPairs_Contexts_ContextId",
                table: "TranslationPairs",
                column: "ContextId",
                principalTable: "Contexts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationPairs_Contexts_ContextId",
                table: "TranslationPairs");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "TranslationPairs");

            migrationBuilder.DropColumn(
                name: "ModificationTime",
                table: "TranslationPairs");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationPairs_Contexts_ContextId",
                table: "TranslationPairs",
                column: "ContextId",
                principalTable: "Contexts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
