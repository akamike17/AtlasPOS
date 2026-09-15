using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable
namespace PuntoDeVentaAtlas.Web.Data.Migrations;

public partial class MultiWorkstationTerminals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name:"IX_peripheral_configurations_store_id_key",table:"peripheral_configurations");
        migrationBuilder.AddColumn<string>(name:"workstation_id",table:"sales",type:"varchar(32)",maxLength:32,nullable:false,defaultValue:"SERVER").Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.AddColumn<string>(name:"workstation_id",table:"peripheral_configurations",type:"varchar(32)",maxLength:32,nullable:false,defaultValue:"SERVER").Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.AddColumn<string>(name:"workstation_id",table:"cash_shifts",type:"varchar(32)",maxLength:32,nullable:false,defaultValue:"SERVER").Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.CreateTable(name:"workstations",columns:table=>new
        {
            id=table.Column<long>(type:"bigint",nullable:false).Annotation("MySql:ValueGenerationStrategy",MySqlValueGenerationStrategy.IdentityColumn),
            store_id=table.Column<long>(type:"bigint",nullable:false),terminal_id=table.Column<string>(type:"varchar(32)",maxLength:32,nullable:false).Annotation("MySql:CharSet","utf8mb4"),name=table.Column<string>(type:"longtext",nullable:false).Annotation("MySql:CharSet","utf8mb4"),enabled=table.Column<bool>(type:"tinyint(1)",nullable:false),first_seen_at=table.Column<DateTime>(type:"datetime(6)",nullable:false),last_seen_at=table.Column<DateTime>(type:"datetime(6)",nullable:false)
        },constraints:table=>table.PrimaryKey("PK_workstations",x=>x.id)).Annotation("MySql:CharSet","utf8mb4");
        migrationBuilder.CreateIndex(name:"IX_peripheral_configurations_store_id_workstation_id_key",table:"peripheral_configurations",columns:new[]{"store_id","workstation_id","key"},unique:true);
        migrationBuilder.CreateIndex(name:"IX_workstations_store_id_terminal_id",table:"workstations",columns:new[]{"store_id","terminal_id"},unique:true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name:"workstations");migrationBuilder.DropIndex(name:"IX_peripheral_configurations_store_id_workstation_id_key",table:"peripheral_configurations");migrationBuilder.DropColumn(name:"workstation_id",table:"sales");migrationBuilder.DropColumn(name:"workstation_id",table:"peripheral_configurations");migrationBuilder.DropColumn(name:"workstation_id",table:"cash_shifts");migrationBuilder.CreateIndex(name:"IX_peripheral_configurations_store_id_key",table:"peripheral_configurations",columns:new[]{"store_id","key"},unique:true);
    }
}
