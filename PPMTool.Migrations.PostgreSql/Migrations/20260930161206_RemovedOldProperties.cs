using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPMTool.Migrations.PostgreSql.Migrations
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
                UPDATE ""FinancialReferences"" fr
                SET
                    ""Grade41Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade41Costs' LIMIT 1), 0::real),
                    ""Grade51Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade51Costs' LIMIT 1), 0::real),
                    ""Grade55Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade55Costs' LIMIT 1), 0::real),
                    ""Grade65Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade65Costs' LIMIT 1), 0::real),
                    ""Grade71Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade71Costs' LIMIT 1), 0::real),
                    ""Grade75Costs"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'Grade75Costs' LIMIT 1), 0::real),
                    ""RecoveryTarget"" = COALESCE((SELECT ""Value"" FROM ""FinancialReferenceValues"" frv WHERE frv.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND frv.""ValueName"" = 'RecoveryTarget' LIMIT 1), 0::real);");
        }
    }
}
