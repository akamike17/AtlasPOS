using Microsoft.EntityFrameworkCore;

namespace PuntoDeVentaAtlas.Web.Data;

public sealed class AtlasDbContext(DbContextOptions<AtlasDbContext> options) : DbContext(options)
{
    public DbSet<StoreEntity> Stores => Set<StoreEntity>(); public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>(); public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>(); public DbSet<CashShiftEntity> CashShifts => Set<CashShiftEntity>(); public DbSet<CashMovementEntity> CashMovements => Set<CashMovementEntity>();
    public DbSet<SaleEntity> Sales => Set<SaleEntity>(); public DbSet<SaleLineEntity> SaleLines => Set<SaleLineEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>(); public DbSet<InventoryMovementEntity> InventoryMovements => Set<InventoryMovementEntity>();
    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>(); public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();
    public DbSet<PurchaseLineEntity> PurchaseLines => Set<PurchaseLineEntity>(); public DbSet<SaleReturnEntity> SaleReturns => Set<SaleReturnEntity>(); public DbSet<SaleReturnLineEntity> SaleReturnLines => Set<SaleReturnLineEntity>();
    public DbSet<AuditEntity> AuditLog => Set<AuditEntity>(); public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();
    public DbSet<SignatureEntity> Signatures => Set<SignatureEntity>();
    public DbSet<PeripheralConfigurationEntity> PeripheralConfigurations => Set<PeripheralConfigurationEntity>();
    public DbSet<WorkstationEntity> Workstations => Set<WorkstationEntity>();
    public DbSet<RecipeEntity> Recipes=>Set<RecipeEntity>(); public DbSet<RecipeLineEntity> RecipeLines=>Set<RecipeLineEntity>(); public DbSet<ProductionOrderEntity> ProductionOrders=>Set<ProductionOrderEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<StoreEntity>().ToTable("stores"); b.Entity<UserEntity>().ToTable("users"); b.Entity<CategoryEntity>().ToTable("categories"); b.Entity<ProductEntity>().ToTable("products"); b.Entity<CustomerEntity>().ToTable("customers"); b.Entity<CashShiftEntity>().ToTable("cash_shifts"); b.Entity<SaleEntity>().ToTable("sales"); b.Entity<SaleLineEntity>().ToTable("sale_lines"); b.Entity<PaymentEntity>().ToTable("payments"); b.Entity<InventoryMovementEntity>().ToTable("inventory_movements"); b.Entity<SupplierEntity>().ToTable("suppliers"); b.Entity<PurchaseEntity>().ToTable("purchases"); b.Entity<PurchaseLineEntity>().ToTable("purchase_lines"); b.Entity<SaleReturnEntity>().ToTable("sale_returns"); b.Entity<AuditEntity>().ToTable("audit_log"); b.Entity<InvoiceEntity>().ToTable("invoices"); b.Entity<SignatureEntity>().ToTable("signatures"); b.Entity<PeripheralConfigurationEntity>().ToTable("peripheral_configurations"); b.Entity<RecipeEntity>().ToTable("recipes");b.Entity<RecipeLineEntity>().ToTable("recipe_lines");b.Entity<ProductionOrderEntity>().ToTable("production_orders");
        b.Entity<CashMovementEntity>().ToTable("cash_movements"); b.Entity<SaleReturnLineEntity>().ToTable("sale_return_lines"); b.Entity<WorkstationEntity>().ToTable("workstations");
        b.Entity<ProductEntity>().HasIndex(x=>new{x.StoreId,x.Sku}).IsUnique(); b.Entity<ProductEntity>().HasIndex(x=>new{x.StoreId,x.Barcode}).IsUnique(); b.Entity<SaleEntity>().HasIndex(x=>new{x.StoreId,x.Folio}).IsUnique(); b.Entity<SaleEntity>().HasIndex(x=>new{x.StoreId,x.ClientOperationId}).IsUnique(); b.Entity<UserEntity>().HasIndex(x=>x.Email).IsUnique();
        b.Entity<CashShiftEntity>().HasMany(x=>x.Movements).WithOne().HasForeignKey(x=>x.ShiftId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<PeripheralConfigurationEntity>().HasIndex(x=>new{x.StoreId,x.WorkstationId,x.Key}).IsUnique();
        b.Entity<WorkstationEntity>().HasIndex(x=>new{x.StoreId,x.TerminalId}).IsUnique();
        b.Entity<CashShiftEntity>().Property(x=>x.Status).HasMaxLength(255);
        b.Entity<CashShiftEntity>().Property<string?>("OpenShiftKey").HasColumnName("open_shift_key").HasMaxLength(1).HasComputedColumnSql("CASE WHEN status = 'open' THEN '1' ELSE NULL END", stored:true);
        b.Entity<CashShiftEntity>().HasIndex("StoreId","UserId","WorkstationId","OpenShiftKey").IsUnique();
        b.Entity<WorkstationEntity>().Property(x=>x.TerminalId).HasMaxLength(32); b.Entity<PeripheralConfigurationEntity>().Property(x=>x.WorkstationId).HasMaxLength(32); b.Entity<SaleEntity>().Property(x=>x.WorkstationId).HasMaxLength(32); b.Entity<CashShiftEntity>().Property(x=>x.WorkstationId).HasMaxLength(32);
        b.Entity<RecipeEntity>().HasMany(x=>x.Components).WithOne().HasForeignKey(x=>x.RecipeId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SaleEntity>().HasMany(x=>x.Lines).WithOne().HasForeignKey(x=>x.SaleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SaleEntity>().HasMany(x=>x.Payments).WithOne().HasForeignKey(x=>x.SaleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SaleReturnEntity>().HasMany(x=>x.Lines).WithOne().HasForeignKey(x=>x.ReturnId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<PurchaseEntity>().HasMany(x=>x.Lines).WithOne().HasForeignKey(x=>x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
        foreach(var property in b.Model.GetEntityTypes().SelectMany(x=>x.GetProperties()).Where(x=>x.ClrType==typeof(decimal)||x.ClrType==typeof(decimal?)))
        {
            if(property.Name is "Quantity" or "Stock" or "MinimumStock" or "StockAfter") property.SetColumnType("decimal(18,3)");
            else if(property.Name is "Cost" or "UnitCost") property.SetColumnType("decimal(18,4)");
            else if(property.Name=="TaxRate") property.SetColumnType("decimal(7,4)");
            else property.SetColumnType("decimal(18,2)");
        }
        foreach(var type in b.Model.GetEntityTypes()) foreach(var property in type.GetProperties()) property.SetColumnName(ToSnake(property.Name));
    }
    private static string ToSnake(string value)=>string.Concat(value.Select((c,i)=>char.IsUpper(c)&&i>0?"_"+char.ToLowerInvariant(c):char.ToLowerInvariant(c).ToString()));
}

public sealed class StoreEntity { public long Id{get;set;} public string Name{get;set;}=""; public string? Rfc{get;set;} public string Timezone{get;set;}="America/Mexico_City"; public DateTime CreatedAt{get;set;}=DateTime.Now; }
public sealed class UserEntity { public long Id{get;set;} public long StoreId{get;set;} public string Name{get;set;}=""; public string Email{get;set;}=""; public string PasswordHash{get;set;}=""; public string Role{get;set;}="Cashier"; public bool Active{get;set;}=true; }
public sealed class WorkstationEntity { public long Id{get;set;} public long StoreId{get;set;} public string TerminalId{get;set;}=""; public string Name{get;set;}=""; public bool Enabled{get;set;}=true; public DateTime FirstSeenAt{get;set;}=DateTime.Now; public DateTime LastSeenAt{get;set;}=DateTime.Now; }
public sealed class CategoryEntity { public long Id{get;set;} public long StoreId{get;set;} public string Name{get;set;}=""; public bool Active{get;set;}=true; }
public sealed class ProductEntity { public long Id{get;set;} public long StoreId{get;set;} public long? CategoryId{get;set;} public CategoryEntity? Category{get;set;} public string Sku{get;set;}=""; public string? Barcode{get;set;} public string Name{get;set;}=""; public string Unit{get;set;}="pza"; public bool IsWeighted{get;set;} public decimal Price{get;set;} public decimal Cost{get;set;} public decimal TaxRate{get;set;} public decimal Stock{get;set;} public decimal MinimumStock{get;set;} public bool Active{get;set;}=true; }
public sealed class CustomerEntity { public long Id{get;set;} public long StoreId{get;set;} public string Name{get;set;}=""; public string? Rfc{get;set;} public string? LegalName{get;set;} public string? FiscalRegime{get;set;} public string? FiscalZip{get;set;} public string? CfdiUse{get;set;} public string? Email{get;set;} public string? Phone{get;set;} }
public sealed class CashShiftEntity { public long Id{get;set;} public long StoreId{get;set;} public long UserId{get;set;} public string WorkstationId{get;set;}="SERVER"; public DateTime OpenedAt{get;set;} public DateTime? ClosedAt{get;set;} public decimal OpeningAmount{get;set;} public decimal? ExpectedAmount{get;set;} public decimal? CountedAmount{get;set;} public string Status{get;set;}="open"; public List<SaleEntity> Sales{get;set;}=[]; public List<CashMovementEntity> Movements{get;set;}=[]; }
public sealed class CashMovementEntity { public long Id{get;set;} public long ShiftId{get;set;} public long UserId{get;set;} public string Kind{get;set;}="deposit"; public decimal Amount{get;set;} public string Reason{get;set;}=""; public DateTime CreatedAt{get;set;}=DateTime.Now; }
public sealed class SaleEntity { public long Id{get;set;} public long StoreId{get;set;} public string WorkstationId{get;set;}="SERVER"; public string ClientOperationId{get;set;}=""; public long ShiftId{get;set;} public CashShiftEntity Shift{get;set;}=null!; public long? CustomerId{get;set;} public string Folio{get;set;}=""; public decimal Subtotal{get;set;} public decimal Discount{get;set;} public decimal Tax{get;set;} public decimal Total{get;set;} public string Status{get;set;}="completed"; public DateTime CreatedAt{get;set;} public List<SaleLineEntity> Lines{get;set;}=[]; public List<PaymentEntity> Payments{get;set;}=[]; }
public sealed class SaleLineEntity { public long Id{get;set;} public long SaleId{get;set;} public long ProductId{get;set;} public string Description{get;set;}=""; public decimal Quantity{get;set;} public decimal UnitPrice{get;set;} public decimal Tax{get;set;} public decimal Total{get;set;} }
public sealed class PaymentEntity { public long Id{get;set;} public long SaleId{get;set;} public string Method{get;set;}="cash"; public decimal Amount{get;set;} public string? Reference{get;set;} public string? Authorization{get;set;} public string? Provider{get;set;} public string Status{get;set;}="approved"; public DateTime CreatedAt{get;set;} }
public sealed class InventoryMovementEntity { public long Id{get;set;} public long ProductId{get;set;} public long? UserId{get;set;} public long? SaleId{get;set;} public string Kind{get;set;}=""; public decimal Quantity{get;set;} public decimal StockAfter{get;set;} public string? Note{get;set;} public DateTime CreatedAt{get;set;} }
public sealed class SupplierEntity { public long Id{get;set;} public long StoreId{get;set;} public string Name{get;set;}=""; public string? Rfc{get;set;} public string? Email{get;set;} public string? Phone{get;set;} public bool Active{get;set;}=true; }
public sealed class PurchaseEntity { public long Id{get;set;} public long StoreId{get;set;} public long SupplierId{get;set;} public SupplierEntity Supplier{get;set;}=null!; public string Folio{get;set;}=""; public decimal Total{get;set;} public string Status{get;set;}="received"; public DateTime CreatedAt{get;set;} public List<PurchaseLineEntity> Lines{get;set;}=[]; }
public sealed class PurchaseLineEntity { public long Id{get;set;} public long PurchaseId{get;set;} public long ProductId{get;set;} public decimal Quantity{get;set;} public decimal UnitCost{get;set;} public decimal Total{get;set;} }
public sealed class SaleReturnEntity { public long Id{get;set;} public long StoreId{get;set;} public long SaleId{get;set;} public string Folio{get;set;}=""; public decimal Total{get;set;} public string Reason{get;set;}=""; public string Status{get;set;}="completed"; public DateTime CreatedAt{get;set;} public List<SaleReturnLineEntity> Lines{get;set;}=[]; }
public sealed class SaleReturnLineEntity { public long Id{get;set;} public long ReturnId{get;set;} public long SaleLineId{get;set;} public long ProductId{get;set;} public decimal Quantity{get;set;} public decimal Total{get;set;} public bool Restocked{get;set;} }
public sealed class AuditEntity { public long Id{get;set;} public long StoreId{get;set;} public long? UserId{get;set;} public string Action{get;set;}=""; public string Entity{get;set;}=""; public string? EntityId{get;set;} public string? Detail{get;set;} public DateTime CreatedAt{get;set;}=DateTime.Now; }
public sealed class InvoiceEntity { public long Id{get;set;} public long SaleId{get;set;} public string? Uuid{get;set;} public string Rfc{get;set;}=""; public string Status{get;set;}="pending"; public byte[]? Xml{get;set;} public byte[]? Pdf{get;set;} public DateTime? StampedAt{get;set;} }
public sealed class SignatureEntity { public long Id{get;set;} public long SaleId{get;set;} public string? DeviceId{get;set;} public byte[] Image{get;set;}=[]; public byte[]? BiometricData{get;set;} public DateTime CapturedAt{get;set;} }
public sealed class PeripheralConfigurationEntity { public long Id{get;set;} public long StoreId{get;set;} public string WorkstationId{get;set;}="SERVER"; public string Key{get;set;}=""; public string Provider{get;set;}=""; public string ConnectionType{get;set;}=""; public string? Endpoint{get;set;} public int? Port{get;set;} public string? Mode{get;set;} public string? DeviceId{get;set;} public string? ProtectedSecret{get;set;} public bool Enabled{get;set;}=true; public string LastStatus{get;set;}="not_tested"; public string? LastMessage{get;set;} public DateTime? LastTestedAt{get;set;} public DateTime UpdatedAt{get;set;}=DateTime.Now; }
public sealed class RecipeEntity{public long Id{get;set;}public long StoreId{get;set;}public long ProductId{get;set;}public ProductEntity Product{get;set;}=null!;public decimal OutputQuantity{get;set;}=1;public bool Active{get;set;}=true;public List<RecipeLineEntity> Components{get;set;}=[];}
public sealed class RecipeLineEntity{public long Id{get;set;}public long RecipeId{get;set;}public long ProductId{get;set;}public ProductEntity Product{get;set;}=null!;public decimal Quantity{get;set;}}
public sealed class ProductionOrderEntity{public long Id{get;set;}public long StoreId{get;set;}public long RecipeId{get;set;}public long ProductId{get;set;}public long UserId{get;set;}public string Folio{get;set;}="";public decimal Batches{get;set;}public decimal QuantityProduced{get;set;}public string Status{get;set;}="completed";public DateTime CreatedAt{get;set;}=DateTime.Now;}

public static class AtlasDatabaseSeeder
{
    public static Task ApplyMigrationsAsync(AtlasDbContext db, CancellationToken ct = default)
        => db.Database.MigrateAsync(ct);

    public static async Task SeedDemoAsync(AtlasDbContext db, CancellationToken ct = default)
    {
        if(await db.Stores.AnyAsync(ct)){await EnsureDemoCustomersAsync(db, ct);return;}
        db.Stores.Add(new(){Id=1,Name="Sucursal Centro"}); db.Users.Add(new(){Id=1,StoreId=1,Name="Administrador",Email="admin@atlas.local",PasswordHash="CONFIGURAR_IDENTITY",Role="Administrator"});
        var names=new[]{"Abarrotes","Lácteos","Frutas y verduras","Panadería","Bebidas","Limpieza"}; for(var i=0;i<names.Length;i++)db.Categories.Add(new(){Id=i+1,StoreId=1,Name=names[i]});
        db.Products.AddRange(new ProductEntity{Id=1,StoreId=1,CategoryId=1,Sku="CAF-001",Barcode="7501001000011",Name="Café artesanal 500 g",Price=149,Cost=90,TaxRate=.16m,Stock=24,MinimumStock=5},new ProductEntity{Id=2,StoreId=1,CategoryId=2,Sku="LEC-001",Barcode="7501001000028",Name="Leche entera 1 L",Price=29.5m,Cost=21,Stock=48,MinimumStock=10},new ProductEntity{Id=3,StoreId=1,CategoryId=3,Sku="MAN-KG",Barcode="2000000001012",Name="Manzana Gala",Unit="kg",IsWeighted=true,Price=46.9m,Cost=28,Stock=18.75m,MinimumStock=5},new ProductEntity{Id=4,StoreId=1,CategoryId=4,Sku="PAN-001",Barcode="7501001000042",Name="Pan integral",Price=54,Cost=32,Stock=12,MinimumStock=4},new ProductEntity{Id=5,StoreId=1,CategoryId=5,Sku="REF-600",Barcode="7501001000059",Name="Refresco 600 ml",Price=22,Cost=14,TaxRate=.16m,Stock=6,MinimumStock=8});
        db.Customers.AddRange(new(){Id=1,StoreId=1,Name="Público general",Rfc="XAXX010101000"},new(){Id=2,StoreId=1,Name="Mariana López",Rfc="LOPM850312AB2",Email="mariana@correo.mx",Phone="55 1234 5678"}); db.Suppliers.Add(new(){Id=1,StoreId=1,Name="Proveedor General",Email="ventas@proveedor.local"}); db.CashShifts.Add(new(){Id=1,StoreId=1,UserId=1,OpenedAt=DateTime.Now,OpeningAmount=0,Status="open"}); await db.SaveChangesAsync(ct); await EnsureDemoCustomersAsync(db, ct);
    }

    private static async Task EnsureDemoCustomersAsync(AtlasDbContext db, CancellationToken ct)
    {
        var current=await db.Customers.CountAsync(x=>x.StoreId==1, ct); if(current>=100)return;
        var first=new[]{"Ana","Luis","María","Carlos","Sofía","Jorge","Diana","Miguel","Laura","Fernando"};
        var last=new[]{"García","Hernández","Martínez","López","González","Pérez","Ramírez","Sánchez","Flores","Torres"};
        for(var i=current;i<100;i++)
        {
            var n=i+1; var fiscal=n%3!=0; var name=$"{first[i%first.Length]} {last[(i/first.Length)%last.Length]} {n:000}";
            db.Customers.Add(new(){StoreId=1,Name=name,Rfc=fiscal?$"ATL{(800101+n):000000}A{n%10}B":null,LegalName=fiscal?name.ToUpperInvariant():null,FiscalRegime=fiscal?"612":null,FiscalZip=fiscal?$"{60000+n:00000}":null,CfdiUse=fiscal?"G03":null,Email=n%4==0?null:$"cliente{n:000}@atlas.demo",Phone=n%5==0?null:$"55 10{n:00} {n:0000}"});
        }
        await db.SaveChangesAsync(ct);
    }
}
