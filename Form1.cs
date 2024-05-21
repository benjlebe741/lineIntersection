using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace lineIntersection
{
    public partial class Form1 : Form
    {
        bool[] WSAD = new bool[] { false, false, false, false };
        Point pOne = new Point(450, 455);
        Point pTwo = new Point(450, 255);
        Point pThree = new Point(230, 415);
        Rectangle rect = new Rectangle(110, 110, 20, 30);
        Rectangle pastRect = new Rectangle(110, 110, 20, 30);

        List<Point> drawThese = new List<Point>();

        public Form1()
        {
            InitializeComponent();
        }

        bool lineIntersects(Point _pOne, Point _pTwo, Rectangle _rect, Rectangle _pastRect)
        {
            if (_pOne.X == _pTwo.X) 
            {
                _pOne.X += 1;
            }
            Point leftMost = (_pOne.X < _pTwo.X) ? _pOne : _pTwo;
            Point rightMost = (leftMost == _pTwo) ? _pOne : _pTwo;

            bool increasingSlope = (leftMost.Y < rightMost.Y) ? true : false;

            //Find what points are inside the line-to-be-checked's rectangle, we can also narrow down the points to be checked based on slope
            List<Point> rectCorners;

            float deltaX = ((_pTwo.X - _pOne.X) == 0) ? ((_pTwo.X - _pOne.X) + (float)0.00001) : (_pTwo.X - _pOne.X);
            float slope = (_pTwo.Y - _pOne.Y) / deltaX;

            float projectedPreviousX = -(((_pOne.Y - (pastRect.Y + (pastRect.Height /2))) / slope) - _pOne.X);
            float sideOfLine = projectedPreviousX - (pastRect.X + (pastRect.Width / 2));

            if (!increasingSlope)
            {
                //Bottom Right or Top Left
                Point ghostPoint = (sideOfLine > 1) ? new Point(_rect.X + _rect.Width - 1, _rect.Y + _rect.Height - 1) : new Point(_rect.X, _rect.Y);

                rectCorners = new List<Point>
                {
                    ghostPoint,
                };
            }
            else
            {
                //Top Right or Bottom Left
                Point ghostPoint = (sideOfLine > 1) ? new Point(_rect.X + _rect.Width - 1, _rect.Y) : new Point(_rect.X, _rect.Y + _rect.Height - 1);

                rectCorners = new List<Point>
                {
                    ghostPoint,
                };
            }

            for (int i = 0; i < rectCorners.Count; i++)
            {
                //Project this point onto the line-to-be-checked, both using the points x and y coord
                int x = rectCorners[i].X;
                int y = rectCorners[i].Y;
                //Rearrange the line equation with those x and y coordinates as one of the unknowns, get this new point
                Point subbedInX, subbedInY;

                float newY = -((slope * (_pOne.X - x)) - _pOne.Y);
                float newX = -(((_pOne.Y - y) / slope) - _pOne.X);

                subbedInX = new Point(x, (int)newY);
                subbedInY = new Point((int)newX, y);


                //(Does this projected point exist inside the inputed rectangle) ? Intersection : Continue
                bool returnTrue = false;
                if (_rect.Contains(subbedInX)) //Y coord is new rects coord
                {
                    drawThese.Add(subbedInX);
                    returnTrue = true;
                    rect.Y = subbedInX.Y - (y - _rect.Y);
                }
                if (rect.Contains(subbedInY)) //X coord is new rects coord
                {

                    drawThese.Add(subbedInY);
                    returnTrue = true;
                    rect.X = subbedInY.X - (x - _rect.X);
                }
                if (returnTrue)
                {
                    label1.Text = $"{sideOfLine}";
                    drawThese.Add(rectCorners[i]);
                    return true;
                }
                drawThese.Add(subbedInY);
                drawThese.Add(subbedInX);
                drawThese.Add(rectCorners[i]);
            }

            label1.Text = $"{sideOfLine}";
            return false;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //if (lineIntersects(pOne, pTwo, rect))
            if (lineIntersects(pTwo, pOne, rect, pastRect))
            {
                e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Red), 3), pOne, pTwo);
            }
            else
            {
                e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Blue)), pOne, pTwo);
            }
            //if (lineIntersects(pTwo, pThree, rect, pastRect))
            //{
            //    e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Red), 3), pOne, pTwo);
            //}
            //else
            //{
            //    e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Blue)), pOne, pTwo);
            //}

            e.Graphics.FillRectangle(new SolidBrush(Color.BurlyWood), rect);

            foreach (Point p in drawThese)
            {
                try
                {
                    e.Graphics.FillEllipse(new SolidBrush(Color.Black), new Rectangle(p.X - 2, p.Y - 2, 4, 4));
                }
                catch { }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            drawThese.Clear();

            pastRect.Location = rect.Location;

            int speed = 5;

            rect.Y += (WSAD[0]) ? -speed : 0;
            rect.Y += (WSAD[1]) ? speed : 0;
            rect.X += (WSAD[2]) ? -speed : 0;
            rect.X += (WSAD[3]) ? speed : 0;

            Refresh();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    WSAD[0] = true;
                    break;
                case Keys.S:
                    WSAD[1] = true;
                    break;
                case Keys.A:
                    WSAD[2] = true;
                    break;
                case Keys.D:
                    WSAD[3] = true;
                    break;
                case Keys.X:
                    rect.Location = PointToClient(Cursor.Position);
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    WSAD[0] = false;
                    break;
                case Keys.S:
                    WSAD[1] = false;
                    break;
                case Keys.A:
                    WSAD[2] = false;
                    break;
                case Keys.D:
                    WSAD[3] = false;
                    break;
            }
        }
    }
}
