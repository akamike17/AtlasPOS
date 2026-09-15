using Microsoft.AspNetCore.Http.Json;
using PuntoDeVentaAtlas.Web.Integrations;
using System.Text.Json.Serialization;

var builder=WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService(options=>options.ServiceName="Atlas POS Peripheral Agent");
builder.Configuration.AddJsonFile("atlasagentsettings.json",optional:true,reloadOnChange:true);
builder.Configuration.AddEnvironmentVariables();
builder.WebHost.UseUrls(builder.Configuration["Agent:Urls"]??"http://0.0.0.0:17420");
builder.Services.AddSingleton<SerialScaleRuntime>();
builder.Services.Configure<JsonOptions>(x=>x.SerializerOptions.DefaultIgnoreCondition=JsonIgnoreCondition.WhenWritingNull);
var app=builder.Build();

var token=builder.Configuration["Agent:Token"];
if(string.IsNullOrWhiteSpace(token)||token.Length<24)throw new InvalidOperationException("Configura Agent:Token con al menos 24 caracteres.");
app.Use(async(context,next)=>
{
    if(context.Request.Path=="/health"){await next();return;}
    if(!context.Request.Headers.TryGetValue("X-Atlas-Agent-Token",out var supplied)||!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(token),System.Text.Encoding.UTF8.GetBytes(supplied.ToString())))
    {context.Response.StatusCode=401;await context.Response.WriteAsJsonAsync(new{message="Token del agente no válido."});return;}
    await next();
});

app.MapGet("/health",()=>Results.Ok(new{service="Atlas Peripheral Agent",machine=Environment.MachineName,version=typeof(Program).Assembly.GetName().Version?.ToString()}));
app.MapGet("/hardware/discover",()=>Results.Ok(AgentHardwareDiscovery.Discover()));
app.MapPost("/scale/detect",async(SerialScaleRuntime scale,CancellationToken ct)=>Results.Ok(await scale.AutoDetectAsync(ct)));
app.MapPost("/scale/read",async(ScaleAgentRequest request,SerialScaleRuntime scale,CancellationToken ct)=>
{
    var reading=await scale.ReadAsync(new SerialScaleSettings(request.Port,request.BaudRate,Command:request.Continuous?null:request.Command??"P"),ct);
    return Results.Ok(new{kilograms=reading.Kilograms,stable=reading.Stable,raw=reading.Raw,readAt=reading.ReadAt});
});
app.MapPost("/printer/test",async(PrinterAgentRequest request,CancellationToken ct)=>
{
    var ticket=PrinterDiagnostic.Ticket(request.Provider);
    if(request.Connection.Equals("windows",StringComparison.OrdinalIgnoreCase))WindowsRawPrinter.Print(request.DeviceId??"",ticket);
    else if(request.Connection.Equals("tcp",StringComparison.OrdinalIgnoreCase)&&!string.IsNullOrWhiteSpace(request.Endpoint))
    {
        if(request.Mode?.Contains("zpl",StringComparison.OrdinalIgnoreCase)==true||request.Provider.Contains("zebra",StringComparison.OrdinalIgnoreCase))await new RawTcpZplPrinter(request.Endpoint,request.Port??9100).PrintAsync(PrinterDiagnostic.Zpl(request.Provider),ct);
        else await new RawTcpEscPosPrinter(request.Endpoint,request.Port??9100).PrintAsync(ticket,ct);
    }
    else return Results.BadRequest(new{message="Configura impresora Windows o TCP."});
    return Results.Ok(new{message="Trabajo de diagnóstico enviado. Confirma la impresión física."});
});
app.MapPost("/printer/print",async(PrintAgentRequest request,CancellationToken ct)=>
{
    if(request.Connection.Equals("windows",StringComparison.OrdinalIgnoreCase)){WindowsRawPrinter.Print(request.DeviceId??"",request.Document);if(request.OpenDrawer)WindowsRawPrinter.OpenDrawer(request.DeviceId??"");}
    else if(request.Connection.Equals("tcp",StringComparison.OrdinalIgnoreCase)&&!string.IsNullOrWhiteSpace(request.Endpoint)){await new RawTcpEscPosPrinter(request.Endpoint,request.Port??9100).PrintAsync(request.Document,ct);if(request.OpenDrawer)await RawTcpCashDrawer.OpenAsync(request.Endpoint,request.Port??9100,ct);}
    else return Results.BadRequest(new{message="Configura impresora Windows o TCP."});
    return Results.Ok(new{printed=true,drawerOpened=request.OpenDrawer});
});
app.MapPost("/drawer/open",async(PrinterAgentRequest request,CancellationToken ct)=>
{
    if(request.Connection.Equals("windows",StringComparison.OrdinalIgnoreCase))WindowsRawPrinter.OpenDrawer(request.DeviceId??"");
    else if(request.Connection.Equals("tcp",StringComparison.OrdinalIgnoreCase)&&!string.IsNullOrWhiteSpace(request.Endpoint))await RawTcpCashDrawer.OpenAsync(request.Endpoint,request.Port??9100,ct);
    else return Results.BadRequest(new{message="Configura el cajón a través de una impresora Windows o TCP."});
    return Results.Ok(new{opened=true});
});
app.Run();

public sealed record ScaleAgentRequest(string Port,int BaudRate=9600,bool Continuous=false,string? Command="P");
public sealed record PrinterAgentRequest(string Provider,string Connection,string? DeviceId,string? Endpoint,int? Port,string? Mode);
public sealed record PrintAgentRequest(string Connection,string? DeviceId,string? Endpoint,int? Port,string Document,bool OpenDrawer);

public static class AgentHardwareDiscovery
{
    public static object[] Discover()
    {
        if(!OperatingSystem.IsWindows())return [];
        var found=new List<object>();
        try{using var root=Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers");if(root is not null)foreach(var name in root.GetSubKeyNames()){using var key=root.OpenSubKey(name);var port=key?.GetValue("Port")?.ToString();found.Add(new{id=$"printer:{name}",name,manufacturer=Manufacturer(name),kind="printer",connection="Windows",port,suggestedProfile="generic",status="recognized"});}}catch{ }
        try{using var key=Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");if(key is not null)foreach(var value in key.GetValueNames()){var port=key.GetValue(value)?.ToString();if(!string.IsNullOrWhiteSpace(port))found.Add(new{id=$"serial:{port}",name=$"Puerto serial {port}",manufacturer="No identificado",kind="scale",connection="Serial",port,suggestedProfile="generic",status="detected_unclassified"});}}catch{ }
        return [..found];
    }
    private static string Manufacturer(string name)=>new[]{"Epson","Star","Zebra","Bixolon"}.FirstOrDefault(x=>name.Contains(x,StringComparison.OrdinalIgnoreCase))??"No identificado";
}
