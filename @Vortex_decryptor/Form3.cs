using System;
using System.IO;
using System.Windows.Forms;
using VortexSecOps.AesRsaCipherMix;

namespace _Vortex_decryptor
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = Convert.ToBase64String(File.ReadAllBytes(AesRsaEncryptionManager.AES_KEY_ENCRYPTED_FILE_PATH));
        }
    }
}
