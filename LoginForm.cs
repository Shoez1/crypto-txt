using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CryptoTxt
{
    public partial class LoginForm : Form
    {
        private string validUser = null;
        private string validPass = null;

        public LoginForm()
        {
            InitializeComponent();
            LoadLoginInfo();
        }

        private void LoadLoginInfo()
        {
            // Lê login.txt como recurso embutido de forma robusta
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                if (res.EndsWith("login.txt", StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = res;
                    break;
                }
            }
            if (resourceName == null)
            {
                MessageBox.Show("Erro: login.txt não encontrado como recurso embutido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line) && line.Contains(":"))
                {
                    var parts = line.Split(':');
                    validUser = parts[0];
                    validPass = parts[1];
                }
                else
                {
                    MessageBox.Show("Erro: login.txt embutido está vazio ou mal formatado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUser.Text == validUser && txtPass.Text == validPass)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuário ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPass.Text = "";
                txtUser.Focus();
            }
        }
    }
}
