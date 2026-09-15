using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var mysqlConnection = builder.Configuration.GetConnectionString("AtlasMySql")
    ?? throw new InvalidOperationException("Falta la conexión AtlasMySql.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.ICurrentUserContext,PuntoDeVentaAtlas.Web.Services.CurrentUserContext>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.ICurrentTerminalContext,PuntoDeVentaAtlas.Web.Services.CurrentTerminalContext>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>{options.LoginPath="/Auth/Login";options.AccessDeniedPath="/Auth/Denied";options.Cookie.Name="AtlasPOS.Session";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Lax;options.SlidingExpiration=true;options.ExpireTimeSpan=TimeSpan.FromHours(4);});
builder.Services.AddAuthorization(options=>{options.AddPolicy("ManageInventory",p=>p.RequireRole("Administrator","Manager"));options.AddPolicy("CloseCash",p=>p.RequireRole("Administrator","Manager"));options.AddPolicy("Audit",p=>p.RequireRole("Administrator"));});
builder.Services.AddScoped<IPasswordHasher<PuntoDeVentaAtlas.Web.Data.UserEntity>,PasswordHasher<PuntoDeVentaAtlas.Web.Data.UserEntity>>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.IAuthenticationService,PuntoDeVentaAtlas.Web.Services.AuthenticationService>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.OperationsService>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.UserManagementService>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.CustomerDashboardPdfService>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.PeripheralConfigurationService>();
builder.Services.AddSingleton<PuntoDeVentaAtlas.Web.Services.HardwareDiscoveryService>();
builder.Services.AddSingleton<PuntoDeVentaAtlas.Web.Integrations.SerialScaleRuntime>();
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.ManufacturingService>();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks().AddCheck<PuntoDeVentaAtlas.Web.Services.DatabaseHealthCheck>("mysql");
builder.Services.AddProblemDetails();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")))
    .SetApplicationName("AtlasPOS");
builder.Services.AddSingleton<PuntoDeVentaAtlas.Web.Services.IPointOfSaleService,
    PuntoDeVentaAtlas.Web.Services.PointOfSaleService>();
builder.Services.AddDbContext<PuntoDeVentaAtlas.Web.Data.AtlasDbContext>(options =>
    options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection)));
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.IMySqlPointOfSaleService,
    PuntoDeVentaAtlas.Web.Services.EfPointOfSaleService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope()) { await PuntoDeVentaAtlas.Web.Data.AtlasDatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<PuntoDeVentaAtlas.Web.Data.AtlasDbContext>()); await scope.ServiceProvider.GetRequiredService<PuntoDeVentaAtlas.Web.Services.IAuthenticationService>().EnsureAdminPasswordAsync(default); }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if(builder.Configuration.GetValue<bool>("Atlas:RequireHttps")){app.UseHsts();app.UseHttpsRedirection();}
}
else app.UseExceptionHandler();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<PuntoDeVentaAtlas.Web.Services.WorkstationGuardMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pos}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();
