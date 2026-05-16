using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApiRRHH.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithAuthFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bonos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<double>(type: "float", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cargo",
                columns: table => new
                {
                    IdCargo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SueldoBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargo", x => x.IdCargo);
                });

            migrationBuilder.CreateTable(
                name: "Empleado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DNI = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    EstadoCivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoContrato = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Employee"),
                    idEmpleadoJefe = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleado_Empleado_idEmpleadoJefe",
                        column: x => x.idEmpleadoJefe,
                        principalTable: "Empleado",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Planillas",
                columns: table => new
                {
                    IdPlanilla = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaEfectiva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planillas", x => x.IdPlanilla);
                });

            migrationBuilder.CreateTable(
                name: "TipoDeducciones",
                columns: table => new
                {
                    IdTipoDeduccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDeducciones", x => x.IdTipoDeduccion);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmpleadoAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audit_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargoEmpleado",
                columns: table => new
                {
                    IdCargo = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    CargoIdCargo = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    fechaNombramiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoEmpleado", x => new { x.IdCargo, x.IdEmpleado });
                    table.ForeignKey(
                        name: "FK_CargoEmpleado_Cargo_CargoIdCargo",
                        column: x => x.CargoIdCargo,
                        principalTable: "Cargo",
                        principalColumn: "IdCargo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargoEmpleado_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmpleadoAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anticipos",
                columns: table => new
                {
                    IdAnticipo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Empleado_idEmpleado = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Planilla_idPlanilla = table.Column<int>(type: "int", nullable: false),
                    PlanillaIdPlanilla = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anticipos", x => x.IdAnticipo);
                    table.ForeignKey(
                        name: "FK_Anticipos_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anticipos_Planillas_PlanillaIdPlanilla",
                        column: x => x.PlanillaIdPlanilla,
                        principalTable: "Planillas",
                        principalColumn: "IdPlanilla",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmpleadoBonos",
                columns: table => new
                {
                    Empleado_idEmpleado = table.Column<int>(type: "int", nullable: false),
                    Bono_idBono = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    BonoId = table.Column<int>(type: "int", nullable: false),
                    Planilla_idPlanilla = table.Column<int>(type: "int", nullable: false),
                    PlanillaIdPlanilla = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadoBonos", x => new { x.Empleado_idEmpleado, x.Bono_idBono });
                    table.ForeignKey(
                        name: "FK_EmpleadoBonos_Bonos_BonoId",
                        column: x => x.BonoId,
                        principalTable: "Bonos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpleadoBonos_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpleadoBonos_Planillas_PlanillaIdPlanilla",
                        column: x => x.PlanillaIdPlanilla,
                        principalTable: "Planillas",
                        principalColumn: "IdPlanilla",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deducciones",
                columns: table => new
                {
                    IdDeduccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    idTipoDeduccion = table.Column<int>(type: "int", nullable: false),
                    TipoDeduccionIdTipoDeduccion = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deducciones", x => x.IdDeduccion);
                    table.ForeignKey(
                        name: "FK_Deducciones_TipoDeducciones_TipoDeduccionIdTipoDeduccion",
                        column: x => x.TipoDeduccionIdTipoDeduccion,
                        principalTable: "TipoDeducciones",
                        principalColumn: "IdTipoDeduccion");
                });

            migrationBuilder.CreateTable(
                name: "DeduccionesEmpleados",
                columns: table => new
                {
                    IdDeduccion = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    DeduccionIdDeduccion = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    IdPlanilla = table.Column<int>(type: "int", nullable: false),
                    PlanillaIdPlanilla = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoDeduccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeduccionesEmpleados", x => new { x.IdDeduccion, x.IdEmpleado });
                    table.ForeignKey(
                        name: "FK_DeduccionesEmpleados_Deducciones_DeduccionIdDeduccion",
                        column: x => x.DeduccionIdDeduccion,
                        principalTable: "Deducciones",
                        principalColumn: "IdDeduccion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeduccionesEmpleados_Empleado_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeduccionesEmpleados_Planillas_PlanillaIdPlanilla",
                        column: x => x.PlanillaIdPlanilla,
                        principalTable: "Planillas",
                        principalColumn: "IdPlanilla",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Empleado",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DNI", "Email", "EmailConfirmed", "EstadoCivil", "FailedLoginAttempts", "IsActive", "LastLoginDate", "LockoutEnd", "Name", "PasswordChangedDate", "PasswordHash", "PhoneNumber", "ResetPasswordToken", "ResetPasswordTokenExpiry", "Role", "TipoContrato", "UpdatedAt", "UpdatedBy", "Username", "idEmpleadoJefe" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8295), null, "", "admin@gmail.com", true, null, 0, true, null, null, "Admin", new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8297), "$2a$11$AGIoO2T.SX6GjwDkT5MKeuJ38N3U3bUDg.J27anZGIbKsSOqPuXYy", "+504 9999-0000", null, null, "Admin", null, null, null, null, null },
                    { 2, new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8311), null, "", "juan.perez@yahoo.com", true, null, 0, true, null, null, "Juan", new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8312), "$2a$11$1sLhxL/dAp3p2AmTqifUd.7ozSkVBobtOnvY9DmuZsOA7PUPJdLdu", "+504 9999-8888", null, null, "Empleado", null, null, null, null, null },
                    { 3, new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8318), null, "", "maria.gonzalez@gmail.com", true, null, 0, true, null, null, "María", new DateTime(2026, 5, 15, 7, 18, 9, 957, DateTimeKind.Utc).AddTicks(8319), "$2a$11$1sLhxL/dAp3p2AmTqifUd.7ozSkVBobtOnvY9DmuZsOA7PUPJdLdu", "+504 9999-7777", null, null, "Cliente", null, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anticipos_EmpleadoId",
                table: "Anticipos",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anticipos_PlanillaIdPlanilla",
                table: "Anticipos",
                column: "PlanillaIdPlanilla");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "Audit",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmpleadoId",
                table: "Audit",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "Audit",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_CargoEmpleado_CargoIdCargo",
                table: "CargoEmpleado",
                column: "CargoIdCargo");

            migrationBuilder.CreateIndex(
                name: "IX_CargoEmpleado_EmpleadoId",
                table: "CargoEmpleado",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Deducciones_TipoDeduccionIdTipoDeduccion",
                table: "Deducciones",
                column: "TipoDeduccionIdTipoDeduccion");

            migrationBuilder.CreateIndex(
                name: "IX_DeduccionesEmpleados_DeduccionIdDeduccion",
                table: "DeduccionesEmpleados",
                column: "DeduccionIdDeduccion");

            migrationBuilder.CreateIndex(
                name: "IX_DeduccionesEmpleados_EmpleadoId",
                table: "DeduccionesEmpleados",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_DeduccionesEmpleados_PlanillaIdPlanilla",
                table: "DeduccionesEmpleados",
                column: "PlanillaIdPlanilla");

            migrationBuilder.CreateIndex(
                name: "IX_Empleado_idEmpleadoJefe",
                table: "Empleado",
                column: "idEmpleadoJefe");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Email",
                table: "Empleado",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IsActive",
                table: "Empleado",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Role",
                table: "Empleado",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoBonos_BonoId",
                table: "EmpleadoBonos",
                column: "BonoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoBonos_EmpleadoId",
                table: "EmpleadoBonos",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoBonos_PlanillaIdPlanilla",
                table: "EmpleadoBonos",
                column: "PlanillaIdPlanilla");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_EmpleadoId",
                table: "RefreshToken",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshToken",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshToken",
                column: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anticipos");

            migrationBuilder.DropTable(
                name: "Audit");

            migrationBuilder.DropTable(
                name: "CargoEmpleado");

            migrationBuilder.DropTable(
                name: "DeduccionesEmpleados");

            migrationBuilder.DropTable(
                name: "EmpleadoBonos");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "Cargo");

            migrationBuilder.DropTable(
                name: "Deducciones");

            migrationBuilder.DropTable(
                name: "Bonos");

            migrationBuilder.DropTable(
                name: "Planillas");

            migrationBuilder.DropTable(
                name: "Empleado");

            migrationBuilder.DropTable(
                name: "TipoDeducciones");
        }
    }
}
