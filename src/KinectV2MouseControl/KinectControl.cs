﻿using System;
using System.Windows;
//using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Kinect;
using WindowsInput;

namespace KinectV2MouseControl
{
    class KinectControl
    {
        //DebugWindow dbg = new DebugWindow();
        /*
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        */
        KinectSensor sensor;
        /*
        /// <summary>
        /// Reader for body frames
        /// </summary>
        */
        BodyFrameReader bodyFrameReader;
        /*
        /// <summary>
        /// Array for the bodies
        /// </summary>
        */
        private Body[] bodies = null;
        /*
        /// <summary>
        /// Screen width and height for determining the exact mouse sensitivity
        /// </summary>
        */
        public int screenWidth, screenHeight;
        /*
        /// <summary>
        /// timer for pause-to-click feature
        /// </summary>
        */
        DispatcherTimer timer = new DispatcherTimer();
        /*
        /// <summary>
        /// How far the cursor move according to your hand's movement
        /// </summary>
        */
        public float mouseSensitivity = MOUSE_SENSITIVITY;
        /*
        /// <summary>
        /// Time required as a pause-clicking
        /// </summary>
        */
        public float timeRequired = TIME_REQUIRED;
        /*
        /// <summary>
        /// The radius range your hand move inside a circle for [timeRequired] seconds would be regarded as a pause-clicking
        /// </summary>
        */
        public float pauseThresold = PAUSE_THRESOLD;
        /*
        /// <summary>
        /// Decide if the user need to do clicks or only move the cursor
        /// </summary>
        */
        public bool doClick = DO_CLICK;
        /*
        /// <summary>
        /// Use Grip gesture to click or not
        /// </summary>
        */
        public bool useGripGesture = USE_GRIP_GESTURE;
        /*
        /// <summary>
        /// Value 0 - 0.95f, the larger it is, the smoother the cursor would move
        /// </summary>
        */
        public float cursorSmoothing = CURSOR_SMOOTHING;
        /* following values are used to get the values to show in the debug app
         * */
        public float left_x = 0.0f;
        public float left_x1 = 0.0f;
        public float left_y = 0.0f;
        public float left_z = 0.0f;
        public float right_x = 0.0f;
        public float right_x1 = 0.0f;
        public float right_y = 0.0f;
        public float right_z = 0.0f;
        public float left_right_x = 0.0f;
        public float left_right_y = 0.0f;
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
        /*
        /// <summary>
        /// Determine if we have tracked the hand and used it to move the cursor,
        /// If false, meaning the user may not lift their hands, we don't get the last hand position and some actions like pause-to-click won't be executed.
        /// </summary>
        */
        bool alreadyTrackedPos = false;
        
        /// <summary>
        /// for storing the time passed for pause-to-click
        /// </summary>
        float timeCount = 0;
        /// <summary>
        /// For storing last cursor position
        /// </summary>
        Point lastCurPos = new Point(0, 0);

        /// <summary>
        /// If true, user did a left hand Grip gesture
        /// </summary>
        bool wasLeftGrip = false;
        /// <summary>
        /// If true, user did a right hand Grip gesture
        /// </summary>
        bool wasRightGrip = false;

        public KinectControl()
        {
            // get Active Kinect Sensor
            sensor = KinectSensor.GetDefault();
            // open the reader for the body frames
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // get screen with and height
            this.screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            this.screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // set up timer, execute every 0.1s
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100); 
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            // open the sensor
            sensor.Open();
        }
        /*
        /// <summary>
        /// Pause to click timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        */
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
                //MouseControl.MouseLeftDown();
                //MouseControl.MouseLeftUp();
                MouseControl.DoMouseClick();
                timeCount = 0;
            }
            else
                timeCount = 0;

            lastCurPos = curPos;
        }

        /// <summary>
        /// Read body frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            //dbg.refresh_lbls();
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                        this.bodies = new Body[bodyFrame.BodyCount];
                    /*
                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    */
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
                    this.spine_x = spineBase.X;
                    this.spine_y = spineBase.Y;
                    this.spine_z = spineBase.Z;
                    this.screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                    this.screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                    //if both hands lift up
                    if ((handRight.Z - spineBase.Z < -0.15f) && (handLeft.Z - spineBase.Z < -0.15f))
                    {
                        // this portion of the code is used to return the x,y values of the cursor to the gui
                        Point curPos = MouseControl.GetCursorPosition();
                        this.cursor_x = (int)curPos.X;
                        this.cursor_y = (int)curPos.Y;
                        //this portion (if clause) was added to add another guesture
                        //specifically the windows magnification tool using keyboard shortcut 'windows'+'+' or 'windows'+'-'
                        this.right_x = handRight.X - spineBase.X + 0.05f;
                        this.right_y = handRight.Y - spineBase.Y + 0.51f;
                        this.right_z = handRight.Z - spineBase.Z;
                        this.left_x = handLeft.X - spineBase.X + 0.3f;
                        this.left_y = spineBase.Y - handLeft.Y + 0.51f;
                        this.left_z = spineBase.Z - handLeft.Z;
                        this.left_right_x = left_x - right_x;
                        this.left_right_y = left_x - right_x;

                        if (left_right_x < 0)
                            left_right_x *= (-1);
                        if (left_right_y < 0)
                            left_right_y *= (-1);

                        alreadyTrackedPos = true;
                        
                        if (((body.HandRightState == HandState.Closed) && !wasRightGrip)
                            && ((body.HandLeftState == HandState.Closed) && !wasLeftGrip))
                        {
                            InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
                            // using this logic: (handRight.Z - spineBase.Z < -0.15f) && (handLeft.Z - spineBase.Z < -0.15f)
                            // we can denote that the x and y are relative to the spine instead of the 0,0 of the kinect sensor
                            // thus we get positive values on the right hand and negative values on the left
                            right_x = handRight.X - spineBase.X;
                            left_x = handLeft.X - spineBase.X;
                            
                            if (left_x1 == right_x1 && right_x1 == 0) //this is used to start up windows magnify tool if this piece of code is checked for the first time
                                System.Diagnostics.Process.Start("C:\\WINDOWS\\system32\\Magnifier.exe");//a '\' is used for escape sequence so \\ is needed

                            if ((left_x == left_x1) && (right_x == right_x1)); //visual studio will note this semi colon as a mistakenly empty statement -> intentional
                            //when both hands are closed and are moving in and out to zoom in/out
                            else if ((left_x > left_x1) && (right_x < right_x1)) // send zoom in
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.ADD);
                            else if ((left_x < left_x1) && (right_x > right_x1)) // send zoom out
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.SUBTRACT);
                            
                            //setting the new values to old values
                            right_x1 = right_x;
                            left_x1 = left_x;

                            wasRightGrip = true;
                            wasLeftGrip = true;
                        }
                        else if (((body.HandRightState == HandState.Open || body.HandRightState == HandState.Lasso) && wasRightGrip)
                            && ((body.HandLeftState == HandState.Open || body.HandLeftState == HandState.Lasso) && wasLeftGrip))
                        {
                            InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
                            wasRightGrip = false;
                            wasLeftGrip = false;
                        }
                    }
                    else if (handRight.Z - spineBase.Z < -0.15f) // if right hand lift forward
                    {
                        /*
                         *hand x calculated by this. we don't use shoulder right as a reference cause the shoulder right
                         * is usually behind the lift right hand, and the position would be inferred and unstable.
                         * because the spine base is on the left of right hand, we plus 0.05f to make it closer to the right. 
                         */
                        float x = handRight.X - spineBase.X + 0.05f;
                        /* 
                         * hand y calculated by this. ss spine base is way lower than right hand, we plus 0.51f to make it
                         * higher, the value 0.51f is worked out by testing for a several times, you can set it as another one you like.
                         */
                        float y = spineBase.Y - handRight.Y + 0.51f;
                        this.right_x = handRight.X;
                        this.right_y = handRight.Y;
                        this.right_z = handRight.Z;
                        this.left_right_x = left_x - right_x;
                        this.left_right_y = left_x - right_x;

                        if (left_right_x < 0)
                            left_right_x *= (-1);
                        if (left_right_y < 0)
                            left_right_y *= (-1);

                        // get current cursor position
                        Point curPos = MouseControl.GetCursorPosition();
                        // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
                        float smoothing = 1 - cursorSmoothing;
                        // set cursor position
                        this.cursor_x = (int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing);
                        this.cursor_y = (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing);
                        MouseControl.SetCursorPos(this.cursor_x, this.cursor_y);
                        
                        alreadyTrackedPos = true;

                        this.left_x = handLeft.X;
                        this.left_y = handLeft.Y;
                        this.left_z = handLeft.Z;

                        this.cursor_x = (int)curPos.X;
                        this.cursor_y = (int)curPos.Y;

                        // Grip gesture
                        if (doClick && useGripGesture)
                        {
                            if ((body.HandRightState == HandState.Closed) && !wasRightGrip)
                            {
                                MouseControl.MouseLeftDown();
                                wasRightGrip = true;
                            }
                            else if ((body.HandRightState == HandState.Open || body.HandRightState == HandState.Lasso) && wasRightGrip)
                            {
                                MouseControl.MouseLeftUp();
                                wasRightGrip = false;
                            }
                        }
                    }
                    else if (handLeft.Z - spineBase.Z < -0.15f) // if left hand lift forward
                    {
                        float x = handLeft.X - spineBase.X + 0.3f;
                        float y = spineBase.Y - handLeft.Y + 0.51f;
                        this.left_x = handLeft.X;
                        this.left_y = handLeft.Y;
                        this.left_z = handLeft.Z;
                        Point curPos = MouseControl.GetCursorPosition();
                        float smoothing = 1 - cursorSmoothing;
                        this.cursor_x = (int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing);
                        this.cursor_y = (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing);
                        MouseControl.SetCursorPos(this.cursor_x,this.cursor_y);
                        alreadyTrackedPos = true;

                        this.right_x = handRight.X;
                        this.right_y = spineBase.Y;
                        this.right_z = handRight.Z;
                        this.left_right_x = left_x - right_x;
                        this.left_right_y = left_x - right_x;

                        if (left_right_x < 0)
                            left_right_x *= (-1);
                        if (left_right_y < 0)
                            left_right_y *= (-1);

                        this.cursor_x = (int)curPos.X;
                        this.cursor_y = (int)curPos.Y;

                        if (doClick && useGripGesture)
                        {
                            if ((body.HandLeftState == HandState.Closed) && !wasLeftGrip)
                            {
                                MouseControl.MouseLeftDown();
                                wasLeftGrip = true;
                            }
                            else if ((body.HandLeftState == HandState.Open || body.HandLeftState == HandState.Lasso) && wasLeftGrip)
                            {
                                MouseControl.MouseLeftUp();
                                wasLeftGrip = false;
                            }
                        }
                    }
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