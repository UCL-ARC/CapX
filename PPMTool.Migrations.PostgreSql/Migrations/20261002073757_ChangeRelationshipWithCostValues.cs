using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PPMTool.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationshipWithCostValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostValueName",
                table: "WorkloadModelChanges");

            migrationBuilder.DropColumn(
                name: "ValueName",
                table: "FinancialReferenceValues");

            migrationBuilder.AddColumn<int>(
                name: "CostValueSetId",
                table: "WorkloadModelChanges",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancialReferenceValueSetId",
                table: "FinancialReferenceValues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FinancialReferenceValueSets",
                columns: table => new
                {
                    FinancialReferenceValueSetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValueSets", x => x.FinancialReferenceValueSetId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadModelChanges_CostValueSetId",
                table: "WorkloadModelChanges",
                column: "CostValueSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReferenceValues_FinancialReferenceValueSetId",
                table: "FinancialReferenceValues",
                column: "FinancialReferenceValueSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialReferenceValues_FinancialReferenceValueSets_Financ~",
                table: "FinancialReferenceValues",
                column: "FinancialReferenceValueSetId",
                principalTable: "FinancialReferenceValueSets",
                principalColumn: "FinancialReferenceValueSetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueS~",
                table: "WorkloadModelChanges",
                column: "CostValueSetId",
                principalTable: "FinancialReferenceValueSets",
                principalColumn: "FinancialReferenceValueSetId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialReferenceValues_FinancialReferenceValueSets_Financ~",
                table: "FinancialReferenceValues");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueS~",
                table: "WorkloadModelChanges");

            migrationBuilder.DropTable(
                name: "FinancialReferenceValueSets");

            migrationBuilder.DropIndex(
                name: "IX_WorkloadModelChanges_CostValueSetId",
                table: "WorkloadModelChanges");

            migrationBuilder.DropIndex(
                name: "IX_FinancialReferenceValues_FinancialReferenceValueSetId",
                table: "FinancialReferenceValues");

            migrationBuilder.DropColumn(
                name: "CostValueSetId",
                table: "WorkloadModelChanges");

            migrationBuilder.DropColumn(
                name: "FinancialReferenceValueSetId",
                table: "FinancialReferenceValues");

            migrationBuilder.AddColumn<string>(
                name: "CostValueName",
                table: "WorkloadModelChanges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueName",
                table: "FinancialReferenceValues",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
