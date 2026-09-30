using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPMTool.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ReworkFinancialReferenceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialReferenceValues",
                columns: table => new
                {
                    FinancialReferenceValueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValueName = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false),
                    FinancialReferenceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValues", x => x.FinancialReferenceValueId);
                    table.ForeignKey(
                        name: "FK_FinancialReferenceValues_FinancialReferences_FinancialReferenceId",
                        column: x => x.FinancialReferenceId,
                        principalTable: "FinancialReferences",
                        principalColumn: "FinancialReferenceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReferenceValues_FinancialReferenceId",
                table: "FinancialReferenceValues",
                column: "FinancialReferenceId");

            migrationBuilder.Sql(@"
                INSERT INTO FinancialReferenceValues (ValueName, Value, FinancialReferenceId)
                SELECT 'Grade41Costs', Grade41Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'Grade51Costs', Grade51Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'Grade55Costs', Grade55Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'Grade65Costs', Grade65Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'Grade71Costs', Grade71Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'Grade75Costs', Grade75Costs, FinancialReferenceId FROM FinancialReferences
                UNION ALL
                SELECT 'RecoveryTarget', RecoveryTarget, FinancialReferenceId FROM FinancialReferences;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialReferenceValues");
        }
    }
}
