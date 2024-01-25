using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndexInfo.Migrations
{
    /// <inheritdoc />
    public partial class RenameCompanyMasterToCustomerMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyMaster",
                table: "CompanyMaster");

            migrationBuilder.RenameTable(
                name: "CompanyMaster",
                newName: "CustomerMaster");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "CustomerMaster",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "CompanyMailId",
                table: "CustomerMaster",
                newName: "CustomerMailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerMaster",
                table: "CustomerMaster",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerMaster",
                table: "CustomerMaster");

            migrationBuilder.RenameTable(
                name: "CustomerMaster",
                newName: "CompanyMaster");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "CompanyMaster",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "CustomerMailId",
                table: "CompanyMaster",
                newName: "CompanyMailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyMaster",
                table: "CompanyMaster",
                column: "Id");
        }
    }
}
