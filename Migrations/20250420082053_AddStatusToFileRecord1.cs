using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileDigitilizationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToFileRecord1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DensityType",
                table: "FileRecords",
                newName: "SpecialStatus");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantId",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantName",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantType",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployerInfo",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LandUseType",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Row",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Shelf",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantId",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "ApplicantName",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "ApplicantType",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "EmployerInfo",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "LandUseType",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "Shelf",
                table: "FileRecords");

            migrationBuilder.RenameColumn(
                name: "SpecialStatus",
                table: "FileRecords",
                newName: "DensityType");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FileRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                table: "FileRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "FileRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
