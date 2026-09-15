using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize]
public sealed class PeripheralsController(PeripheralConfigurationService service,HardwareDiscoveryService discovery):Controller
{
    [Authorize(Roles="Administrator"),HttpGet] public IActionResult Catalog()=>Ok(discovery.Catalog());
    [Authorize(Roles="Administrator"),HttpGet] public async Task<IActionResult> Discover(CancellationToken ct)=>Ok(await service.DiscoverAsync(ct));
    [Authorize(Roles="Administrator"),HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AutoDetectScale(CancellationToken ct)=>Ok(await service.AutoDetectScaleAsync(ct));
 [Authorize(Roles="Administrator"),HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.ListAsync(ct));
 [Authorize(Roles="Administrator"),HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Save([FromBody]SavePeripheralConfigurationRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);return Ok(await service.SaveAsync(request,ActorId(),ct));}
 [Authorize(Roles="Administrator"),HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Test([FromQuery]string key,CancellationToken ct){if(string.IsNullOrWhiteSpace(key))return BadRequest(new{message="Selecciona un dispositivo."});try{return Ok(await service.TestAsync(key,ActorId(),ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> PrintSale([FromQuery]string folio,CancellationToken ct){try{return Ok(new{message=await service.PrintSaleAsync(folio,ActorId(),ct)});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
 [Authorize(Roles="Administrator,Manager"),HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> OpenDrawer([FromBody]OpenDrawerRequest request,CancellationToken ct){try{return Ok(new{message=await service.OpenDrawerAsync(request.Reason,ActorId(),ct)});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
 private long ActorId()=>long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:1;
}
