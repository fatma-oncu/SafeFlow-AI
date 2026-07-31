using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskAssessmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_Module_Action",
                schema: "identity",
                table: "RolePermissions");

            migrationBuilder.EnsureSchema(
                name: "risk");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                schema: "employee",
                table: "Employees",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                schema: "risk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibleEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OverallRiskLevel = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PreviousAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessmentApprovals",
                schema: "risk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessmentApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessmentApprovals_RiskAssessments_RiskAssessmentId",
                        column: x => x.RiskAssessmentId,
                        principalSchema: "risk",
                        principalTable: "RiskAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessmentHistory",
                schema: "risk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    PerformedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessmentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessmentHistory_RiskAssessments_RiskAssessmentId",
                        column: x => x.RiskAssessmentId,
                        principalSchema: "risk",
                        principalTable: "RiskAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskHazards",
                schema: "risk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InitialLikelihood = table.Column<int>(type: "int", nullable: false),
                    InitialSeverity = table.Column<int>(type: "int", nullable: false),
                    InitialScore = table.Column<int>(type: "int", nullable: false),
                    InitialRiskLevel = table.Column<int>(type: "int", nullable: false),
                    ResidualLikelihood = table.Column<int>(type: "int", nullable: false),
                    ResidualSeverity = table.Column<int>(type: "int", nullable: false),
                    ResidualScore = table.Column<int>(type: "int", nullable: false),
                    ResidualRiskLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskHazards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskHazards_RiskAssessments_RiskAssessmentId",
                        column: x => x.RiskAssessmentId,
                        principalSchema: "risk",
                        principalTable: "RiskAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskControlMeasures",
                schema: "risk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskHazardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsImplemented = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ImplementedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskControlMeasures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskControlMeasures_RiskHazards_RiskHazardId",
                        column: x => x.RiskHazardId,
                        principalSchema: "risk",
                        principalTable: "RiskHazards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentApprovals_RiskAssessmentId",
                schema: "risk",
                table: "RiskAssessmentApprovals",
                column: "RiskAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentHistory_RiskAssessmentId",
                schema: "risk",
                table: "RiskAssessmentHistory",
                column: "RiskAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_AssessmentNumber",
                schema: "risk",
                table: "RiskAssessments",
                column: "AssessmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_DepartmentId",
                schema: "risk",
                table: "RiskAssessments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_OverallRiskLevel",
                schema: "risk",
                table: "RiskAssessments",
                column: "OverallRiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_ResponsibleEmployeeId",
                schema: "risk",
                table: "RiskAssessments",
                column: "ResponsibleEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_Status",
                schema: "risk",
                table: "RiskAssessments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RiskControlMeasures_RiskHazardId",
                schema: "risk",
                table: "RiskControlMeasures",
                column: "RiskHazardId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskHazards_RiskAssessmentId",
                schema: "risk",
                table: "RiskHazards",
                column: "RiskAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskAssessmentApprovals",
                schema: "risk");

            migrationBuilder.DropTable(
                name: "RiskAssessmentHistory",
                schema: "risk");

            migrationBuilder.DropTable(
                name: "RiskControlMeasures",
                schema: "risk");

            migrationBuilder.DropTable(
                name: "RiskHazards",
                schema: "risk");

            migrationBuilder.DropTable(
                name: "RiskAssessments",
                schema: "risk");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                schema: "employee",
                table: "Employees",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_Module_Action",
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "PermissionModule", "PermissionAction" },
                unique: true);
        }
    }
}
