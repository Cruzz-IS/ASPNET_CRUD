using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiRRHH.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1020), new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1021), "$2a$11$FLsW5L7u3glmPGjmODdN.eNrJyV8Pa7qUP4gM/AyMPBFItkO7BnzG" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1030), new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1030), "$2a$11$ULJVV3o9Y3Sp/FOQAXV/C.VOLSV8LM1Nf1TmwRFIF3cjK/eLobPx." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1034), new DateTime(2026, 4, 11, 5, 40, 36, 783, DateTimeKind.Utc).AddTicks(1035), "$2a$11$ULJVV3o9Y3Sp/FOQAXV/C.VOLSV8LM1Nf1TmwRFIF3cjK/eLobPx." });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "Audits",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "Audits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "Audits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(764), new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(765), "$2a$11$VAIs76c.8EHk.xYe4DuF3OphojU0QqvLzNzpysm5dbSOejAikI8BO" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(774), new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(774), "$2a$11$7u3QkCJJBwNrgnoH7MNB8epC1QHT.VypVA1bRHeOxDHaa.pvgIxAC" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordChangedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(778), new DateTime(2026, 4, 10, 20, 28, 36, 762, DateTimeKind.Utc).AddTicks(779), "$2a$11$7u3QkCJJBwNrgnoH7MNB8epC1QHT.VypVA1bRHeOxDHaa.pvgIxAC" });
        }
    }
}
