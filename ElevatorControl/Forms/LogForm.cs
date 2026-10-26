using System.ComponentModel;
using System.Data;
using ElevatorControl.Data;

namespace ElevatorControl.Forms
{
    // Shows the saved log from the database in a grid.
    // A BackgroundWorker loads the data so the window never freezes.
    public class LogForm : Form
    {
        private readonly ElevatorLogRepository repository;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private DataGridView dgvLog;
        private Button btnRefresh;
        private Label lblInfo;

        public LogForm(ElevatorLogRepository repository)
        {
            this.repository = repository;

            BuildLayout();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            Load += LogForm_Load;
        }

        private void BuildLayout()
        {
            Text = "Elevator log (from the database)";
            Size = new Size(850, 500);
            StartPosition = FormStartPosition.CenterParent;

            dgvLog = new DataGridView();
            dgvLog.Dock = DockStyle.Fill;
            dgvLog.ReadOnly = true;
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(dgvLog);

            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 45;
            Controls.Add(topPanel);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(10, 8);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Click += btnRefresh_Click;
            topPanel.Controls.Add(btnRefresh);

            lblInfo = new Label();
            lblInfo.Location = new Point(125, 13);
            lblInfo.Size = new Size(650, 25);
            topPanel.Controls.Add(lblInfo);
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLogs();
        }

        // Public so the main form can ask for a reload
        public void LoadLogs()
        {
            if (worker.IsBusy)
            {
                return;
            }

            lblInfo.Text = "Loading...";
            btnRefresh.Enabled = false;
            worker.RunWorkerAsync();
        }

        // Background thread: only talks to the database
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = repository.GetLogs();
        }

        // Back on the GUI thread: show the result
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnRefresh.Enabled = true;

            if (e.Error != null)
            {
                lblInfo.Text = "Could not load the log: " + e.Error.Message;
                return;
            }

            DataTable table = (DataTable)e.Result;
            dgvLog.DataSource = table;
            lblInfo.Text = table.Rows.Count + " rows (newest first)";
        }
    }
}
