using System.IO.Compression;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;

namespace PuntoDeVentaAtlas.Web.Services;
public sealed class OperationsService(AtlasDbContext db)
{
    public async Task<byte[]> CreateBackupAsync(CancellationToken ct)
    {
        var snapshot=new BackupDocument
        {
            Metadata=new BackupMetadata("atlas-pos-backup-v1",DateTimeOffset.UtcNow,"mysql"),
            Stores=await db.Stores.AsNoTracking().ToListAsync(ct), Users=await db.Users.AsNoTracking().ToListAsync(ct),
            Categories=await db.Categories.AsNoTracking().ToListAsync(ct), Products=await db.Products.AsNoTracking().ToListAsync(ct), Customers=await db.Customers.AsNoTracking().ToListAsync(ct),
            CashShifts=await db.CashShifts.AsNoTracking().ToListAsync(ct), Sales=await db.Sales.AsNoTracking().ToListAsync(ct), SaleLines=await db.SaleLines.AsNoTracking().ToListAsync(ct),
            Payments=await db.Payments.AsNoTracking().ToListAsync(ct), Movements=await db.InventoryMovements.AsNoTracking().ToListAsync(ct), Suppliers=await db.Suppliers.AsNoTracking().ToListAsync(ct),
            Purchases=await db.Purchases.AsNoTracking().ToListAsync(ct), PurchaseLines=await db.PurchaseLines.AsNoTracking().ToListAsync(ct), Returns=await db.SaleReturns.AsNoTracking().ToListAsync(ct), ReturnLines=await db.SaleReturnLines.AsNoTracking().ToListAsync(ct), CashMovements=await db.CashMovements.AsNoTracking().ToListAsync(ct), Audit=await db.AuditLog.AsNoTracking().ToListAsync(ct),
            Workstations=await db.Workstations.AsNoTracking().ToListAsync(ct), PeripheralConfigurations=await db.PeripheralConfigurations.AsNoTracking().ToListAsync(ct), Recipes=await db.Recipes.AsNoTracking().ToListAsync(ct), RecipeLines=await db.RecipeLines.AsNoTracking().ToListAsync(ct), ProductionOrders=await db.ProductionOrders.AsNoTracking().ToListAsync(ct), Invoices=await db.Invoices.AsNoTracking().ToListAsync(ct), Signatures=await db.Signatures.AsNoTracking().ToListAsync(ct)
        };
        await using var output=new MemoryStream();await using(var gzip=new GZipStream(output,CompressionLevel.Optimal,true))await JsonSerializer.SerializeAsync(gzip,snapshot,cancellationToken:ct);return output.ToArray();
    }

    public async Task RestoreIntoEmptyDatabaseAsync(Stream compressedBackup,CancellationToken ct)
    {
        if (compressedBackup is null || !compressedBackup.CanRead) throw new ArgumentException("El respaldo no es legible.", nameof(compressedBackup));
        await using var gzip=new GZipStream(compressedBackup,CompressionMode.Decompress,true);
        var snapshot=await JsonSerializer.DeserializeAsync<BackupDocument>(gzip,cancellationToken:ct)??throw new InvalidOperationException("El respaldo está vacío o corrupto.");
        if(snapshot.Metadata.Format!="atlas-pos-backup-v1")throw new InvalidOperationException("Formato de respaldo no compatible.");
        if(await db.Stores.AnyAsync(ct)||await db.Users.AnyAsync(ct)||await db.Products.AnyAsync(ct)||await db.Sales.AnyAsync(ct))throw new InvalidOperationException("Restore sólo admite una base de destino vacía.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        db.Stores.AddRange(snapshot.Stores);db.Users.AddRange(snapshot.Users);db.Categories.AddRange(snapshot.Categories);db.Products.AddRange(snapshot.Products);db.Customers.AddRange(snapshot.Customers);db.Suppliers.AddRange(snapshot.Suppliers);db.Workstations.AddRange(snapshot.Workstations);await db.SaveChangesAsync(ct);
        db.CashShifts.AddRange(snapshot.CashShifts);db.Sales.AddRange(snapshot.Sales);db.Purchases.AddRange(snapshot.Purchases);db.SaleReturns.AddRange(snapshot.Returns);db.PeripheralConfigurations.AddRange(snapshot.PeripheralConfigurations);db.Recipes.AddRange(snapshot.Recipes);db.ProductionOrders.AddRange(snapshot.ProductionOrders);db.Invoices.AddRange(snapshot.Invoices);db.Signatures.AddRange(snapshot.Signatures);await db.SaveChangesAsync(ct);
        db.SaleLines.AddRange(snapshot.SaleLines);db.Payments.AddRange(snapshot.Payments);db.PurchaseLines.AddRange(snapshot.PurchaseLines);db.SaleReturnLines.AddRange(snapshot.ReturnLines);db.CashMovements.AddRange(snapshot.CashMovements);db.InventoryMovements.AddRange(snapshot.Movements);db.RecipeLines.AddRange(snapshot.RecipeLines);db.AuditLog.AddRange(snapshot.Audit);await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private sealed record BackupMetadata(string Format,DateTimeOffset CreatedAt,string Database);
    private sealed class BackupDocument
    {
        public BackupMetadata Metadata { get; set; }=new("",default,""); public List<StoreEntity> Stores{get;set;}=[]; public List<UserEntity> Users{get;set;}=[]; public List<CategoryEntity> Categories{get;set;}=[]; public List<ProductEntity> Products{get;set;}=[]; public List<CustomerEntity> Customers{get;set;}=[]; public List<CashShiftEntity> CashShifts{get;set;}=[]; public List<SaleEntity> Sales{get;set;}=[]; public List<SaleLineEntity> SaleLines{get;set;}=[]; public List<PaymentEntity> Payments{get;set;}=[]; public List<InventoryMovementEntity> Movements{get;set;}=[]; public List<SupplierEntity> Suppliers{get;set;}=[]; public List<PurchaseEntity> Purchases{get;set;}=[]; public List<PurchaseLineEntity> PurchaseLines{get;set;}=[]; public List<SaleReturnEntity> Returns{get;set;}=[]; public List<SaleReturnLineEntity> ReturnLines{get;set;}=[]; public List<CashMovementEntity> CashMovements{get;set;}=[]; public List<AuditEntity> Audit{get;set;}=[]; public List<WorkstationEntity> Workstations{get;set;}=[]; public List<PeripheralConfigurationEntity> PeripheralConfigurations{get;set;}=[]; public List<RecipeEntity> Recipes{get;set;}=[]; public List<RecipeLineEntity> RecipeLines{get;set;}=[]; public List<ProductionOrderEntity> ProductionOrders{get;set;}=[]; public List<InvoiceEntity> Invoices{get;set;}=[]; public List<SignatureEntity> Signatures{get;set;}=[];
    }
}
