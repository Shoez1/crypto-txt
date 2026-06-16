using System;
using System.IO;
using System.Windows.Forms;

namespace CryptoTxt
{
    public partial class MainForm : Form
    {
        private const string PlainTextExtension = ".txt";
        private const string EncryptedTextExtension = ".txt.enc";

        public MainForm()
        {
            InitializeComponent();
            Text = $"CryptoTxt - v{AppInfo.Version}";
        }

        private static bool IsPlainTextFile(string path)
        {
            return path.EndsWith(PlainTextExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEncryptedTextFile(string path)
        {
            return path.EndsWith(EncryptedTextExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedTxtPath(string path)
        {
            return File.Exists(path) && (IsPlainTextFile(path) || IsEncryptedTextFile(path));
        }

        private static void ShowTxtOnlyWarning()
        {
            MessageBox.Show(
                "Este aplicativo trabalha somente com arquivos .txt e .txt.enc.",
                "CryptoTxt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void SetSelectedPath(string path)
        {
            if (!IsSupportedTxtPath(path))
            {
                ShowTxtOnlyWarning();
                return;
            }

            txtFilePath.Text = path;
        }

        private static string GetAvailablePath(string desiredPath)
        {
            if (!File.Exists(desiredPath))
            {
                return desiredPath;
            }

            string directory = Path.GetDirectoryName(desiredPath) ?? string.Empty;
            string fileName = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);

            for (int index = 1; ; index++)
            {
                string candidate = Path.Combine(directory, $"{fileName} ({index}){extension}");
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        private void btnSelectFile_Click(object? sender, EventArgs e)
        {
            SelectFilePath();
        }

        private void SelectFilePath()
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos CryptoTxt (*.txt;*.txt.enc)|*.txt;*.txt.enc",
                Title = "Selecionar arquivo .txt ou .txt.enc"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetSelectedPath(openFileDialog.FileName);
            }
        }

        private void btnEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath?.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt válido.");
                return;
            }

            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt válido.");
                return;
            }

            if (!IsPlainTextFile(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt para criptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string plainText = File.ReadAllText(txtFilePath.Text);
                string encrypted = Utils.CryptoUtils.Encrypt(plainText);
                string outputPath = GetAvailablePath(txtFilePath.Text + ".enc");
                File.WriteAllText(outputPath, encrypted);
                MessageBox.Show($"Arquivo .txt criptografado com sucesso!\nSalvo como: {outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criptografar: {ex.Message}");
            }
        }

        private void btnDecrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath?.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc válido.");
                return;
            }

            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc válido.");
                return;
            }

            if (!IsEncryptedTextFile(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc para descriptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string plainText;
                try
                {
                    string encrypted = File.ReadAllText(txtFilePath.Text);
                    plainText = Utils.CryptoUtils.Decrypt(encrypted);
                }
                catch
                {
                    MessageBox.Show("Não foi possível descriptografar. A chave pode estar incorreta ou o arquivo pode ter sido alterado.", "Erro de Chave", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string outputPath = txtFilePath.Text.Substring(0, txtFilePath.Text.Length - ".enc".Length);
                outputPath = GetAvailablePath(outputPath);
                File.WriteAllText(outputPath, plainText);
                MessageBox.Show($"Arquivo .txt descriptografado com sucesso!\nSalvo como: {outputPath}");
            }
            catch (Exception)
            {
                MessageBox.Show($"Erro ao descriptografar {txtFilePath.Text}.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreview_Click(object? sender, EventArgs e)
        {
            if (txtFilePath?.Text == null || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc válido.");
                return;
            }

            VisualizarArquivoSelecionado(txtFilePath.Text);
        }

        private void btnExportKey_Click(object? sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivo de Chave (*.txt)|*.txt",
                Title = "Exportar Chave de Criptografia",
                FileName = "chave_crypto.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Utils.CryptoUtils.ExportKeyAndIV(saveFileDialog.FileName);
                    MessageBox.Show($"Chave exportada com sucesso em:\n{saveFileDialog.FileName}", "Exportar Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar chave: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportKey_Click(object? sender, EventArgs e)
        {
            if (Utils.CryptoUtils.IsCustomKeyActive)
            {
                Utils.CryptoUtils.ClearImportedKeyAndIV();
                btnImportKey.Text = "Importar Chave";
                btnImportKey.BackColor = System.Drawing.Color.LightGray;
                MessageBox.Show("Chave ativa desativada. O programa voltou a usar a chave padrão embutida.", "Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivo de Chave (*.txt)|*.txt",
                Title = "Importar Chave de Criptografia"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bool ok = Utils.CryptoUtils.ImportKeyAndIVFromFile(openFileDialog.FileName);
                    if (!ok)
                    {
                        throw new Exception("Arquivo de chave inválido. Use somente o padrão CSK3 exportado por CryptoFotos, CryptoMulti ou CryptoTxt.");
                    }

                    btnImportKey.Text = "Chave Importada";
                    btnImportKey.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                    MessageBox.Show("Chave importada com sucesso! Ela será usada para criptografar/descriptografar arquivos até ser desativada.", "Importar Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao importar chave: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnGenerateKey_Click(object? sender, EventArgs e)
        {
            byte[]? keyBytes = null;
            byte[]? ivBytes = null;

            try
            {
                (keyBytes, ivBytes) = Utils.CryptoUtils.GenerateNewKeyMaterial();
                Utils.CryptoUtils.ImportKeyAndIV(keyBytes, ivBytes);
                btnImportKey.Text = "Chave Gerada";
                btnImportKey.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);

                MessageBox.Show(
                    "Nova chave gerada e carregada. Use Exportar Chave para salvar essa chave em arquivo.",
                    "CryptoTxt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar chave: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Utils.CryptoUtils.ClearSensitiveBytes(keyBytes);
                Utils.CryptoUtils.ClearSensitiveBytes(ivBytes);
            }
        }

        private void txtFilePath_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = e.Data?.GetData(DataFormats.FileDrop) is string[] files
                && files.Length > 0
                && IsSupportedTxtPath(files[0])
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
        }

        private void txtFilePath_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                SetSelectedPath(files[0]);
            }
        }

        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = e.Data?.GetData(DataFormats.FileDrop) is string[] files
                && files.Length > 0
                && IsSupportedTxtPath(files[0])
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
        }

        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                SetSelectedPath(files[0]);
            }
        }

        private void VisualizarArquivoSelecionado(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho) || !IsEncryptedTextFile(caminho))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc para visualizar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string encrypted = File.ReadAllText(caminho);
                string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                ShowPreviewForm(Path.GetFileName(caminho), plainText);
            }
            catch (Exception)
            {
                MessageBox.Show($"Erro ao visualizar {Path.GetFileName(caminho)}: a chave pode estar incorreta ou o arquivo pode ter sido alterado.");
            }
        }

        private void ShowPreviewForm(string fileName, string plainText)
        {
            using var previewForm = new Form
            {
                Text = $"Visualização: {fileName}",
                Width = 600,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                Text = plainText
            };

            previewForm.Controls.Add(textBox);
            previewForm.ShowDialog(this);
        }
    }
}
