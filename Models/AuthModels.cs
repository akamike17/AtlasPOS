using System.ComponentModel.DataAnnotations;
namespace PuntoDeVentaAtlas.Web.Models;
public sealed class LoginViewModel { [Required,EmailAddress] public string Email{get;set;}=""; [Required,DataType(DataType.Password)] public string Password{get;set;}=""; public bool RememberMe{get;set;} public string? Error{get;set;} }
public sealed record UserSummary(long Id,string Name,string Email,string Role,bool Active);
public sealed class CreateUserRequest { [Required,StringLength(160)] public string Name{get;set;}=""; [Required,EmailAddress] public string Email{get;set;}=""; [Required,MinLength(10)] public string Password{get;set;}=""; [Required] public string Role{get;set;}="Cashier"; }
