using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kanini.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "PatientIdentifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlobalPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalPatientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNameHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FirstNameHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DateOfBirthHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientIdentifiers_Organizations_SourceOrganizationId",
                        column: x => x.SourceOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlobalPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestingUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestingOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrganizationId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_DataRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId");
                    table.ForeignKey(
                        name: "FK_DataRequests_Organizations_OrganizationId1",
                        column: x => x.OrganizationId1,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId");
                    table.ForeignKey(
                        name: "FK_DataRequests_Organizations_RequestingOrganizationId",
                        column: x => x.RequestingOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRequests_Organizations_SourceOrganizationId",
                        column: x => x.SourceOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRequests_Users_RequestingUserId",
                        column: x => x.RequestingUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConversionJobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InputFormat = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PatientsCount = table.Column<int>(type: "int", nullable: false),
                    ObservationsCount = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionJobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_ConversionJobs_DataRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DataRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConversionJobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversionJobs_RequestId",
                table: "ConversionJobs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversionJobs_UserId",
                table: "ConversionJobs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_ApprovedByUserId",
                table: "DataRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_ExpiresAt",
                table: "DataRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_OrganizationId",
                table: "DataRequests",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_OrganizationId1",
                table: "DataRequests",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_Patient_Organizations",
                table: "DataRequests",
                columns: new[] { "GlobalPatientId", "RequestingOrganizationId", "SourceOrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_RequestingOrganizationId",
                table: "DataRequests",
                column: "RequestingOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_RequestingUserId",
                table: "DataRequests",
                column: "RequestingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_SourceOrganizationId",
                table: "DataRequests",
                column: "SourceOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_Status",
                table: "DataRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OrganizationId",
                table: "Organizations",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIdentifiers_GlobalPatientId_SourceOrganizationId",
                table: "PatientIdentifiers",
                columns: new[] { "GlobalPatientId", "SourceOrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIdentifiers_LastNameHash_DateOfBirthHash",
                table: "PatientIdentifiers",
                columns: new[] { "LastNameHash", "DateOfBirthHash" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIdentifiers_SourceOrganizationId",
                table: "PatientIdentifiers",
                column: "SourceOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionJobs");

            migrationBuilder.DropTable(
                name: "PatientIdentifiers");

            migrationBuilder.DropTable(
                name: "DataRequests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
