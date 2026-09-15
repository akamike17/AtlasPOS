using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace PuntoDeVentaAtlas.Web.Data.Migrations;

[DbContext(typeof(AtlasDbContext))]
[Migration("20260813220000_AddPartialReturnLines")]
public partial class AddPartialReturnLines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name:"sale_return_lines",columns:table=>new
        {
            id=table.Column<long>(type:"bigint",nullable:false).Annotation("MySql:ValueGenerationStrategy",MySqlValueGenerationStrategy.IdentityColumn),
            return_id=table.Column<long>(type:"bigint",nullable:false),sale_line_id=table.Column<long>(type:"bigint",nullable:false),product_id=table.Column<long>(type:"bigint",nullable:false),
            quantity=table.Column<decimal>(type:"decimal(18,3)",nullable:false),total=table.Column<decimal>(type:"decimal(18,2)",nullable:false),restocked=table.Column<bool>(type:"tinyint(1)",nullable:false)
        },constraints:table=>{table.PrimaryKey("PK_sale_return_lines",x=>x.id);table.ForeignKey("FK_sale_return_lines_sale_returns_return_id",x=>x.return_id,"sale_returns","id",onDelete:ReferentialAction.Cascade);}).Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.CreateIndex(name:"IX_sale_return_lines_return_id",table:"sale_return_lines",column:"return_id");
        migrationBuilder.CreateIndex(name:"IX_sale_return_lines_sale_line_id",table:"sale_return_lines",column:"sale_line_id");
    }
    protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropTable(name:"sale_return_lines");
}
