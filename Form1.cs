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
        const int MAX_Y_SPEED = 16;
        double ySpeed = 0;
        int time = 0;
        double impulse = -35;
        double accelleration = 1.8;
        double deceleration = 1;

        bool airBorn = false;
        bool jumpPressed = false;



        bool[] WSAD = new bool[] { false, false, false, false };

        List<Point> points = new List<Point>
        {
        };

        int normalX, normalY;

        Rectangle rect = new Rectangle(220, 220, 50, 60);
        Rectangle pastRect;

        List<Point> drawThese = new List<Point>();

        public Form1()
        {
            InitializeComponent();
            pastRect = rect;
        }

        bool lineIntersects(Point _pOne, Point _pTwo, Rectangle _rect, Rectangle _pastRect, bool move)
        {
            if (_rect.Contains(_pOne) || _rect.Contains(_pTwo))
            {
                return true;
            }

            if (_pOne.X == _pTwo.X)
            {
                _pOne.X += 1;
            }
            if (_pOne.Y == _pTwo.Y)
            {
                _pOne.Y += 1;
            }


            Point leftMost = (_pOne.X < _pTwo.X) ? _pOne : _pTwo;
            Point rightMost = (leftMost == _pTwo) ? _pOne : _pTwo;
            Point upMost = (_pOne.Y < _pTwo.Y) ? _pOne : _pTwo;
            Point downMost = (upMost == _pTwo) ? _pOne : _pTwo;


            //Are we to the left or right or above or below the line?
            if (_rect.Right < leftMost.X || _rect.X > rightMost.X || _rect.Bottom < upMost.Y || _rect.Y > downMost.Y)
            { return false; }


            bool increasingSlope = (leftMost.Y < rightMost.Y) ? true : false;

            //Find what points are inside the line-to-be-checked's rectangle, we can also narrow down the points to be checked based on slope
            List<Point> rectCorners;

            float deltaX = ((_pTwo.X - _pOne.X) == 0) ? ((_pTwo.X - _pOne.X) + (float)0.00001) : (_pTwo.X - _pOne.X);
            float slope = (_pTwo.Y - _pOne.Y) / deltaX;

            float projectedPreviousX = -(((_pOne.Y - (pastRect.Y + (pastRect.Height / 2))) / slope) - _pOne.X);
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
                if (_rect.Contains(subbedInX) || rect.Contains(subbedInY))
                {
                    drawThese.Add(subbedInX);

                    drawThese.Add(subbedInY);

                    drawThese.Add(rectCorners[i]);

                    int fn = 3;
                    if (!increasingSlope && move)
                    {
                        rect.X += (sideOfLine > 1) ? -fn : fn;
                        rect.Y += (sideOfLine > 1) ? -fn : fn;
                    }
                    else if (move)
                    {
                        rect.X += (sideOfLine > 1) ? -fn : fn;
                        rect.Y += (sideOfLine > 1) ? fn : -fn;
                    }
                    return true;

                }
                drawThese.Add(rectCorners[i]);
            }

            return false;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int p = 0; p < points.Count; p++)
            {
                Point pOne = points[p];
                Point pTwo = (p + 1 >= points.Count) ? points[0] : points[p + 1];
                if (lineIntersects(pOne, pTwo, rect, pastRect, false))
                {
                    e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Red), 3), pOne, pTwo);
                }
                else
                {
                    e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Blue)), pOne, pTwo);
                }
            }
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
            airBorn = true;
            time++;
            drawThese.Clear();

            pastRect.Location = rect.Location;

            normalX = normalY = 0;

            #region Jumping
            if (jumpPressed)
            {
                ySpeed = impulse;
                jumpPressed = false;
            }
            if (time % deceleration == 0)
            {
                ySpeed = (ySpeed + accelleration > MAX_Y_SPEED) ? MAX_Y_SPEED : ySpeed + accelleration;
            }
            rect.Y += (int)ySpeed;

            label1.Text = $"{airBorn}";
            #endregion

            collisionChecks();


            //NOW DO YOUR PLAYER MOVEMENTS
            int speed = 8;
           // rect.Y += (WSAD[0]) ? -speed : 0;
            rect.Y += (WSAD[1]) ? speed : 0;
            rect.X += (WSAD[2]) ? -speed : 0;
            rect.X += (WSAD[3]) ? speed : 0;


            int xChange = pastRect.X - rect.X;
            int yChange = pastRect.Y - rect.Y;

            collisionChecks();



            //NOW ADD THE NORMAL FORCE
            pastRect = rect;

            normalX /= (normalX != 0) ? Math.Abs(normalX) : 1;
            normalY /= (normalY != 0) ? Math.Abs(normalY) : 1;

            if (yChange != 0)
            {
                rect.X += normalX * 2;
            }
            if (xChange != 0)
            {
                rect.Y += normalY * 2;
            }
            collisionChecks();


            Refresh();
        }


        void collisionChecks()
        {
            int deltaX = (pastRect.X - rect.X);
            int deltaY = (pastRect.Y - rect.Y);

            double xDirectionChange = (deltaX == 0) ? 0 : (Math.Abs(deltaX) / deltaX);
            double yDirectionChange = (deltaY == 0) ? 0 : (Math.Abs(deltaY) / deltaY);

            bool intersection = false;

            for (int p = 0; p < points.Count; p++)
            {
                Point pOne = points[p];
                Point pTwo = (p + 1 >= points.Count) ? points[0] : points[p + 1];
                if (lineIntersects(pOne, pTwo, rect, pastRect, false)) //If there is an intersection
                {
                    intersection = true;
                    #region new stuff

                    Point leftMost = (pOne.X < pTwo.X) ? pOne : pTwo;
                    Point rightMost = (leftMost == pTwo) ? pOne : pTwo;
                    Point upMost = (pOne.Y < pTwo.Y) ? pOne : pTwo;
                    Point downMost = (upMost == pTwo) ? pOne : pTwo;


                    bool increasingSlope = (leftMost.Y < rightMost.Y) ? true : false;

                    float deltaLineX = ((pTwo.X - pOne.X) == 0) ? ((pTwo.X - pOne.X) + (float)0.00001) : (pTwo.X - pOne.X);
                    float slope = (pTwo.Y - pOne.Y) / deltaLineX;

                    float projectedPreviousX = -(((pOne.Y - (pastRect.Y + (pastRect.Height / 2))) / slope) - pOne.X);
                    float sideOfLine = projectedPreviousX - (pastRect.X + (pastRect.Width / 2));

                    int slopeX = 0;
                    int slopeY = 0;

                    int fn = 1;
                    if (!increasingSlope)
                    {
                        slopeX = (sideOfLine > 1) ? -fn : fn;
                        slopeY = (sideOfLine > 1) ? -fn : fn;

                        if (sideOfLine > 1)
                        {
                            airBorn = false;
                        }
                    }
                    else
                    {
                        slopeX = (sideOfLine > 1) ? -fn : fn;
                        slopeY = (sideOfLine > 1) ? fn : -fn;

                        if (sideOfLine < 1)
                        {
                            airBorn = false;
                        }

                    }

                    normalX += slopeX;
                    normalY += slopeY;


                    if (xDirectionChange != slopeX)
                    { xDirectionChange = 0; }
                    if (yDirectionChange != slopeY)
                    { yDirectionChange = 0; }
                    #endregion
                }
            }

            int count = (Math.Abs(deltaX) > Math.Abs(deltaY)) ? Math.Abs(deltaX) : Math.Abs(deltaY);
            while (intersection)
            {
                count--;
                if (count <= 0)
                {
                    rect.Location = pastRect.Location;
                    break;
                }
                intersection = false;

                rect.X += (int)xDirectionChange;
                rect.Y += (int)yDirectionChange;

                for (int p = 0; p < points.Count; p++)
                {
                    Point pOne = points[p];
                    Point pTwo = (p + 1 >= points.Count) ? points[0] : points[p + 1];
                    if (lineIntersects(pOne, pTwo, rect, pastRect, false)) //If there is an intersection
                    {
                        intersection = true;
                    }
                }
            }
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
                case Keys.Space:
                    if (airBorn == false)
                    {
                        jumpPressed = true;
                        airBorn = true;
                    }
                    break;
                case Keys.X:
                    rect.Location = PointToClient(Cursor.Position);
                    break;
                case Keys.Y:
                    points.Add(PointToClient(Cursor.Position));
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
                case Keys.Space:
                    ySpeed = (ySpeed > 0) ? ySpeed : 0;
                    break;
            }
        }
    }
}
