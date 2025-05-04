using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace VortexDecrypt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static byte[] aesKey = null;

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string PrivateKey = "<RSAKeyValue><Modulus>27MHYRVoyzJJG8gMI+YRNXWd1nexs2xhzf/ERILBTO1ZvSXOLciv3XgVb4QCzFAbi+zTE9xeGuzSTx7I+gdw0urmjbVOpXmXIGe6LpFfjSHkvtaTRMZjv5VuXAvRckAn4+51NUPHqbPp590eyu1VC8qm31HP56V6nz13yJLAZprsLL7NY7nA5SVHnQIG1m/T20iPJzOhnPCO4x0Dvdyi7SrphWtvx1Qs88P+p0yhgqPTZqpB4CMOusHu7eyQlvrpRFqn/6LqDVneNHa73OQvdZS0XzNCrzTxcwlThs+NBRkWcr9KlwtEUXwT5nv7pCxW+Yy+ohryBTYBG3aCwUycnaaBe9L/4xqiaoc1oT48Kn3BKoUa9BDjVuo4dEmkoTFr8WUTZSTr3YRz3P9j9B+CMZDtA4FpnKAjvXAGS9Vy1hwa/MJIfvnXAQyuirr1BM3YyTGoC44KabPnydG+1TrrhVA1kGSohXYtmGsQKC2LO0NWfOADZAdh7OkAPxlm7UtFeWr6j1mxxmW55fMXGnUA0LGMPJoDhcU/aIOYO8v/fSRbgEvTYDelmnd9g3kCV/o187Km/Yd3wBHKf6uIz8BPGMXW5nOiXas0kcpjGCbUVlHsL9BkQt8vANBE+JpvA0eO8Mfa3QX409Pgtx6RtkT74TqkbX2g1yQQsg7FMjnhchU=</Modulus><Exponent>AQAB</Exponent><P>5P9B85zbD6jVgF2y07hRH3Fq/rovVUuk1l+e1uKF5Sefy5MOzS8KAuQj25oNZdE/XNgZEbUb+dl/YsnwQAYIppTOb42p009IhwfYghq5sI0WKCaRcciSRiIrVaAMZFcrjIoLCSm/fB2Kpvuk+FAaNbEHMaf69pmaP8OPqQNKoJXqFd5lmH2AHcCHrHRd737+lCkhqxQ6yyXGmqqXklcYH0zI7QUsnlI+B8ut3Uccbfqlz4Hj87P1hzWSkBP0FQZM/Zmlcm6rtVITxbrEnIIyb95IWOZQfwI03G07CcqQc2Yccjs6r56pW84bzKYN8FbvbwB+1GRjJZ/wvSkRYhzUxw==</P><Q>9ZsZY5VmFBMv6OWPLMM+QUcfmgT+6yP4OVfdpavElN3pvuTTHKYKldy1RRX5M6kdhVPi7UtAd65m/w5xPcM8VqbWgdES7fDex+OE9Tf91JWoSU+8OFtWODR7Jy82DvDZpy1bZgP4qRyCf5Fz6JJtxpsEmyJbF/LHqQzy+7B0rRdo3g3orw0vgCgyJQb6z1A3xLtn1ed0rtaQk62OGf5fzrQ/Tk4pCp0vZrcuBKVJNRY0gKtHTZbFvCcyWvas3NqMPe9mYV0hFpOCJAsTje5rK8zxhb8S9zEeFT8kpo4tXeRmYwWEbaKnt0wAcLFnZzZlRkli17HYaL73iQdWQxYuQw==</Q><DP>lmgFxGFhDI8C9BYWz7K/LZzL/tUBI8/US61wYYlVEmcNMKO9VA0yUSXRW3p6lEZ0jKGvDY9b+aXeYl9qelK8OdWBJYnxYkYvx+jiTsoI7qosRGDYpNhtIr1sowfFO955TwyYJQOTroyktfqQpzvNizhkFjxTvMa0pm7nG8Z6rLDqmESDjD3Z+TCsrBueWyCZS20cnQZje2yrXojvlwG3aU+ApRUB8lboQSyJXM5JIP9BCuMweq/Xc2A/jHxek7SYuvTA00FXbHelXvjGDFoDkLB56lITyugDGkhwP4UZjYIi6vB2IkVezVq5rZffjd071DeKFix9Zof/ke64aS2wnw==</DP><DQ>O7XaZVCEAatsgWWV+l75OquM7kf6pbYX+fD3rPhmkX6l/kfpX1SqzCZdV49xB3M8/xYW3HPoF3CqD6Y2N7rIZ5SmH+dxf7GIp/YTc/6J0m4T/MbKzrS98Gop6qvaq6U4dSFc0B91C8AnpdX55Pf8gkVbyE+A8ACRM/M2+6O5qf8/+oRNJonBG5oqHsBkp5QsCsM5ClL8FimJ+q0+F+m05y8BGQt9hNwUExfDs1Nvyd09JG7/pchbCFRLNLw16HelksUqEc+vZKq2AAdFWMM3AO4RZd+/P6OMWcQBKBD2zJXjon1L5c+ekLyEeuFWb0xVMNJUmnXdVBHhYkoLWcIBuw==</DQ><InverseQ>qW+cLnRF7NCn3sgzBbAm5wQ3pmiXhGDcbiTNOW1zIS8sayx/gUZBDj48HX2vKyu0bmcSYg2eDj87raBv0VxS0xFoF50hATzV3lrSWVef+3NDAKsYG2xXgxrsamVJ9jrnZdiaP9MXatJ+1H94N4kdBiECjEpC7WH0+NE/XeBSdmkMyXB6vSnhFaLkMx9uHnCaywmgDKYNYzg9ngA+RwjFSdm3mkntOVhFHOLqe0dVwaWOfwQLU9Ti+qLV4gifQC6xHfgFVGZQbb7hUwyIlg5v3Y9V/kNU0isaVU+DG66qZqsBGjEBL356LoIYU83YLxcdPelehl/t1Nrlxjc6zPnQaA==</InverseQ><D>HTtk7/X5S5AVSi0D8ILcJ1DO/4pNiqIRIWe2pacAMUwjdJdjJ5RcUgt4Tq4x8R13jNal/y6DFvILyZs/AoWa7XVz18cX/8x930Ht8+RD4nNwDDRgw/Xhr8wiMv0fkSQYKqu/zKaIGnxl0wQI5NdPSZJdQbmRADF0b1uuZHGiSAS1B/8bNPmyQMsFDE8Iud+HQNYWthCYoFkhqfZiWRDSXNwo1ifFZ7A7ivO5HmmD0+4YYsCkoz4G57KGKAIh+vTHtOCbGdiDa+NZPYHO3UU/S1XN6hzX8pgIyFWlV3qmONdttTbepdpJ7M2lmDVG9kEwEWQ4uTj8mtTksNLD8xROgixPRHM0mShQ34NsjF+bEQh77Ut1qoxh2YjJQC95mXAtoAPmTT+/eFxkJU+sgR4kTJgz9tvTRUbuVV79nQCs/zIsZXLWp5EAAVjOb2dgYQgbsLKB69I3CVonRZw3hEAr8Q/ZeZvACzGKNi2XgqGgkLoTGZiFLua9AxGzSE/+x0/Gvtd4fg/8Q3uyyTxvjQbA8th498uMz2aUNeox7L+pz6e7wd3wK3dxpexb7ZCcyvHtbcM8kk3kD4+n6gFSyMeOw8WTNlOuKPiNedYZFjpuylT1r8PKsc82LZxAkZgT/ooN8NMmznP4dt1Pj6wIJDtzSh80PQZUev8f+MlHguzliME=</D></RSAKeyValue>";
                using (RSA rSA = new RSACng(4096))
                {
                    rSA.FromXmlString(PrivateKey);
                    aesKey = rSA.Decrypt(Convert.FromBase64String(richTextBox1.Text.Trim()), RSAEncryptionPadding.OaepSHA512);
                    textBox1.Text = Convert.ToBase64String(aesKey);
                }
            }
            catch (Exception ex)
            {
                // 弹出错误消息框
                MessageBox.Show("发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(textBox1.Text.Trim());
            }
            catch (Exception)
            {

            }
        }
    }
}
