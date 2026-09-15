using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoDeVentaAtlas.Web.Services;
namespace PuntoDeVentaAtlas.Web.Controllers;
[Authorize(Roles="Administrator")] public sealed class OperationsController(OperationsService operations):Controller
{
    [HttpGet] public async Task<IActionResult> Backup(CancellationToken ct){var bytes=await operations.CreateBackupAsync(ct);return File(bytes,"application/gzip",$"atlas-pos-{DateTime.Now:yyyyMMdd-HHmmss}.json.gz");}
}
