using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
namespace PuntoDeVentaAtlas.Web.Services;
public interface IAuthenticationService { Task<UserEntity?> ValidateAsync(string email,string password,CancellationToken ct); Task EnsureAdminPasswordAsync(CancellationToken ct); }
public sealed class AuthenticationService(AtlasDbContext db,IConfiguration config,IPasswordHasher<UserEntity> hasher):IAuthenticationService
{
    public async Task<UserEntity?> ValidateAsync(string email,string password,CancellationToken ct){var user=await db.Users.FirstOrDefaultAsync(x=>x.Email==email&&x.Active,ct);if(user is null)return null;var result=hasher.VerifyHashedPassword(user,user.PasswordHash,password);if(result==PasswordVerificationResult.Failed)return null;if(result==PasswordVerificationResult.SuccessRehashNeeded){user.PasswordHash=hasher.HashPassword(user,password);await db.SaveChangesAsync(ct);}return user;}
    public async Task EnsureAdminPasswordAsync(CancellationToken ct){var user=await db.Users.FirstOrDefaultAsync(x=>x.Email=="admin@atlas.local",ct);if(user is null||user.PasswordHash!="CONFIGURAR_IDENTITY")return;var initial=config["Atlas:InitialAdminPassword"];if(string.IsNullOrWhiteSpace(initial))throw new InvalidOperationException("Configura Atlas:InitialAdminPassword en Secret Manager.");user.PasswordHash=hasher.HashPassword(user,initial);await db.SaveChangesAsync(ct);}
}
