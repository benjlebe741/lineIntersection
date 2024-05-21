using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lineIntersection
{
    public partial class Form1 : Form
    {
        bool[] WSAD = new bool[] { false, false, false, false };
        Point pOne = new Point(400, 100);
        Point pTwo = new Point(34, 450);
        Rectangle rect = new Rectangle(110, 110, 20, 30);

        List<Point> drawThese = new List<Point>();

        public Form1()
        {
            InitializeComponent();
        }

        bool lineIntersects(Point _pOne, Point _pTwo, Rectangle _rect)
        {
            Point lrTop = new Point((_pOne.X < _pTwo.X) ? _pOne.X : _pTwo.X, (_pOne.Y < _pTwo.Y) ? _pOne.Y : _pTwo.Y);
            Point lrBottom = new Point((_pOne.X > _pTwo.X) ? _pOne.X : _pTwo.X, (_pOne.Y > _pTwo.Y) ? _pOne.Y : _pTwo.Y);

            Rectangle lineRect = new Rectangle(lrTop.X, lrTop.Y, lrBottom.X - lrTop.X, lrBottom.Y - lrTop.Y);
            if (lineRect.Width < 30) { lineRect.Width = 30; } //Protection from lines that are quite vertical
            if (lineRect.Height < 30) { lineRect.Height = 30; } //Protection from lines that are quite horizontal
            //First check, is it possible that the rectangle and line overlap, or is the rectangle further away from the line to the point where we dont need to go further.
            if (_rect.Contains(_pOne) || _rect.Contains(_pTwo))
            {
                return true;
            }
            if (!_rect.IntersectsWith(lineRect))
            {
                return false;
            }

            Point leftMost = (_pOne.X < _pTwo.X) ? _pOne : _pTwo;
            Point rightMost = (leftMost == _pTwo) ? _pOne : _pTwo;

            bool increasingSlope = (leftMost.Y < rightMost.Y) ? true : false;

            //Find what points are inside the line-to-be-checked's rectangle, we can also narrow down the points to be checked based on slope
            List<Point> rectCorners;
            if (!increasingSlope)
            {
                rectCorners = new List<Point>
                {
                new Point(_rect.X + _rect.Width,_rect.Y + _rect.Height),
                //new Point(_rect.X,_rect.Y),
                //new Point(_rect.X,_rect.Y + _rect.Height),
                //new Point(_rect.X + _rect.Width,_rect.Y),
                };
            }
            else
            {
                rectCorners = new List<Point>
                {
                //new Point(_rect.X,_rect.Y),
                new Point(_rect.X,_rect.Y + _rect.Height),
                new Point(_rect.X + _rect.Width,_rect.Y),
                //new Point(_rect.X + _rect.Width,_rect.Y + _rect.Height),
                };
            }



            for (int i = 0; i < rectCorners.Count; i++)
            {
                if (lineRect.Contains(rectCorners[i]))
                {
                    //Project this point onto the line-to-be-checked, both using the points x and y coord
                    int x = rectCorners[i].X;
                    int y = rectCorners[i].Y;

                    float deltaX = ((_pTwo.X - _pOne.X) == 0) ? ((_pTwo.X - _pOne.X) + (float)0.00001) : (_pTwo.X - _pOne.X);
                    float slope = (_pTwo.Y - _pOne.Y) / deltaX;
                    //Rearrange the line equation with those x and y coordinates as one of the unknowns, get this new point.
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
                        rect.Y = subbedInX.Y - (y - rect.Y);
                        //                        rect.Location = new Point(subbedInX.X - widthDifference, subbedInX.Y - heightDifference);
                    }
                    if (_rect.Contains(subbedInY)) //X coord is new rects coord
                    {

                        drawThese.Add(subbedInY);
                        returnTrue = true;
                        rect.X = subbedInY.X - (x - rect.X);
                        //rect.Location = new Point(subbedInY.X - widthDifference, subbedInY.Y - heightDifference);  
                    }
                    if (returnTrue)
                    {
                        drawThese.Add(rectCorners[i]);
                        return true;
                    }

                    drawThese.Add(rectCorners[i]);

                }
            }


            return false;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //if (lineIntersects(pOne, pTwo, rect))
            if (lineIntersects(pTwo, pOne, rect))
            {
                e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Red), 3), pOne, pTwo);
            }
            else
            {
                e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Blue)), pOne, pTwo);
            }

            e.Graphics.FillRectangle(new SolidBrush(Color.BurlyWood), rect);

            foreach (Point p in drawThese)
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.Black), new Rectangle(p.X - 2, p.Y - 2, 4, 4));
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            drawThese.Clear();
            //rect.Location = PointToClient(Cursor.Position);



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
