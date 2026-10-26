using System.Text;
using ElevatorControl.Data;
using ElevatorControl.Elevators;
using ElevatorControl.Elevators.Observers;
using ElevatorControl.Models;

namespace ElevatorControl.Forms
{
    // The main window.
    //  - INHERITANCE: MainForm inherits from Form
    //  - OBSERVER: it also implements IElevatorObserver to refresh the floor status displays
    public class MainForm : Form, IElevatorObserver
    {
        private readonly ElevatorController controller = new ElevatorController(1);
        private readonly ElevatorLogRepository repository = new ElevatorLogRepository();
        private readonly LoggingObserver logger;

        // The timer that drives the animation (every 30 ms)
        private readonly System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();

        private Button btnRequestFloor1;
        private Button btnRequestFloor0;
        private Label lblFloor1Status;
        private Label lblFloor0Status;
        private Button btnLog;
        private Label lblStatus;
        private LogForm logForm;

        public MainForm()
        {
            BuildLayout();

            // The logger watches every elevator and saves the events to the database
            logger = new LoggingObserver(repository);
            logger.LogError += Logger_LogError;

            foreach (Elevator elevator in controller.Elevators)
            {
                elevator.Attach(this);
                elevator.Attach(logger);
            }

            animationTimer.Interval = 30;
            animationTimer.Tick += animationTimer_Tick;
            animationTimer.Start();

            RefreshFloorDisplays();
        }

        // ---------- building the window ----------

        private void BuildLayout()
        {
            Text = "Elevator Control - 2 floors, 2 elevators";
            ClientSize = new Size(620, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Request buttons OUTSIDE the elevators (one per floor)
            GroupBox grpRequests = new GroupBox();
            grpRequests.Text = "Request buttons (on the floors)";
            grpRequests.Location = new Point(20, 20);
            grpRequests.Size = new Size(230, 370);
            Controls.Add(grpRequests);

            btnRequestFloor1 = CreateRequestButton("Request Elevator - Floor 1", new Point(15, 55));
            btnRequestFloor1.Click += btnRequestFloor1_Click;
            lblFloor1Status = CreateStatusLabel(new Point(15, 108));
            AddFloorTitle(grpRequests, "FLOOR 1", 25);
            grpRequests.Controls.Add(btnRequestFloor1);
            grpRequests.Controls.Add(lblFloor1Status);

            btnRequestFloor0 = CreateRequestButton("Request Elevator - Floor 0", new Point(15, 235));
            btnRequestFloor0.Click += btnRequestFloor0_Click;
            lblFloor0Status = CreateStatusLabel(new Point(15, 288));
            AddFloorTitle(grpRequests, "FLOOR 0", 205);
            grpRequests.Controls.Add(btnRequestFloor0);
            grpRequests.Controls.Add(lblFloor0Status);

            // The log button
            btnLog = new Button();
            btnLog.Text = "Log";
            btnLog.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnLog.Location = new Point(20, 410);
            btnLog.Size = new Size(230, 50);
            btnLog.Click += btnLog_Click;
            Controls.Add(btnLog);

            // One panel (shaft + car + control panel) for each elevator
            int x = 270;
            foreach (Elevator elevator in controller.Elevators)
            {
                ElevatorPanelControl panel = new ElevatorPanelControl(elevator);
                panel.Location = new Point(x, 40);
                Controls.Add(panel);
                x += 330;
            }

            lblStatus = new Label();
            lblStatus.Location = new Point(20, 520);
            lblStatus.Size = new Size(580, 25);
            lblStatus.Text = "Ready.";
            Controls.Add(lblStatus);
        }

        private Button CreateRequestButton(string text, Point location)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = location;
            button.Size = new Size(200, 45);
            button.FlatStyle = FlatStyle.Flat;
            return button;
        }

        private Label CreateStatusLabel(Point location)
        {
            Label label = new Label();
            label.Location = location;
            label.Size = new Size(200, 60);
            label.BorderStyle = BorderStyle.FixedSingle;
            return label;
        }

        private void AddFloorTitle(GroupBox group, string text, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            label.Location = new Point(15, y);
            label.Size = new Size(200, 22);
            group.Controls.Add(label);
        }

        // ---------- button event handlers ----------

        private void btnRequestFloor1_Click(object sender, EventArgs e)
        {
            RequestFromFloor(1);
        }

        private void btnRequestFloor0_Click(object sender, EventArgs e)
        {
            RequestFromFloor(0);
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            // Open the log window (or bring it to the front and reload it if it is already open)
            if (logForm == null || logForm.IsDisposed)
            {
                logForm = new LogForm(repository);
                logForm.Show(this);
            }
            else
            {
                logForm.Activate();
                logForm.LoadLogs();
            }
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            controller.Tick();
        }

        private void Logger_LogError(string message)
        {
            lblStatus.Text = "Database warning: " + message;
        }

        // ---------- helper methods ----------

        private void RequestFromFloor(int floor)
        {
            try
            {
                Elevator chosen = controller.CallElevator(floor);
                lblStatus.Text = "Floor " + floor + " request: Elevator " + chosen.Id + " is on its way.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Could not handle the request: " + ex.Message;
            }
        }

        // Called by the elevators (Observer pattern)
        public void OnElevatorEvent(Elevator elevator, ElevatorEventType type, string message)
        {
            RefreshFloorDisplays();
        }

        private void RefreshFloorDisplays()
        {
            // The request button is lit until an elevator has arrived
            btnRequestFloor1.BackColor = controller.IsFloorCalled(1) ? Color.Gold : SystemColors.Control;
            btnRequestFloor0.BackColor = controller.IsFloorCalled(0) ? Color.Gold : SystemColors.Control;

            lblFloor1Status.Text = BuildFloorStatus(1);
            lblFloor0Status.Text = BuildFloorStatus(0);
        }

        // The floor status display: tells which elevators are on this floor
        private string BuildFloorStatus(int floor)
        {
            StringBuilder text = new StringBuilder();

            foreach (Elevator elevator in controller.Elevators)
            {
                string where = "not here";
                if (elevator.CurrentFloor == floor)
                {
                    where = elevator.DoorState == DoorState.Open ? "HERE (door open)" : "HERE";
                }
                text.AppendLine("Elevator " + elevator.Id + ": " + where);
            }

            return text.ToString();
        }
    }
}
