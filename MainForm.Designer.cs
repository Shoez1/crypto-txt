namespace CryptoTxt
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.Button btnPreview;
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
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
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
            this.txtFilePath.Size = new System.Drawing.Size(250, 20);
            this.txtFilePath.TabIndex = 0;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(280, 18);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(90, 23);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "Selecionar...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.Location = new System.Drawing.Point(20, 60);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(170, 30);
            this.btnEncrypt.TabIndex = 2;
            this.btnEncrypt.Text = "Criptografar";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Location = new System.Drawing.Point(200, 60);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(170, 30);
            this.btnDecrypt.TabIndex = 3;
            this.btnDecrypt.Text = "Descriptografar";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(20, 100);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(350, 30);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "Visualizar Conteúdo";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnExportKey
            // 
            this.btnExportKey.Location = new System.Drawing.Point(20, 150);
            this.btnExportKey.Name = "btnExportKey";
            this.btnExportKey.Size = new System.Drawing.Size(125, 23);
            this.btnExportKey.TabIndex = 5;
            this.btnExportKey.Text = "Exportar Chave";
            this.btnExportKey.UseVisualStyleBackColor = true;
            this.btnExportKey.Click += new System.EventHandler(this.btnExportKey_Click);
            // 
            // btnImportKey
            // 
            this.btnImportKey.BackColor = System.Drawing.Color.LightGray;
            this.btnImportKey.Location = new System.Drawing.Point(155, 150);
            this.btnImportKey.Name = "btnImportKey";
            this.btnImportKey.Size = new System.Drawing.Size(115, 23);
            this.btnImportKey.TabIndex = 6;
            this.btnImportKey.Text = "Importar Chave";
            this.btnImportKey.UseVisualStyleBackColor = true;
            this.btnImportKey.Click += new System.EventHandler(this.btnImportKey_Click);
            //
            // btnGenerateKey
            //
            this.btnGenerateKey.Location = new System.Drawing.Point(280, 150);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(90, 23);
            this.btnGenerateKey.TabIndex = 7;
            this.btnGenerateKey.Text = "Gerar Chave";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            //
            // lblSignature
            //
            this.lblSignature.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblSignature.ForeColor = System.Drawing.Color.DimGray;
            this.lblSignature.Location = new System.Drawing.Point(20, 186);
            this.lblSignature.Name = "lblSignature";
            this.lblSignature.Size = new System.Drawing.Size(350, 14);
            this.lblSignature.TabIndex = 8;
            this.lblSignature.Text = "MTSproductions@2026";
            this.lblSignature.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 210);
            this.Controls.Add(this.lblSignature);
            this.Controls.Add(this.btnGenerateKey);
            this.Controls.Add(this.btnImportKey);
            this.Controls.Add(this.btnExportKey);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnDecrypt);
            this.Controls.Add(this.btnEncrypt);
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
