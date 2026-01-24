using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MigraTrackAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixVerificationAndTransferSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing tables to recreate with Identity columns
            migrationBuilder.Sql("DROP TABLE IF EXISTS VerificationRecords");
            migrationBuilder.Sql("DROP TABLE IF EXISTS DataTransferChecks");

            migrationBuilder.CreateTable(
                name: "DataTransferChecks",
                columns: table => new
                {
                    TransferId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TableNameDesktop = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TableNameWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecordCountDesktop = table.Column<long>(type: "bigint", nullable: true),
                    RecordCountWeb = table.Column<long>(type: "bigint", nullable: true),
                    MatchPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MigratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataTransferChecks", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_DataTransferChecks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationRecords",
                columns: table => new
                {
                    VerificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SqlQuery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRecords", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_VerificationRecords_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataTransferChecks_ProjectId",
                table: "DataTransferChecks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRecords_ProjectId",
                table: "VerificationRecords",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VerificationId",
                table: "VerificationRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "TransferId",
                table: "DataTransferChecks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
