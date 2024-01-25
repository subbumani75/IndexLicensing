﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndexInfo.Migrations
{
    /// <inheritdoc />
    public partial class AddExpireDateInLicenseTableNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
              name: "GenarationOn",
              table: "License",
              type: "datetime2",
              nullable: false,
              defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredOn",
                table: "License",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
              name: "GenarationOn",
              table: "License");

            migrationBuilder.DropColumn(
                name: "ExpiredOn",
                table: "License");
        }
    }
}
