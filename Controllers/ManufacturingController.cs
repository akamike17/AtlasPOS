using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using PuntoDeVentaAtlas.Web.Models;using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize(Policy="ManageInventory")]public sealed class ManufacturingController(ManufacturingService service):Controller
{
 [HttpGet]public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.ListAsync(ct));
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> SaveRecipe([FromBody]SaveRecipeRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await service.SaveRecipeAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Produce([FromBody]ProduceRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await service.ProduceAsync(request,ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
}
