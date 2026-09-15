using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using PuntoDeVentaAtlas.Web.Controllers;
using System.Reflection;
using Xunit;

namespace PuntoDeVentaAtlas.Web.Tests;

public sealed class RoleAuthorizationTests
{
    [Theory]
    [InlineData(typeof(PosController),nameof(PosController.SaveProduct),"ManageInventory",null)]
    [InlineData(typeof(PosController),nameof(PosController.ReceivePurchase),"ManageInventory",null)]
    [InlineData(typeof(PosController),nameof(PosController.AdjustInventory),"ManageInventory",null)]
    [InlineData(typeof(PosController),nameof(PosController.CloseShift),"CloseCash",null)]
    [InlineData(typeof(PosController),nameof(PosController.Audit),"Audit",null)]
    [InlineData(typeof(PosController),nameof(PosController.ReturnSale),null,"Administrator,Manager")]
    [InlineData(typeof(ManufacturingController),nameof(ManufacturingController.Produce),"ManageInventory",null)]
    [InlineData(typeof(OperationsController),nameof(OperationsController.Backup),null,"Administrator")]
    [InlineData(typeof(UsersController),nameof(UsersController.Create),null,"Administrator")]
    [InlineData(typeof(WorkstationsController),nameof(WorkstationsController.SetEnabled),null,"Administrator")]
    [InlineData(typeof(PeripheralsController),nameof(PeripheralsController.OpenDrawer),null,"Administrator,Manager")]
    public void SensitiveEndpointsDeclareTheirMinimumAuthorization(Type controller,string action,string? policy,string? roles)
    {
        var method=controller.GetMethod(action)!;
        var attributes=controller.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>().Concat(method.GetCustomAttributes<AuthorizeAttribute>(true)).ToList();
        Assert.Contains(attributes,x=>x.Policy==policy&&x.Roles==roles);
    }

    [Theory]
    [InlineData("Administrator", "ManageInventory", true)]
    [InlineData("Manager", "ManageInventory", true)]
    [InlineData("Cashier", "ManageInventory", false)]
    [InlineData("Administrator", "Audit", true)]
    [InlineData("Manager", "Audit", false)]
    [InlineData("Cashier", "Audit", false)]
    public async Task DirectHttpAuthorizationPipelineEnforcesRolePolicies(string role,string policy,bool allowed)
    {
        using var provider=Services();var nextCalled=false;
        var context=new DefaultHttpContext{RequestServices=provider,User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role,role)],"test"))};
        context.SetEndpoint(new Endpoint(_=>{nextCalled=true;return Task.CompletedTask;},new EndpointMetadataCollection(new AuthorizeAttribute{Policy=policy}),"test"));
        await new AuthorizationMiddleware(_=>{nextCalled=true;return Task.CompletedTask;},provider.GetRequiredService<IAuthorizationPolicyProvider>()).Invoke(context);
        Assert.Equal(allowed,nextCalled);
        if(!allowed)Assert.Equal(StatusCodes.Status403Forbidden,context.Response.StatusCode);
    }

    private static ServiceProvider Services()=>new ServiceCollection().AddLogging().AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,TestAuthenticationHandler>("Test",_=>{}).Services.AddAuthorization(options=>{options.AddPolicy("ManageInventory",p=>p.RequireRole("Administrator","Manager"));options.AddPolicy("Audit",p=>p.RequireRole("Administrator"));}).BuildServiceProvider();

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(AuthenticateResult.NoResult());
    }
}
