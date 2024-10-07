using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abp.SilkierQuartzDemo.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Added_QuartzRecentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Silkier");

            migrationBuilder.CreateTable(
                name: "QuartzExecutionHistories",
                schema: "Silkier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FireInstanceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchedulerInstanceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchedulerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Job = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Trigger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ScheduledFireTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ActualFireTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Recovering = table.Column<bool>(type: "bit", nullable: false),
                    Vetoed = table.Column<bool>(type: "bit", nullable: false),
                    FinishedTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzExecutionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuartzJobSummaries",
                schema: "Silkier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchedulerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalJobsExecuted = table.Column<int>(type: "int", nullable: false),
                    TotalJobsFailed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzJobSummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuartzExecutionHistories_FireInstanceId",
                schema: "Silkier",
                table: "QuartzExecutionHistories",
                column: "FireInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuartzExecutionHistories",
                schema: "Silkier");

            migrationBuilder.DropTable(
                name: "QuartzJobSummaries",
                schema: "Silkier");
        }
    }
}
