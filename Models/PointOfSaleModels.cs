using System.ComponentModel.DataAnnotations;

namespace PuntoDeVentaAtlas.Web.Models;

public sealed record Product(int Id, string Sku, string Barcode, string Name, string Category,
    decimal Price, decimal Stock, string Unit, bool IsWeighted, decimal TaxRate, string Color);

public sealed record Customer(int Id, string Name, string Rfc, string Email, string Phone);

public sealed class SaleRequest
{
    [Required, StringLength(64)] public string ClientOperationId { get; set; } = "";
    public List<SaleLineRequest> Lines { get; set; } = [];
    public List<PaymentRequest> Payments { get; set; } = [];
    public int? CustomerId { get; set; }
    [Range(0, 100)] public decimal DiscountPercent { get; set; }
    public string? Notes { get; set; }
}

public sealed class SaleLineRequest
{
    public int ProductId { get; set; }
    [Range(0.001, 999999)] public decimal Quantity { get; set; }
}

public sealed class PaymentRequest
{
    public string Method { get; set; } = "cash";
    [Range(0.01, 999999999)] public decimal Amount { get; set; }
    public string? Reference { get; set; }
}

public sealed record SaleResult(string Folio, DateTime CreatedAt, decimal Subtotal, decimal Discount,
    decimal Tax, decimal Total, decimal Paid, decimal Change, IReadOnlyList<SaleTicketLine> Lines,
    IReadOnlyList<PaymentRequest> Payments, string Cashier);

public sealed record SaleTicketLine(string Name, decimal Quantity, string Unit, decimal UnitPrice, decimal Total);

public sealed record DashboardViewModel(IReadOnlyList<Product> Products, IReadOnlyList<Customer> Customers,
    IReadOnlyList<SaleResult> RecentSales, decimal TodaySales, int TodayTickets, decimal LowStockCount,
    CustomerDashboard CustomerDashboard);

public sealed record CustomerDashboard(int Total, int WithRfc, int WithEmail, int WithPhone,
    int FiscalReady, IReadOnlyList<CustomerSegment> Segments);
public sealed record CustomerSegment(string Name, int Count, decimal Percentage);

public sealed record DeviceStatus(string Key, string Name, string Kind, bool Connected, string Detail,
    string ProviderContract, string ConfigurationKey, bool RequiresProvider);
public sealed record PeripheralConfigurationView(string Key,string Provider,string ConnectionType,string Endpoint,int? Port,string Mode,string DeviceId,bool HasSecret,bool Enabled,string LastStatus,string? LastMessage,DateTime? LastTestedAt);
public sealed record HardwareProfile(string Key,string Manufacturer,string Family,string Kind,string Protocol,string[] Connections,string Integration,string Documentation,bool AutomaticDiscovery,string Notes);
public sealed record DiscoveredHardware(string Id,string Name,string Manufacturer,string Kind,string Connection,string? Port,string SuggestedProfile,string Status);
public sealed record ScaleReading(bool Configured,bool Connected,bool Stable,decimal Kilograms,string Status,string Message,DateTime ReadAt);
public sealed record WorkstationView(string TerminalId,string Name,bool Enabled,DateTime LastSeenAt);
public sealed class RegisterWorkstationRequest { [Required,StringLength(32,MinimumLength=32)] public string TerminalId{get;set;}=""; [Required,StringLength(120)] public string Name{get;set;}=""; }
public sealed class OpenDrawerRequest { [Required,StringLength(180)] public string Reason{get;set;}=""; }
public sealed class SavePeripheralConfigurationRequest
{
    [Required,RegularExpression("^(printer|scale|pinpad|signature|invoice|scanner)$")] public string Key{get;set;}="";
    [Required,StringLength(120)] public string Provider{get;set;}="";
    [Required,StringLength(40)] public string ConnectionType{get;set;}="";
    [StringLength(500)] public string? Endpoint{get;set;}
    [Range(1,65535)] public int? Port{get;set;}
    [StringLength(60)] public string? Mode{get;set;}
    [StringLength(160)] public string? DeviceId{get;set;}
    [StringLength(500)] public string? Secret{get;set;}
    public bool Enabled{get;set;}=true;
}
public sealed record RecipeSummary(long Id,int ProductId,string ProductName,decimal OutputQuantity,int Components);
public sealed class SaveRecipeRequest{[Range(1,int.MaxValue)]public int ProductId{get;set;}[Range(.001,999999)]public decimal OutputQuantity{get;set;}=1;public List<RecipeComponentRequest> Components{get;set;}=[];}
public sealed class RecipeComponentRequest{[Range(1,int.MaxValue)]public int ProductId{get;set;}[Range(.001,999999)]public decimal Quantity{get;set;}}
public sealed class ProduceRequest{[Range(1,long.MaxValue)]public long RecipeId{get;set;}[Range(.001,999999)]public decimal Batches{get;set;}=1;}
public sealed record ProductionResult(string Folio,string Product,decimal Produced,DateTime CreatedAt);

public sealed record Supplier(int Id, string Name, string Rfc, string Email, string Phone, bool Active);
public sealed record PurchaseSummary(long Id, string Folio, string Supplier, decimal Total, DateTime CreatedAt, string Status);
public sealed record CashShiftSummary(long Id, DateTime OpenedAt, DateTime? ClosedAt, decimal OpeningAmount,
    decimal CashSales, decimal Deposits, decimal Withdrawals, decimal ExpectedAmount, string Status, decimal? CountedAmount = null);
public sealed class OpenShiftRequest { [Range(0,999999999)] public decimal OpeningAmount{get;set;} }
public sealed class CashMovementRequest { [Required,RegularExpression("^(deposit|withdrawal)$")] public string Kind{get;set;}="deposit"; [Range(.01,999999999)] public decimal Amount{get;set;} [Required,StringLength(180)] public string Reason{get;set;}=""; }
public sealed record AuditEntry(long Id, string UserName, string Action, string Entity, string? EntityId, DateTime CreatedAt);
public sealed class PurchaseRequest { public int SupplierId{get;set;} public string? Reference{get;set;} public List<PurchaseLineRequest> Lines{get;set;}=[]; }
public sealed class PurchaseLineRequest { public int ProductId{get;set;} [Range(.001,999999999)] public decimal Quantity{get;set;} [Range(0,999999999)] public decimal UnitCost{get;set;} }
public sealed class ReturnRequest { [Required] public string SaleFolio{get;set;}=""; [Required,StringLength(255)] public string Reason{get;set;}=""; public List<ReturnLineRequest> Lines{get;set;}=[]; }
public sealed class ReturnLineRequest { [Range(1,long.MaxValue)] public long SaleLineId{get;set;} [Range(.001,999999999)] public decimal Quantity{get;set;} public bool Restock{get;set;}=true; }
public sealed record ReturnableSale(string Folio,string Status,decimal Total,IReadOnlyList<ReturnableSaleLine> Lines);
public sealed record ReturnableSaleLine(long SaleLineId,string Product,decimal SoldQuantity,decimal ReturnedQuantity,decimal AvailableQuantity,string Unit,decimal UnitRefund);
public sealed record ReturnResult(string Folio,decimal Total,string SaleStatus);
public sealed class InventoryAdjustmentRequest { [Range(1,int.MaxValue)] public int ProductId{get;set;} [Required,RegularExpression("^(in|out|waste|count)$")] public string Kind{get;set;}="count"; [Range(0,999999999)] public decimal Quantity{get;set;} [Required,StringLength(180)] public string Reason{get;set;}=""; }
public sealed record InventoryAdjustmentResult(int ProductId,decimal PreviousStock,decimal Stock,decimal Difference,string Kind);

public sealed class ProductUpsertRequest
{
    public int? Id { get; set; }
    [Required, StringLength(60)] public string Sku { get; set; } = "";
    [StringLength(80)] public string? Barcode { get; set; }
    [Required, StringLength(180)] public string Name { get; set; } = "";
    [Required, StringLength(120)] public string Category { get; set; } = "General";
    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal Cost { get; set; }
    [Range(0, 999999999)] public decimal Stock { get; set; }
    [Range(0, 1)] public decimal TaxRate { get; set; }
    public string Unit { get; set; } = "pza";
    public bool IsWeighted { get; set; }
}

public sealed class CustomerUpsertRequest
{
    public int? Id { get; set; }
    [Required, StringLength(180)] public string Name { get; set; } = "";
    [RegularExpression("^([A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}|XAXX010101000)?$", ErrorMessage="RFC no válido.")] public string? Rfc { get; set; }
    [EmailAddress] public string? Email { get; set; }
    [StringLength(30)] public string? Phone { get; set; }
    [StringLength(180)] public string? LegalName { get; set; }
    [RegularExpression("^([0-9]{3})?$", ErrorMessage="Régimen fiscal no válido.")] public string? FiscalRegime { get; set; }
    [RegularExpression("^([0-9]{5})?$", ErrorMessage="Código postal no válido.")] public string? FiscalZip { get; set; }
    [RegularExpression("^([A-Z0-9]{3,4})?$", ErrorMessage="Uso CFDI no válido.")] public string? CfdiUse { get; set; }
}

public sealed class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
