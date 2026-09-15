using System.Security.Claims;
namespace PuntoDeVentaAtlas.Web.Services;
public interface ICurrentUserContext{long UserId{get;}long StoreId{get;}string Name{get;}}
public sealed class CurrentUserContext(IHttpContextAccessor accessor):ICurrentUserContext
{
 private ClaimsPrincipal User=>accessor.HttpContext?.User??throw new InvalidOperationException("No hay contexto HTTP activo.");
 public long UserId=>Parse(ClaimTypes.NameIdentifier,"usuario");public long StoreId=>Parse("store_id","sucursal");public string Name=>User.Identity?.Name??"Sistema";
 private long Parse(string type,string label)=>long.TryParse(User.FindFirstValue(type),out var id)&&id>0?id:throw new UnauthorizedAccessException($"La sesión no contiene {label} válido.");
}
