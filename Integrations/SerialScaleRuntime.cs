using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace PuntoDeVentaAtlas.Web.Integrations;

public sealed record SerialScaleSettings(string Port,int BaudRate=9600,int DataBits=8,Parity Parity=Parity.None,StopBits StopBits=StopBits.One,string? Command="P");
public sealed record SerialScaleSample(decimal Kilograms,bool Stable,string Raw,DateTime ReadAt);
public sealed record SerialScaleProbe(string Port,int BaudRate,decimal Kilograms,string Raw,bool Continuous);

public static partial class ScaleFrameParser
{
    [GeneratedRegex(@"(?<!\d)([-+]?\d{1,6}(?:[\.,]\d{1,3})?)(?:\s*)(kg|g|lb|lbs)?",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant)] private static partial Regex WeightPattern();
    public static bool TryParse(string raw,out decimal kilograms)
    {
        kilograms=0;if(string.IsNullOrWhiteSpace(raw))return false;var matches=WeightPattern().Matches(raw);for(var i=matches.Count-1;i>=0;i--){var match=matches[i];if(!decimal.TryParse(match.Groups[1].Value.Replace(',','.'),NumberStyles.Number,CultureInfo.InvariantCulture,out var value)||value<0)continue;var unit=match.Groups[2].Value.ToLowerInvariant();kilograms=unit=="g"?value/1000m:unit is "lb" or "lbs"?value*0.45359237m:value;if(kilograms<=99999)return true;}return false;
    }
}

public sealed class SerialScaleRuntime : IDisposable
{
    private readonly SemaphoreSlim gate=new(1,1);private SerialPort? serial;private SerialScaleSettings? active;private readonly Queue<(decimal Weight,DateTime At)> recent=new();
    public async Task<SerialScaleSample> ReadAsync(SerialScaleSettings settings,CancellationToken ct)
    {
        await gate.WaitAsync(ct);try{EnsureOpen(settings);serial!.DiscardInBuffer();if(!string.IsNullOrEmpty(settings.Command))serial.Write(settings.Command);var raw=await ReceiveAsync(serial,TimeSpan.FromMilliseconds(650),ct);if(!ScaleFrameParser.TryParse(raw,out var weight))throw new InvalidOperationException($"La báscula respondió, pero no se reconoció el peso. Trama: {Safe(raw)}");var now=DateTime.Now;recent.Enqueue((weight,now));while(recent.Count>5||recent.TryPeek(out var first)&&now-first.At>TimeSpan.FromSeconds(2))recent.Dequeue();var stable=recent.Count>=2&&recent.Max(x=>x.Weight)-recent.Min(x=>x.Weight)<=.002m;return new(decimal.Round(weight,3),stable,Safe(raw),now);}catch(Exception ex) when(ex is IOException or InvalidOperationException or UnauthorizedAccessException or TimeoutException){Reset();throw new InvalidOperationException($"No se pudo leer {settings.Port}: {ex.Message}",ex);}finally{gate.Release();}
    }
    public async Task<IReadOnlyList<SerialScaleProbe>> AutoDetectAsync(CancellationToken ct)
    {
        var found=new List<SerialScaleProbe>();foreach(var port in SerialPort.GetPortNames().OrderBy(x=>x)){foreach(var baud in new[]{9600,4800,2400,19200}){ct.ThrowIfCancellationRequested();try{using var probe=Create(new(port,baud));probe.Open();string raw;var continuous=true;try{raw=await ReceiveAsync(probe,TimeSpan.FromMilliseconds(280),ct);}catch(TimeoutException){continuous=false;probe.DiscardInBuffer();probe.Write("P");raw=await ReceiveAsync(probe,TimeSpan.FromMilliseconds(450),ct);}if(ScaleFrameParser.TryParse(raw,out var kg)){found.Add(new(port,baud,decimal.Round(kg,3),Safe(raw),continuous));break;}}catch(Exception ex) when(ex is IOException or InvalidOperationException or UnauthorizedAccessException or TimeoutException){ }}}return found;
    }
    private void EnsureOpen(SerialScaleSettings settings){if(serial?.IsOpen==true&&active==settings)return;Reset();serial=Create(settings);serial.Open();active=settings;recent.Clear();}
    private static SerialPort Create(SerialScaleSettings x)=>new(x.Port,x.BaudRate,x.Parity,x.DataBits,x.StopBits){Encoding=Encoding.ASCII,ReadTimeout=150,WriteTimeout=150,Handshake=Handshake.None,DtrEnable=false,RtsEnable=false,NewLine="\r"};
    private static async Task<string> ReceiveAsync(SerialPort port,TimeSpan timeout,CancellationToken ct){var until=DateTime.UtcNow+timeout;var buffer=new StringBuilder();while(DateTime.UtcNow<until){ct.ThrowIfCancellationRequested();var chunk=port.ReadExisting();if(chunk.Length>0){buffer.Append(chunk);if(chunk.Contains('\r')||chunk.Contains('\n'))break;}await Task.Delay(35,ct);}if(buffer.Length==0)throw new TimeoutException("sin respuesta; revisa cable, puerto, baud rate y que ninguna otra aplicación tenga abierto el COM");return buffer.ToString();}
    private static string Safe(string raw){var text=string.Concat(raw.Take(120).Select(c=>char.IsControl(c)?$"<{(int)c:X2}>":c.ToString()));return text;}
    private void Reset(){try{serial?.Close();}catch{ }serial?.Dispose();serial=null;active=null;recent.Clear();}
    public void Dispose(){Reset();gate.Dispose();}
}
