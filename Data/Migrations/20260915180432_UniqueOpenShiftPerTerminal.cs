using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoDeVentaAtlas.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueOpenShiftPerTerminal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "open_shift_key",
                table: "cash_shifts",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true,
                computedColumnSql: "CASE WHEN status = 'open' THEN '1' ELSE NULL END",
                stored: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cash_shifts_store_id_user_id_workstation_id_open_shift_key",
                table: "cash_shifts",
                columns: new[] { "store_id", "user_id", "workstation_id", "open_shift_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cash_shifts_store_id_user_id_workstation_id_open_shift_key",
                table: "cash_shifts");

            migrationBuilder.DropColumn(
                name: "open_shift_key",
                table: "cash_shifts");

            migrationBuilder.CreateIndex(
                name: "IX_cash_shifts_store_id_workstation_id_user_id_status",
                table: "cash_shifts",
                columns: new[] { "store_id", "workstation_id", "user_id", "status" });
        }
    }
}
