using Microsoft.EntityFrameworkCore.Migrations;

namespace SystemClockSetterNTP.Migrations
{
    public partial class ChangedSentAndReceivedName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReceived",
                table: "ComputerDatas");

            migrationBuilder.DropColumn(
                name: "DataSent",
                table: "ComputerDatas");

            migrationBuilder.AddColumn<double>(
                name: "GigabytesReceived",
                table: "ComputerDatas",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GigabytesSent",
                table: "ComputerDatas",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GigabytesReceived",
                table: "ComputerDatas");

            migrationBuilder.DropColumn(
                name: "GigabytesSent",
                table: "ComputerDatas");

            migrationBuilder.AddColumn<double>(
                name: "DataReceived",
                table: "ComputerDatas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DataSent",
                table: "ComputerDatas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
