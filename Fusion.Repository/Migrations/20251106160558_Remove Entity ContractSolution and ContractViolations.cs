using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntityContractSolutionandContractViolations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSolutions");

            migrationBuilder.DropTable(
                name: "ContractViolations");

            migrationBuilder.AddColumn<string>(
                name: "contract_name",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_name",
                table: "Contracts");

            migrationBuilder.CreateTable(
                name: "ContractViolations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    violation_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
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
                    violation_id1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    other_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    penalty_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    solution_detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    violation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "IX_ContractSolutions_violation_id1",
                table: "ContractSolutions",
                column: "violation_id1");

            migrationBuilder.CreateIndex(
                name: "IX_ContractViolations_contract_id",
                table: "ContractViolations",
                column: "contract_id");
        }
    }
}
