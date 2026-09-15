using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize]
public sealed class WorkstationsController(AtlasDbContext db,ICurrentUserContext current):Controller
{
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromBody]RegisterWorkstationRequest request,CancellationToken ct)
    {
        if(!Guid.TryParseExact(request.TerminalId,"N",out var parsed))return BadRequest(new{message="Identificador de terminal no válido."});
        var id=parsed.ToString("N");var workstation=await db.Workstations.FirstOrDefaultAsync(x=>x.StoreId==current.StoreId&&x.TerminalId==id,ct);
        if(workstation is null){workstation=new(){StoreId=current.StoreId,TerminalId=id,Name=request.Name.Trim(),FirstSeenAt=DateTime.Now};db.Workstations.Add(workstation);}
        if(!workstation.Enabled)return StatusCode(403,new{message="Esta terminal fue deshabilitada por un administrador."});
        workstation.Name=request.Name.Trim();workstation.LastSeenAt=DateTime.Now;await db.SaveChangesAsync(ct);
        Response.Cookies.Append("AtlasPOS.Terminal",id,new CookieOptions{HttpOnly=false,SameSite=SameSiteMode.Lax,Secure=Request.IsHttps,Expires=DateTimeOffset.UtcNow.AddYears(2),IsEssential=true});
        return Ok(new WorkstationView(id,workstation.Name,workstation.Enabled,workstation.LastSeenAt));
    }
    [Authorize(Roles="Administrator")][HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.Workstations.AsNoTracking().Where(x=>x.StoreId==current.StoreId).OrderBy(x=>x.Name).Select(x=>new WorkstationView(x.TerminalId,x.Name,x.Enabled,x.LastSeenAt)).ToListAsync(ct));
}
