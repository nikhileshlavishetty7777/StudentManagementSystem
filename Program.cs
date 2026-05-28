using System;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Forms;

namespace StudentManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                DatabaseManager.Initialize();

                var form = new MainForm();
                form.Show();
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Startup Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}