using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PuntoDeVentaAtlas.Web.Integrations;

public static class WindowsRawPrinter
{
    public static void Print(string printerName,string document)
        =>Send(printerName,BuildEscPos(document),"Atlas POS - ticket");

    public static void OpenDrawer(string printerName)
        =>Send(printerName,[0x1b,0x40,0x1b,0x70,0x00,0x19,0xfa],"Atlas POS - abrir cajón");

    private static void Send(string printerName,byte[] bytes,string documentName)
    {
        if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException("La cola de impresión local sólo está disponible en Windows.");
        if(string.IsNullOrWhiteSpace(printerName))throw new InvalidOperationException("Selecciona el nombre de la impresora instalada en Windows.");
        if(!OpenPrinter(printerName,out var handle,IntPtr.Zero))throw Error("No se pudo abrir la impresora de Windows");
        try
        {
            var info=new DocInfo{DocumentName=documentName,DataType="RAW"};
            if(StartDocPrinter(handle,1,info)==0)throw Error("Windows no aceptó el documento");
            try
            {
                if(!StartPagePrinter(handle))throw Error("Windows no pudo iniciar la página");
                try{if(!WritePrinter(handle,bytes,bytes.Length,out var written)||written!=bytes.Length)throw Error("La cola no recibió todo el ticket");}
                finally{EndPagePrinter(handle);}
            }
            finally{EndDocPrinter(handle);}
        }
        finally{ClosePrinter(handle);}
    }

    private static byte[] BuildEscPos(string document)
    {
        var text=Encoding.Latin1.GetBytes(document.Replace("\r\n","\n"));
        return [0x1b,0x40,..text,0x0a,0x0a,0x0a,0x1d,0x56,0x00];
    }
    private static Win32Exception Error(string message)=>new(Marshal.GetLastWin32Error(),message);

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    private sealed class DocInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string DocumentName="";
        [MarshalAs(UnmanagedType.LPWStr)] public string? OutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string DataType="RAW";
    }
    [DllImport("winspool.drv",SetLastError=true,CharSet=CharSet.Unicode)] private static extern bool OpenPrinter(string name,out IntPtr printer,IntPtr defaults);
    [DllImport("winspool.drv",SetLastError=true)] private static extern bool ClosePrinter(IntPtr printer);
    [DllImport("winspool.drv",SetLastError=true,CharSet=CharSet.Unicode)] private static extern int StartDocPrinter(IntPtr printer,int level,[In] DocInfo info);
    [DllImport("winspool.drv",SetLastError=true)] private static extern bool EndDocPrinter(IntPtr printer);
    [DllImport("winspool.drv",SetLastError=true)] private static extern bool StartPagePrinter(IntPtr printer);
    [DllImport("winspool.drv",SetLastError=true)] private static extern bool EndPagePrinter(IntPtr printer);
    [DllImport("winspool.drv",SetLastError=true)] private static extern bool WritePrinter(IntPtr printer,byte[] bytes,int count,out int written);
}
