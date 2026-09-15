using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable
namespace PuntoDeVentaAtlas.Web.Data.Migrations;

[DbContext(typeof(AtlasDbContext))]
[Migration("20260813210000_OperationalCashAndIdempotency")]
public partial class OperationalCashAndIdempotency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name:"client_operation_id",table:"sales",type:"varchar(64)",maxLength:64,nullable:true)
            .Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.Sql("UPDATE sales SET client_operation_id = CONCAT('legacy-', id) WHERE client_operation_id IS NULL");
        migrationBuilder.AlterColumn<string>(name:"client_operation_id",table:"sales",type:"varchar(64)",maxLength:64,nullable:false,oldClrType:typeof(string),oldType:"varchar(64)",oldMaxLength:64,oldNullable:true)
            .Annotation("MySql:CharSet","utf8mb4").OldAnnotation("MySql:CharSet","utf8mb4");
        migrationBuilder.CreateTable(name:"cash_movements",columns:table=>new
        {
            id=table.Column<long>(type:"bigint",nullable:false).Annotation("MySql:ValueGenerationStrategy",MySqlValueGenerationStrategy.IdentityColumn),
            shift_id=table.Column<long>(type:"bigint",nullable:false),user_id=table.Column<long>(type:"bigint",nullable:false),
            kind=table.Column<string>(type:"longtext",nullable:false).Annotation("MySql:CharSet","utf8mb4"),amount=table.Column<decimal>(type:"decimal(18,2)",nullable:false),
            reason=table.Column<string>(type:"longtext",nullable:false).Annotation("MySql:CharSet","utf8mb4"),created_at=table.Column<DateTime>(type:"datetime(6)",nullable:false)
        },constraints:table=>{table.PrimaryKey("PK_cash_movements",x=>x.id);table.ForeignKey("FK_cash_movements_cash_shifts_shift_id",x=>x.shift_id,"cash_shifts","id",onDelete:ReferentialAction.Cascade);})
        .Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.CreateIndex(name:"IX_sales_store_id_client_operation_id",table:"sales",columns:new[]{"store_id","client_operation_id"},unique:true);
        migrationBuilder.CreateIndex(name:"IX_cash_movements_shift_id",table:"cash_movements",column:"shift_id");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name:"cash_movements");migrationBuilder.DropIndex(name:"IX_sales_store_id_client_operation_id",table:"sales");migrationBuilder.DropColumn(name:"client_operation_id",table:"sales");
    }
}
