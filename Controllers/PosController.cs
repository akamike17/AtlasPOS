using Microsoft.AspNetCore.Mvc;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace PuntoDeVentaAtlas.Web.Controllers;

[Authorize]
public sealed class PosController(IMySqlPointOfSaleService pos, IDeviceCatalogService devices, CustomerDashboardPdfService pdf,PeripheralConfigurationService peripherals) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await pos.DashboardAsync(ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([FromBody] SaleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { return Ok(await pos.CheckoutAsync(request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062) { return Conflict(new { message = "Ya existe un registro con ese SKU, código o folio." }); }
    }

    [Authorize(Policy="ManageInventory"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct([FromBody] ProductUpsertRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        return Ok(await pos.SaveProductAsync(request, ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomer([FromBody] CustomerUpsertRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { return Ok(await pos.SaveCustomerAsync(request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet] public IActionResult Devices() => Ok(devices.Devices());
    [HttpGet] public async Task<IActionResult> ScaleReading(CancellationToken ct)=>Ok(await peripherals.ReadScaleAsync(ct));
    [HttpGet] public async Task<IActionResult> CustomerDashboard(CancellationToken ct) => Ok(await pos.CustomerDashboardAsync(ct));
    [HttpGet] public async Task<IActionResult> CustomerDashboardPdf(CancellationToken ct) => File(pdf.Create(await pos.CustomerDashboardAsync(ct)), "application/pdf", $"atlas-clientes-{DateTime.Today:yyyy-MM-dd}.pdf");
    [HttpGet] public async Task<IActionResult> Suppliers(CancellationToken ct) => Ok(await pos.SuppliersAsync(ct));
    [HttpGet] public async Task<IActionResult> Purchases(CancellationToken ct) => Ok(await pos.PurchasesAsync(ct));
    [HttpGet] public async Task<IActionResult> CurrentShift(CancellationToken ct)
    {
        try { return Ok(await pos.CurrentShiftAsync(ct)); }
        catch (InvalidOperationException)
        {
            return Ok(new CashShiftSummary(0, DateTime.Today, null, 0, 0, 0, 0, 0, "closed"));
        }
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request,CancellationToken ct){try{return Ok(await pos.OpenShiftAsync(request.OpeningAmount,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> CashMovement([FromBody] CashMovementRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await pos.AddCashMovementAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [Authorize(Policy="CloseCash"), HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> CloseShift([FromBody] decimal countedAmount, CancellationToken ct) => Ok(await pos.CloseShiftAsync(countedAmount, ct));
    [Authorize(Policy="Audit"), HttpGet] public async Task<IActionResult> Audit(CancellationToken ct) => Ok(await pos.AuditAsync(ct));
    [Authorize(Policy="ManageInventory"), HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ReceivePurchase([FromBody] PurchaseRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await pos.ReceivePurchaseAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpGet] public async Task<IActionResult> ReturnableSale(string folio,CancellationToken ct){try{return Ok(await pos.ReturnableSaleAsync(folio,ct));}catch(InvalidOperationException ex){return NotFound(new{message=ex.Message});}}
    [Authorize(Policy="ManageInventory"),HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AdjustInventory([FromBody] InventoryAdjustmentRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await pos.AdjustInventoryAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [Authorize(Roles="Administrator,Manager"),HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ReturnSale([FromBody] ReturnRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await pos.ReturnSaleAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
}
