using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptionsTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionsPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnderlyingSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OptionType = table.Column<int>(type: "int", nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    StrikePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contracts = table.Column<int>(type: "int", nullable: false),
                    PremiumPerContract = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnderlyingPositionId = table.Column<int>(type: "int", nullable: true),
                    RolledFromId = table.Column<int>(type: "int", nullable: true),
                    RolledToId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionsPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionsPositions_OptionsPositions_RolledFromId",
                        column: x => x.RolledFromId,
                        principalTable: "OptionsPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptionsPositions_Positions_UnderlyingPositionId",
                        column: x => x.UnderlyingPositionId,
                        principalTable: "Positions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RollHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromOptionsPositionId = table.Column<int>(type: "int", nullable: false),
                    ToOptionsPositionId = table.Column<int>(type: "int", nullable: false),
                    RollDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NetCredit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RollHistories_OptionsPositions_FromOptionsPositionId",
                        column: x => x.FromOptionsPositionId,
                        principalTable: "OptionsPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RollHistories_OptionsPositions_ToOptionsPositionId",
                        column: x => x.ToOptionsPositionId,
                        principalTable: "OptionsPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Fees = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OptionType = table.Column<int>(type: "int", nullable: true),
                    StrikePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    OptionsPositionId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_OptionsPositions_OptionsPositionId",
                        column: x => x.OptionsPositionId,
                        principalTable: "OptionsPositions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionsPositions_ExpirationDate",
                table: "OptionsPositions",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_OptionsPositions_RolledFromId",
                table: "OptionsPositions",
                column: "RolledFromId",
                unique: true,
                filter: "[RolledFromId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OptionsPositions_Status",
                table: "OptionsPositions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OptionsPositions_UnderlyingPositionId",
                table: "OptionsPositions",
                column: "UnderlyingPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionsPositions_UnderlyingSymbol",
                table: "OptionsPositions",
                column: "UnderlyingSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Account",
                table: "Positions",
                column: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Symbol",
                table: "Positions",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistories_FromOptionsPositionId",
                table: "RollHistories",
                column: "FromOptionsPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistories_ToOptionsPositionId",
                table: "RollHistories",
                column: "ToOptionsPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Account",
                table: "Transactions",
                column: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OptionsPositionId",
                table: "Transactions",
                column: "OptionsPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PositionId",
                table: "Transactions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Symbol",
                table: "Transactions",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RollHistories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "OptionsPositions");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
