using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Models;
using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize]
public sealed class WorkstationsController(AtlasDbContext db,ICurrentUserContext current,ICurrentTerminalContext terminal):Controller
{
    [HttpGet]
    public IActionResult Onboarding()=>View();

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromBody]RegisterWorkstationRequest request,CancellationToken ct)
    {
        if(!Guid.TryParseExact(request.TerminalId,"N",out var parsed))return BadRequest(new{message="Identificador de terminal no válido."});
        var id=parsed.ToString("N");
        if(terminal.IsServerContext||!terminal.HasValidIdentity||terminal.TerminalId!=id)return BadRequest(new{message="La identidad de esta caja no coincide con la terminal solicitada."});
        var workstation=await db.Workstations.FirstOrDefaultAsync(x=>x.StoreId==current.StoreId&&x.TerminalId==id,ct);
        var newlyDiscovered=workstation is null;
        if(newlyDiscovered){workstation=new(){StoreId=current.StoreId,TerminalId=id,Name=request.Name.Trim(),Enabled=false,FirstSeenAt=DateTime.Now};db.Workstations.Add(workstation);db.AuditLog.Add(new(){StoreId=current.StoreId,UserId=current.UserId,Action="workstation.registration_requested",Entity="workstation",EntityId=id});}
        if(workstation is null)return Problem("No fue posible preparar el registro de la terminal.");
        if(!newlyDiscovered&&!workstation.Enabled)return StatusCode(403,new{message="Esta terminal fue deshabilitada por un administrador."});
        workstation.Name=request.Name.Trim();workstation.LastSeenAt=DateTime.Now;await db.SaveChangesAsync(ct);
        Response.Cookies.Append("AtlasPOS.Terminal",id,new CookieOptions{HttpOnly=false,SameSite=SameSiteMode.Lax,Secure=Request.IsHttps,Expires=DateTimeOffset.UtcNow.AddYears(2),IsEssential=true});
        return newlyDiscovered?StatusCode(StatusCodes.Status202Accepted,new{message="Caja registrada y pendiente de autorización administrativa.",workstation=new WorkstationView(id,workstation.Name,workstation.Enabled,workstation.LastSeenAt)}):Ok(new WorkstationView(id,workstation.Name,workstation.Enabled,workstation.LastSeenAt));
    }
    [Authorize(Roles="Administrator")][HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.Workstations.AsNoTracking().Where(x=>x.StoreId==current.StoreId).OrderBy(x=>x.Name).Select(x=>new WorkstationView(x.TerminalId,x.Name,x.Enabled,x.LastSeenAt)).ToListAsync(ct));

    [Authorize(Roles="Administrator"),HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEnabled([FromQuery]string terminalId,[FromBody]bool enabled,CancellationToken ct)
    {
        if(!Guid.TryParseExact(terminalId,"N",out var parsed))return BadRequest(new{message="Identificador de terminal no válido."});
        var id=parsed.ToString("N");var workstation=await db.Workstations.FirstOrDefaultAsync(x=>x.StoreId==current.StoreId&&x.TerminalId==id,ct);
        if(workstation is null)return NotFound(new{message="Terminal no encontrada."});
        workstation.Enabled=enabled;workstation.LastSeenAt=DateTime.Now;db.AuditLog.Add(new(){StoreId=current.StoreId,UserId=current.UserId,Action=enabled?"workstation.enabled":"workstation.disabled",Entity="workstation",EntityId=id});await db.SaveChangesAsync(ct);
        return Ok(new WorkstationView(id,workstation.Name,workstation.Enabled,workstation.LastSeenAt));
    }
}
