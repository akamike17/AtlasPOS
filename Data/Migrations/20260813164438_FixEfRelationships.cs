using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoDeVentaAtlas.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEfRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_sales_sale_entity_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_lines_purchases_purchase_entity_id",
                table: "purchase_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_lines_sales_sale_entity_id",
                table: "sale_lines");

            migrationBuilder.DropIndex(
                name: "IX_sale_lines_sale_entity_id",
                table: "sale_lines");

            migrationBuilder.DropIndex(
                name: "IX_purchase_lines_purchase_entity_id",
                table: "purchase_lines");

            migrationBuilder.DropIndex(
                name: "IX_payments_sale_entity_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "sale_entity_id",
                table: "sale_lines");

            migrationBuilder.DropColumn(
                name: "purchase_entity_id",
                table: "purchase_lines");

            migrationBuilder.DropColumn(
                name: "sale_entity_id",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_sale_lines_sale_id",
                table: "sale_lines",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_lines_purchase_id",
                table: "purchase_lines",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_sale_id",
                table: "payments",
                column: "sale_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_sales_sale_id",
                table: "payments",
                column: "sale_id",
                principalTable: "sales",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_lines_purchases_purchase_id",
                table: "purchase_lines",
                column: "purchase_id",
                principalTable: "purchases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_lines_sales_sale_id",
                table: "sale_lines",
                column: "sale_id",
                principalTable: "sales",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_sales_sale_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_lines_purchases_purchase_id",
                table: "purchase_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_lines_sales_sale_id",
                table: "sale_lines");

            migrationBuilder.DropIndex(
                name: "IX_sale_lines_sale_id",
                table: "sale_lines");

            migrationBuilder.DropIndex(
                name: "IX_purchase_lines_purchase_id",
                table: "purchase_lines");

            migrationBuilder.DropIndex(
                name: "IX_payments_sale_id",
                table: "payments");

            migrationBuilder.AddColumn<long>(
                name: "sale_entity_id",
                table: "sale_lines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "purchase_entity_id",
                table: "purchase_lines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "sale_entity_id",
                table: "payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sale_lines_sale_entity_id",
                table: "sale_lines",
                column: "sale_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_lines_purchase_entity_id",
                table: "purchase_lines",
                column: "purchase_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_sale_entity_id",
                table: "payments",
                column: "sale_entity_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_sales_sale_entity_id",
                table: "payments",
                column: "sale_entity_id",
                principalTable: "sales",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_lines_purchases_purchase_entity_id",
                table: "purchase_lines",
                column: "purchase_entity_id",
                principalTable: "purchases",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_sale_lines_sales_sale_entity_id",
                table: "sale_lines",
                column: "sale_entity_id",
                principalTable: "sales",
                principalColumn: "id");
        }
    }
}
