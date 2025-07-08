using System;
using System.IO;
using System.Windows.Forms;

namespace CryptoTxt
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Arquivos de Texto ou Criptografados (*.txt;*.enc)|*.txt;*.enc|Todos os arquivos (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                }
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo válido.");
                return;
            }
            try
            {
                string plainText = File.ReadAllText(txtFilePath.Text);
                string encrypted = Utils.CryptoUtils.Encrypt(plainText);
                File.WriteAllText(txtFilePath.Text + ".enc", encrypted);
                MessageBox.Show("Arquivo criptografado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criptografar: {ex.Message}");
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo válido.");
                return;
            }
            if (!txtFilePath.Text.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Selecione um arquivo .enc para descriptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string encrypted = File.ReadAllText(txtFilePath.Text);
                string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                string originalPath = txtFilePath.Text;
                string outputPath;
                if (originalPath.EndsWith(".txt.enc", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = originalPath.Substring(0, originalPath.Length - 8) + ".decrypted.txt";
                }
                else if (originalPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = originalPath.Substring(0, originalPath.Length - 4) + ".decrypted.txt";
                }
                else
                {
                    outputPath = originalPath + ".decrypted.txt";
                }
                File.WriteAllText(outputPath, plainText);
                MessageBox.Show($"Arquivo descriptografado com sucesso!\nSalvo como: {outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao descriptografar: {ex.Message}");
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo válido.");
                return;
            }
            if (!txtFilePath.Text.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Selecione um arquivo .enc para visualizar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string encrypted = File.ReadAllText(txtFilePath.Text);
                string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                using (var previewForm = new Form())
                {
                    previewForm.Text = "Visualização do Conteúdo";
                    previewForm.Width = 600;
                    previewForm.Height = 500;
                    var textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.ReadOnly = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Dock = DockStyle.Fill;
                    textBox.Font = new System.Drawing.Font("Consolas", 10);
                    textBox.Text = plainText;
                    previewForm.Controls.Add(textBox);
                    previewForm.StartPosition = FormStartPosition.CenterParent;
                    previewForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao visualizar: {ex.Message}");
            }
        }

        private void txtFilePath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void txtFilePath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    txtFilePath.Text = files[0];
                }
            }
        }
    }
}
