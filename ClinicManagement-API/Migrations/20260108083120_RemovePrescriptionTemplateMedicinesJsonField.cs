using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement_API.Migrations
{
    /// <inheritdoc />
    public partial class RemovePrescriptionTemplateMedicinesJsonField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Medicines",
                table: "PrescriptionTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Medicines",
                table: "PrescriptionTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
