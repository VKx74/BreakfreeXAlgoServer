using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Algoserver.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(Program.DbName);

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Ip = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AccountSize = table.Column<decimal>(nullable: false),
                    Market = table.Column<string>(nullable: true),
                    TimeFramePeriodicity = table.Column<string>(nullable: true),
                    TimeFrameInterval = table.Column<int>(nullable: false),
                    StopLossRatio = table.Column<decimal>(nullable: false),
                    RiskOverride = table.Column<decimal>(nullable: false),
                    SplitPositions = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");
        }
    }
}
