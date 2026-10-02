using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PPMTool.Migrations.PostgreSql.Migrations
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
                    FinancialReferenceValueSetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValueSets", x => x.FinancialReferenceValueSetId);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReferenceValues",
                columns: table => new
                {
                    FinancialReferenceValueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<float>(type: "real", nullable: false),
                    FinancialReferenceId = table.Column<int>(type: "integer", nullable: false),
                    FinancialReferenceValueSetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceValues", x => x.FinancialReferenceValueId);
                    table.ForeignKey(
                        name: "FK_FinancialReferenceValues_FinancialReferences_FinancialRefer~",
                        column: x => x.FinancialReferenceId,
                        principalTable: "FinancialReferences",
                        principalColumn: "FinancialReferenceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinancialReferenceValues_FinancialReferenceValueSets_Financ~",
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
                type: "integer",
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
                INSERT INTO ""FinancialReferenceValueSets"" (""Name"")
                VALUES ('Grade41Costs'), ('Grade51Costs'), ('Grade55Costs'), ('Grade65Costs'), ('Grade71Costs'), ('Grade75Costs'), ('RecoveryTarget');

                INSERT INTO ""FinancialReferenceValues"" (""Value"", ""FinancialReferenceId"", ""FinancialReferenceValueSetId"")
                SELECT fr.""Grade41Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade41Costs'
                UNION ALL
                SELECT fr.""Grade51Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade51Costs'
                UNION ALL
                SELECT fr.""Grade55Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade55Costs'
                UNION ALL
                SELECT fr.""Grade65Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade65Costs'
                UNION ALL
                SELECT fr.""Grade71Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade71Costs'
                UNION ALL
                SELECT fr.""Grade75Costs"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'Grade75Costs'
                UNION ALL
                SELECT fr.""RecoveryTarget"", fr.""FinancialReferenceId"", fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferences"" fr
                CROSS JOIN ""FinancialReferenceValueSets"" fvs
                WHERE fvs.""Name"" = 'RecoveryTarget';

                UPDATE ""WorkloadModelChanges"" wmc
                SET ""CostValueSetId"" = fvs.""FinancialReferenceValueSetId""
                FROM ""FinancialReferenceValueSets"" fvs
                WHERE
                    (wmc.""Grade"" = 4 AND fvs.""Name"" = 'Grade41Costs') OR
                    (wmc.""Grade"" = 5 AND fvs.""Name"" = 'Grade51Costs') OR
                    (wmc.""Grade"" = 6 AND fvs.""Name"" = 'Grade65Costs') OR
                    (wmc.""Grade"" = 7 AND fvs.""Name"" = 'Grade75Costs');

                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade41Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade51Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade55Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade65Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade71Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""Grade75Costs"";
                ALTER TABLE ""FinancialReferences"" DROP COLUMN ""RecoveryTarget"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade41Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade51Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade55Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade65Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade71Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""Grade75Costs"" real NOT NULL DEFAULT 0;
                ALTER TABLE ""FinancialReferences"" ADD COLUMN ""RecoveryTarget"" real NOT NULL DEFAULT 0;

                UPDATE ""FinancialReferences"" fr SET
                    ""Grade41Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade41Costs' LIMIT 1), 0),
                    ""Grade51Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade51Costs' LIMIT 1), 0),
                    ""Grade55Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade55Costs' LIMIT 1), 0),
                    ""Grade65Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade65Costs' LIMIT 1), 0),
                    ""Grade71Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade71Costs' LIMIT 1), 0),
                    ""Grade75Costs"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'Grade75Costs' LIMIT 1), 0),
                    ""RecoveryTarget"" = COALESCE((SELECT v.""Value"" FROM ""FinancialReferenceValues"" v JOIN ""FinancialReferenceValueSets"" s ON s.""FinancialReferenceValueSetId"" = v.""FinancialReferenceValueSetId"" WHERE v.""FinancialReferenceId"" = fr.""FinancialReferenceId"" AND s.""Name"" = 'RecoveryTarget' LIMIT 1), 0);
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
