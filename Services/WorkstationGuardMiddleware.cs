using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
namespace PuntoDeVentaAtlas.Web.Services;
public sealed class WorkstationGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context,AtlasDbContext db,ICurrentTerminalContext terminal,ICurrentUserContext current)
    {
        if(context.User.Identity?.IsAuthenticated==true&&!terminal.IsServerContext)
        {
            if(context.Request.Path.StartsWithSegments("/Workstations/Register")||context.Request.Path.StartsWithSegments("/Workstations/Onboarding"))
            {
                await next(context);return;
            }
            if(!terminal.HasIdentity)
            {
                if(HttpMethods.IsGet(context.Request.Method)&&(context.Request.Path=="/"||context.Request.Path.StartsWithSegments("/Pos")))
                {
                    context.Response.Redirect("/Workstations/Onboarding");return;
                }
                await Reject(context,StatusCodes.Status403Forbidden,"Esta caja no tiene una terminal registrada. Completa el alta de esta caja.");return;
            }
            if(!terminal.HasValidIdentity){await Reject(context,StatusCodes.Status400BadRequest,"El identificador de terminal no es válido.");return;}
            var known=await db.Workstations.AsNoTracking().FirstOrDefaultAsync(x=>x.StoreId==current.StoreId&&x.TerminalId==terminal.TerminalId,context.RequestAborted);
            if(known is null){await Reject(context,StatusCodes.Status403Forbidden,"Esta terminal no está registrada. Completa el alta y espera autorización administrativa.");return;}
            if(!known.Enabled){await Reject(context,StatusCodes.Status403Forbidden,"Esta terminal está deshabilitada.");return;}
        }
        await next(context);
    }

    private static async Task Reject(HttpContext context,int status,string message)
    {
        context.Response.StatusCode=status;
        await context.Response.WriteAsJsonAsync(new{message});
    }
}
