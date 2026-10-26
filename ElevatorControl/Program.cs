using ElevatorControl.Data;
using ElevatorControl.Forms;

namespace ElevatorControl
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Make sure the database and tables exist before the form opens.
            // If SQL Server is not running we still start the program, but tell the user.
            try
            {
                DatabaseHelper.EnsureDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The database could not be prepared, so elevator events will NOT be logged.\n\n" + ex.Message,
                    "Database warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Application.Run(new MainForm());
        }
    }
}
