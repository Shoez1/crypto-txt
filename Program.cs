using System;
using System.Windows.Forms;

namespace CryptoTxt
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show($"Erro fatal: {args.Exception.Message}\n\n{args.Exception}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"Erro fatal: {ex?.Message}\n\n{ex}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            };
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
