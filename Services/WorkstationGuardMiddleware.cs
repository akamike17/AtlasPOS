using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
namespace PuntoDeVentaAtlas.Web.Services;
public sealed class WorkstationGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context,AtlasDbContext db,ICurrentTerminalContext terminal,ICurrentUserContext current)
    {
        if(context.User.Identity?.IsAuthenticated==true&&terminal.TerminalId!="SERVER"&&!context.Request.Path.StartsWithSegments("/Workstations/Register"))
        {
            var known=await db.Workstations.AsNoTracking().FirstOrDefaultAsync(x=>x.StoreId==current.StoreId&&x.TerminalId==terminal.TerminalId,context.RequestAborted);
            if(known is {Enabled:false}){context.Response.StatusCode=403;await context.Response.WriteAsJsonAsync(new{message="Esta terminal está deshabilitada."});return;}
        }
        await next(context);
    }
}
