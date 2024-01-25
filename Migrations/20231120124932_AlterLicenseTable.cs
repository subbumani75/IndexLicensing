using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndexInfo.Migrations
{
    /// <inheritdoc />
    public partial class AlterLicenseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "License");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "License",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "License");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "License",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
