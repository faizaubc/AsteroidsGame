using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace AsteroidLab2
{
    //Shape class for Rock 
    public abstract class ShapeBase
    {
        //name the variables 
        protected float _fRotation;
        protected float _fRotationIncrement;
        protected float _fXSpeed;
        protected float _fYSpeed;
        public float radius = 30;
        public int opacity { get; set; }//for opacity 

        //random static object 
        protected static Random _random = new Random();

        //boolean indicating if the shape is dead
        public bool IsMarkedForDeath { private get; set; }
        /// <summary>
        /// Function returning the boolean value
        /// </summary>
        /// <returns></returns>
        public bool dead()
        {
            return IsMarkedForDeath;
        }

        //Coordinates 
        public PointF coordinates { get; set; }

        //Constructor to assign all the variables plus opacity 
        //variables passed in are point, radius, and the opacity value
        public ShapeBase(PointF point, float radi, int op)
        {
            coordinates = point;
            _fRotation = 0;
            _fRotationIncrement = (float)(_random.NextDouble() * (3 - -3) + -3);
            _fXSpeed = (float)(_random.NextDouble() * (2.5 - -2.5) + -2.5);
            _fYSpeed = (float)(_random.NextDouble() * (2.5 - -2.5) + -2.5);
            radius = radi;
            opacity = op;
        }

        /// <summary>
        /// Creates a graphics  path for a raock 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="vertexCount"></param>
        /// <param name="variance"></param>
        /// <returns></returns>
        protected static GraphicsPath MakePolyPath(float radius, int vertexCount, float variance)
        {
            PointF[] points = new PointF[vertexCount];//array of points 
            GraphicsPath gp = new GraphicsPath();//a graphics path variable 
            double angle = 0;
            int count = 0;
            //if the count is less than the vertex count add a point to the array of collection 
            while (count < vertexCount)
            {
                angle = (Math.PI * 2 / vertexCount) * count;
                //get x and y value of the point 
                float XValue = (float)(Math.Cos(angle) * (radius - _random.NextDouble() * radius * variance));
                float YValue = (float)(Math.Sin(angle) * (radius - _random.NextDouble() * radius * variance));
                points[count] = new PointF(XValue, YValue);
                count++;
            }
            //start the figure and connect all the points with lines 
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();
            //return the new path 
            return gp;
        }

        /// <summary>
        /// Method for Get Path
        /// GetPath() implementation that will produce a fully transformed
        /// instance of the cloned model.
        /// </summary>
        /// <returns></returns>
        public abstract GraphicsPath GetPath();

        /// <summary>
        /// The Render method will simply fll the GetPath return value with a provided colour
        /// </summary>
        /// <param name="c"></param>
        /// <param name="gp"></param>
        public void Render(Color c, Graphics gp)
        {
            SolidBrush rectBrush = new SolidBrush(c);
            //include the opacity value as well for the solid brush 
            gp.FillPath(new SolidBrush(Color.FromArgb(opacity, c)), GetPath());
        }

        /// <summary>
        /// The Tick method will accept a Size and will move the shape according to the current speed
        /// values. To create teh effect of wrapping the bounds are checked 
        /// and adjusted accordingly
        /// </summary>
        /// <param name="size"></param>
        public virtual void Tick(System.Drawing.Size size)
        {
            //increent the rotation
            _fRotation += _fRotationIncrement;

            float xVal1 = coordinates.X;
            float yVal1 = coordinates.Y;

            //if x is greater than width wrap it to zero
            if (coordinates.X > size.Width)
            {
                xVal1 = 0;
            }
            //if x value is less than 0 wrap it to size.Wdith 
            else if (coordinates.X < 0)
            {
                xVal1 = size.Width;
            }
            //if y vaue is greater than height wrap it to zero
            else if (coordinates.Y > size.Height)
            {
                yVal1 = 0;
            }
            //if value is less than 0 wrap it to the size.Height
            else if (coordinates.Y < 0)
            {
                yVal1 = size.Height;
            }
            //assign the x and y coordinates
            float xVal = xVal1 + _fXSpeed;
            float yVal = yVal1 + _fYSpeed;

            coordinates = new PointF(xVal, yVal);

            //if opacity is 255 let it stay at 255
            if (opacity == 255)
                opacity = 255;
            else
                opacity += 1;//keep incrementing the opacity
        }

    }
    //Rock Class Derived from ShapeBase 
    public class Rock : ShapeBase
    {
        public static int totalRocks = 2;
        //readonly graphicspath
        readonly GraphicsPath _modelGraphicsPath;

        //constructor for rock shape 
        public Rock(PointF point, float radius, int opacit) : base(point, radius, opacit)
        {
            // use a polygon that consists of a random number of sides between 6 and 12
            // inclusive, with a variance of 30 %
            _modelGraphicsPath = MakePolyPath(radius, _random.Next(8, 18), 0.5f);
        }

        /// <summary>
        /// Override the Get Path
        /// need to clone your model and apply transforms for rotation and translation, returning it when
        ///done
        /// </summary>
        /// <returns></returns>
        public override GraphicsPath GetPath()
        {
            //create a graphics path object
            GraphicsPath GP = (GraphicsPath)_modelGraphicsPath.Clone();

            //create a matrix
            Matrix m = new Matrix();
            //rotate and translate 
            m.Rotate(this._fRotation);
            m.Translate(this.coordinates.X, this.coordinates.Y, MatrixOrder.Append);
            //perform transformation
            GP.Transform(m);

            return GP;
        }

    }

    //create a new shapes absctract class for the arrow and the bullet
    public abstract class Shapes
    {
        //set the variables 
        public PointF coordinates { get; set; }
        public float rotation;
        public bool IsMarkedForDeath { private get; set; }
        public bool dead()
        {
            return IsMarkedForDeath;
        }

        //Shapes constructor to set the coordinates and the roation
        public Shapes(PointF point)
        {
            coordinates = point;
            rotation = 0;
        }

        /// <summary>
        /// Abstract Get Path method that returns the the 
        /// transformed copy of the GRaphics Path
        /// </summary>
        /// <returns></returns>
        public abstract GraphicsPath GetPath();

        /// <summary>
        /// Abstract method that returns the Graphics Path itself
        /// before it is transformed
        /// </summary>
        /// <returns></returns>
        public abstract GraphicsPath MakePath();

        //render method shared by the both shapes 
        public void Render(Color c, Graphics gp)
        {
            gp.FillPath(new SolidBrush(c), GetPath());
        }

    }

    //derived fromt he shapes is the bullet class
    public class Bullets : Shapes
    {
        //graphics path for the bullet
        readonly GraphicsPath _modelStaticPath;

        //a constructor gets passed a point and a rotation value 
        public Bullets(PointF point, float rota) : base(point)
        {
            //get the rotation angle of the arrow
            rotation = rota;
            //constructor calls upon Make Path to get the shape of the Bullet
            _modelStaticPath = MakePath();
        }

        /// <summary>
        /// Override the Make Path method to create a shape of 
        /// a bullet using an ellipse
        /// </summary>
        /// <returns></returns>
        public override GraphicsPath MakePath()
        {
            //create a graphics method and add a ellipse 
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, 2, 2);

            return gp;
        }

        /// <summary>
        /// Override GetPath method creates a clone of the graphics path 
        /// and transforms it using the translate method 
        /// </summary>
        /// <returns></returns>
        public override GraphicsPath GetPath()
        {
            //create a clone of the graohics path 
            GraphicsPath GP = (GraphicsPath)_modelStaticPath.Clone();
            //transform it according to the new coordinates
            Matrix m = new Matrix();
            m.Translate(coordinates.X, coordinates.Y, MatrixOrder.Append);
            GP.Transform(m);
            return GP;
        }

        /// <summary>
        /// Tick Bullet method moves the bullets 
        /// if the bullet is out of bounds it is adjusted accordingly
        /// </summary>
        /// <param name="size"></param>
        public void TickBullet(System.Drawing.Size size)
        {
            float xVal1 = coordinates.X;
            float yVal1 = coordinates.Y;
            if (coordinates.X > size.Width)
            {
                xVal1 = 0;
            }
            else if (coordinates.X < 0)
            {
                xVal1 = size.Width;
            }
            else if (coordinates.Y > size.Height)
            {
                yVal1 = 0;
            }
            else if (coordinates.Y < 0)
            {
                yVal1 = size.Height;
            }
            //move the bullet according to the rotation angle 
            float xVal = xVal1 + (float)(Math.Sin(rotation)) * 10;
            float yVal = yVal1 - (float)(Math.Cos(rotation)) * 10;
            coordinates = new PointF(xVal, yVal);
        }
    }

    //derived from shapes is the arrow shape
    public class Arrows : Shapes
    {
        //stopping collision flag
        public bool collisionStopMomentaraly = false;
        //shipcount indicates the total ships dead
        public static int shipcount = 0;
        //acceleration value
        public float acceleration = 1;
        //create a grapphics path 
        readonly GraphicsPath _modelStaticPath;
        //initialize the base constructor
        public Arrows(PointF point) : base(point)
        {
            _modelStaticPath = MakePath();
        }
        /// <summary>
        /// Override the current Make Path method 
        /// uses an array of points to create a shape
        /// returns the shape after the points have been all connected 
        /// </summary>
        /// <returns></returns>
        public override GraphicsPath MakePath()
        {
            PointF[] points = new PointF[4];
            GraphicsPath gp = new GraphicsPath();
            points[0] = new PointF(0, 0);
            points[1] = new PointF(-10, 20);
            points[2] = new PointF(0, 10);
            points[3] = new PointF(10, 20);
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();
            return gp;

        }

        /// <summary>
        /// The tick method rotates the ship
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotationPassed"></param>
        public void Tick(System.Drawing.Size size, float rotationPassed)
        {
            rotation += rotationPassed;            
        }

        /// <summary>
        /// The Get Math method transforms the ship according to the new coordinates and the new rotation
        /// </summary>
        /// <returns></returns>
        public override GraphicsPath GetPath()
        {
            //create a clone
            GraphicsPath GP = (GraphicsPath)_modelStaticPath.Clone();
            //transform
            Matrix m = new Matrix();
            m.Rotate((float)(rotation * 360 / (Math.PI * 2)));
            m.Translate(this.coordinates.X, this.coordinates.Y, MatrixOrder.Append);
            GP.Transform(m);
            return GP;

        }

        /// <summary>
        /// The move up function moves the ship in upwards direction at the specified angle
        /// if the ship is out of bound the wrapping effect is created
        /// </summary>
        /// <param name="size"></param>
        public void MoveArrowUp(System.Drawing.Size size)
        {
            //when the direction is changed slowly accelerate in the up direction
            acceleration += (float)0.01;
            //once it is sucessfully accelerated, keep it at the level
            if (acceleration >= 4)
            {
                acceleration = 4;
                collisionStopMomentaraly = false;
            }
            float xVal1 = coordinates.X;
            float yVal1 = coordinates.Y;
            //if the x is greater than the width, set the x value to zero 
            if (coordinates.X > size.Width)
            {
                xVal1 = 0;
            }
            //if x is less than 0 , set the x value to size.width
            else if (coordinates.X < 0)
            {
                xVal1 = size.Width;
            }
            //if y value is greater than size.height set the y value to zero
            else if (coordinates.Y > size.Height)
            {
                yVal1 = 0;
            }
            //if the y value is less than zero, set the y value to size.height
            else if (coordinates.Y < 0)
            {
                yVal1 = size.Height;
            }
            //increase the value of x and y by the roation angle value multiply by 5
            float xVal = xVal1 + (float)(Math.Sin(rotation)) * acceleration;
            float yVal = yVal1 - (float)(Math.Cos(rotation)) * acceleration;
            coordinates = new PointF(xVal, yVal);
        }


        /// <summary>
        /// The move up function moves the ship in downwards direction at the specified angle
        /// if the ship is out of bound the wrapping effect is created
        /// </summary>
        /// <param name="size"></param>
        public void MoveArrowDown(System.Drawing.Size size)
        {
            //when the direction is changed slowly accelerate in the up direction
            acceleration += (float)0.01;
            //once it is sucessfully accelerated, keep it at the level
            if (acceleration >= 4)
            {
                acceleration = 4;
                collisionStopMomentaraly = false;//set the flag to false incase it was set true to stop collision momentaraly
            }
            float xVal1 = coordinates.X;
            float yVal1 = coordinates.Y;
            //if the x is greater than the width, set the x value to zero 
            if (coordinates.X > size.Width)
            {
                xVal1 = 0;
            }
            //if x is less than 0 , set the x value to size.width
            else if (coordinates.X < 0)
            {
                xVal1 = size.Width;
            }
            //if y value is greater than size.height set the y value to zero
            else if (coordinates.Y > size.Height)
            {
                yVal1 = 0;
            }
            //increase the value of x and y by the roation angle value multiply by 5
            else if (coordinates.Y < 0)
            {
                yVal1 = size.Height;
            }
            //increase the value of x and y by the roation angle value multiply by 5
            PointF point = new PointF();
            point.X = xVal1 + (float)Math.Sin(rotation + Math.PI) * acceleration;
            point.Y = yVal1 - (float)Math.Cos(rotation + Math.PI) * acceleration;
            coordinates = point;
        }
    }

    //Make a Scores Class: includes helps setting of Window size, helps count maximum and total scores
    //This is a Miscellaneous class that has alot of helper function
    //makes the code easier to read
    public class Scores
    {
        //set the variables        
        public int max { get; set; } //set the maximum value in scores
        public int level { get; set; }//set the level of the game 
        private int sizeX;//for setting the size of the window in x direction 
        private int sizeY;//for setting the size of the window in y direction 
        //the following two array helps in creation of random rocks 
        public int[] xarray { get; } // x array for storing the maximum width and the min in x direction
        public int[] yarray { get; } // y array for storing the maximum width and the min in y  direction
        public int totalscore { get; set; }//variable to store the total score 

        //once this is called it initializes the variables 
        public Scores(int sizex, int sizey)
        {
            xarray = new int[2];//create array of 2 values in x direction 
            yarray = new int[2];//create array of 2 values in y direction 
            max = 0;//set the maximum score to zero 
            level = 1;//set the level to 1 to begin
            sizeX = sizex; //set the x size value 
            sizeY = sizey;//set the y size value 
            totalscore = 0;// indicate the total score to be 0 
        }

        /// <summary>
        /// This function returns the new window size selected by the user it also sets the new 
        /// sizes for the bounds of the Rocks to be generated from 
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Size GetSize()
        {
            xarray[0] = sizeX - 1;
            xarray[1] = 0;
            yarray[0] = sizeY - 1;
            yarray[1] = 0;
            return new System.Drawing.Size(sizeX, sizeY);
        }

        /// <summary>
        /// This function upgrades the level value when called upon
        /// also this function updates the total amount of rocks per level
        /// </summary>
        public void LevelMaker()
        {
            level++;
            Rock.totalRocks = 2 * level;
        }

        /// <summary>
        /// This function helps calculate the maximum  score 
        /// </summary>
        /// <param name="scores"></param>
        public void MaximumScore(List<int> scores)
        {
            int small = 0;
            foreach (var x in scores)
            {
                if (x > small)
                    max = x;
            }
        }

    }
}
