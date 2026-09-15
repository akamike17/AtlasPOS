using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Models;
namespace PuntoDeVentaAtlas.Web.Services;
public sealed class UserManagementService(AtlasDbContext db,IPasswordHasher<UserEntity> hasher,ICurrentUserContext current)
{
    private static readonly string[] Roles=["Administrator","Manager","Cashier"];
    public async Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken ct)=>await db.Users.AsNoTracking().Where(x=>x.StoreId==current.StoreId).OrderBy(x=>x.Name).Select(x=>new UserSummary(x.Id,x.Name,x.Email,x.Role,x.Active)).ToListAsync(ct);
    public async Task<UserSummary> CreateAsync(CreateUserRequest x,long actorId,CancellationToken ct){if(!Roles.Contains(x.Role))throw new InvalidOperationException("Rol no válido.");var email=x.Email.Trim().ToLowerInvariant();if(await db.Users.AnyAsync(u=>u.Email==email,ct))throw new InvalidOperationException("El correo ya está registrado.");var user=new UserEntity{StoreId=current.StoreId,Name=x.Name.Trim(),Email=email,Role=x.Role,Active=true};user.PasswordHash=hasher.HashPassword(user,x.Password);db.Users.Add(user);db.AuditLog.Add(new(){StoreId=current.StoreId,UserId=actorId,Action="user.created",Entity="user",EntityId=email});await db.SaveChangesAsync(ct);return new(user.Id,user.Name,user.Email,user.Role,user.Active);}
    public async Task SetActiveAsync(long id,bool active,long actorId,CancellationToken ct){if(id==actorId&&!active)throw new InvalidOperationException("No puedes desactivar tu propia cuenta.");var user=await db.Users.FirstOrDefaultAsync(x=>x.Id==id&&x.StoreId==current.StoreId,ct)??throw new InvalidOperationException("Usuario no encontrado.");user.Active=active;db.AuditLog.Add(new(){StoreId=current.StoreId,UserId=actorId,Action=active?"user.activated":"user.deactivated",Entity="user",EntityId=user.Email});await db.SaveChangesAsync(ct);}
}
