using System;
using System.IO;
using System.Linq;
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
            UpdateKeyStatusUI();
        }

        private static bool IsPlainTextFile(string path)
        {
            return path.EndsWith(PlainTextExtension, StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(EncryptedTextExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEncryptedTextFile(string path)
        {
            return path.EndsWith(EncryptedTextExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedTxtPath(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }

            return File.Exists(path) && (IsPlainTextFile(path) || IsEncryptedTextFile(path));
        }

        private static void ShowTxtOnlyWarning()
        {
            MessageBox.Show(
                "Este aplicativo trabalha somente com arquivos .txt, .txt.enc e pastas contendo esses arquivos.",
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

        private void UpdateKeyStatusUI()
        {
            if (Utils.CryptoUtils.IsCustomKeyActive)
            {
                lblKeyStatus.Text = "Chave ativa: Chave Personalizada (CSK3)";
                lblKeyStatus.ForeColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnImportKey.Text = "Chave Ativa";
                btnImportKey.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnImportKey.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                lblKeyStatus.Text = "Chave ativa: Chave Padrão (Compartilhada)";
                lblKeyStatus.ForeColor = System.Drawing.Color.DimGray;
                btnImportKey.Text = "Importar Chave";
                btnImportKey.BackColor = System.Drawing.Color.LightGray;
                btnImportKey.ForeColor = System.Drawing.Color.Black;
            }
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

        private void btnSelectFolder_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Selecione uma pasta contendo arquivos .txt ou .txt.enc",
                UseDescriptionForTitle = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                SetSelectedPath(folderDialog.SelectedPath);
            }
        }

        private void btnEncrypt_Click(object? sender, EventArgs e)
        {
            string path = txtFilePath?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Selecione um arquivo .txt ou uma pasta válida.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Directory.Exists(path))
            {
                EncryptDirectory(path);
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show("O arquivo selecionado não foi encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsPlainTextFile(path))
            {
                MessageBox.Show("Selecione um arquivo .txt para criptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string outputPath = GetAvailablePath(path + ".enc");
                Utils.CryptoUtils.EncryptFile(path, outputPath);
                MessageBox.Show($"Arquivo criptografado com sucesso!\n\nSalvo como: {outputPath}", "CryptoTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criptografar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EncryptDirectory(string directoryPath)
        {
            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(EncryptedTextExtension, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (files.Length == 0)
                {
                    MessageBox.Show("Nenhum arquivo .txt encontrado na pasta selecionada.", "CryptoTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int success = 0;
                int failed = 0;

                foreach (string file in files)
                {
                    try
                    {
                        string outputPath = GetAvailablePath(file + ".enc");
                        Utils.CryptoUtils.EncryptFile(file, outputPath);
                        success++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                MessageBox.Show(
                    $"Criptografia em lote concluída!\n\nArquivos criptografados: {success}\nFalhas: {failed}",
                    "CryptoTxt",
                    MessageBoxButtons.OK,
                    failed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao ler pasta: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDecrypt_Click(object? sender, EventArgs e)
        {
            string path = txtFilePath?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc ou uma pasta válida.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Directory.Exists(path))
            {
                DecryptDirectory(path);
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show("O arquivo selecionado não foi encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsEncryptedTextFile(path))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc para descriptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string outputPath = path.Substring(0, path.Length - ".enc".Length);
                outputPath = GetAvailablePath(outputPath);
                Utils.CryptoUtils.DecryptFile(path, outputPath);
                MessageBox.Show($"Arquivo descriptografado com sucesso!\n\nSalvo como: {outputPath}", "CryptoTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Não foi possível descriptografar. A chave pode estar incorreta ou o arquivo pode ter sido alterado.", "Erro de Chave", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DecryptDirectory(string directoryPath)
        {
            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*.txt.enc", SearchOption.AllDirectories);

                if (files.Length == 0)
                {
                    MessageBox.Show("Nenhum arquivo .txt.enc encontrado na pasta selecionada.", "CryptoTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int success = 0;
                int failed = 0;

                foreach (string file in files)
                {
                    try
                    {
                        string outputPath = file.Substring(0, file.Length - ".enc".Length);
                        outputPath = GetAvailablePath(outputPath);
                        Utils.CryptoUtils.DecryptFile(file, outputPath);
                        success++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                MessageBox.Show(
                    $"Descriptografia em lote concluída!\n\nArquivos descriptografados: {success}\nFalhas: {failed}",
                    "CryptoTxt",
                    MessageBoxButtons.OK,
                    failed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao ler pasta: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreview_Click(object? sender, EventArgs e)
        {
            string path = txtFilePath?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || !IsEncryptedTextFile(path))
            {
                MessageBox.Show("Selecione um arquivo .txt.enc válido para visualizar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            VisualizarArquivoSelecionado(path);
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
                UpdateKeyStatusUI();
                MessageBox.Show("Chave personalizada desativada. O programa voltou a usar a chave padrão embutida.", "Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                    UpdateKeyStatusUI();
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
                UpdateKeyStatusUI();

                MessageBox.Show(
                    "Nova chave gerada e carregada com sucesso!\n\nLembre-se de clicar em 'Exportar Chave' para salvar o arquivo de chave antes de fechar o programa.",
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
            try
            {
                string encrypted = File.ReadAllText(caminho).Trim();
                string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                ShowPreviewForm(Path.GetFileName(caminho), plainText);
            }
            catch (Exception)
            {
                MessageBox.Show($"Erro ao visualizar {Path.GetFileName(caminho)}: a chave pode estar incorreta ou o arquivo pode ter sido alterado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPreviewForm(string fileName, string plainText)
        {
            using var previewForm = new Form
            {
                Text = $"Visualização: {fileName}",
                Width = 650,
                Height = 520,
                StartPosition = FormStartPosition.CenterParent,
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(8, 4, 8, 4)
            };

            int lineCount = plainText.Length == 0 ? 0 : plainText.Split('\n').Length;
            var lblInfo = new Label
            {
                Text = $"Linhas: {lineCount} | Caracteres: {plainText.Length}",
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 300,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.DimGray
            };

            var btnCopy = new Button
            {
                Text = "Copiar Conteúdo",
                Dock = DockStyle.Right,
                Width = 140,
                Height = 28
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(plainText.Length > 0 ? plainText : " ");
                MessageBox.Show("Conteúdo copiado para a área de transferência!", "CryptoTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlTop.Controls.Add(lblInfo);
            pnlTop.Controls.Add(btnCopy);

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                Text = plainText,
                WordWrap = false
            };

            previewForm.Controls.Add(textBox);
            previewForm.Controls.Add(pnlTop);
            previewForm.ShowDialog(this);
        }
    }
}
