using System.Net.Sockets;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PuntoDeVentaAtlas.Web.Data;
using PuntoDeVentaAtlas.Web.Models;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text;
using PuntoDeVentaAtlas.Web.Integrations;

namespace PuntoDeVentaAtlas.Web.Services;

public sealed class PeripheralConfigurationService(AtlasDbContext db,IDataProtectionProvider protection,IHttpClientFactory clients,ICurrentUserContext current,ICurrentTerminalContext terminal,SerialScaleRuntime serialScale)
{
    private long StoreId=>current.StoreId;
    private string WorkstationId=>terminal.TerminalId;
    private readonly IDataProtector protector=protection.CreateProtector("AtlasPOS.PeripheralSecrets.v1");
    public async Task<IReadOnlyList<PeripheralConfigurationView>> ListAsync(CancellationToken ct)=>await db.PeripheralConfigurations.AsNoTracking().Where(x=>x.StoreId==StoreId&&x.WorkstationId==WorkstationId).OrderBy(x=>x.Key).Select(x=>new PeripheralConfigurationView(x.Key,x.Provider,x.ConnectionType,x.Endpoint??"",x.Port,x.Mode??"",x.DeviceId??"",x.ProtectedSecret!=null,x.Enabled,x.LastStatus,x.LastMessage,x.LastTestedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<DiscoveredHardware>> DiscoverAsync(CancellationToken ct)
    {
        var agent=await db.PeripheralConfigurations.AsNoTracking().FirstOrDefaultAsync(x=>x.StoreId==StoreId&&x.WorkstationId==WorkstationId&&x.ConnectionType=="Agent"&&x.Enabled&&x.ProtectedSecret!=null,ct);
        if(agent is null)return [];
        using var request=AgentRequest(agent,HttpMethod.Get,"hardware/discover");using var response=await clients.CreateClient().SendAsync(request,ct);var body=await response.Content.ReadAsStringAsync(ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"No se pudo consultar el hardware de esta caja: {body}");return JsonSerializer.Deserialize<List<DiscoveredHardware>>(body,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??[];
    }
    public async Task<PeripheralConfigurationView> SaveAsync(SavePeripheralConfigurationRequest x,long actorId,CancellationToken ct)
    {
        var entity=await db.PeripheralConfigurations.FirstOrDefaultAsync(p=>p.StoreId==StoreId&&p.WorkstationId==WorkstationId&&p.Key==x.Key,ct)??new(){StoreId=StoreId,WorkstationId=WorkstationId,Key=x.Key};if(entity.Id==0)db.PeripheralConfigurations.Add(entity);
        entity.Provider=x.Provider.Trim();entity.ConnectionType=x.ConnectionType.Trim();entity.Endpoint=Clean(x.Endpoint);entity.Port=x.Port;entity.Mode=Clean(x.Mode);entity.DeviceId=Clean(x.DeviceId);entity.Enabled=x.Enabled;entity.UpdatedAt=DateTime.Now;entity.LastStatus="not_tested";entity.LastMessage="Configuración guardada; ejecuta la prueba.";if(!string.IsNullOrWhiteSpace(x.Secret))entity.ProtectedSecret=protector.Protect(x.Secret.Trim());
        db.AuditLog.Add(new(){StoreId=StoreId,UserId=actorId,Action="peripheral.configured",Entity="peripheral",EntityId=x.Key,Detail=$"Proveedor: {entity.Provider}; conexión: {entity.ConnectionType}"});await db.SaveChangesAsync(ct);return Map(entity);
    }
    public async Task<PeripheralConfigurationView> TestAsync(string key,long actorId,CancellationToken ct)
    {
        var x=await db.PeripheralConfigurations.FirstOrDefaultAsync(p=>p.StoreId==StoreId&&p.WorkstationId==WorkstationId&&p.Key==key,ct)??throw new InvalidOperationException("Primero guarda la configuración del dispositivo.");
        try
        {
            var type=x.ConnectionType.ToLowerInvariant();string message;string status;
            if(type=="agent")
            {message=await TestAgentAsync(x,ct);status="connected";}
            else if(x.Key=="printer"&&type is "windows" or "spooler" or "usb")
            {WindowsRawPrinter.Print(x.DeviceId??"",PrinterDiagnostic.Ticket(x.Provider));status="connected";message=$"Ticket de diagnóstico enviado a la cola '{x.DeviceId}'. Confirma que salió impreso.";}
            else if(x.Key=="printer"&&type is "tcp" or "network")
            {if(string.IsNullOrWhiteSpace(x.Endpoint))throw new InvalidOperationException("Captura la IP de la impresora.");var port=x.Port??9100;if(x.Provider.Contains("zebra",StringComparison.OrdinalIgnoreCase)||x.Mode?.Contains("zpl",StringComparison.OrdinalIgnoreCase)==true)await new RawTcpZplPrinter(x.Endpoint,port).PrintAsync(PrinterDiagnostic.Zpl(x.Provider),ct);else await new RawTcpEscPosPrinter(x.Endpoint,port).PrintAsync(PrinterDiagnostic.Ticket(x.Provider),ct);status="connected";message=$"Ticket de diagnóstico enviado a {x.Endpoint}:{port}. Confirma que salió impreso.";}
            else if(type is "hid" or "canvas"){status="connected";message="Interfaz local disponible y lista.";}
            else if(type is "tcp" or "network")
            {if(string.IsNullOrWhiteSpace(x.Endpoint)||x.Port is null)throw new InvalidOperationException("Captura host/IP y puerto.");using var tcp=new TcpClient();using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(4));await tcp.ConnectAsync(x.Endpoint,x.Port.Value,timeout.Token);status="connected";message=$"Conexión TCP correcta a {x.Endpoint}:{x.Port}.";}
            else if(type is "http" or "https" or "api" or "pac")
            {if(!Uri.TryCreate(x.Endpoint,UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https"))throw new InvalidOperationException("Captura una URL HTTP/HTTPS válida.");using var request=new HttpRequestMessage(HttpMethod.Get,uri);if(x.ProtectedSecret is {Length:>0})request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",protector.Unprotect(x.ProtectedSecret));using var response=await clients.CreateClient().SendAsync(request,HttpCompletionOption.ResponseHeadersRead,ct);status=response.IsSuccessStatusCode?"connected":"reachable";message=$"Proveedor respondió HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).";}
            else if(x.Key=="scale"&&type is "serial" or "usb" or "usb-serial")
            {var reading=await serialScale.ReadAsync(SerialSettings(x),ct);status="connected";message=$"Báscula configurada correctamente: {reading.Kilograms:0.000} kg. Trama {reading.Raw}";}
            else
            {if(string.IsNullOrWhiteSpace(x.DeviceId)&&string.IsNullOrWhiteSpace(x.Endpoint))throw new InvalidOperationException("Captura puerto, identificador o ruta del dispositivo.");status="configuration_ready";message="Parámetros validados. La comunicación física final se habilita al instalar el SDK/controlador del proveedor.";}
            x.LastStatus=status;x.LastMessage=message;
        }
        catch(Exception ex) when(ex is not OperationCanceledException){x.LastStatus="failed";x.LastMessage=ex.Message;}
        x.LastTestedAt=DateTime.Now;db.AuditLog.Add(new(){StoreId=StoreId,UserId=actorId,Action="peripheral.tested",Entity="peripheral",EntityId=x.Key,Detail=x.LastStatus});await db.SaveChangesAsync(ct);return Map(x);
    }
    public async Task<ScaleReading> ReadScaleAsync(CancellationToken ct)
    {
        var x=await db.PeripheralConfigurations.AsNoTracking().FirstOrDefaultAsync(p=>p.StoreId==StoreId&&p.WorkstationId==WorkstationId&&p.Key=="scale"&&p.Enabled,ct);if(x is null)return new(false,false,false,0,"not_configured","No hay báscula configurada para esta terminal.",DateTime.Now);
        var type=x.ConnectionType.ToLowerInvariant();if(type=="agent")return await ReadAgentScaleAsync(x,ct);if(type is "serial" or "usb" or "usb-serial"){try{var sample=await serialScale.ReadAsync(SerialSettings(x),ct);return new(true,true,sample.Stable,sample.Kilograms,sample.Stable?"ready":"unstable",sample.Stable?"Báscula configurada correctamente y peso estable.":"Báscula conectada; esperando peso estable.",sample.ReadAt);}catch(InvalidOperationException ex){return new(true,false,false,0,"read_failed",ex.Message,DateTime.Now);}}if(type is not ("http" or "https" or "api"))return new(true,false,false,0,"driver_required",$"{x.Provider} está configurada en {x.DeviceId??x.Endpoint??type}, pero falta activar su protocolo/SDK de lectura.",DateTime.Now);
        try{if(!Uri.TryCreate(x.Endpoint,UriKind.Absolute,out var uri))throw new InvalidOperationException("La URL de la báscula no es válida.");using var request=new HttpRequestMessage(HttpMethod.Get,uri);if(x.ProtectedSecret is {Length:>0})request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",protector.Unprotect(x.ProtectedSecret));using var response=await clients.CreateClient().SendAsync(request,ct);response.EnsureSuccessStatusCode();var raw=await response.Content.ReadAsStringAsync(ct);decimal weight;bool stable=true;try{using var json=JsonDocument.Parse(raw);var root=json.RootElement;weight=ReadDecimal(root,"kilograms")??ReadDecimal(root,"weight")??ReadDecimal(root,"value")??throw new InvalidOperationException("La respuesta no contiene kilograms, weight o value.");if(root.ValueKind==JsonValueKind.Object&&root.TryGetProperty("stable",out var stableValue)&&stableValue.ValueKind is JsonValueKind.True or JsonValueKind.False)stable=stableValue.GetBoolean();}catch(JsonException){if(!decimal.TryParse(raw,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out weight))throw new InvalidOperationException("La báscula respondió un formato de peso no reconocido.");}if(weight<0||weight>99999)throw new InvalidOperationException("La lectura de peso está fuera de rango.");return new(true,true,stable,decimal.Round(weight,3),stable?"ready":"unstable",stable?"Báscula conectada y lectura estable.":"Esperando que el peso se estabilice.",DateTime.Now);}catch(Exception ex) when(ex is not OperationCanceledException){return new(true,false,false,0,"read_failed",ex.Message,DateTime.Now);}
    }
    public async Task<string> PrintSaleAsync(string folio,long actorId,CancellationToken ct)
    {
        var sale=await db.Sales.AsNoTracking().Include(x=>x.Lines).Include(x=>x.Payments).FirstOrDefaultAsync(x=>x.StoreId==StoreId&&x.WorkstationId==WorkstationId&&x.Folio==folio,ct)??throw new InvalidOperationException("La venta no pertenece a esta caja.");
        var printer=await PrinterAsync(ct);var document=BuildTicket(sale);var openDrawer=sale.Payments.Any(x=>x.Method=="cash")&&printer.Mode?.Contains("drawer=auto",StringComparison.OrdinalIgnoreCase)==true;await SendPrintAsync(printer,document,openDrawer,ct);
        db.AuditLog.Add(new(){StoreId=StoreId,UserId=actorId,Action=openDrawer?"sale.printed.drawer_opened":"sale.printed",Entity="sale",EntityId=folio,Detail=$"Terminal: {WorkstationId}"});await db.SaveChangesAsync(ct);return openDrawer?"Ticket impreso y cajón abierto.":"Ticket enviado a la impresora de esta caja.";
    }
    public async Task<string> OpenDrawerAsync(string reason,long actorId,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(reason))throw new InvalidOperationException("Captura el motivo de apertura.");var printer=await PrinterAsync(ct);if(printer.Mode?.Contains("drawer=off",StringComparison.OrdinalIgnoreCase)==true)throw new InvalidOperationException("El cajón está deshabilitado en esta caja.");await SendDrawerAsync(printer,ct);db.AuditLog.Add(new(){StoreId=StoreId,UserId=actorId,Action="cash_drawer.opened",Entity="workstation",EntityId=WorkstationId,Detail=reason.Trim()});await db.SaveChangesAsync(ct);return "Cajón abierto y movimiento auditado.";
    }
    private async Task<PeripheralConfigurationEntity> PrinterAsync(CancellationToken ct)=>await db.PeripheralConfigurations.AsNoTracking().FirstOrDefaultAsync(x=>x.StoreId==StoreId&&x.WorkstationId==WorkstationId&&x.Key=="printer"&&x.Enabled,ct)??throw new InvalidOperationException("No hay impresora configurada para esta caja.");
    private async Task SendPrintAsync(PeripheralConfigurationEntity x,string document,bool openDrawer,CancellationToken ct)
    {
        var type=x.ConnectionType.ToLowerInvariant();if(type=="agent"){using var request=AgentRequest(x,HttpMethod.Post,"printer/print");var tcp=x.Mode?.Contains("tcp",StringComparison.OrdinalIgnoreCase)==true;request.Content=JsonContent.Create(new{connection=tcp?"tcp":"windows",deviceId=tcp?null:x.DeviceId,endpoint=tcp?x.DeviceId:null,port=x.Port,document,openDrawer});using var response=await clients.CreateClient().SendAsync(request,ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));return;}if(type is "windows" or "spooler" or "usb"){WindowsRawPrinter.Print(x.DeviceId??"",document);if(openDrawer)WindowsRawPrinter.OpenDrawer(x.DeviceId??"");return;}if(type is "tcp" or "network"){await new RawTcpEscPosPrinter(x.Endpoint??throw new InvalidOperationException("Falta IP de impresora."),x.Port??9100).PrintAsync(document,ct);if(openDrawer)await RawTcpCashDrawer.OpenAsync(x.Endpoint!,x.Port??9100,ct);return;}throw new InvalidOperationException("La impresora configurada no admite tickets automáticos.");
    }
    private async Task SendDrawerAsync(PeripheralConfigurationEntity x,CancellationToken ct)
    {
        var type=x.ConnectionType.ToLowerInvariant();if(type=="agent"){using var request=AgentRequest(x,HttpMethod.Post,"drawer/open");var tcp=x.Mode?.Contains("tcp",StringComparison.OrdinalIgnoreCase)==true;request.Content=JsonContent.Create(new{provider=x.Provider,connection=tcp?"tcp":"windows",deviceId=tcp?null:x.DeviceId,endpoint=tcp?x.DeviceId:null,port=x.Port,mode=x.Mode});using var response=await clients.CreateClient().SendAsync(request,ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException(await response.Content.ReadAsStringAsync(ct));return;}if(type is "windows" or "spooler" or "usb"){WindowsRawPrinter.OpenDrawer(x.DeviceId??"");return;}if(type is "tcp" or "network"){await RawTcpCashDrawer.OpenAsync(x.Endpoint??throw new InvalidOperationException("Falta IP de impresora."),x.Port??9100,ct);return;}throw new InvalidOperationException("El cajón requiere una impresora ESC/POS Windows, TCP o Atlas Agent.");
    }
    private static string BuildTicket(SaleEntity sale)
    {
        var b=new StringBuilder();b.AppendLine("ATLAS POS").AppendLine($"Folio: {sale.Folio}").AppendLine($"Fecha: {sale.CreatedAt:yyyy-MM-dd HH:mm:ss}").AppendLine(new string('-',32));foreach(var x in sale.Lines)b.AppendLine(x.Description).AppendLine($" {x.Quantity:0.###} x {x.UnitPrice:0.00}   {x.Total:0.00}");b.AppendLine(new string('-',32)).AppendLine($"Subtotal: {sale.Subtotal:0.00}").AppendLine($"Descuento: {sale.Discount:0.00}").AppendLine($"IVA: {sale.Tax:0.00}").AppendLine($"TOTAL: {sale.Total:0.00}").AppendLine().AppendLine("Gracias por su compra");return b.ToString();
    }
    private static decimal? ReadDecimal(JsonElement root,string name){if(root.ValueKind!=JsonValueKind.Object||!root.TryGetProperty(name,out var value))return null;if(value.ValueKind==JsonValueKind.Number&&value.TryGetDecimal(out var number))return number;if(value.ValueKind==JsonValueKind.String&&decimal.TryParse(value.GetString(),System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out number))return number;return null;}
    public async Task<IReadOnlyList<SerialScaleProbe>> AutoDetectScaleAsync(CancellationToken ct)
    {
        var x=await db.PeripheralConfigurations.AsNoTracking().FirstOrDefaultAsync(p=>p.StoreId==StoreId&&p.WorkstationId==WorkstationId&&p.Key=="scale",ct);
        if(x?.ConnectionType.Equals("agent",StringComparison.OrdinalIgnoreCase)!=true)return await serialScale.AutoDetectAsync(ct);
        using var request=AgentRequest(x,HttpMethod.Post,"scale/detect");request.Content=JsonContent.Create(new{});using var response=await clients.CreateClient().SendAsync(request,ct);var body=await response.Content.ReadAsStringAsync(ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"El agente no pudo detectar la báscula: {body}");return JsonSerializer.Deserialize<List<SerialScaleProbe>>(body,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??[];
    }
    private async Task<string> TestAgentAsync(PeripheralConfigurationEntity x,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(x.Endpoint)||x.ProtectedSecret is null)throw new InvalidOperationException("Captura la URL y token del Atlas Peripheral Agent de esta caja.");
        if(x.Key=="scale"){var reading=await ReadAgentScaleAsync(x,ct);if(!reading.Connected)throw new InvalidOperationException(reading.Message);return $"Agente y báscula listos: {reading.Kilograms:0.000} kg.";}
        if(x.Key=="printer")
        {
            using var request=AgentRequest(x,HttpMethod.Post,"printer/test");request.Content=JsonContent.Create(new{provider=x.Provider,connection=x.Mode?.Contains("tcp",StringComparison.OrdinalIgnoreCase)==true?"tcp":"windows",deviceId=x.DeviceId,endpoint=(string?)null,port=x.Port,mode=x.Mode});
            using var response=await clients.CreateClient().SendAsync(request,ct);var body=await response.Content.ReadAsStringAsync(ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"El agente rechazó la impresión: {body}");return "El agente recibió el ticket. Confirma que salió en la impresora de esta caja.";
        }
        using(var request=AgentRequest(x,HttpMethod.Get,"health")){using var response=await clients.CreateClient().SendAsync(request,ct);response.EnsureSuccessStatusCode();return "Atlas Peripheral Agent conectado en esta caja.";}
    }
    private async Task<ScaleReading> ReadAgentScaleAsync(PeripheralConfigurationEntity x,CancellationToken ct)
    {
        try
        {
            var settings=SerialSettings(x);using var request=AgentRequest(x,HttpMethod.Post,"scale/read");request.Content=JsonContent.Create(new{port=settings.Port,baudRate=settings.BaudRate,continuous=settings.Command is null,command=settings.Command});using var response=await clients.CreateClient().SendAsync(request,ct);var raw=await response.Content.ReadAsStringAsync(ct);if(!response.IsSuccessStatusCode)throw new InvalidOperationException(raw);using var json=JsonDocument.Parse(raw);var root=json.RootElement;var kg=ReadDecimal(root,"kilograms")??throw new InvalidOperationException("El agente no devolvió peso.");var stable=root.TryGetProperty("stable",out var s)&&s.GetBoolean();return new(true,true,stable,kg,stable?"ready":"unstable",stable?"Báscula local conectada mediante Atlas Agent.":"Esperando peso estable en la báscula local.",DateTime.Now);
        }
        catch(Exception ex) when(ex is not OperationCanceledException){return new(true,false,false,0,"agent_failed",$"No fue posible leer el agente de esta caja: {ex.Message}",DateTime.Now);}
    }
    private HttpRequestMessage AgentRequest(PeripheralConfigurationEntity x,HttpMethod method,string path)
    {
        if(!Uri.TryCreate(x.Endpoint?.TrimEnd('/')+"/"+path,UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https"))throw new InvalidOperationException("URL del agente no válida.");
        var request=new HttpRequestMessage(method,uri);request.Headers.Add("X-Atlas-Agent-Token",protector.Unprotect(x.ProtectedSecret??throw new InvalidOperationException("Falta el token del agente.")));return request;
    }
    private static SerialScaleSettings SerialSettings(PeripheralConfigurationEntity x){if(string.IsNullOrWhiteSpace(x.DeviceId))throw new InvalidOperationException("Selecciona el puerto COM de la báscula.");var baud=9600;if(!string.IsNullOrWhiteSpace(x.Mode)){var match=System.Text.RegularExpressions.Regex.Match(x.Mode,@"(?:baud\s*=\s*)?(2400|4800|9600|19200|38400)",System.Text.RegularExpressions.RegexOptions.IgnoreCase);if(match.Success)baud=int.Parse(match.Groups[1].Value);}var command=x.Mode?.Contains("continuous",StringComparison.OrdinalIgnoreCase)==true?null:"P";return new(x.DeviceId.Trim(),baud,Command:command);}
    private static PeripheralConfigurationView Map(PeripheralConfigurationEntity x)=>new(x.Key,x.Provider,x.ConnectionType,x.Endpoint??"",x.Port,x.Mode??"",x.DeviceId??"",x.ProtectedSecret!=null,x.Enabled,x.LastStatus,x.LastMessage,x.LastTestedAt);
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
