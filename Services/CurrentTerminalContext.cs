namespace PuntoDeVentaAtlas.Web.Services;
public interface ICurrentTerminalContext { string TerminalId{get;} }
public sealed class CurrentTerminalContext(IHttpContextAccessor accessor):ICurrentTerminalContext
{
    public string TerminalId
    {
        get
        {
            var value=accessor.HttpContext?.Request.Headers["X-Atlas-Terminal-Id"].ToString();
            if(string.IsNullOrWhiteSpace(value))value=accessor.HttpContext?.Request.Cookies["AtlasPOS.Terminal"];
            return Guid.TryParse(value,out var id)?id.ToString("N"):"SERVER";
        }
    }
}
