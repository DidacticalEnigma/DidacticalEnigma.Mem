using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DidacticalEnigma.Mem.Migrations
{
    public partial class UserManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "TranslationPairs",
                nullable: false,
                defaultValue: new Guid("32674391-9E28-422B-9DD7-3EDEFADB3417"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Projects",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Contexts",
                nullable: false,
                defaultValue: new Guid("32674391-9E28-422B-9DD7-3EDEFADB3417"));

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(nullable: false),
                    CanAddContexts = table.Column<bool>(nullable: false),
                    CanDeleteContexts = table.Column<bool>(nullable: false),
                    CanAddProjects = table.Column<bool>(nullable: false),
                    CanDeleteProjects = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.UniqueConstraint("AK_Groups_GroupName", x => x.GroupName);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    IsSpecialUser = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "GroupProjectClaims",
                columns: table => new
                {
                    ProjectId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    CanAddTranslations = table.Column<bool>(nullable: false),
                    CanDeleteTranslations = table.Column<bool>(nullable: false),
                    CanReadTranslations = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupProjectClaims", x => new { x.GroupId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_GroupProjectClaims_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupProjectClaims_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupMemberships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMemberships", x => new { x.GroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "IsSpecialUser", "Name" },
                values: new object[,]
                {
                    { new Guid("32674391-9e28-422b-9dd7-3edefadb3417"), true, "<anonymous user>" },
                    { new Guid("b97335bd-6670-414f-b7e7-c7be511b4a6c"), true, "<administrator>" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranslationPairs_AuthorId",
                table: "TranslationPairs",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contexts_AuthorId",
                table: "Contexts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupProjectClaims_ProjectId",
                table: "GroupProjectClaims",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_UserId",
                table: "UserGroupMemberships",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contexts_Users_AuthorId",
                table: "Contexts",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationPairs_Users_AuthorId",
                table: "TranslationPairs",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contexts_Users_AuthorId",
                table: "Contexts");

            migrationBuilder.DropForeignKey(
                name: "FK_TranslationPairs_Users_AuthorId",
                table: "TranslationPairs");

            migrationBuilder.DropTable(
                name: "GroupProjectClaims");

            migrationBuilder.DropTable(
                name: "UserGroupMemberships");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TranslationPairs_AuthorId",
                table: "TranslationPairs");

            migrationBuilder.DropIndex(
                name: "IX_Contexts_AuthorId",
                table: "Contexts");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "TranslationPairs");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Contexts");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Projects",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);
        }
    }
}
