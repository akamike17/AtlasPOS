using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;

namespace PuntoDeVentaAtlas.Web.Services;

public sealed class UserSessionValidator(AtlasDbContext db)
{
    public async Task<bool> IsValidAsync(ClaimsPrincipal principal,CancellationToken ct)
    {
        if(!long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier),out var userId)||userId<=0)return false;
        if(!long.TryParse(principal.FindFirstValue("store_id"),out var storeId)||storeId<=0)return false;
        var role=principal.FindFirstValue(ClaimTypes.Role);
        if(string.IsNullOrWhiteSpace(role))return false;
        var user=await db.Users.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==userId,ct);
        return user is {Active:true }&&user.StoreId==storeId&&string.Equals(user.Role,role,StringComparison.Ordinal);
    }
}
