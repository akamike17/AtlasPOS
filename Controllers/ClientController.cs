using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace PuntoDeVentaAtlas.Web.Controllers;
[AllowAnonymous]
public sealed class ClientController:Controller
{
    public IActionResult Setup()=>View();
}
