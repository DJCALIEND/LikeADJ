using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class SelectionRangeSlider : UserControl
    {
        public int Min
        {
            get { return min; }
            set { min = value; Invalidate(); }
        }
        int min = 0;

        public int Max
        {
            get { return max; }
            set { max = value; Invalidate(); }
        }
        int max = 255;

        public int SelectedMin
        {
            get { return selectedMin; }
            set
            {
                selectedMin = value;
                SelectionChanged?.Invoke(this, null);
                Invalidate();
            }
        }
        int selectedMin = 0;

        public int SelectedMax
        {
            get { return selectedMax; }
            set
            {
                selectedMax = value;
                SelectionChanged?.Invoke(this, null);
                Invalidate();
            }
        }
        int selectedMax = 255;

        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                ValueChanged?.Invoke(this, null);
                Invalidate();
            }
        }
        int value = 50;

        public event EventHandler SelectionChanged;
        public event EventHandler ValueChanged;

        public SelectionRangeSlider()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Paint += new PaintEventHandler(SelectionRangeSlider_Paint);
            MouseDown += new MouseEventHandler(SelectionRangeSlider_MouseDown);
            MouseMove += new MouseEventHandler(SelectionRangeSlider_MouseMove);
        }

        void SelectionRangeSlider_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
            Rectangle selectionRect = new Rectangle((selectedMin - Min) * Width / (Max - Min), 0, (selectedMax - selectedMin) * Width / (Max - Min), Height);
            LinearGradientBrush linGrBrush = new LinearGradientBrush(new Point(0, 0), new Point(Width, 0), Color.Black, Color.Yellow);
            e.Graphics.FillRectangle(linGrBrush, selectionRect);
            if (SelectedMin < 0) SelectedMin = 0;
            if (SelectedMax > 255) SelectedMax = 255;
            if (SelectedMin < 15) e.Graphics.DrawString(SelectedMin.ToString(), new Font("Arial", 8), new SolidBrush(Color.White), new RectangleF(0, Height / 4, 25, 20)); 
            else e.Graphics.DrawString(SelectedMin.ToString(), new Font("Arial", 8), new SolidBrush(Color.Black), new RectangleF(0, Height/4, 25, 20));
            e.Graphics.DrawString(SelectedMax.ToString(), new Font("Arial", 8), new SolidBrush(Color.Black), new RectangleF(Width-35, Height / 4, 35,15));
        }

        void SelectionRangeSlider_MouseDown(object sender, MouseEventArgs e)
        {
            int pointedValue = Min + e.X * (Max - Min) / Width;
            int distValue = Math.Abs(pointedValue - Value);
            int distMin = Math.Abs(pointedValue - SelectedMin);
            int distMax = Math.Abs(pointedValue - SelectedMax);
            int minDist = Math.Min(distValue, Math.Min(distMin, distMax));
            if (minDist == distValue) movingMode = MovingMode.MovingValue;
            else if (minDist == distMin) movingMode = MovingMode.MovingMin;
            else movingMode = MovingMode.MovingMax;
            SelectionRangeSlider_MouseMove(sender, e);
        }

        void SelectionRangeSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int pointedValue = Min + e.X * (Max - Min) / Width;
            if (movingMode == MovingMode.MovingValue) Value = pointedValue;
            else if (movingMode == MovingMode.MovingMin) SelectedMin = pointedValue;
            else if (movingMode == MovingMode.MovingMax) SelectedMax = pointedValue;
        }

        enum MovingMode { MovingValue, MovingMin, MovingMax }
        MovingMode movingMode;
    }
}
