using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Algoserver.API.Migrations
{
    public partial class AddNAStatistic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.CreateTable(
                name: "NAAccountBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Account = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    Balance = table.Column<double>(nullable: false),
                    Pnl = table.Column<double>(nullable: false),
                    AccountType = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NAAccountBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NALogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Account = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NALogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NAAccountBalances");

            migrationBuilder.DropTable(
                name: "NALogs");

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountSize = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    Ip = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Market = table.Column<string>(nullable: true),
                    RiskOverride = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    SplitPositions = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    StopLossRatio = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TimeFrameInterval = table.Column<int>(nullable: false),
                    TimeFramePeriodicity = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                });
        }
    }
}
