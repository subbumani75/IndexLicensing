using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndexInfo.Migrations
{
    /// <inheritdoc />
    public partial class AlterLicenseTableColumnInCompanyIdToTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "License",
                newName: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "License",
                newName: "CompanyId");
        }
    }
}
