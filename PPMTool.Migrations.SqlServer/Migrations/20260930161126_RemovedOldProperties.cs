using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPMTool.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOldProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade41Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "Grade51Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "Grade55Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "Grade65Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "Grade71Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "Grade75Costs",
                table: "FinancialReferences");

            migrationBuilder.DropColumn(
                name: "RecoveryTarget",
                table: "FinancialReferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Grade41Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade51Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade55Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade65Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade71Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade75Costs",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "RecoveryTarget",
                table: "FinancialReferences",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.Sql(@"
                UPDATE fr
                SET
                    Grade41Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade41Costs'), 0),
                    Grade51Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade51Costs'), 0),
                    Grade55Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade55Costs'), 0),
                    Grade65Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade65Costs'), 0),
                    Grade71Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade71Costs'), 0),
                    Grade75Costs = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'Grade75Costs'), 0),
                    RecoveryTarget = COALESCE((SELECT TOP 1 [Value] FROM FinancialReferenceValues frv WHERE frv.FinancialReferenceId = fr.FinancialReferenceId AND frv.ValueName = 'RecoveryTarget'), 0)
                FROM FinancialReferences fr;");
        }
    }
}
