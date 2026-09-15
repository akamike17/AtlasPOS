using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;

namespace PuntoDeVentaAtlas.Web.Services;
public sealed class OperationsService(AtlasDbContext db)
{
    public async Task<byte[]> CreateBackupAsync(CancellationToken ct)
    {
        var snapshot=new Dictionary<string,object>{
            ["metadata"]=new{format="atlas-pos-backup-v1",createdAt=DateTimeOffset.Now,database="mysql"},
            ["stores"]=await db.Stores.AsNoTracking().ToListAsync(ct),["users"]=await db.Users.AsNoTracking().Select(x=>new{x.Id,x.StoreId,x.Name,x.Email,x.Role,x.Active}).ToListAsync(ct),
            ["categories"]=await db.Categories.AsNoTracking().ToListAsync(ct),["products"]=await db.Products.AsNoTracking().ToListAsync(ct),["customers"]=await db.Customers.AsNoTracking().ToListAsync(ct),
            ["cashShifts"]=await db.CashShifts.AsNoTracking().ToListAsync(ct),["sales"]=await db.Sales.AsNoTracking().ToListAsync(ct),["saleLines"]=await db.SaleLines.AsNoTracking().ToListAsync(ct),
            ["payments"]=await db.Payments.AsNoTracking().ToListAsync(ct),["movements"]=await db.InventoryMovements.AsNoTracking().ToListAsync(ct),["suppliers"]=await db.Suppliers.AsNoTracking().ToListAsync(ct),
            ["purchases"]=await db.Purchases.AsNoTracking().ToListAsync(ct),["purchaseLines"]=await db.PurchaseLines.AsNoTracking().ToListAsync(ct),["returns"]=await db.SaleReturns.AsNoTracking().ToListAsync(ct),["returnLines"]=await db.SaleReturnLines.AsNoTracking().ToListAsync(ct),["cashMovements"]=await db.CashMovements.AsNoTracking().ToListAsync(ct),["audit"]=await db.AuditLog.AsNoTracking().ToListAsync(ct)
        };
        await using var output=new MemoryStream();await using(var gzip=new GZipStream(output,CompressionLevel.Optimal,true))await JsonSerializer.SerializeAsync(gzip,snapshot,cancellationToken:ct);return output.ToArray();
    }
}
