using System;
using System.Windows.Forms;

namespace CryptoTxt
{
    public class SelectTypeForm : Form
    {
        public Button btnArquivo;
        public Button btnPasta;
        public SelectTypeForm()
        {
            this.Text = "Selecionar Tipo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Width = 300;
            this.Height = 140;
            this.ShowInTaskbar = false;

            var lbl = new Label();
            lbl.Text = "O que deseja selecionar?";
            lbl.AutoSize = false;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl.Dock = DockStyle.Top;
            lbl.Height = 40;
            this.Controls.Add(lbl);

            btnArquivo = new Button();
            btnArquivo.Text = "Arquivo";
            btnArquivo.Width = 100;
            btnArquivo.Height = 30;
            btnArquivo.Left = 40;
            btnArquivo.Top = 55;
            btnArquivo.DialogResult = DialogResult.OK;
            this.Controls.Add(btnArquivo);

            btnPasta = new Button();
            btnPasta.Text = "Pasta";
            btnPasta.Width = 100;
            btnPasta.Height = 30;
            btnPasta.Left = 150;
            btnPasta.Top = 55;
            btnPasta.DialogResult = DialogResult.Yes;
            this.Controls.Add(btnPasta);

            this.AcceptButton = btnArquivo;
            this.CancelButton = btnPasta;
        }
    }
}
