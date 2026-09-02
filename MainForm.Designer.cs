namespace CryptoTxt
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Label lblKeyStatus;
        private System.Windows.Forms.Button btnExportKey;
        private System.Windows.Forms.Button btnImportKey;
        private System.Windows.Forms.Button btnGenerateKey;
        private System.Windows.Forms.Label lblSignature;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.lblKeyStatus = new System.Windows.Forms.Label();
            this.btnExportKey = new System.Windows.Forms.Button();
            this.btnImportKey = new System.Windows.Forms.Button();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.lblSignature = new System.Windows.Forms.Label();
            this.txtFilePath.AllowDrop = true;
            this.txtFilePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtFilePath_DragEnter);
            this.txtFilePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtFilePath_DragDrop);
            this.SuspendLayout();
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(20, 20);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.PlaceholderText = "Selecione ou arraste um arquivo ou pasta...";
            this.txtFilePath.Size = new System.Drawing.Size(185, 23);
            this.txtFilePath.TabIndex = 0;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(212, 19);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 25);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "Arquivo...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(292, 19);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(78, 25);
            this.btnSelectFolder.TabIndex = 2;
            this.btnSelectFolder.Text = "Pasta...";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.Location = new System.Drawing.Point(20, 58);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(170, 32);
            this.btnEncrypt.TabIndex = 3;
            this.btnEncrypt.Text = "Criptografar";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Location = new System.Drawing.Point(200, 58);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(170, 32);
            this.btnDecrypt.TabIndex = 4;
            this.btnDecrypt.Text = "Descriptografar";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(20, 98);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(350, 30);
            this.btnPreview.TabIndex = 5;
            this.btnPreview.Text = "Visualizar Conteúdo (.txt.enc)";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // lblKeyStatus
            // 
            this.lblKeyStatus.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblKeyStatus.ForeColor = System.Drawing.Color.DimGray;
            this.lblKeyStatus.Location = new System.Drawing.Point(20, 137);
            this.lblKeyStatus.Name = "lblKeyStatus";
            this.lblKeyStatus.Size = new System.Drawing.Size(350, 16);
            this.lblKeyStatus.TabIndex = 6;
            this.lblKeyStatus.Text = "Chave ativa: Chave Padrão (Compartilhada)";
            this.lblKeyStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExportKey
            // 
            this.btnExportKey.Location = new System.Drawing.Point(20, 158);
            this.btnExportKey.Name = "btnExportKey";
            this.btnExportKey.Size = new System.Drawing.Size(110, 26);
            this.btnExportKey.TabIndex = 7;
            this.btnExportKey.Text = "Exportar Chave";
            this.btnExportKey.UseVisualStyleBackColor = true;
            this.btnExportKey.Click += new System.EventHandler(this.btnExportKey_Click);
            // 
            // btnImportKey
            // 
            this.btnImportKey.BackColor = System.Drawing.Color.LightGray;
            this.btnImportKey.Location = new System.Drawing.Point(135, 158);
            this.btnImportKey.Name = "btnImportKey";
            this.btnImportKey.Size = new System.Drawing.Size(120, 26);
            this.btnImportKey.TabIndex = 8;
            this.btnImportKey.Text = "Importar Chave";
            this.btnImportKey.UseVisualStyleBackColor = true;
            this.btnImportKey.Click += new System.EventHandler(this.btnImportKey_Click);
            // 
            // btnGenerateKey
            // 
            this.btnGenerateKey.Location = new System.Drawing.Point(260, 158);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(110, 26);
            this.btnGenerateKey.TabIndex = 9;
            this.btnGenerateKey.Text = "Gerar Chave";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            // 
            // lblSignature
            // 
            this.lblSignature.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblSignature.ForeColor = System.Drawing.Color.DimGray;
            this.lblSignature.Location = new System.Drawing.Point(20, 194);
            this.lblSignature.Name = "lblSignature";
            this.lblSignature.Size = new System.Drawing.Size(350, 16);
            this.lblSignature.TabIndex = 10;
            this.lblSignature.Text = "MTSproductions@2026";
            this.lblSignature.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 218);
            this.Controls.Add(this.lblSignature);
            this.Controls.Add(this.btnGenerateKey);
            this.Controls.Add(this.btnImportKey);
            this.Controls.Add(this.btnExportKey);
            this.Controls.Add(this.lblKeyStatus);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnDecrypt);
            this.Controls.Add(this.btnEncrypt);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.txtFilePath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.AllowDrop = true;
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CryptoTxt - Criptografar .txt";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
