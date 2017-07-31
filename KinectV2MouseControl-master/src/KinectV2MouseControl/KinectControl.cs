using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Kinect;
using WindowsInput;

namespace KinectV2MouseControl
{
    class KinectControl
    {
        /// Active Kinect sensor
        KinectSensor sensor;
        /// Reader for body frames
        BodyFrameReader bodyFrameReader;
        /// Array for the bodies
        private Body[] bodies = null;
        /// Screen width and height for determining the exact mouse sensitivity
        public int screenWidth, screenHeight;
        /// timer for pause-to-click feature
        DispatcherTimer timer = new DispatcherTimer();
        /// How far the cursor move according to your hand's movement
        public float mouseSensitivity = MOUSE_SENSITIVITY;
        /// Time required as a pause-clicking
        public float timeRequired = TIME_REQUIRED;
        /// The radius range your hand move inside a circle for [timeRequired] seconds would be regarded as a pause-clicking
        public float pauseThresold = PAUSE_THRESOLD;
        /// Decide if the user need to do clicks or only move the cursor
        public bool doClick = DO_CLICK;
        /// Use Grip gesture to click or not
        public bool useGripGesture = USE_GRIP_GESTURE;
        /// Value 0 - 0.95f, the larger it is, the smoother the cursor would move
        public float cursorSmoothing = CURSOR_SMOOTHING;
        // following values are used to get the values to show in the debug app
        public float left_x = 0.0f;
        //public float left_x1 = 0.0f;
        public float left_y = 0.0f;
        public float left_z = 0.0f;
        public float right_x = 0.0f;
        //public float right_x1 = 0.0f;
        public float right_y = 0.0f;
        public float right_z = 0.0f;
        public float right_left_x = 0.0f;
        public float right_left_y = 0.0f;
        public float right_left_z = 0.0f;
        public float handdistance = 0.0f;
        public float handdistance1 = 0.0f;
        public float spine_x = 0.0f;
        public float spine_y = 0.0f;
        public float spine_z = 0.0f;
        public Int32 cursor_x = 0;
        public Int32 cursor_y = 0;
        // Default values
        public const float MOUSE_SENSITIVITY = 3.5f;
        public const float TIME_REQUIRED = 2f;
        public const float PAUSE_THRESOLD = 60f;
        public const bool DO_CLICK = true;
        public const bool USE_GRIP_GESTURE = true;
        public const float CURSOR_SMOOTHING = 0.4f;
        /// Determine if we have tracked the hand and used it to move the cursor,
        /// If false, meaning the user may not lift their hands, we don't get the last hand position and some actions like pause-to-click won't be executed.
        bool alreadyTrackedPos = false;
        /// for storing the time passed for pause-to-click
        float timeCount = 0;
        /// For storing last cursor position
        Point lastCurPos = new Point(0, 0);
        /// If true, user did a left hand Grip gesture
        bool wasLeftGrip = false;
        /// If true, user did a right hand Grip gesture
        bool wasRightGrip = false;
        /// sleeptime is used for the keypress event
        private const int sleeptime = 85;
        /// If true, user did a right hand Grip gesture
        bool wasLRGrip = false;
        /// this kinect control is for setting up the program to connect to the connect
        /// and also setting up the timer for the pause to click
        public KinectControl()
        {
            // get Active Kinect Sensor
            sensor = KinectSensor.GetDefault();
            // open the reader for the body frames
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
            // get the screen x,y values
            this.screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            this.screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            // set up timer, execute every 0.1s
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100); 
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
            // open the sensor
            sensor.Open();
        }
        /// Pause to click timer
        void Timer_Tick(object sender, EventArgs e)
        {
            if (!doClick || useGripGesture)
                return;

            if (!alreadyTrackedPos)
            {
                timeCount = 0;
                return;
            }
            
            Point curPos = MouseControl.GetCursorPosition();

            if (((lastCurPos - curPos).Length < pauseThresold) && ((timeCount += 0.1f) > timeRequired))
            {
                MouseControl.DoMouseClick();
                timeCount = 0;
            }
            else
                timeCount = 0;

            lastCurPos = curPos;
        }
        /// Read body frames
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                        this.bodies = new Body[bodyFrame.BodyCount];
                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived) 
            {
                alreadyTrackedPos = false;
                return;
            }
            
            foreach (Body body in this.bodies)
            {
                // get first tracked body only, notice there's a break below.
                if (body.IsTracked)
                {
                    // get various skeletal positions
                    CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                    CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;
                    /* hand x calculated by this. we don't use shoulder right as a reference cause the shoulder right
                     * is usually behind the lift right hand, and the position would be inferred and unstable.
                     * because the spine base is on the left of right hand, we plus 0.05f to make it closer to the right.*/
                    float rx = this.right_x = handRight.X - spineBase.X + 0.05f;
                    /* hand y calculated by this. ss spine base is way lower than right hand, we plus 0.51f to make it
                     * higher, the value 0.51f is worked out by testing for a several times, you can set it as another one you like.*/
                    float ry = this.right_y = spineBase.Y - handRight.Y + 0.51f;
                    this.right_z = handRight.Z - spineBase.Z;
                    float lx = this.left_x = handLeft.X - spineBase.X + 0.3f;
                    float ly = this.left_y = spineBase.Y - handLeft.Y + 0.51f;
                    this.left_z = spineBase.Z - handLeft.Z;

                    this.spine_x = spineBase.X;
                    this.spine_y = spineBase.Y;
                    this.spine_z = spineBase.Z;
                    this.right_left_x = this.right_x - this.left_x;
                    this.right_left_y = this.right_y + this.left_y;
                    this.handdistance = (float)Math.Sqrt(Math.Pow(this.right_left_x, 2) + Math.Pow(this.right_left_y, 2) + Math.Pow(this.right_left_z, 2));
                    //if both hands lift up
                    if ((handRight.Z - spineBase.Z < -0.15f) && (handLeft.Z - spineBase.Z < -0.15f))
                    {
                        //this portion (if clause) was added to add another guesture
                        //specifically the windows magnification tool using keyboard shortcut 'windows'+'+' or 'windows'+'-'
                        alreadyTrackedPos = true;
                        if (this.handdistance1 == 0)
                            System.Diagnostics.Process.Start("C:\\Windows\\System32\\Magnify.exe");

                        InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
                        //this line is used to give the system time to recognize the keypress
                        System.Threading.Thread.Sleep(sleeptime);
                        bool righth_closed = body.HandRightState == HandState.Closed;
                        bool lefth_closed = body.HandLeftState == HandState.Closed;
                        //if both hands are closed zoom in
                        if(righth_closed && lefth_closed)//if need be change to this.handdistance < 1.20
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.ADD);
                        //if right hand is closed move cursor with left hand
                        else if (righth_closed)
                            setcursor(lx, ly, body);
                        //if left hand is closed move cursor with right hand
                        else if (lefth_closed)
                            setcursor(rx, ry, body);
                        //if both hands open zoom out until out of this loop
                        else if(!righth_closed && !lefth_closed)
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.SUBTRACT);
                        else {/*unknown hand state*/}
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else if (handRight.Z - spineBase.Z < -0.15f) // if right hand lift forward
                        setcursor(rx, ry, body);
                    else if (handLeft.Z - spineBase.Z < -0.15f)
                        setcursor(lx, ly, body);
                    else
                    {
                        wasLeftGrip = true;
                        wasRightGrip = true;
                        alreadyTrackedPos = false;
                    }
                    // get first tracked body only
                    break;
                }
            }
        }
        /// this function is used to set the cursor relative to the left/right hand position towards the screen
        private void setcursor (float x, float y, Body body)
        {
            //killmag();
            Point curPos = MouseControl.GetCursorPosition();
            // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
            float smoothing = 1 - cursorSmoothing;
            // set cursor position
            this.cursor_x = (int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing);
            this.cursor_y = (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing);
            MouseControl.SetCursorPos(this.cursor_x, this.cursor_y);
            alreadyTrackedPos = true;

            // Grip gesture
            /// the following is used to click
            if (doClick && useGripGesture)
            {
                if ((body.HandRightState == HandState.Closed) && !wasLRGrip)
                {
                    MouseControl.MouseLeftDown();
                    wasLRGrip = true;
                }
                else if ((body.HandRightState == HandState.Open || body.HandRightState == HandState.Lasso) && wasLRGrip)
                {
                    MouseControl.MouseLeftUp();
                    wasLRGrip = false;
                }
            }
        }
        /// this killmag funciton is used to kill the magnify application when not using the magnify application
        private void killmag ()
        {
            try
            {
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("Magnify");
                foreach (var process in processes)
                    process.Kill();
            }
            catch { } // for catching any exceptions when killing the process "Magnify"
        }
        /// this close function is used to stop the timer
        /// and disconnect from the sensor when the program closes
        public void Close()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }
    }
}