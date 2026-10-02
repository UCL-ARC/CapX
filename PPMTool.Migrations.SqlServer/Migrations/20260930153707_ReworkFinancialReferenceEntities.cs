using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPMTool.Migrations.SqlServer.Migrations
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
                    FinancialReferenceValueSetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValueSets", x => x.FinancialReferenceValueSetId);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReferenceValues",
                columns: table => new
                {
                    FinancialReferenceValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<float>(type: "real", nullable: false),
                    FinancialReferenceId = table.Column<int>(type: "int", nullable: false),
                    FinancialReferenceValueSetId = table.Column<int>(type: "int", nullable: false)
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
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadModelChanges_CostValueSetId",
                table: "WorkloadModelChanges",
                column: "CostValueSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueSetId",
                table: "WorkloadModelChanges",
                column: "CostValueSetId",
                principalTable: "FinancialReferenceValueSets",
                principalColumn: "FinancialReferenceValueSetId",
                onDelete: ReferentialAction.SetNull);

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

                UPDATE wmc
                SET CostValueSetId = fvs.FinancialReferenceValueSetId
                FROM WorkloadModelChanges wmc
                JOIN FinancialReferenceValueSets fvs ON
                    (wmc.Grade = 4 AND fvs.Name = 'Grade41Costs') OR
                    (wmc.Grade = 5 AND fvs.Name = 'Grade51Costs') OR
                    (wmc.Grade = 6 AND fvs.Name = 'Grade65Costs') OR
                    (wmc.Grade = 7 AND fvs.Name = 'Grade75Costs');

                ALTER TABLE FinancialReferences DROP COLUMN Grade41Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade51Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade55Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade65Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade71Costs;
                ALTER TABLE FinancialReferences DROP COLUMN Grade75Costs;
                ALTER TABLE FinancialReferences DROP COLUMN RecoveryTarget;
            ");
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
                UPDATE fr SET
                    Grade41Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade41Costs'), 0),
                    Grade51Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade51Costs'), 0),
                    Grade55Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade55Costs'), 0),
                    Grade65Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade65Costs'), 0),
                    Grade71Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade71Costs'), 0),
                    Grade75Costs = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'Grade75Costs'), 0),
                    RecoveryTarget = ISNULL((SELECT TOP 1 v.Value FROM FinancialReferenceValues v JOIN FinancialReferenceValueSets s ON s.FinancialReferenceValueSetId = v.FinancialReferenceValueSetId WHERE v.FinancialReferenceId = fr.FinancialReferenceId AND s.Name = 'RecoveryTarget'), 0)
                FROM FinancialReferences fr;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkloadModelChanges_FinancialReferenceValueSets_CostValueSetId",
                table: "WorkloadModelChanges");

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
