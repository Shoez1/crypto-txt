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
            this.Text = $"CryptoTxt - v{AppInfo.Version}";
            if (this.Controls["lstArquivos"] is ListBox lst)
                lst.DoubleClick += lstArquivos_DoubleClick;
        }

        private void AtualizarListaArquivosPasta(string pastaOuArquivo)
        {
            if (this.Controls["lstArquivos"] is ListBox lst)
            {
                lst.Items.Clear();
                if (!string.IsNullOrWhiteSpace(pastaOuArquivo))
                {
                    if (Directory.Exists(pastaOuArquivo))
                    {
                        var arquivos = Directory.GetFiles(pastaOuArquivo, "*.enc", SearchOption.TopDirectoryOnly);
                        foreach (var arq in arquivos)
                            lst.Items.Add(Path.GetFileName(arq));
                    }
                    else if (File.Exists(pastaOuArquivo) && pastaOuArquivo.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                    {
                        lst.Items.Add(Path.GetFileName(pastaOuArquivo));
                    }
                }
            }
        }

        private static string GetAvailablePath(string desiredPath)
        {
            if (!File.Exists(desiredPath))
                return desiredPath;

            string directory = Path.GetDirectoryName(desiredPath) ?? string.Empty;
            string fileName = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);

            for (int index = 1; ; index++)
            {
                string candidate = Path.Combine(directory, $"{fileName} ({index}){extension}");
                if (!File.Exists(candidate))
                    return candidate;
            }
        }

        private void btnSelectFile_Click(object? sender, EventArgs e)
        {
            using (var selectType = new SelectTypeForm())
            {
                var result = selectType.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // Selecionar arquivo
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Arquivos de Texto ou Criptografados (*.txt;*.enc)|*.txt;*.enc|Todos os arquivos (*.*)|*.*";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            txtFilePath.Text = ofd.FileName;
                            AtualizarListaArquivosPasta(ofd.FileName);
                        }
                    }
                }
                else if (result == DialogResult.Yes)
                {
                    // Selecionar pasta
                    using (var fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtFilePath.Text = fbd.SelectedPath;
                            AtualizarListaArquivosPasta(fbd.SelectedPath);
                        }
                    }
                }
                // Se Cancelar, não faz nada
            }
        }

        private void btnEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath?.Text))
            {
                MessageBox.Show("Selecione um arquivo ou pasta válida.");
                return;
            }
            if (File.Exists(txtFilePath.Text))
            {
                // Criptografar arquivo único
                try
                {
                    string plainText = File.ReadAllText(txtFilePath.Text);
                    string encrypted = Utils.CryptoUtils.Encrypt(plainText);
                    string outputPath = GetAvailablePath(txtFilePath.Text + ".enc");
                    File.WriteAllText(outputPath, encrypted);
                    MessageBox.Show($"Arquivo criptografado com sucesso!\nSalvo como: {outputPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criptografar: {ex.Message}");
                }
            }
            else if (Directory.Exists(txtFilePath.Text))
            {
                // Criptografar todos os .txt da pasta
                try
                {
                    var files = Directory.GetFiles(txtFilePath.Text, "*.txt", SearchOption.TopDirectoryOnly);
                    if (files.Length == 0)
                    {
                        MessageBox.Show("Nenhum arquivo .txt encontrado na pasta selecionada.");
                        return;
                    }
                    int count = 0;
                    foreach (var file in files)
                    {
                        string plainText = File.ReadAllText(file);
                        string encrypted = Utils.CryptoUtils.Encrypt(plainText);
                        File.WriteAllText(GetAvailablePath(file + ".enc"), encrypted);
                        count++;
                    }
                    MessageBox.Show($"{count} arquivos .txt criptografados com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criptografar arquivos da pasta: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Selecione um arquivo ou pasta válida.");
            }
        }

        private void btnDecrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath?.Text))
            {
                MessageBox.Show("Selecione um arquivo ou pasta válida.");
                return;
            }
            if (File.Exists(txtFilePath.Text))
            {
                // Descriptografar arquivo único
                if (!txtFilePath.Text.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Selecione um arquivo .enc para descriptografar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string originalPath = txtFilePath.Text;
                    string outputPath;
                    if (originalPath.EndsWith(".txt.enc", StringComparison.OrdinalIgnoreCase))
                    {
                        outputPath = originalPath.Substring(0, originalPath.Length - 4); // Remove apenas .enc
                    }
                    else if (originalPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                    {
                        outputPath = originalPath.Substring(0, originalPath.Length - 4); // Remove apenas .enc
                    }
                    else
                    {
                        outputPath = originalPath + ".txt";
                    }
                    outputPath = GetAvailablePath(outputPath);
                    File.WriteAllText(outputPath, plainText);
                    MessageBox.Show($"Arquivo descriptografado com sucesso!\nSalvo como: {outputPath}");
                }
                catch (Exception)
                {
                    MessageBox.Show($"Erro ao descriptografar {txtFilePath.Text}.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (Directory.Exists(txtFilePath.Text))
            {
                // Descriptografar todos os .enc da pasta para uma nova pasta com sufixo _dec
                try
                {
                    var files = Directory.GetFiles(txtFilePath.Text, "*.enc", SearchOption.TopDirectoryOnly);
                    if (files.Length == 0)
                    {
                        MessageBox.Show("Nenhum arquivo .enc encontrado na pasta selecionada.");
                        return;
                    }
                    string pastaOrigem = txtFilePath.Text;
                    string nomePasta = Path.GetFileName(pastaOrigem.TrimEnd(Path.DirectorySeparatorChar));
                    string dirPai = Path.GetDirectoryName(pastaOrigem.TrimEnd(Path.DirectorySeparatorChar)) ?? string.Empty;
                    string pastaDestino = Path.Combine(dirPai, nomePasta + "_dec");
                    Directory.CreateDirectory(pastaDestino);
                    int count = 0;
                    int erros = 0;
                    foreach (var file in files)
                    {
                        try
                        {
                            string encrypted = File.ReadAllText(file);
                            string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                            string nomeArquivo = Path.GetFileNameWithoutExtension(file); // remove .enc
                            string outputPath;
                            if (nomeArquivo.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                                outputPath = Path.Combine(pastaDestino, nomeArquivo);
                            else
                                outputPath = Path.Combine(pastaDestino, nomeArquivo + ".txt");
                            File.WriteAllText(GetAvailablePath(outputPath), plainText);
                            count++;
                        }
                        catch
                        {
                            erros++;
                        }
                    }
                    string msg = $"{count} arquivos .enc descriptografados com sucesso!\nSalvos em: {pastaDestino}";
                    if (erros > 0)
                        msg += $"\n{erros} arquivos não puderam ser descriptografados (chave incorreta).";
                    MessageBox.Show(msg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao descriptografar arquivos da pasta: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Selecione um arquivo ou pasta válida.");
            }
        }

        private void btnPreview_Click(object? sender, EventArgs e)
        {
            if (txtFilePath?.Text == null || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Selecione um arquivo válido.");
                return;
            }
            if (txtFilePath?.Text == null || !txtFilePath.Text.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
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
            catch (Exception)
            {
                MessageBox.Show($"Erro ao visualizar {txtFilePath.Text}: a chave pode estar incorreta ou o arquivo pode ter sido alterado.");
            }
        }

        private void btnExportKey_Click(object? sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Arquivo de Chave (*.txt)|*.txt";
            sfd.Title = "Exportar Chave de Criptografia";
            sfd.FileName = "chave_crypto.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Utils.CryptoUtils.ExportKeyAndIV(sfd.FileName);
                    MessageBox.Show("Chave exportada com sucesso. Guarde esse arquivo em local seguro; ele não será exibido na tela.", "Exportar Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar chave: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportKey_Click(object? sender, EventArgs e)
        {
            // Se já está importada, desativa
            if (Utils.CryptoUtils.IsCustomKeyActive)
            {
                Utils.CryptoUtils.ClearImportedKeyAndIV();
                btnImportKey.Text = "Importar Chave";
                btnImportKey.BackColor = System.Drawing.Color.LightGray;
                MessageBox.Show("Chave importada desativada. O programa voltou a usar a chave local protegida do usuário atual.", "Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var ofd = new OpenFileDialog();
            ofd.Filter = "Arquivo de Chave (*.txt)|*.txt";
            ofd.Title = "Importar Chave de Criptografia";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bool ok = Utils.CryptoUtils.ImportKeyAndIVFromFile(ofd.FileName);
                    if (!ok)
                        throw new Exception("Arquivo de chave inválido. Deve conter 48 bytes (32 da chave + 16 do IV) ou estar em Base64.");
                    btnImportKey.Text = "Chave Importada";
                    btnImportKey.BackColor = System.Drawing.Color.FromArgb(40, 167, 69); // Verde estilo iPhone
                    MessageBox.Show("Chave importada com sucesso! Ela será usada para criptografar/descriptografar arquivos até ser desativada.", "Importar Chave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao importar chave: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtFilePath_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void txtFilePath_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                if (txtFilePath != null)
                {
                    txtFilePath.Text = files[0];
                    AtualizarListaArquivosPasta(files[0]);
                }
            }
        }

        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                if (txtFilePath != null)
                {
                    txtFilePath.Text = files[0];
                    AtualizarListaArquivosPasta(files[0]);
                }
            }
        }

        private void lstArquivos_DoubleClick(object? sender, EventArgs e)
        {
            if (this.Controls["lstArquivos"] is ListBox lst && lst.SelectedItem != null)
            {
                string basePath = txtFilePath.Text ?? string.Empty;
                string? nomeArquivo = lst.SelectedItem.ToString();
                if (string.IsNullOrWhiteSpace(nomeArquivo))
                    return;

                string caminhoCompleto = Directory.Exists(basePath)
                    ? Path.Combine(basePath, nomeArquivo)
                    : basePath;
                if (File.Exists(caminhoCompleto) && nomeArquivo.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string plainText;
                        try
                        {
                            string encrypted = File.ReadAllText(caminhoCompleto);
                            plainText = Utils.CryptoUtils.Decrypt(encrypted);
                        }
                        catch
                        {
                            MessageBox.Show("Não foi possível descriptografar. A chave pode estar incorreta ou o arquivo pode ter sido alterado.", "Erro de Chave", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        using (var previewForm = new Form())
                        {
                            previewForm.Text = $"Visualização: {nomeArquivo}";
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
                    catch (Exception)
                    {
                        MessageBox.Show($"Erro ao visualizar {nomeArquivo}.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void VisualizarArquivoSelecionado(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho) || !caminho.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                return;
            try
            {
                string encrypted = File.ReadAllText(caminho);
                string plainText = Utils.CryptoUtils.Decrypt(encrypted);
                using (var previewForm = new Form())
                {
                    previewForm.Text = $"Visualização: {Path.GetFileName(caminho)}";
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
            catch (Exception)
            {
                MessageBox.Show($"Erro ao visualizar {Path.GetFileName(caminho)}: a chave pode estar incorreta ou o arquivo pode ter sido alterado.");
            }
        }
    }
}
