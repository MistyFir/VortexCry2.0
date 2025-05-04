using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSA密钥对生成器
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            using (RSA rSA = new RSACng(4096))
            {
                string publicKey = rSA.ToXmlString(false);
                string privateKey = rSA.ToXmlString(true);
                File.WriteAllText("publicKey.xml", publicKey);
                File.WriteAllText("privateKey.xml", privateKey);
            }
            button1.Enabled = true;
        }
    }
}
