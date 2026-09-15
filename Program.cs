using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Services;

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
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>{options.LoginPath="/Auth/Login";options.AccessDeniedPath="/Auth/Denied";options.Cookie.Name="AtlasPOS.Session";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Lax;options.Cookie.SecurePolicy=builder.Environment.IsProduction()?CookieSecurePolicy.Always:CookieSecurePolicy.SameAsRequest;options.SlidingExpiration=true;options.ExpireTimeSpan=TimeSpan.FromHours(4);});
builder.Services.AddAntiforgery(options=>{options.HeaderName="RequestVerificationToken";options.Cookie.Name="AtlasPOS.AntiForgery";options.Cookie.HttpOnly=false;options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.SecurePolicy=builder.Environment.IsProduction()?CookieSecurePolicy.Always:CookieSecurePolicy.SameAsRequest;});
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
builder.Services.AddSingleton<PuntoDeVentaAtlas.Web.Services.IDeviceCatalogService,
    PuntoDeVentaAtlas.Web.Services.DeviceCatalogService>();
builder.Services.AddDbContext<PuntoDeVentaAtlas.Web.Data.AtlasDbContext>(options =>
    options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection)));
builder.Services.AddScoped<PuntoDeVentaAtlas.Web.Services.IMySqlPointOfSaleService,
    PuntoDeVentaAtlas.Web.Services.EfPointOfSaleService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var applyMigrations = builder.Configuration.GetValue<bool>("Atlas:ApplyMigrations");
    if (app.Environment.IsProduction() && applyMigrations)
        throw new InvalidOperationException("Production no aplica migraciones automáticamente. Ejecuta la herramienta de upgrade con respaldo previo.");
    if (applyMigrations)
        await AtlasDatabaseSeeder.ApplyMigrationsAsync(scope.ServiceProvider.GetRequiredService<AtlasDbContext>(), default);
    if (builder.Configuration.GetValue<bool>("Atlas:DemoMode"))
    {
        if (app.Environment.IsProduction()) throw new InvalidOperationException("Atlas:DemoMode está prohibido en Production.");
        await AtlasDatabaseSeeder.SeedDemoAsync(scope.ServiceProvider.GetRequiredService<AtlasDbContext>(), default);
    }
    if (builder.Configuration.GetValue<bool>("Atlas:AllowInitialAdminBootstrap"))
        await scope.ServiceProvider.GetRequiredService<IAuthenticationService>().EnsureAdminPasswordAsync(default);
}

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
