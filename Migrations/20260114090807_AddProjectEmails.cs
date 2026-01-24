using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MigraTrackAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CustomizationPoints and ModuleMaster changes removed to prevent conflict


            migrationBuilder.CreateTable(
                name: "ProjectEmails",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Receivers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmailDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BodyContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedModule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEmails", x => x.EmailId);
                    table.ForeignKey(
                        name: "FK_ProjectEmails_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEmails_ProjectId",
                table: "ProjectEmails",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectEmails");

        }
    }
}
