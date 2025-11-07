using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddContractsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    project_request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    attachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expired_date = table.Column<DateOnly>(type: "date", nullable: false),
                    budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Contracts_ProjectRequests_project_request_id",
                        column: x => x.project_request_id,
                        principalTable: "ProjectRequests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractAppendices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    appendix_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    file_path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAppendices", x => x.id);
                    table.ForeignKey(
                        name: "FK_ContractAppendices_Contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "Contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractViolations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    violation_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractViolations", x => x.id);
                    table.ForeignKey(
                        name: "FK_ContractViolations_Contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "Contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractSolutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    violation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    solution_detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    penalty_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    other_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    violation_id1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSolutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_ContractSolutions_ContractViolations_violation_id1",
                        column: x => x.violation_id1,
                        principalTable: "ContractViolations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAppendices_contract_id",
                table: "ContractAppendices",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_project_request_id",
                table: "Contracts",
                column: "project_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractSolutions_violation_id1",
                table: "ContractSolutions",
                column: "violation_id1");

            migrationBuilder.CreateIndex(
                name: "IX_ContractViolations_contract_id",
                table: "ContractViolations",
                column: "contract_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAppendices");

            migrationBuilder.DropTable(
                name: "ContractSolutions");

            migrationBuilder.DropTable(
                name: "ContractViolations");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
