using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

internal static class InstallerBootstrap
{
    [STAThread]
    private static int Main(string[] args)
    {
        string script=Environment.GetEnvironmentVariable("ATLAS_INSTALL_SCRIPT")??"Install-AtlasServer.ps1";
        string temp=Path.Combine(Path.GetTempPath(),"AtlasPOS-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
        try
        {
            using(Stream input=Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
            {if(input==null)throw new InvalidOperationException("El instalador no contiene su carga útil.");using(var archive=new ZipArchive(input,ZipArchiveMode.Read)){archive.ExtractToDirectory(temp);}}
            string extra=args.Length>0?" "+String.Join(" ",Array.ConvertAll(args,x=>"\""+x.Replace("\"","\\\"")+"\"")):"";
            var start=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""+Path.Combine(temp,script)+"\""+extra){UseShellExecute=true,Verb="runas",WorkingDirectory=temp};
            int exitCode;using(Process process=Process.Start(start)){process.WaitForExit();exitCode=process.ExitCode;}
            return exitCode;
        }
        catch(Exception ex){System.Windows.Forms.MessageBox.Show(ex.Message,"Atlas POS Installer",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);return 1;}
        finally{try{Directory.Delete(temp,true);}catch{}}
    }

}
