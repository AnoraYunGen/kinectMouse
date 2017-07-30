using System.Windows;

namespace KinectV2MouseControl
{
    public partial class MainWindow : Window
    {
        KinectControl kinectCtrl = new KinectControl();

        System.Windows.Forms.Timer refresh_tmr = new System.Windows.Forms.Timer();
        const int timer_ms = 500; // 500 ms for timer refresh

        public MainWindow()
        {
            InitializeComponent();
            refresh_tmr.Tick += new System.EventHandler(refresh_lbls);
            refresh_tmr.Interval = (1) * (timer_ms); // refresh every 1/2 second
            refresh_tmr.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MouseSensitivity.Value = Properties.Settings.Default.MouseSensitivity;
            PauseToClickTime.Value = Properties.Settings.Default.PauseToClickTime;
            PauseThresold.Value = Properties.Settings.Default.PauseThresold;
            chkNoClick.IsChecked = !Properties.Settings.Default.DoClick;
            CursorSmoothing.Value = Properties.Settings.Default.CursorSmoothing;
            
            if (Properties.Settings.Default.GripGesture)
                rdiGrip.IsChecked = true;
            else
                rdiPause.IsChecked = true;
        }

        private void MouseSensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtMouseSensitivity.IsLoaded)
            {
                kinectCtrl.mouseSensitivity = (float)MouseSensitivity.Value;
                txtMouseSensitivity.Text = kinectCtrl.mouseSensitivity.ToString("f2");
            }
        }

        private void PauseToClickTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PauseToClickTime.IsLoaded)
            {
                kinectCtrl.timeRequired = (float)PauseToClickTime.Value;
                txtTimeRequired.Text = kinectCtrl.timeRequired.ToString("f2");
            }
        }

        private void PauseThresold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PauseThresold.IsLoaded)
            {
                kinectCtrl.pauseThresold = (float)PauseThresold.Value;
                txtPauseThresold.Text = kinectCtrl.pauseThresold.ToString("f2");
            }
        }

        private void CursorSmoothing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CursorSmoothing.IsLoaded)
            {
                kinectCtrl.cursorSmoothing = (float)CursorSmoothing.Value;
                txtCursorSmoothing.Text = kinectCtrl.cursorSmoothing.ToString("f2");
            }
        }

        private void btn_Default_Click(object sender, RoutedEventArgs e)
        {
            MouseSensitivity.Value = KinectControl.MOUSE_SENSITIVITY;
            PauseToClickTime.Value = KinectControl.TIME_REQUIRED;
            PauseThresold.Value = KinectControl.PAUSE_THRESOLD;
            CursorSmoothing.Value = KinectControl.CURSOR_SMOOTHING;

            chkNoClick.IsChecked = !KinectControl.DO_CLICK;
            rdiGrip.IsChecked = KinectControl.USE_GRIP_GESTURE;
            
            rfsh_lbls();
        }
        
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MouseSensitivity = (float)MouseSensitivity.Value;
            Properties.Settings.Default.PauseToClickTime = (float)PauseToClickTime.Value;
            Properties.Settings.Default.PauseThresold = (float)PauseThresold.Value;
            Properties.Settings.Default.CursorSmoothing = (float)CursorSmoothing.Value;
            Properties.Settings.Default.PauseThresold = kinectCtrl.pauseThresold;
            Properties.Settings.Default.GripGesture = kinectCtrl.useGripGesture;
            Properties.Settings.Default.DoClick = kinectCtrl.doClick;
            Properties.Settings.Default.Save();
        }

        private void chkNoClick_Checked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }

        public void chkNoClickChange()
        {
            kinectCtrl.doClick = !chkNoClick.IsChecked.Value;
        }

        private void chkNoClick_Unchecked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectCtrl.Close();
        }

        public void rdiGripGestureChange()
        {
            kinectCtrl.useGripGesture = rdiGrip.IsChecked.Value;
        }

        private void rdiGrip_Checked(object sender, RoutedEventArgs e)
        {
            rdiGripGestureChange();
        }

        private void rdiPause_Checked(object sender, RoutedEventArgs e)
        {
            rdiGripGestureChange();
        }

        private void btn_msn_Click(object sender, RoutedEventArgs e)
        {
            MouseSensitivity.Value = kinectCtrl.mouseSensitivity = (float)kinectCtrl.mouseSensitivity - (float)0.1;
            if (kinectCtrl.mouseSensitivity < 1)
                MouseSensitivity.Value = kinectCtrl.mouseSensitivity = (float)1;
            txtMouseSensitivity.Text = kinectCtrl.mouseSensitivity.ToString("f2");
        }

        private void btn_msp_Click(object sender, RoutedEventArgs e)
        {
            MouseSensitivity.Value = kinectCtrl.mouseSensitivity = (float)kinectCtrl.mouseSensitivity + (float)0.1;
            if (kinectCtrl.mouseSensitivity > 5)
                MouseSensitivity.Value = kinectCtrl.mouseSensitivity = (float)5;
            txtMouseSensitivity.Text = kinectCtrl.mouseSensitivity.ToString("f2");
        }

        private void btn_trn_Click(object sender, RoutedEventArgs e)
        {
            PauseToClickTime.Value = kinectCtrl.timeRequired = (float)kinectCtrl.timeRequired - (float)0.1;
            if (kinectCtrl.timeRequired < 0.3)
                PauseToClickTime.Value = kinectCtrl.timeRequired = (float)0.3;
            txtTimeRequired.Text = kinectCtrl.timeRequired.ToString("f2");
        }

        private void btn_trp_Click(object sender, RoutedEventArgs e)
        {
            PauseToClickTime.Value = kinectCtrl.timeRequired = (float)kinectCtrl.timeRequired + (float)0.1;
            if (kinectCtrl.timeRequired > 5)
                PauseToClickTime.Value = kinectCtrl.timeRequired = (float)5;
            txtTimeRequired.Text = kinectCtrl.timeRequired.ToString("f2");
        }

        private void btn_mtn_Click(object sender, RoutedEventArgs e)
        {
            PauseThresold.Value = kinectCtrl.pauseThresold = (float)kinectCtrl.pauseThresold - (float)1;
            if (kinectCtrl.pauseThresold < 10)
                PauseThresold.Value = kinectCtrl.pauseThresold = (float)10;
            txtPauseThresold.Text = kinectCtrl.pauseThresold.ToString("f2");
        }

        private void btn_mtp_Click(object sender, RoutedEventArgs e)
        {
                PauseThresold.Value = kinectCtrl.pauseThresold = (float)kinectCtrl.pauseThresold + (float)1;
                if (kinectCtrl.pauseThresold > 160)
                    PauseThresold.Value = kinectCtrl.pauseThresold = (float)160;
                txtPauseThresold.Text = kinectCtrl.pauseThresold.ToString("f2");
        }

        private void btn_csn_Click(object sender, RoutedEventArgs e)
        {
            CursorSmoothing.Value = kinectCtrl.cursorSmoothing = (float)kinectCtrl.cursorSmoothing - (float)0.05;
            if (kinectCtrl.cursorSmoothing < 0)
                CursorSmoothing.Value = kinectCtrl.cursorSmoothing = (float)0;
            txtCursorSmoothing.Text = kinectCtrl.cursorSmoothing.ToString("f2");
        }

        private void btn_csp_Click(object sender, RoutedEventArgs e)
        {
            CursorSmoothing.Value = kinectCtrl.cursorSmoothing = (float)kinectCtrl.cursorSmoothing + (float)0.05;
            if (kinectCtrl.cursorSmoothing > 1)
                CursorSmoothing.Value = kinectCtrl.cursorSmoothing = (float)1;
            txtCursorSmoothing.Text = kinectCtrl.cursorSmoothing.ToString("f2");
        }

        private void refresh_lbls(object sender, System.EventArgs e)
        {
            rfsh_lbls();
        }

        private void rfsh_lbls()
        {
            //Main menu value updates
            txtMouseSensitivity.Text = kinectCtrl.mouseSensitivity.ToString("f2");
            txtTimeRequired.Text = kinectCtrl.timeRequired.ToString("f2");
            txtPauseThresold.Text = kinectCtrl.pauseThresold.ToString("f2");
            txtCursorSmoothing.Text = kinectCtrl.cursorSmoothing.ToString("f2");

            //Debug Menu value updates
            rix_val.Text = kinectCtrl.right_x.ToString("f2");
            lex_val.Text = kinectCtrl.left_x.ToString("f2");
            mox_val.Text = kinectCtrl.cursor_x.ToString("f2");
            spx_val.Text = kinectCtrl.spine_x.ToString("f2");
            scx_val.Text = kinectCtrl.screenHeight.ToString("f2");
            lrx_val.Text = kinectCtrl.left_right_x.ToString("f2");
            riy_val.Text = kinectCtrl.right_y.ToString("f2");
            ley_val.Text = kinectCtrl.left_y.ToString("f2");
            moy_val.Text = kinectCtrl.cursor_y.ToString("f2");
            spy_val.Text = kinectCtrl.spine_y.ToString("f2");
            scy_val.Text = kinectCtrl.screenWidth.ToString("f2");
            lry_val.Text = kinectCtrl.left_right_y.ToString("f2");
            riz_val.Text = kinectCtrl.right_z.ToString("f2");
            lez_val.Text = kinectCtrl.left_z.ToString("f2");
            spz_val.Text = kinectCtrl.spine_z.ToString("f2");

            se_val.Text = kinectCtrl.mouseSensitivity.ToString("f2");
            cs_val.Text = kinectCtrl.cursorSmoothing.ToString("f2");
            kinectCtrl.useGripGesture = rdiGrip.IsChecked.Value;
            gr_val.Text = kinectCtrl.useGripGesture.ToString();
            kinectCtrl.doClick = chkNoClick.IsChecked.Value;
            cl_val.Text = (!kinectCtrl.doClick).ToString();
        }
    }
}