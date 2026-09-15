using System.Net.Sockets;
using System.Text;

namespace PuntoDeVentaAtlas.Web.Integrations;

public sealed class RawTcpEscPosPrinter(string host,int port=9100) : ITicketPrinter
{
    public async Task PrintAsync(string document,CancellationToken ct)
    {
        using var client=new TcpClient();using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(8));await client.ConnectAsync(host,port,timeout.Token);await using var stream=client.GetStream();
        var text=Encoding.Latin1.GetBytes(document.Replace("\r\n","\n"));
        byte[] start=[0x1b,0x40],feedAndCut=[0x0a,0x0a,0x0a,0x1d,0x56,0x00];await stream.WriteAsync(start,ct);await stream.WriteAsync(text,ct);await stream.WriteAsync(feedAndCut,ct);await stream.FlushAsync(ct);
    }
}

public static class PrinterDiagnostic
{
    public static string Ticket(string provider)=>
        $"ATLAS POS\nPRUEBA DE IMPRESORA\nProveedor: {provider}\nFecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\nSi puede leer esto, la impresora esta lista.\n";

    public static string Zpl(string provider)=>
        $"^XA^PW600^LL320^FO40,35^A0N,42,42^FDATLAS POS^FS^FO40,100^A0N,28,28^FDPRUEBA DE IMPRESORA^FS^FO40,150^A0N,22,22^FD{provider.Replace("^","").Replace("~","")}^FS^FO40,195^A0N,22,22^FD{DateTime.Now:yyyy-MM-dd HH:mm:ss}^FS^XZ";
}

public sealed class RawTcpZplPrinter(string host,int port=9100) : ITicketPrinter
{
    public async Task PrintAsync(string zplDocument,CancellationToken ct)
    {
        if(!zplDocument.TrimStart().StartsWith("^XA",StringComparison.Ordinal))throw new InvalidOperationException("El documento Zebra debe ser ZPL y comenzar con ^XA.");using var client=new TcpClient();using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(8));await client.ConnectAsync(host,port,timeout.Token);await using var stream=client.GetStream();await stream.WriteAsync(Encoding.UTF8.GetBytes(zplDocument),ct);await stream.FlushAsync(ct);
    }
}

public static class RawTcpCashDrawer
{
    public static async Task OpenAsync(string host,int port=9100,CancellationToken ct=default)
    {
        using var client=new TcpClient();using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(8));await client.ConnectAsync(host,port,timeout.Token);await using var stream=client.GetStream();byte[] pulse=[0x1b,0x40,0x1b,0x70,0x00,0x19,0xfa];await stream.WriteAsync(pulse,ct);await stream.FlushAsync(ct);
    }
}
