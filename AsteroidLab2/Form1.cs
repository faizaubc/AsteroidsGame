///This Asteroid Lab has following functionality:
///The left, right keys are used to rotate the ship 
///the up down keys are used to move the ship up and down as a thruster it will slowly accelerate
///the space bar is used to shoot the bullets
///the enter key can pause the game and pressing again will resume it 
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
    public partial class Form1 : Form
    {
        Arrows arrow;//Instance of class Arrow
        //boolean indicators for button presses
        bool shoot = false, m_bLeft = false, m_bRight = false, m_bDown = false, m_bUp = false, pause = false, paused = false, close=false;
        List<Bullets> bullets = new List<Bullets>();//list of bullets
        List<ShapeBase> list = new List<ShapeBase>();//list of rocks
        Random rand = new Random();//random object
        Dictionary<Region, long> dic = new Dictionary<Region, long>(); //dictionary to store collision of bullet and rock
        Dictionary<Region, long> dicarrow = new Dictionary<Region, long>(); //dictionary to store collision of arrow and rock
        Stopwatch sw = new Stopwatch();  //stop watch     
        List<int> scores = new List<int>(); //list of scores to keep track of highest score
        Scores scoreClass;   //score class is a helper class helps calculate total scores, size of the drawer etc.
        bool locking = false; // once the number of rocks for the particular level is released, it locks from producing more 

        public Form1()
        {
            InitializeComponent();
            sw.Start();// start the stop watch

        }
        //form load 
        private void Form1_Load(object sender, EventArgs e)
        {
            int sizeX = 1000;//initialize the size of the drawer in x direction 
            int sizeY = 1000;//initialize the size of the drawer in y direction 
            Size dialog = new Size();// create a dialog for size the initial size is 1000x 1000, user can choose a different screen from this dialog
            if (dialog.ShowDialog() == DialogResult.OK)//if the user clicks okay retrieve the chosen size X and size Y
            {            
                sizeX = dialog.SizeXP;
                sizeY = dialog.SizeYP;
            }

            //create a instance of the store class
            scoreClass = new Scores(sizeX, sizeY);
            //set the new size to the form size and the client size
            this.Size = scoreClass.GetSize();
            ClientSize = scoreClass.GetSize();

            //enable the timer 
            timer1.Enabled = true;
        }

        //Timer tick 
        private void timer1_Tick(object sender, EventArgs e)
        {
            //if the user clicks enter the game is paused and the "paused" variable turns true
            //once the user clicks enter again the game is resumed
            if (pause)
                paused=!paused ;
           
            if (!paused)//if the "paused" is true game is paused 
            {
                // This block demonstrates double-buffering. This will help you avoid screen flickering.
                using (BufferedGraphicsContext bgc = new BufferedGraphicsContext())
                {
                    // create a virtual surface to draw on
                    using (BufferedGraphics bg = bgc.Allocate(CreateGraphics(), ClientRectangle))
                    {
                        bg.Graphics.Clear(Color.Black);//clear the graphics background to black

                        if (arrow == null)//if there is no ship present create one 
                            arrow = (new Arrows(new PointF(ClientSize.Width / 2, ClientSize.Height / 2)));

                        if (shoot)//if the space bar is pressed create an instance of a bullet 
                        {
                            if (bullets.Count() < 8)//as long as number of bullets are less than 8 on the screen, create a bullet.
                            {
                                //add a bullet pass in current coordinates of the arrow and the rotation of the arrow so that 
                                //the bullet shoots in the proper direction
                                bullets.Add(new Bullets(new PointF(arrow.coordinates.X, arrow.coordinates.Y), arrow.rotation));
                            }
                        }

                        //lock so that number of rocks can be produced according to a level played
                        if (!locking)                      
                            CreateRocks();  //create the rocks                        
                                           
                        list.ForEach(s => s.Tick(ClientSize));//for each rock move it 

                        //if the left arrow key is pressed rotate the ship to the left
                        if (m_bLeft)
                            arrow.Tick(ClientSize, -0.1f);
                        //if the right arrow key is pressed rotate the ship to the right
                        if (m_bRight)
                            arrow.Tick(ClientSize, 0.1f);

                        //if the up arrow key is pressed move the ship to the up
                        if (m_bUp)                                             
                            arrow.MoveArrowUp(ClientSize);
                  

                        //if the down arrow key is pressed move the ship to the down
                        if (m_bDown)
                            arrow.MoveArrowDown(ClientSize);

                        
                        //if there are bullets being shot move them 
                        if (bullets.Count > 0)
                            bullets.ForEach(s => s.TickBullet(ClientSize));

                        //render the ship in color red
                        arrow.Render(Color.Red, bg.Graphics);

                        //Intersection Detection for the lock and the bullet
                        IntersectionDetectionBulletAndRock(bg.Graphics);


                        //Intersection Detection for the rock and the ship
                        //the flag collsion stop momentaraly happens when the ship is spawned again and waits for it to slowly accelerate
                        //once it reaches a velocity the flag is turned back to false
                        if(!arrow.collisionStopMomentaraly)
                        IntersectionDetectionShipAndRock(bg.Graphics);
                        
                        //Remove the bullets that are out of the bound of the size of the client window
                        //also render the bullets
                        foreach (var x in bullets.ToList())
                        {
                            //render the bullets in yellow
                            x.Render(Color.Yellow, bg.Graphics);
                            //remove the bullets that are out of the bounds
                           // if (x.coordinates.X > ClientSize.Width || x.coordinates.Y > ClientSize.Height
                            //    || x.coordinates.X < 0 || x.coordinates.Y < 0)                           
                           //    bullets.Remove(x);                           
                        }

                        //Generate rocks according to the radius size if they are marked dead
                        //Also this function renders all the rocks
                        GenerateSmallerRocks(bg.Graphics);
                       
                        //remove all the rocks that are marked dead
                        list.RemoveAll(s => s.dead());

                        //remove all the bullets that are marked dead 
                        bullets.RemoveAll(s => s.dead());

                        //if the arrow is marked dead make it point to null so a new one can be generated
                        if (arrow.dead())                            
                            arrow = null;

                        //if the ship is destroyed, it updates the graphics to show the remaining ships 
                        //by looking at the ship count
                        UpdateArrows(bg.Graphics);

                        //This function shows the level, total score, and a message box to ask user if they want to
                        //continue the game
                        UpdateGraphicsDisplay(bg.Graphics);
                        
                        //render
                        bg.Render();

                        //if the user has clicked no they do not want to continue close the form
                        if (close)
                            this.Close();


                    }

                }
            }

           
        }

        /// <summary>
        /// This function creates Rocks from different directions so they are 
        /// generated from random locations
        /// </summary>
        private void CreateRocks()
        {
            if (list.Count < Rock.totalRocks)
            {
                //the first loop produce a rock from either y = 0 or y = ClientSize.Height
                //the x is random location 
                for (int i = 0; i < Rock.totalRocks / 2; i++)//randomly produce rocks from different directions
                {
                    //the list of array helps choose which direction the rock will be produced from
                    //in this case the y direction can be a 0 or clientheight
                    list.Add(new Rock(new PointF(rand.Next(0, ClientSize.Width), scoreClass.yarray[rand.Next(2)]), 40, 0));
                }


                //the second loop produce a rock from either x = 0 or y = ClientSize.Width
                //the y is random location 
                for (int i = 0; i < Rock.totalRocks / 2; i++)
                {
                    //the list of array helps choose which direction the rock will be produced from
                    //in this case the x direction can be a 0 or clientwidth
                    list.Add(new Rock(new PointF(scoreClass.xarray[rand.Next(2)], rand.Next(0, ClientSize.Height)), 40, 0));
                }
            }
            //set the lock to true so even if the rocks are destroyed more wont be created. 
            locking = true;
        }

        /// <summary>
        /// The following function detects the intersection between the rock and a 
        /// bullet and marks them both dead if there is an intersection
        /// It also shows a red circle around the collisions
        /// </summary>
        /// <param name="bg"></param>
        private void IntersectionDetectionBulletAndRock(Graphics bg)
        {
            //loop through every bullet and compare it to every other rock  in the list of rocks
            foreach (Bullets s1 in bullets)
            {
                foreach (ShapeBase s2 in list)//loop through all the rocks 
                {

                    //if the distance is less than three times the radius draw the circle indicator around the same path
                    if (Distance(s1, s2) < (s2.radius * 2) && !s1.Equals(s2))
                    {
                        //call the function indicate circle to show a collision detection is present
                        IndicateCircleRadius(Color.Red, s1.coordinates.X, s1.coordinates.Y, bg, s1.GetPath(), s2.radius);
                        IndicateCircleRadius(Color.Red, s2.coordinates.X, s2.coordinates.Y, bg, s2.GetPath(), s2.radius);

                        //create a region for both shapes
                        Region RA = new Region(s1.GetPath());
                        Region RB = new Region(s2.GetPath());

                        //check if they intersect
                        RB.Intersect(RA);

                        //the result is stored in the region B 
                        //if the region B is not empty there is intersection that happened 
                        if (!RB.IsEmpty(bg))
                        {
                            //indicate the s1 to be dead and s2 to be dead since they both collided
                            s1.IsMarkedForDeath = true;
                            s2.IsMarkedForDeath = true;
                            //add the collided region and the time elapsed in the dictionary
                            dicarrow.Add(RB, sw.ElapsedMilliseconds);

                        }

                    }

                }

            }
        }

        /// <summary>
        /// This Function helps with the collision between the ship and a rock
        /// if a collison is detected the boolean variable are marked true
        /// </summary>
        /// <param name="bg"></param>
        private void IntersectionDetectionShipAndRock(Graphics bg)
        {
            //loop through all the rocks in the list
            foreach (ShapeBase s2 in list)
            {
                //if the opacity of the rock is full meaning visible is the only way the ship can be destroyed
                if (s2.opacity == 255)
                {
                    //if the distance is less than three times the radius draw the circle indicator around the same path
                    if (DistanceArrow(arrow, s2) < (s2.radius * 2) && !arrow.Equals(s2))
                    {
                        //indicate the red cirle that indicates a collision is near
                        IndicateCircleRadius(Color.Red, arrow.coordinates.X, arrow.coordinates.Y, bg, arrow.GetPath(), s2.radius);
                        IndicateCircleRadius(Color.Red, s2.coordinates.X, s2.coordinates.Y, bg, s2.GetPath(), s2.radius);

                        //create a region for both shapes
                        Region RA = new Region(arrow.GetPath());
                        Region RB = new Region(s2.GetPath());

                        //check if they intersect
                        RB.Intersect(RA);

                        //the result is stored in the region B 
                        //if the region B is not empty there is intersection that happened 
                        if (!RB.IsEmpty(bg))
                        {
                            //indicate the s1 to be dead and s2 to be dead since they both collided
                            arrow.IsMarkedForDeath = true;
                            s2.IsMarkedForDeath = true;
                            //add the collided region and the time elapsed in the dictionary
                            dic.Add(RB, sw.ElapsedMilliseconds);
                            //if the ship has been destroyed one update the ship count
                            Arrows.shipcount += 1;
                            //decrease the acceleration once the arrow is hit 
                            arrow.acceleration = 1;
                            //set the stop collision to true for a while till the ship is accelerated 
                            arrow.collisionStopMomentaraly = true;
                        }

                    }
                }
            }

        }

        /// <summary>
        /// This function helps calculate distance between the bullet and a rock
        /// this helps in collision detection to determine if the two have collided
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private float Distance(Bullets s1, ShapeBase s2)
        {
            //returns the distance between the two that is a bullet and a rock
            return (float)Math.Sqrt(Math.Pow(s1.coordinates.X - s2.coordinates.X, 2) + Math.Pow(s1.coordinates.Y - s2.coordinates.Y, 2));
        }

        /// <summary>
        /// This helps detect the distance between an arrow and a rock
        ///this helps in collision detection if two are collided 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private float DistanceArrow(Arrows s1, ShapeBase s2)
        {
            //returns the distance between the two that is a arrow and a rock
            return (float)Math.Sqrt(Math.Pow(s1.coordinates.X - s2.coordinates.X, 2) + Math.Pow(s1.coordinates.Y - s2.coordinates.Y, 2));
        }

        /// <summary>
        /// This helps indicate a cirlce around a object that is almost about to collide 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gr"></param>
        /// <param name="gp"></param>
        /// <param name="radius"></param>
        public void IndicateCircleRadius(Color c, float x, float y, Graphics gr, GraphicsPath gp, float radius)
        {
            //create a path
            GraphicsPath tGP = (GraphicsPath)gp.Clone();
            //add a ellipse to the path
            tGP.AddEllipse(x - radius, y - radius, radius * 2, radius * 2);
            //draw the path
            gr.DrawPath(new Pen(c), tGP);
        }

        /// <summary>
        /// This Functions helps generate the smaller rocks depending on the radius of the dead rock
        /// if the radius is the largest two medium rocks are generated
        /// if the radius is the medium three small rocks are generated
        /// if the radius is small the rock is completely destroyed
        /// </summary>
        /// <param name="bg"></param>
        private void GenerateSmallerRocks(Graphics bg)
        {
            //create a copy of the current list
            List<ShapeBase> list1 = new List<ShapeBase>(list);
            foreach (var x in list)
            {
                if (x.dead())//if the rock is marked dead
                {
                    if (x.radius == 30)//if the radius is medium genrate 3 small rocks 
                    {
                        list1.Add(new Rock(new PointF(x.coordinates.X, x.coordinates.Y), 20, 255));
                        list1.Add(new Rock(new PointF(x.coordinates.X, x.coordinates.Y), 20, 255));
                        list1.Add(new Rock(new PointF(x.coordinates.X, x.coordinates.Y), 20, 255));
                        scoreClass.totalscore += 200;//count the score 
                    }
                    else if (x.radius == 40)//if the radius is large generate 2 medium rocks 
                    {
                        list1.Add(new Rock(new PointF(x.coordinates.X, x.coordinates.Y), 30, 255));
                        list1.Add(new Rock(new PointF(x.coordinates.X, x.coordinates.Y), 30, 255));

                        scoreClass.totalscore += 100;//count the score 
                    }
                    else if (x.radius == 20)// if the radius is small add up the score                                
                        scoreClass.totalscore += 300;

                }

                //render the rocks 
                x.Render(Color.Gray, bg);
                // if (x.coordinates.X > (ClientSize.Width) || (x.coordinates.X) < 0)
                //     list1.Remove(x);
                // if (x.coordinates.Y > (ClientSize.Height) || (x.coordinates.Y) < 0)
                //     list1.Remove(x);
            }
            list = list1;//assign the original list to the new list
        }

        /// <summary>
        /// The Arrows are updates and drawn depending on the number of ships
        /// available
        /// </summary>
        /// <param name="bg"></param>
        private void UpdateArrows(Graphics bg)
        {
            //if there are 0 ships dead draw the 3 ships utlizing the Generate Transforms helper function. 
            if (Arrows.shipcount == 0)
            {
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(40));
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(60));
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(80));
            }
            //if there are 1 ships dead draw the 2 ships utlizing the Generate Transforms helper function. 
            else if (Arrows.shipcount == 1)
            {
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(40));
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(60));
            }
            //if there are 2 ships dead draw the 1 ship utlizing the Generate Transforms helper function. 
            else if (Arrows.shipcount == 2)
            {
                bg.FillPath(new SolidBrush(Color.Blue), GenerateTransforms(40));
            }
        }

        /// <summary>
        /// This function updates the level and the total score of the game
        /// once the level is passed or failed a message box is displayed to ask the user if they want to 
        /// continue the game or quit
        /// </summary>
        /// <param name="bg"></param>
        private void UpdateGraphicsDisplay(Graphics bg)
        {
            //create a point to display the graphics at
            Point point1 = new Point(30, 10);
            Point point3 = new Point(30, 80);

            //if the ship has died less than 3 times and the rocks are still alive 
            //show the level , score and the high score
            if (Arrows.shipcount <= 3 && list.Count > 0)
            {
                Font font = new Font("Times New Roman", 24, FontStyle.Bold, GraphicsUnit.Pixel);
                TextRenderer.DrawText(bg, "Level :" + scoreClass.level + " Score: " + scoreClass.totalscore.ToString(), font, point1, Color.Blue);
                TextRenderer.DrawText(bg, "Highest Score: " + scoreClass.max, font, point3, Color.Blue);
            }
            else//if the ship died and the rocks are still in the list 
            {
                Point point2 = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
                Font font = new Font("Times New Roman", 24, FontStyle.Bold, GraphicsUnit.Pixel);
                //show the score and the level
                TextRenderer.DrawText(bg, "Level " + scoreClass.level + " Score: " + scoreClass.totalscore.ToString(), font, point1, Color.Blue);
                Font font2 = new Font("Times New Roman", 100, FontStyle.Bold, GraphicsUnit.Pixel);
                //indicate that the game is over
                TextRenderer.DrawText(bg, "Game Over", font, point2, Color.Blue);
                //pause the game 
                paused = true;
                //add the score to the list of scores
                scores.Add(scoreClass.totalscore);
                //point arrow to null
                arrow = null;
                //create a message label for the message box
                string message = " ";
                
                //if the rocks are killed show the correct message
                if (list.Count == 0)
                    message = "You passed level " + scoreClass.level + " play the next level ??";
                else//if the rocks are remaining indicate in the message box appropriately
                    message = "You failed level " + scoreClass.level + " Play Again ??";
                //create a messagebox
                DialogResult result = MessageBox.Show(message, "Play again", MessageBoxButtons.YesNo);
                //if the user clicked yes 
                if (result == DialogResult.Yes)
                {
                    if (list.Count == 0)//and the rocks are all destroyed upgrade the level
                        scoreClass.LevelMaker();
                    //set the total sccore variable to 0
                    scoreClass.totalscore = 0;
                    //set the total ship dead variable to 0
                    Arrows.shipcount = 0;
                    //set the paused to false so the game resumes
                    paused = false;
                    //set all the boolean values to false
                    m_bDown = false;
                    m_bLeft = false;
                    m_bRight = false;
                    m_bUp = false;
                    //set the locking to false
                    locking = false;
                    //clear the bullets and clear the lists
                    bullets.Clear();
                    list.Clear();
                    //clear the dictionaries
                    dic.ToList().Clear();
                    dicarrow.ToList().Clear();
                    //calculate and update the maximum score 
                    scoreClass.MaximumScore(scores);

                }
                else               
                   close = true;
                



            }
        }

        /// <summary>
        /// This is a helper function for the update arrows function it draws the arrow at a correct location
        /// using combination of translation
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private GraphicsPath GenerateTransforms(int size)
        {
            //creates a ship 
            //create a points of array 
            PointF[] points = new PointF[4];
            //create a graphics path 
            GraphicsPath gp = new GraphicsPath();
            //add points to the array of points
            points[0] = new PointF(0, 0);
            points[1] = new PointF(-10, 20);
            points[2] = new PointF(0, 10);
            points[3] = new PointF(10, 20);
            //create a path 
            gp.StartFigure();
            gp.AddLines(points);
            gp.CloseFigure();
           
            //apply transformation according to the suplied x argument on the same row
            Matrix transforms = new Matrix();         
            transforms.Translate(size, 40);           
            GraphicsPath obj = (GraphicsPath)gp.Clone();  
            obj.Transform(transforms);
            //returns the new path with the ship drawn 
            return obj;
        }

        //Key Down function to help control the user interaction
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                m_bLeft = true;
            if (e.KeyCode == Keys.Right)
                m_bRight = true;
            if (e.KeyCode == Keys.Space)
                shoot = true;
            if (e.KeyCode == Keys.Enter)
                pause = true;
            if (e.KeyCode == Keys.Up)
                m_bUp = true;
            if (e.KeyCode == Keys.Down)
                m_bDown = true;

        }

        //Key Up Function 
        //The implementation of thusters is implemented here for change in direction 
        //upon the release of the key
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                m_bLeft = false;
            if (e.KeyCode == Keys.Right)
                m_bRight = false;
            if (e.KeyCode == Keys.Space)
                shoot = false;
            if (e.KeyCode == Keys.Enter)
                pause = false;
            //allow the thusters to change direction if up is realeased make sure the 
            //the down motion is false
            if (e.KeyCode == Keys.Up)
            {
                m_bDown = false;
                arrow.acceleration = 1;
            }
            //if down key is released make sure up motion is false 
            if (e.KeyCode == Keys.Down)
            {
                m_bUp = false;
                arrow.acceleration = 1;
            }
           
        }


    }
}
