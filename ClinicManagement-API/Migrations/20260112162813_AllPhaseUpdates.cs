using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement_API.Migrations
{
    /// <inheritdoc />
    public partial class AllPhaseUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Medicines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "Medicines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Medicines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<byte>(type: "smallint", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<byte>(type: "smallint", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ChangesSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditLogs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ToothRecords",
                columns: table => new
                {
                    ToothRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToothNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    PreviousStatus = table.Column<byte>(type: "smallint", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastTreatment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastMedicalRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastTreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTreatedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothRecords", x => x.ToothRecordId);
                    table.ForeignKey(
                        name: "FK_ToothRecords_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToothRecords_Doctors_LastTreatedByDoctorId",
                        column: x => x.LastTreatedByDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ToothRecords_MedicalRecords_LastMedicalRecordId",
                        column: x => x.LastMedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ToothRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ClinicId",
                table: "AuditLogs",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_ClinicId",
                table: "ToothRecords",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_LastMedicalRecordId",
                table: "ToothRecords",
                column: "LastMedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_LastTreatedByDoctorId",
                table: "ToothRecords",
                column: "LastTreatedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_PatientId_ToothNumber",
                table: "ToothRecords",
                columns: new[] { "PatientId", "ToothNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ToothRecords");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Medicines");
        }
    }
}
