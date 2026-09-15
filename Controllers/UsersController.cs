using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize(Roles="Administrator")] public sealed class UsersController(UserManagementService users):Controller
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await users.ListAsync(ct));
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create([FromBody]CreateUserRequest request,CancellationToken ct){if(!ModelState.IsValid)return ValidationProblem(ModelState);try{return Ok(await users.CreateAsync(request,ActorId(),ct));}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> SetActive(long id,[FromBody]bool active,CancellationToken ct){try{await users.SetActiveAsync(id,active,ActorId(),ct);return Ok();}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    private long ActorId()=>long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
