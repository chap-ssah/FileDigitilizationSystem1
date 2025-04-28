using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileDigitilizationSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRequests_AspNetUsers_RequesterId",
                table: "FileRequests");

            migrationBuilder.DropTable(
                name: "DigitizationRequests");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "FileRequests",
                newName: "Suburb");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantType",
                table: "FileRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRequests_AspNetUsers_RequesterId",
                table: "FileRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRequests_AspNetUsers_RequesterId",
                table: "FileRequests");

            migrationBuilder.DropColumn(
                name: "ApplicantType",
                table: "FileRequests");

            migrationBuilder.RenameColumn(
                name: "Suburb",
                table: "FileRequests",
                newName: "Location");

            migrationBuilder.CreateTable(
                name: "DigitizationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileRecordId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitizationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitizationRequests_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitizationRequests_FileRecords_FileRecordId",
                        column: x => x.FileRecordId,
                        principalTable: "FileRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitizationRequests_FileRecordId",
                table: "DigitizationRequests",
                column: "FileRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitizationRequests_RequesterId",
                table: "DigitizationRequests",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRequests_AspNetUsers_RequesterId",
                table: "FileRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
