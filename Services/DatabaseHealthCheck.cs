using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PuntoDeVentaAtlas.Web.Data;
namespace PuntoDeVentaAtlas.Web.Services;
public sealed class DatabaseHealthCheck(IServiceScopeFactory scopes):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken ct=default){try{await using var scope=scopes.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<AtlasDbContext>();return await db.Database.CanConnectAsync(ct)?HealthCheckResult.Healthy("MySQL disponible"):HealthCheckResult.Unhealthy("MySQL no responde");}catch(Exception ex){return HealthCheckResult.Unhealthy("Error de conexión MySQL",ex);}}
}
