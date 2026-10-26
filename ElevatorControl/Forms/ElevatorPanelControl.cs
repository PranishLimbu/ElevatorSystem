using ElevatorControl.Elevators;
using ElevatorControl.Elevators.Observers;
using ElevatorControl.Models;

namespace ElevatorControl.Forms
{
    // One of these is shown for each elevator. It draws the shaft and the car
    // and contains that elevator's own control panel (display + two floor buttons).
    //  - INHERITANCE: it inherits from UserControl
    //  - OBSERVER: it implements IElevatorObserver so the elevator can tell it to refresh
    //  - DELEGATE: it subscribes to the Animated event to redraw during movement
    public class ElevatorPanelControl : UserControl, IElevatorObserver
    {
        // Drawing measurements (in pixels)
        private const int ShaftLeft = 15;
        private const int ShaftWidth = 150;
        private const int CarWidth = 120;
        private const int CarHeight = 110;
        private const int Floor1Y = 190;   // y position of the floor 1 line
        private const int Floor0Y = 400;   // y position of the floor 0 line

        private readonly Elevator elevator;
        private Label lblTitle;
        private Label lblDisplay;
        private Label lblState;
        private Button btnFloor1;
        private Button btnFloor0;

        public ElevatorPanelControl(Elevator elevator)
        {
            this.elevator = elevator;

            DoubleBuffered = true;   // stops the flicker while animating
            Size = new Size(320, 460);

            BuildControlPanel();

            elevator.Animated += Elevator_Animated;   // subscribe to the delegate event
            elevator.Attach(this);                    // register as an observer

            UpdatePanel();
        }

        // ---------- building the control panel ----------

        private void BuildControlPanel()
        {
            lblTitle = new Label();
            lblTitle.Text = "Elevator " + elevator.Id + " panel";
            lblTitle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblTitle.Location = new Point(185, 10);
            lblTitle.Size = new Size(130, 25);
            Controls.Add(lblTitle);

            // The display window that shows the current floor
            lblDisplay = new Label();
            lblDisplay.BackColor = Color.Black;
            lblDisplay.ForeColor = Color.Lime;
            lblDisplay.Font = new Font("Consolas", 26f, FontStyle.Bold);
            lblDisplay.TextAlign = ContentAlignment.MiddleCenter;
            lblDisplay.Location = new Point(185, 40);
            lblDisplay.Size = new Size(125, 60);
            Controls.Add(lblDisplay);

            btnFloor1 = CreateFloorButton("1", new Point(215, 120));
            btnFloor1.Click += btnFloor1_Click;
            Controls.Add(btnFloor1);

            btnFloor0 = CreateFloorButton("0", new Point(215, 190));
            btnFloor0.Click += btnFloor0_Click;
            Controls.Add(btnFloor0);

            lblState = new Label();
            lblState.Location = new Point(185, 270);
            lblState.Size = new Size(125, 40);
            lblState.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblState);
        }

        private Button CreateFloorButton(string text, Point location)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            button.Size = new Size(60, 60);
            button.Location = location;
            button.FlatStyle = FlatStyle.Flat;
            return button;
        }

        // ---------- button event handlers ----------

        private void btnFloor1_Click(object sender, EventArgs e)
        {
            PressFloorButton(1);
        }

        private void btnFloor0_Click(object sender, EventArgs e)
        {
            PressFloorButton(0);
        }

        // Requests from INSIDE an elevator go only to this elevator
        private void PressFloorButton(int floor)
        {
            try
            {
                elevator.RequestFloor(floor, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not handle the button press: " + ex.Message);
            }
        }

        // ---------- keeping the display up to date ----------

        // Called by the elevator (Observer pattern)
        public void OnElevatorEvent(Elevator source, ElevatorEventType type, string message)
        {
            UpdatePanel();
            Invalidate();
        }

        // Called every timer tick while moving (delegate)
        private void Elevator_Animated(Elevator sender)
        {
            UpdatePanel();
            Invalidate();
        }

        private void UpdatePanel()
        {
            string arrow = "";
            if (elevator.Direction == Direction.Up) arrow = " \u25B2";
            if (elevator.Direction == Direction.Down) arrow = " \u25BC";

            lblDisplay.Text = elevator.CurrentFloor + arrow;
            lblState.Text = elevator.StateName;

            // A lit button = a request that has not been served yet
            btnFloor1.BackColor = elevator.PanelLightOn(1) ? Color.Gold : SystemColors.Control;
            btnFloor0.BackColor = elevator.PanelLightOn(0) ? Color.Gold : SystemColors.Control;
        }

        // ---------- drawing the shaft and the car ----------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            int carX = ShaftLeft + (ShaftWidth - CarWidth) / 2;
            int shaftTop = Floor1Y - CarHeight - 30;

            // Elevator shaft
            g.DrawRectangle(Pens.Gray, ShaftLeft, shaftTop, ShaftWidth, Floor0Y - shaftTop);

            // "Floor doors": closed grey doors on each floor (the car is drawn on top of them)
            DrawFloorDoor(g, carX, Floor1Y);
            DrawFloorDoor(g, carX, Floor0Y);

            // Floor lines and labels
            using (Pen floorPen = new Pen(Color.Black, 3))
            using (Font font = new Font("Segoe UI", 9f))
            {
                g.DrawLine(floorPen, ShaftLeft - 10, Floor1Y, ShaftLeft + ShaftWidth + 10, Floor1Y);
                g.DrawLine(floorPen, ShaftLeft - 10, Floor0Y, ShaftLeft + ShaftWidth + 10, Floor0Y);
                g.DrawString("Floor 1", font, Brushes.Black, ShaftLeft, Floor1Y + 4);
                g.DrawString("Floor 0", font, Brushes.Black, ShaftLeft, Floor0Y + 4);
            }

            // The car moves between the two floor lines according to Position (0.0 - 1.0)
            float carBottom = Floor0Y - (float)(elevator.Position * (Floor0Y - Floor1Y));
            float carTop = carBottom - CarHeight;

            using (Brush bodyBrush = new SolidBrush(Color.FromArgb(75, 0, 90)))
            {
                g.FillRectangle(bodyBrush, carX, carTop, CarWidth, CarHeight);
            }

            // Inside of the car + the two sliding door panels
            float doorLeft = carX + 8;
            float doorTop = carTop + 8;
            float doorWidth = CarWidth - 16;
            float doorHeight = CarHeight - 8;
            float panelWidth = (doorWidth / 2f) * (float)(1.0 - elevator.DoorOpenAmount);

            g.FillRectangle(Brushes.LightYellow, doorLeft, doorTop, doorWidth, doorHeight);
            g.FillRectangle(Brushes.Gray, doorLeft, doorTop, panelWidth, doorHeight);
            g.FillRectangle(Brushes.Gray, doorLeft + doorWidth - panelWidth, doorTop, panelWidth, doorHeight);
        }

        private void DrawFloorDoor(Graphics g, int x, int floorY)
        {
            g.FillRectangle(Brushes.Silver, x, floorY - CarHeight, CarWidth, CarHeight);
            g.DrawRectangle(Pens.DimGray, x, floorY - CarHeight, CarWidth, CarHeight);
            g.DrawLine(Pens.DimGray, x + CarWidth / 2, floorY - CarHeight, x + CarWidth / 2, floorY);
        }
    }
}
