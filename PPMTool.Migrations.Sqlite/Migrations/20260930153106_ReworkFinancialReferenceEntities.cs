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
                name: "FinancialReferenceValueSets",
                columns: table => new
                {
                    FinancialReferenceValueSetId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValueSets", x => x.FinancialReferenceValueSetId);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReferenceValues",
                columns: table => new
                {
                    FinancialReferenceValueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<float>(type: "REAL", nullable: false),
                    FinancialReferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    FinancialReferenceValueSetId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_FinancialReferenceValues_FinancialReferenceValueSets_FinancialReferenceValueSetId",
                        column: x => x.FinancialReferenceValueSetId,
                        principalTable: "FinancialReferenceValueSets",
                        principalColumn: "FinancialReferenceValueSetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReferenceValues_FinancialReferenceId",
                table: "FinancialReferenceValues",
                column: "FinancialReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReferenceValues_FinancialReferenceValueSetId",
                table: "FinancialReferenceValues",
                column: "FinancialReferenceValueSetId");

            migrationBuilder.AddColumn<int>(
                name: "CostValueSetId",
                table: "WorkloadModelChanges",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadModelChanges_CostValueSetId",
                table: "WorkloadModelChanges",
                column: "CostValueSetId");

            migrationBuilder.Sql(@"
                INSERT INTO FinancialReferenceValueSets (Name)
                VALUES ('Grade41Costs'), ('Grade51Costs'), ('Grade55Costs'), ('Grade65Costs'), ('Grade71Costs'), ('Grade75Costs'), ('RecoveryTarget');

                INSERT INTO FinancialReferenceValues (Value, FinancialReferenceId, FinancialReferenceValueSetId)
                SELECT fr.Grade41Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade41Costs'
                UNION ALL
                SELECT fr.Grade51Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade51Costs'
                UNION ALL
                SELECT fr.Grade55Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade55Costs'
                UNION ALL
                SELECT fr.Grade65Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade65Costs'
                UNION ALL
                SELECT fr.Grade71Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade71Costs'
                UNION ALL
                SELECT fr.Grade75Costs, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'Grade75Costs'
                UNION ALL
                SELECT fr.RecoveryTarget, fr.FinancialReferenceId, fvs.FinancialReferenceValueSetId
                FROM FinancialReferences fr
                CROSS JOIN FinancialReferenceValueSets fvs
                WHERE fvs.Name = 'RecoveryTarget';

                UPDATE WorkloadModelChanges
                SET CostValueSetId = (
                    SELECT FinancialReferenceValueSetId FROM FinancialReferenceValueSets WHERE Name =
                        CASE
                            WHEN Grade = 4 THEN 'Grade41Costs'
                            WHEN Grade = 5 THEN 'Grade51Costs'
                            WHEN Grade = 6 THEN 'Grade65Costs'
                            WHEN Grade = 7 THEN 'Grade75Costs'
                            ELSE NULL
                        END
                )
                WHERE Grade IN (4,5,6,7);

                ALTER TABLE FinancialReferences DROP COLUMN Grade41Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade51Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade55Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade65Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade71Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade75Costs;
                ALTER TABLE FinancialReferences DROP COLUMN RecoveryTarget;
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueSetId",
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
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueSetId",
                table: "WorkloadModelChanges");

            migrationBuilder.AddColumn<float>(
                name: "Grade41Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade51Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade55Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade65Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade71Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Grade75Costs",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "RecoveryTarget",
                table: "FinancialReferences",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.Sql(@"
                UPDATE FinancialReferences
                SET
                    Grade41Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade41Costs' LIMIT 1), 0),
                    Grade51Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade51Costs' LIMIT 1), 0),
                    Grade55Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade55Costs' LIMIT 1), 0),
                    Grade65Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade65Costs' LIMIT 1), 0),
                    Grade71Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade71Costs' LIMIT 1), 0),
                    Grade75Costs = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'Grade75Costs' LIMIT 1), 0),
                    RecoveryTarget = COALESCE((SELECT v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = FinancialReferences.FinancialReferenceId AND s.Name = 'RecoveryTarget' LIMIT 1), 0);
            ");

            migrationBuilder.DropIndex(
                name: "IX_WorkloadModelChanges_CostValueSetId",
                table: "WorkloadModelChanges");

            migrationBuilder.DropColumn(
                name: "CostValueSetId",
                table: "WorkloadModelChanges");

            migrationBuilder.DropTable(
                name: "FinancialReferenceValues");

            migrationBuilder.DropTable(
                name: "FinancialReferenceValueSets");
        }
    }
}
