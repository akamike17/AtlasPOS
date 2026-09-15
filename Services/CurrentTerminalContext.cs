using System.Net;

namespace PuntoDeVentaAtlas.Web.Services;
public interface ICurrentTerminalContext
{
    string TerminalId { get; }
    bool IsServerContext { get; }
    bool HasIdentity { get; }
    bool HasValidIdentity { get; }
}

public sealed class CurrentTerminalContext(IHttpContextAccessor accessor, IConfiguration configuration):ICurrentTerminalContext
{
    private HttpContext? HttpContext => accessor.HttpContext;
    private string? RawIdentity
    {
        get
        {
            var value=HttpContext?.Request.Headers["X-Atlas-Terminal-Id"].ToString();
            return string.IsNullOrWhiteSpace(value)?HttpContext?.Request.Cookies["AtlasPOS.Terminal"]:value;
        }
    }

    public bool HasIdentity=>!string.IsNullOrWhiteSpace(RawIdentity);
    public bool HasValidIdentity=>Guid.TryParseExact(RawIdentity,"N",out _);
    public bool IsServerContext=>!HasIdentity&&configuration.GetValue<bool>("Atlas:ServerContext")&&IsLocalRequest(HttpContext);
    public string TerminalId=>IsServerContext?"SERVER":HasValidIdentity&&Guid.TryParseExact(RawIdentity,"N",out var id)?id.ToString("N"):"";

    private static bool IsLocalRequest(HttpContext? context)
    {
        var address=context?.Connection.RemoteIpAddress;
        return address is null||IPAddress.IsLoopback(address);
    }
}
