using Microsoft.EntityFrameworkCore.Migrations;

namespace SystemClockSetterNTP.Migrations
{
    public partial class AddedSentAndReceivedFieldDataFromNIC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DataReceived",
                table: "ComputerDatas",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DataSent",
                table: "ComputerDatas",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReceived",
                table: "ComputerDatas");

            migrationBuilder.DropColumn(
                name: "DataSent",
                table: "ComputerDatas");
        }
    }
}
