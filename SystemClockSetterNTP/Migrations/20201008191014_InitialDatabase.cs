using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SystemClockSetterNTP.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputerDatas",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<string>(nullable: false),
                    Time = table.Column<TimeSpan>(nullable: false),
                    PowerOnCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerDatas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComputerDatas_Date",
                table: "ComputerDatas",
                column: "Date",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComputerDatas");
        }
    }
}
