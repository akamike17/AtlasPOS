using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
using Xunit;

namespace PuntoDeVentaAtlas.Web.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MySqlIntegrationCollection : ICollectionFixture<MySqlFixture>
{
    public const string Name = "MySQL integration";
}

[Collection(MySqlIntegrationCollection.Name)]
public sealed class MySqlIntegrationTests(MySqlFixture fixture)
{
    [Fact]
    public async Task MigrationsAndStoreScopedDataAreAvailable()
    {
        fixture.RequireEnabled();
        await using var db = fixture.CreateContext();
        Assert.True(await db.Database.CanConnectAsync());
        Assert.Equal(1, await db.Stores.CountAsync(x => x.Id == 1));
        Assert.Equal(1, await db.Products.CountAsync(x => x.StoreId == 1 && x.Sku == "TEST-IDEM"));
        Assert.Equal(1, await db.Products.CountAsync(x => x.StoreId == 2 && x.Sku == "TEST-OTHER"));
    }

    [Fact]
    public async Task UpgradeFromPreviousMigrationPreservesProductData()
    {
        fixture.RequireEnabled();
        Assert.True(await fixture.UpgradePreservesDataAsync());
    }

    [Fact]
    public async Task BackupRestoresIntoAnEmptyDatabase()
    {
        fixture.RequireEnabled();
        await using var source = fixture.CreateContext();
        var backup = await new OperationsService(source).CreateBackupAsync(default);
        Assert.NotEmpty(backup);
        Assert.True(await fixture.RestoreBackupAsync(backup));
    }

    [Fact]
    public async Task CheckoutRetryWithSameClientOperationIdCreatesOneSale()
    {
        fixture.RequireEnabled();
        await using var db = fixture.CreateContext();
        var before = await db.Sales.CountAsync(x => x.ClientOperationId == "mysql-idempotency-001");
        var service = fixture.CreateService(db, 1, "terminal-idem");
        var request = fixture.Sale("mysql-idempotency-001", fixture.ProductId("TEST-IDEM"), 1, 20);

        var first = await service.CheckoutAsync(request, default);
        var retry = await service.CheckoutAsync(request, default);

        Assert.Equal(first.Folio, retry.Folio);
        Assert.Equal(before + 1, await db.Sales.CountAsync(x => x.ClientOperationId == request.ClientOperationId));
        Assert.Equal(1, await db.InventoryMovements.CountAsync(x => x.Note == first.Folio && x.Kind == "sale"));
        Assert.Equal(1m, await db.Products.Where(x => x.Id == fixture.ProductId("TEST-IDEM")).Select(x => x.Stock).SingleAsync());
    }

    [Fact]
    public async Task ConcurrentCheckoutForLastUnitAllowsOnlyOneSale()
    {
        fixture.RequireEnabled();
        var productId = fixture.ProductId("TEST-RACE");
        var requests = new[] { fixture.Sale("mysql-race-a", productId, 1, 20), fixture.Sale("mysql-race-b", productId, 1, 20) };
        var results = await Task.WhenAll(
            fixture.RunCheckoutAsync(requests[0], "terminal-race-a"),
            fixture.RunCheckoutAsync(requests[1], "terminal-race-b"));

        Assert.Equal(1, results.Count(x => x.Success));
        await using var db = fixture.CreateContext();
        Assert.Equal(0m, await db.Products.Where(x => x.Id == productId).Select(x => x.Stock).SingleAsync());
        Assert.Equal(1, await db.Sales.CountAsync(x => x.ClientOperationId.StartsWith("mysql-race-")));
    }

    [Fact]
    public async Task CheckoutRejectsProductFromAnotherStore()
    {
        fixture.RequireEnabled();
        await using var db = fixture.CreateContext();
        var service = fixture.CreateService(db, 1, "terminal-isolation");
        var request = fixture.Sale("mysql-isolation-001", fixture.ProductId("TEST-OTHER"), 1, 20);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(request, default));
        Assert.Equal(0, await db.Sales.CountAsync(x => x.ClientOperationId == request.ClientOperationId));
    }

    [Fact]
    public async Task PurchaseRejectsCrossStoreLineWithoutPartialMutation()
    {
        fixture.RequireEnabled();
        var localProductId = fixture.ProductId("TEST-PURCHASE");
        await using var beforeDb = fixture.CreateContext();
        var beforeStock = await beforeDb.Products.Where(x => x.Id == localProductId).Select(x => x.Stock).SingleAsync();
        var request = new PurchaseRequest
        {
            SupplierId = 1,
            Reference = "mysql-purchase-rollback",
            Lines = [new() { ProductId = localProductId, Quantity = 2, UnitCost = 4 }, new() { ProductId = fixture.ProductId("TEST-OTHER"), Quantity = 1, UnitCost = 4 }]
        };

        await using var db = fixture.CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.CreateService(db, 1, "terminal-purchase").ReceivePurchaseAsync(request, default));

        await using var afterDb = fixture.CreateContext();
        Assert.Equal(beforeStock, await afterDb.Products.Where(x => x.Id == localProductId).Select(x => x.Stock).SingleAsync());
        Assert.False(await afterDb.Purchases.AnyAsync(x => x.Folio == request.Reference));
        Assert.False(await afterDb.InventoryMovements.AnyAsync(x => x.Note == request.Reference));
    }

    [Fact]
    public async Task ConcurrentOpenShiftAllowsOnlyOneOpenShiftPerTerminal()
    {
        fixture.RequireEnabled();
        var results = await Task.WhenAll(
            fixture.RunOpenShiftAsync("terminal-open-race"),
            fixture.RunOpenShiftAsync("terminal-open-race"));

        Assert.Equal(1, results.Count(x => x.Success));
        await using var db = fixture.CreateContext();
        Assert.Equal(1, await db.CashShifts.CountAsync(x => x.StoreId == 1 && x.UserId == 1 && x.WorkstationId == "terminal-open-race" && x.Status == "open"));
    }

    [Fact]
    public async Task ConcurrentReturnsNeverExceedSoldQuantity()
    {
        fixture.RequireEnabled();
        var productId = await fixture.AddProductAsync($"TEST-PARTIAL-{Guid.NewGuid():N}", 3);
        var sale = await fixture.RunCheckoutAsync(fixture.Sale("mysql-return-sale", productId, 1, 20), "terminal-return-sale");
        Assert.True(sale.Success, sale.Error);
        var saleLineId = sale.LineId;
        var requests = new[]
        {
            new ReturnRequest { SaleFolio = sale.Folio!, Reason = "Prueba concurrente A", Lines = [new() { SaleLineId = saleLineId, Quantity = 1, Restock = false }] },
            new ReturnRequest { SaleFolio = sale.Folio!, Reason = "Prueba concurrente B", Lines = [new() { SaleLineId = saleLineId, Quantity = 1, Restock = false }] }
        };
        var results = await Task.WhenAll(
            fixture.RunReturnAsync(requests[0], "terminal-return-a"),
            fixture.RunReturnAsync(requests[1], "terminal-return-b"));

        Assert.Equal(1, results.Count(x => x.Success));
        await using var db = fixture.CreateContext();
        Assert.Equal(1m, await db.SaleReturnLines.Where(x => x.SaleLineId == saleLineId).SumAsync(x => x.Quantity));
    }

    [Fact]
    public async Task InvalidCheckoutAndPurchaseInputsDoNotMutateData()
    {
        fixture.RequireEnabled();
        var productId = fixture.ProductId("TEST-IDEM");
        await using var before = fixture.CreateContext();
        var stock = await before.Products.Where(x => x.Id == productId).Select(x => x.Stock).SingleAsync();
        var sales = await before.Sales.CountAsync(x => x.ClientOperationId == "mysql-invalid-sale");

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.CreateService(before, 1, "terminal-idem")
            .CheckoutAsync(fixture.Sale("mysql-invalid-sale", productId, 1, 0), default));

        await using var afterCheckout = fixture.CreateContext();
        Assert.Equal(stock, await afterCheckout.Products.Where(x => x.Id == productId).Select(x => x.Stock).SingleAsync());
        Assert.Equal(sales, await afterCheckout.Sales.CountAsync(x => x.ClientOperationId == "mysql-invalid-sale"));

        var request = new PurchaseRequest
        {
            SupplierId = 1,
            Reference = "mysql-invalid-purchase",
            Lines = [new() { ProductId = productId, Quantity = 0, UnitCost = 4 }]
        };
        await using var purchaseDb = fixture.CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.CreateService(purchaseDb, 1, "terminal-purchase")
            .ReceivePurchaseAsync(request, default));

        await using var afterPurchase = fixture.CreateContext();
        Assert.Equal(stock, await afterPurchase.Products.Where(x => x.Id == productId).Select(x => x.Stock).SingleAsync());
        Assert.False(await afterPurchase.Purchases.AnyAsync(x => x.Folio == request.Reference));
        Assert.False(await afterPurchase.InventoryMovements.AnyAsync(x => x.Note == request.Reference));
    }

    [Fact]
    public async Task PartialReturnsRespectRemainingQuantityAndRestock()
    {
        fixture.RequireEnabled();
        var productId = fixture.ProductId("TEST-RETURN");
        var sale = await fixture.RunCheckoutAsync(fixture.Sale("mysql-partial-return-sale", productId, 1, 20), "terminal-return-sale");
        Assert.True(sale.Success, sale.Error);

        await using var returnDb = fixture.CreateContext();
        var service = fixture.CreateService(returnDb, 1, "terminal-return-a");
        var first = await service.ReturnSaleAsync(new ReturnRequest
        {
            SaleFolio = sale.Folio!, Reason = "Devolución parcial", Lines = [new() { SaleLineId = sale.LineId, Quantity = .5m, Restock = true }]
        }, default);
        Assert.Equal("partially_returned", first.SaleStatus);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReturnSaleAsync(new ReturnRequest
        {
            SaleFolio = sale.Folio!, Reason = "Exceso", Lines = [new() { SaleLineId = sale.LineId, Quantity = .6m, Restock = true }]
        }, default));

        var remaining = await service.ReturnSaleAsync(new ReturnRequest
        {
            SaleFolio = sale.Folio!, Reason = "Devolución restante", Lines = [new() { SaleLineId = sale.LineId, Quantity = .5m, Restock = false }]
        }, default);
        Assert.Equal("returned", remaining.SaleStatus);

        await using var db = fixture.CreateContext();
        Assert.Equal(1m, await db.SaleReturnLines.Where(x => x.SaleLineId == sale.LineId).SumAsync(x => x.Quantity));
        Assert.Equal(1, await db.InventoryMovements.CountAsync(x => x.Note == first.Folio && x.Kind == "return"));
        Assert.Contains(await db.AuditLog.Where(x => x.EntityId == sale.Folio).Select(x => x.Action).ToListAsync(), x => x == "sale.returned");
    }

    [Fact]
    public async Task CustomerInventoryAndManufacturingFlowsRemainStoreScopedAndAtomic()
    {
        fixture.RequireEnabled();
        await using var db = fixture.CreateContext();
        var pos = fixture.CreateService(db, 1, "terminal-idem");
        var customer = await pos.SaveCustomerAsync(new CustomerUpsertRequest
        {
            Name = "Cliente de laboratorio", Rfc = "LABA800101AB1", Email = "LAB@EXAMPLE.MX", Phone = "5512345678",
            LegalName = "CLIENTE DE LABORATORIO", FiscalRegime = "612", FiscalZip = "06000", CfdiUse = "G03"
        }, default);
        Assert.Equal("lab@example.mx", customer.Email);

        var raw = new ProductEntity { StoreId = 1, Sku = "RAW-LAB", Name = "Insumo laboratorio", Stock = 5, Active = true };
        var finished = new ProductEntity { StoreId = 1, Sku = "FIN-LAB", Name = "Producto terminado", Stock = 0, Active = true };
        db.Products.AddRange(raw, finished);
        await db.SaveChangesAsync();

        var manufacturing = fixture.CreateManufacturingService(db, 1);
        var recipe = await manufacturing.SaveRecipeAsync(new SaveRecipeRequest
        {
            ProductId = (int)finished.Id, OutputQuantity = 1,
            Components = [new() { ProductId = (int)raw.Id, Quantity = 2 }]
        }, default);
        var produced = await manufacturing.ProduceAsync(new ProduceRequest { RecipeId = recipe.Id, Batches = 2 }, default);
        Assert.Equal(2m, produced.Produced);

        await using var after = fixture.CreateContext();
        Assert.Equal(1m, await after.Products.Where(x => x.Id == raw.Id).Select(x => x.Stock).SingleAsync());
        Assert.Equal(2m, await after.Products.Where(x => x.Id == finished.Id).Select(x => x.Stock).SingleAsync());
        Assert.Equal(2, await after.InventoryMovements.CountAsync(x => x.Note == produced.Folio));
        var ordersBeforeFailure = await after.ProductionOrders.CountAsync(x => x.RecipeId == recipe.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manufacturing.ProduceAsync(new ProduceRequest { RecipeId = recipe.Id, Batches = 2 }, default));
        await using var final = fixture.CreateContext();
        Assert.Equal(ordersBeforeFailure, await final.ProductionOrders.CountAsync(x => x.RecipeId == recipe.Id));
        Assert.Equal(1m, await final.Products.Where(x => x.Id == raw.Id).Select(x => x.Stock).SingleAsync());
        Assert.Equal(2m, await final.Products.Where(x => x.Id == finished.Id).Select(x => x.Stock).SingleAsync());
    }
}

public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly string? adminConnection = Environment.GetEnvironmentVariable("ATLAS_MYSQL_ADMIN_CONNECTION");
    private readonly bool keepDatabase = string.Equals(Environment.GetEnvironmentVariable("ATLAS_MYSQL_PERSIST_DATABASE"), "true", StringComparison.OrdinalIgnoreCase);
    public string? DatabaseName { get; private set; }
    private string? connectionString;

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(adminConnection)) return;
        var admin = new MySqlConnectionStringBuilder(adminConnection!) { Database = "" };
        DatabaseName = $"atlas_pos_test_{Guid.NewGuid():N}";
        await using var connection = new MySqlConnection(admin.ConnectionString);
        await connection.OpenAsync();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE `{DatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
            await command.ExecuteNonQueryAsync();
        }
        var test = new MySqlConnectionStringBuilder(admin.ConnectionString) { Database = DatabaseName };
        connectionString = test.ConnectionString;
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await SeedAsync(db);
        if (keepDatabase) Console.WriteLine($"ATLAS_TEST_DATABASE={DatabaseName}");
    }

    public async Task DisposeAsync()
    {
        if (keepDatabase || string.IsNullOrWhiteSpace(DatabaseName) || string.IsNullOrWhiteSpace(adminConnection)) return;
        var admin = new MySqlConnectionStringBuilder(adminConnection!) { Database = "" };
        await using var connection = new MySqlConnection(admin.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS `{DatabaseName}`";
        await command.ExecuteNonQueryAsync();
    }

    public void RequireEnabled()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Define ATLAS_MYSQL_ADMIN_CONNECTION para ejecutar Integration.MySql.");
    }

    public AtlasDbContext CreateContext()
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("MySQL fixture is disabled.");
        var options = new DbContextOptionsBuilder<AtlasDbContext>().UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).Options;
        return new AtlasDbContext(options);
    }

    public EfPointOfSaleService CreateService(AtlasDbContext db, long storeId, string terminalId)
        => new(db, new TestUserContext(storeId), new TestTerminalContext(terminalId));

    public ManufacturingService CreateManufacturingService(AtlasDbContext db, long storeId)
        => new(db, new TestUserContext(storeId));

    public async Task<CheckoutAttempt> RunCheckoutAsync(SaleRequest request, string terminalId)
    {
        await using var db = CreateContext();
        try
        {
            var result = await CreateService(db, 1, terminalId).CheckoutAsync(request, default);
            var saleId = await db.Sales.Where(s => s.Folio == result.Folio).Select(s => s.Id).SingleAsync();
            var lineId = await db.SaleLines.Where(x => x.SaleId == saleId).Select(x => x.Id).FirstAsync();
            return new(true, result.Folio, lineId, null);
        }
        catch (Exception ex)
        {
            return new(false, null, 0, ex.Message);
        }
    }

    public async Task<ReturnAttempt> RunReturnAsync(ReturnRequest request, string terminalId)
    {
        await using var db = CreateContext();
        try { var result = await CreateService(db, 1, terminalId).ReturnSaleAsync(request, default); return new(true, result.Folio, null); }
        catch (Exception ex) { return new(false, null, ex.Message); }
    }

    public async Task<OpenShiftAttempt> RunOpenShiftAsync(string terminalId)
    {
        await using var db = CreateContext();
        try { await CreateService(db, 1, terminalId).OpenShiftAsync(0, default); return new(true, null); }
        catch (Exception ex) { return new(false, ex.Message); }
    }

    public int ProductId(string sku)
    {
        using var db = CreateContext();
        return (int)db.Products.Single(x => x.Sku == sku).Id;
    }

    public async Task<int> AddProductAsync(string sku, decimal stock)
    {
        await using var db = CreateContext();
        var product = new ProductEntity { StoreId = 1, Sku = sku, Name = sku, Price = 10, Stock = stock, Active = true };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return (int)product.Id;
    }

    public SaleRequest Sale(string operationId, int productId, decimal quantity, decimal payment)
        => new() { ClientOperationId = operationId, Lines = [new() { ProductId = productId, Quantity = quantity }], Payments = [new() { Method = "cash", Amount = payment }] };

    public async Task<bool> UpgradePreservesDataAsync()
    {
        var admin = new MySqlConnectionStringBuilder(adminConnection!) { Database = "" };
        var name = $"atlas_pos_upgrade_{Guid.NewGuid():N}";
        await using var adminDb = new MySqlConnection(admin.ConnectionString);
        await adminDb.OpenAsync();
        await using (var create = adminDb.CreateCommand())
        {
            create.CommandText = $"CREATE DATABASE `{name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
            await create.ExecuteNonQueryAsync();
        }
        try
        {
            var target = new MySqlConnectionStringBuilder(admin.ConnectionString) { Database = name };
            await using var db = new AtlasDbContext(new DbContextOptionsBuilder<AtlasDbContext>().UseMySql(target.ConnectionString, ServerVersion.AutoDetect(target.ConnectionString)).Options);
            await db.GetService<IMigrator>().MigrateAsync("20260813201012_SnapshotSyncMultiTerminal");
            db.Stores.Add(new StoreEntity { Id = 1, Name = "Upgrade Store" });
            db.Products.Add(new ProductEntity { Id = 1, StoreId = 1, Sku = "UPGRADE-001", Name = "Preserved", Price = 12.50m, Stock = 7 });
            await db.SaveChangesAsync();
            await db.Database.MigrateAsync();
            return await db.Products.AnyAsync(x => x.Sku == "UPGRADE-001" && x.Stock == 7);
        }
        finally
        {
            await using var drop = adminDb.CreateCommand();
            drop.CommandText = $"DROP DATABASE IF EXISTS `{name}`";
            await drop.ExecuteNonQueryAsync();
        }
    }

    public async Task<bool> RestoreBackupAsync(byte[] backup)
    {
        var admin = new MySqlConnectionStringBuilder(adminConnection!) { Database = "" };
        var name = $"atlas_pos_restore_{Guid.NewGuid():N}";
        await using var adminDb = new MySqlConnection(admin.ConnectionString);
        await adminDb.OpenAsync();
        await using (var create = adminDb.CreateCommand())
        {
            create.CommandText = $"CREATE DATABASE `{name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
            await create.ExecuteNonQueryAsync();
        }
        try
        {
            var target = new MySqlConnectionStringBuilder(admin.ConnectionString) { Database = name };
            await using var db = new AtlasDbContext(new DbContextOptionsBuilder<AtlasDbContext>().UseMySql(target.ConnectionString, ServerVersion.AutoDetect(target.ConnectionString)).Options);
            await db.Database.MigrateAsync();
            await new OperationsService(db).RestoreIntoEmptyDatabaseAsync(new MemoryStream(backup), default);
            return await db.Stores.AnyAsync(x => x.Id == 1) && await db.Products.AnyAsync(x => x.Sku == "TEST-IDEM");
        }
        finally
        {
            await using var drop = adminDb.CreateCommand();
            drop.CommandText = $"DROP DATABASE IF EXISTS `{name}`";
            await drop.ExecuteNonQueryAsync();
        }
    }

    private static async Task SeedAsync(AtlasDbContext db)
    {
        db.Stores.AddRange(new StoreEntity { Id = 1, Name = "Test Store A" }, new StoreEntity { Id = 2, Name = "Test Store B" });
        var testAdmin = new UserEntity { Id = 1, StoreId = 1, Name = "Test Admin", Email = "admin@atlas.local", PasswordHash = "hash", Role = "Administrator" };
        var appPassword = Environment.GetEnvironmentVariable("ATLAS_TEST_APP_PASSWORD");
        if (!string.IsNullOrWhiteSpace(appPassword)) testAdmin.PasswordHash = new PasswordHasher<UserEntity>().HashPassword(testAdmin, appPassword);
        db.Users.AddRange(testAdmin, new UserEntity { Id = 2, StoreId = 2, Name = "Test Other", Email = "test-b@local", PasswordHash = "hash", Role = "Administrator" });
        db.Customers.Add(new CustomerEntity { Id = 1, StoreId = 1, Name = "Público A" });
        db.Suppliers.Add(new SupplierEntity { Id = 1, StoreId = 1, Name = "Proveedor A" });
        db.Products.AddRange(
            new ProductEntity { StoreId = 1, Sku = "TEST-IDEM", Name = "Idempotent", Price = 10, Stock = 2, Active = true },
            new ProductEntity { StoreId = 1, Sku = "TEST-RACE", Name = "Race", Price = 10, Stock = 1, Active = true },
            new ProductEntity { StoreId = 1, Sku = "TEST-RETURN", Name = "Return", Price = 10, Stock = 1, Active = true },
            new ProductEntity { StoreId = 1, Sku = "TEST-PURCHASE", Name = "Purchase", Price = 10, Stock = 3, Active = true },
            new ProductEntity { StoreId = 2, Sku = "TEST-OTHER", Name = "Other Store", Price = 10, Stock = 5, Active = true });
        db.CashShifts.AddRange(
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-idem", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-race-a", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-race-b", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-return-sale", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-return-a", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-return-b", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "terminal-isolation", OpenedAt = DateTime.Now, Status = "open" },
            new CashShiftEntity { StoreId = 1, UserId = 1, WorkstationId = "SERVER", OpenedAt = DateTime.Now, Status = "open" });
        await db.SaveChangesAsync();
    }

    public sealed record CheckoutAttempt(bool Success, string? Folio, long LineId, string? Error);
    public sealed record ReturnAttempt(bool Success, string? Folio, string? Error);
    public sealed record OpenShiftAttempt(bool Success, string? Error);

    private sealed class TestUserContext(long storeId) : ICurrentUserContext
    {
        public long UserId => 1;
        public long StoreId => storeId;
        public string Name => "Test Admin";
        public string Role => "Administrator";
    }

    private sealed class TestTerminalContext(string terminalId) : ICurrentTerminalContext
    {
        public string TerminalId => terminalId;
    }
}
