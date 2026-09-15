using Microsoft.Win32;
using PuntoDeVentaAtlas.Web.Models;
using System.Runtime.Versioning;

namespace PuntoDeVentaAtlas.Web.Services;

public sealed class HardwareDiscoveryService
{
    private static readonly HardwareProfile[] Profiles=
    [
        new("epson-escpos","Epson","TM-T20/T70/T88/U220/P20/P80","printer","ESC/POS + OPOS/ePOS",["USB","TCP","Bluetooth","OPOS"],"Public protocol / manufacturer SDK","https://support.epson.net/",true,"Windows supports ESC/POS network/Bluetooth discovery; install Epson driver/OPOS for USB."),
        new("star-prnt","Star Micronics","mC-Print/TSP/SP/BSC","printer","StarPRNT / StarIO / ESC-POS",["USB","TCP","Bluetooth","Serial"],"StarPRNT Windows SDK","https://starmicronics.com/support/developers/windows-sdks/",true,"StarIO supports discovery and detailed status after its licensed package is installed."),
        new("zebra-linkos","Zebra","ZD/ZQ/ZT/ZE","printer","ZPL / Link-OS",["USB","TCP","Bluetooth","WebSocket"],"Link-OS SDK","https://techdocs.zebra.com/link-os/",true,"Use Link-OS for labels; raw TCP 9100 can send ZPL when configured."),
        new("bixolon-pos","Bixolon","SRP series","printer","ESC/POS / Windows POS SDK",["USB","TCP","Bluetooth","Serial"],"Windows POS SDK / OPOS","https://www.bixolon.com/",true,"Detected through installed Windows printer or USB identity."),
        new("torrey-serial","Torrey","EQB/MF/W-LABEL and compatible","scale","Manufacturer serial protocol",["Serial","USB-Serial"],"Protocol manual required per model","https://torrey.net/",true,"Select the exact model and serial parameters before reading stable weight."),
        new("dibal-scale","Dibal","Retail scales","scale","Dibal communications / serial",["Serial","TCP"],"Manufacturer integration package","https://www.dibal.com/",true,"Protocol and command set vary by family and firmware."),
        new("mettler-opos","Mettler Toledo","Retail POS scales","scale","OPOS / UPOS",["USB","Serial","OPOS"],"Mettler Toledo OPOS","https://www.mt.com/",true,"Install and configure the official OPOS service object."),
        new("cas-scale","CAS","POS scales","scale","Serial / OPOS depending on model",["Serial","USB-Serial"],"Model protocol/driver required","https://www.cas-usa.com/",true,"Requires exact model, baud rate and framing."),
        new("mercadopago-point","Mercado Pago","Point Smart 1/2","pinpad","Point Orders API",["HTTPS"],"Official REST API + OAuth","https://www.mercadopago.com.mx/developers/es/docs/mp-point/overview",true,"Cloud discovery lists terminals associated with the merchant account; requires access token and PDV mode."),
        new("generic-semiintegrated","Bank/acquirer","Certified semi-integrated terminal","pinpad","Provider API/SDK",["HTTPS","TCP","Serial"],"Private onboarding and certification","",false,"Clip, NetPay and bank terminals must use the contract supplied to the merchant; never infer approval locally.")
    ];

    public IReadOnlyList<HardwareProfile> Catalog()=>Profiles;

    public IReadOnlyList<DiscoveredHardware> Discover()
    {
        var found=new Dictionary<string,DiscoveredHardware>(StringComparer.OrdinalIgnoreCase);
        if(!OperatingSystem.IsWindows())return [];
        ReadPrinters(found);ReadSerialPorts(found);ReadUsb(found);return found.Values.OrderBy(x=>x.Kind).ThenBy(x=>x.Name).ToList();
    }

    [SupportedOSPlatform("windows")] private static void ReadPrinters(Dictionary<string,DiscoveredHardware> found)
    {
        try{using var root=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers");if(root is null)return;foreach(var name in root.GetSubKeyNames()){using var key=root.OpenSubKey(name);var port=key?.GetValue("Port")?.ToString();Add(found,$"printer:{name}",name,"printer",port?.StartsWith("IP_",StringComparison.OrdinalIgnoreCase)==true?"TCP":"Windows",port);}}catch{ }
    }
    [SupportedOSPlatform("windows")] private static void ReadSerialPorts(Dictionary<string,DiscoveredHardware> found)
    {
        try{using var key=Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");if(key is null)return;foreach(var value in key.GetValueNames()){var port=key.GetValue(value)?.ToString();if(!string.IsNullOrWhiteSpace(port))Add(found,$"serial:{port}",$"Puerto serial {port}","serial","Serial",port);}}catch{ }
    }
    [SupportedOSPlatform("windows")] private static void ReadUsb(Dictionary<string,DiscoveredHardware> found)
    {
        try{using var root=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");if(root is null)return;foreach(var deviceKey in root.GetSubKeyNames()){using var device=root.OpenSubKey(deviceKey);if(device is null)continue;foreach(var instanceName in device.GetSubKeyNames()){using var instance=device.OpenSubKey(instanceName);var name=instance?.GetValue("FriendlyName")?.ToString()??instance?.GetValue("DeviceDesc")?.ToString();if(string.IsNullOrWhiteSpace(name))continue;Add(found,$"usb:{deviceKey}:{instanceName}",name,"usb","USB",deviceKey);}}}catch{ }
    }
    private static void Add(Dictionary<string,DiscoveredHardware> found,string id,string name,string fallbackKind,string connection,string? port)
    {
        var text=name.ToLowerInvariant();var profile=Profiles.FirstOrDefault(x=>text.Contains(x.Manufacturer.ToLowerInvariant())||x.Family.Split('/').Any(f=>f.Length>3&&text.Contains(f.ToLowerInvariant())));var kind=profile?.Kind??(text.Contains("scale")||text.Contains("báscula")||text.Contains("balanza")?"scale":fallbackKind=="printer"?"printer":"unknown");var manufacturer=profile?.Manufacturer??"No identificado";found[id]=new(id,name,manufacturer,kind,connection,port,profile?.Key??"generic",profile is null?"detected_unclassified":"recognized");
    }
}
