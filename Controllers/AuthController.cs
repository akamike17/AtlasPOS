using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoDeVentaAtlas.Web.Models;
using IAtlasAuth=PuntoDeVentaAtlas.Web.Services.IAuthenticationService;
namespace PuntoDeVentaAtlas.Web.Controllers;
[AllowAnonymous] public sealed class AuthController(IAtlasAuth auth):Controller
{
    [HttpGet] public IActionResult Login(string? returnUrl=null){if(User.Identity?.IsAuthenticated==true)return RedirectToAction("Index","Pos");ViewBag.ReturnUrl=returnUrl;return View(new LoginViewModel());}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginViewModel model,string? returnUrl,CancellationToken ct){if(!ModelState.IsValid)return View(model);var user=await auth.ValidateAsync(model.Email.Trim().ToLowerInvariant(),model.Password,ct);if(user is null){model.Error="Correo o contraseña incorrectos.";return View(model);}var claims=new[]{new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),new Claim(ClaimTypes.Name,user.Name),new Claim(ClaimTypes.Email,user.Email),new Claim(ClaimTypes.Role,user.Role),new Claim("store_id",user.StoreId.ToString())};await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)),new AuthenticationProperties{IsPersistent=model.RememberMe,ExpiresUtc=DateTimeOffset.UtcNow.AddHours(model.RememberMe?12:4)});return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:Url.Action("Index","Pos")!);}
    [Authorize,HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);return RedirectToAction(nameof(Login));}
    public IActionResult Denied()=>View();
}
