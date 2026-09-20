using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class Added_AuditLocation_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLocations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AuditTrailId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    IpCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpRegion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IpCity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IpLatitude = table.Column<double>(type: "float", nullable: true),
                    IpLongitude = table.Column<double>(type: "float", nullable: true),
                    DeviceLatitude = table.Column<double>(type: "float", nullable: true),
                    DeviceLongitude = table.Column<double>(type: "float", nullable: true),
                    DeviceAccuracyMeters = table.Column<double>(type: "float", nullable: true),
                    DeviceLocationCapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLocations_AuditTrails_AuditTrailId",
                        column: x => x.AuditTrailId,
                        principalTable: "AuditTrails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLocations_AuditTrailId",
                table: "AuditLocations",
                column: "AuditTrailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLocations");
        }
    }
}
