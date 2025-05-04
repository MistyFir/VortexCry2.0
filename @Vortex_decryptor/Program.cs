using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace _Vortex_decryptor
{
    internal static class Program
    {
        public static string keyHash256;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //声明全局异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            try
            {
                if (!(args is null))
                {
                    if (args.Length >= 1)
                    {
                        keyHash256 = args[0];
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
                Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                // 检查进程数是否大于1，如果打开两个就退出一个
                if (processes.Length > 1)
                {
                    Environment.Exit(0);
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // 处理全局异常
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        { }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        { }
    }
}
