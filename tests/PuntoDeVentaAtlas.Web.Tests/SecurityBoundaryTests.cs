using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PuntoDeVentaAtlas.Web.Services;
using Xunit;

namespace PuntoDeVentaAtlas.Web.Tests;

public sealed class SecurityBoundaryTests
{
    [Fact]
    public void LocalHostWithoutTerminalUsesExplicitServerContext()
    {
        var http=new DefaultHttpContext();
        var context=new CurrentTerminalContext(new HttpContextAccessor{HttpContext=http},Configuration(true));

        Assert.True(context.IsServerContext);
        Assert.Equal("SERVER",context.TerminalId);
        Assert.False(context.HasIdentity);
    }

    [Theory]
    [InlineData("SERVER")]
    [InlineData("not-a-terminal")]
    [InlineData("")]
    public void ClientTerminalNeverFallsBackToServer(string value)
    {
        var http=new DefaultHttpContext();
        http.Connection.RemoteIpAddress=IPAddress.Parse("192.0.2.10");
        if(value.Length>0)http.Request.Headers["X-Atlas-Terminal-Id"]=value;
        var context=new CurrentTerminalContext(new HttpContextAccessor{HttpContext=http},Configuration(true));

        Assert.False(context.IsServerContext);
        Assert.NotEqual("SERVER",context.TerminalId);
        Assert.Equal(value.Length>0,context.HasIdentity);
        Assert.Equal(value.Length==32,context.HasValidIdentity);
    }

    [Fact]
    public void ValidClientTerminalIsNormalizedButNotServer()
    {
        var terminal=Guid.NewGuid().ToString("N");var http=new DefaultHttpContext();http.Request.Headers["X-Atlas-Terminal-Id"]=terminal.ToUpperInvariant();
        var context=new CurrentTerminalContext(new HttpContextAccessor{HttpContext=http},Configuration(true));

        Assert.False(context.IsServerContext);
        Assert.Equal(terminal,context.TerminalId);
        Assert.True(context.HasValidIdentity);
    }

    [Fact]
    public void InvalidActorClaimFailsClosedInsteadOfUsingAdministratorOne()
    {
        var http=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,"bad")],"test"))};
        var context=new CurrentUserContext(new HttpContextAccessor{HttpContext=http});

        Assert.Throws<UnauthorizedAccessException>(()=>context.UserId);
    }

    private static IConfiguration Configuration(bool serverContext)=>new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Atlas:ServerContext",serverContext.ToString()}}).Build();
}
