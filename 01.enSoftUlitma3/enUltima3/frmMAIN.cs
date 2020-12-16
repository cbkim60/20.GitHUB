using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace enUltima3Cap
{
#if _X64_MACHINE
    using LONGPTR = System.Int64;
#else
    using LONGPTR = System.Int32;
#endif

    public partial class frmMAIN : Form
    {
        private clsLOG logMAIN = new clsLOG();
        private clsTHREAD[] m_pTHREAD = new clsTHREAD[averRUN.MAX_DEVICE_COUNT];

        private NOTIFYEVENTCALLBACK m_NotifyEventCallback = new NOTIFYEVENTCALLBACK(NotifyEventCallback);

        private int m_nTimerCount = 0;
        private int m_nPicPaintInterval = 0;
        public bool m_bNormalCheck = false;

        private int m_nCheckRunCount = 0;
        // CPU performance: https://www.sysnet.pe.kr/Default.aspx?mode=2&sub=0&detail=1&wid=1684
        private PerformanceCounter PERF_CPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private int m_nLogDay = 0;
        private String m_CpuStartTime = "";
        private List<float> m_CpuList = new List<float>();

        public frmMAIN()
        {
            InitializeComponent();

            logMAIN.initLogName();
            for (int n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            {
                m_pTHREAD[n] = new clsTHREAD(n);
                m_pTHREAD[n].m_pAVER.m_strLogName = logMAIN.LOG_FILE_NAME;
                m_pTHREAD[n].m_pAVER.m_strCaptureName = logMAIN.CAPTURE_FILE_NAME;
                m_pTHREAD[n].m_pAVER.m_rectPICTURE.Top = 0;
                m_pTHREAD[n].m_pAVER.m_rectPICTURE.Left = 0;
                //if (n == 0)
                //{
                //    m_pTHREAD[n].m_pAVER.m_PictureHandle = this.picSHOW0.Handle;
                //    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Right = this.picSHOW0.Width;
                //    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Bottom = this.picSHOW0.Height;
                //}
                //else
                //{
                //    m_pTHREAD[n].m_pAVER.m_PictureHandle = this.picSHOW1.Handle;
                //    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Right = this.picSHOW1.Width;
                //    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Bottom = this.picSHOW1.Height;
                //}

                if (n == 0)
                {
                    m_pTHREAD[n].m_pAVER.m_PictureHandle = this.picHIDE0.Handle;
                    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Right = this.picHIDE0.Width;
                    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Bottom = this.picHIDE0.Height;
                    m_pTHREAD[n].m_pOpenCV.m_PicShow = this.picSHOW0;
                    //m_pTHREAD[n].m_pAVER.videoCaptureCallBack = this.VideoCapture_CallBack0;
                }
                else
                {
                    m_pTHREAD[n].m_pAVER.m_PictureHandle = this.picHIDE1.Handle;
                    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Right = this.picHIDE1.Width;
                    m_pTHREAD[n].m_pAVER.m_rectPICTURE.Bottom = this.picHIDE1.Height;
                    m_pTHREAD[n].m_pOpenCV.m_PicShow = this.picSHOW1;
                    //m_pTHREAD[n].m_pAVER.videoCaptureCallBack = this.VideoCapture_CallBack1;
                }
            }

            cmbDEVICE.Items.Add("Device0");
            cmbDEVICE.Items.Add("Device1");
            cmbDEVICE.SelectedIndex = 0;

            m_nLogDay = DateTime.Now.Day;
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            //if (m_iCurrentDeviceIndex != -1)
            //{
            //    SaveSettingAsIni();
            //}
            for (int n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
                m_pTHREAD[n].stopAgent();
            base.OnClosed(e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            m_nTimerCount++;
            String strSet = String.Format("{0}]", m_nTimerCount), initMsg;
            float cpuVal;

            int n;
            switch (m_nTimerCount)
            {
                case 1:
                    break;
                case 2:
                    for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
                    {
                        m_pTHREAD[n].m_pOpenCV.init(n);
                        m_pTHREAD[n].m_pOpenCV.ThreadEvent += EventServerHandler;
                        m_pTHREAD[n].RunEvent += EventThreadHandler;
                        m_pTHREAD[n].runAgent();
                        System.Threading.Thread.Sleep(1200);
                    }
                    break;
            }
            if (m_nTimerCount > 2)
                timer.Stop();
           
            //int n;
            //switch (m_nTimerCount)
            //{
            //    case 0:
            //        picSHOW0.TabIndex = 1;
            //        picSHOW0.TabStop = false;
            //        picHIDE0.Paint += new System.Windows.Forms.PaintEventHandler(this.picSHOW0_Paint);
            //        this.picHIDE0.Hide();

            //        picSHOW1.TabIndex = 2;
            //        picSHOW1.TabStop = false;
            //        picHIDE1.Paint += new System.Windows.Forms.PaintEventHandler(this.picSHOW0_Paint);
            //        this.picHIDE1.Hide();
            //        break;
            //    case 1:
            //        for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            //        {
            //            //initMsg = m_pTHREAD[n].m_pAVER.averInit();
            //            //m_pTHREAD[n].m_pOpenCV.init(n);
            //            //m_pTHREAD[n].m_pOpenCV.ThreadEvent += EventServerHandler;
            //            //m_pTHREAD[n].runAgent();
            //        }
            //        break;
            //    default:
            //        if (m_bNormalCheck == true)
            //        {
            //            //cpuVal = PERF_CPU.NextValue();
            //            m_nCheckRunCount++;
            //            for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            //            {
            //                if (n == 0)
            //                {
            //                    if ((m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION.Length > 0) && (m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION != lblDEVICE0.Text))
            //                        lblDEVICE0.Text = m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION;
            //                }
            //                else
            //                {
            //                    if ((m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION.Length > 0) && (m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION != lblDEVICE1.Text))
            //                        lblDEVICE1.Text = m_pTHREAD[n].m_pAVER.RUN_MSG_CAPTION;
            //                }
            //            }
            //            lblSTEP.Text = String.Format("{0} V={1},{2}", m_nCheckRunCount, m_pTHREAD[0].m_pAVER.m_nPicPaintRun, m_pTHREAD[0].m_pAVER.m_nPicPaintSet);
            //            lblRESOLUTION.Text = String.Format("{0} V={1},{2}", m_nCheckRunCount, m_pTHREAD[1].m_pAVER.m_nPicPaintRun, m_pTHREAD[1].m_pAVER.m_nPicPaintSet);
            //            //lblMSG.Text = String.Format("CPU={0:f1}", cpuVal);

            //            if (DateTime.Now.Day != m_nLogDay)
            //            {
            //                m_nLogDay = DateTime.Now.Day;
            //                logMAIN.initLogName();
            //                for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            //                {
            //                    m_pTHREAD[n].m_pAVER.m_strLogName = logMAIN.LOG_FILE_NAME;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            //            {
            //                if (m_pTHREAD[n].m_pAVER.m_bInitEnable == true)
            //                {
            //                    initMsg = m_pTHREAD[n].m_pAVER.averInit();
            //                    if (m_pTHREAD[n].m_pAVER.m_bCallBackNeed == true)
            //                    {
            //                        m_pTHREAD[n].m_pAVER.m_bCallBackNeed = false;
            //                        GCHandle gchThis = GCHandle.Alloc(this);
            //                        AVerCapAPI.AVerSetEventCallback(m_pTHREAD[n].m_pAVER.m_hCaptureDevice, m_NotifyEventCallback, 0, GCHandle.ToIntPtr(gchThis));
            //                    }
            //                    lblMSG.Text = initMsg;
            //                    if (m_pTHREAD[n].m_pAVER.m_bInitEnable == false)
            //                    {
            //                        initMsg = "";
            //                        if (m_pTHREAD[n].m_pAVER.m_bIsStartStreaming == true)
            //                            initMsg = " STREAMING";
            //                        if (n == 0)
            //                            lblDEVICE0.Text = m_pTHREAD[n].m_pAVER.m_strDeviceName + initMsg;
            //                        else
            //                            lblDEVICE1.Text = m_pTHREAD[n].m_pAVER.m_strDeviceName + initMsg;
            //                    }
            //                }
            //                else if (m_pTHREAD[n].m_pAVER.m_bCaptureCheck == false)
            //                {
            //                    initMsg = m_pTHREAD[n].m_pAVER.averInit();
            //                    m_pTHREAD[n].m_pOpenCV.init(n);
            //                    m_pTHREAD[n].m_pOpenCV.ThreadEvent += EventServerHandler;
            //                    m_pTHREAD[n].runAgent();
            //                }
            //            }
            //            for (n = 0; n < averRUN.MAX_DEVICE_COUNT; n++)
            //            {
            //                if (m_pTHREAD[n].m_bStart == false)
            //                    //if (m_pTHREAD[n].m_pAVER.m_bInitEnable == false)
            //                    break;
            //            }
            //            if (n >= averRUN.MAX_DEVICE_COUNT)
            //            {
            //                m_nPicPaintInterval = m_pTHREAD[0].m_pAVER.EN_CONFIG.m_dwPictureInterval;
            //                m_bNormalCheck = true;
            //                timer.Interval = 1000;
            //                cpuVal = PERF_CPU.NextValue();
            //                m_CpuStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            }
            //        }
            //        break;
            //}
            if (m_bNormalCheck == false)
                lblSTEP.Text = strSet;
            //timer.Start();
        }


        private void picSHOW0_Paint(object sender, PaintEventArgs e)
        {
            PictureBox picRUN = sender as PictureBox;
            int runIdx = 0;
            if (picRUN == picSHOW0)
                runIdx = 0;
            else
                runIdx = 1;
            if (m_pTHREAD[runIdx].m_pAVER.m_hCaptureDevice != IntPtr.Zero)
            {
                AVerCapAPI.AVerRepaintVideo(m_pTHREAD[runIdx].m_pAVER.m_hCaptureDevice);
                //if (m_nPicPaintInterval <= 0)
                //{
                //    AVerCapAPI.AVerRepaintVideo(m_pTHREAD[runIdx].m_pAVER.m_hCaptureDevice);
                //    (m_pTHREAD[runIdx].m_pAVER.m_nPicPaintSet)++;
                //}
                //else
                //{
                //    if ((m_pTHREAD[runIdx].m_pAVER.m_nPicPaintRun % m_nPicPaintInterval) == 0)
                //    {
                //        AVerCapAPI.AVerRepaintVideo(m_pTHREAD[runIdx].m_pAVER.m_hCaptureDevice);
                //        (m_pTHREAD[runIdx].m_pAVER.m_nPicPaintSet)++;
                //    }
                //    (m_pTHREAD[runIdx].m_pAVER.m_nPicPaintRun)++;
                //}
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CALL-BACK from server Side
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void EventServerHandler(Object sender, clsEventArgs e)
        {
            this.Invoke(new CaptureNotify(OnCaptureNotify), e);
            //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ServerMessage(this.OnServerMessage), e.MyWorkResult);
            //PictureBox picShow = picSHOW0;
            //if(e.DevNo == 1)
            //    picShow = picSHOW1;
            //picShow.Image = e.MyBitmap;
        }

        public delegate void CaptureNotify(clsEventArgs e);
        public void OnCaptureNotify(clsEventArgs e)
        {
            if (e.MyBitmap != null)
            {
                if (e.DevNo == 0)
                    picSHOW0.Image = e.MyBitmap;
                else
                    picSHOW1.Image = e.MyBitmap;
            }
            if (checkVIEW.Checked == true)
            {
                lbVIEW.Items.Clear();
                int runDev = 0;
                if (cmbDEVICE.SelectedIndex != 0)
                    runDev = 1;
                String strMsg = "";
                strMsg = String.Format("RESULT={0} LAST={1}", m_pTHREAD[runDev].m_pOpenCV.EXTRACT_MESG.CUR_RUN, m_pTHREAD[runDev].m_pOpenCV.EXTRACT_MESG.CUR_LINE);
                lbVIEW.Items.Add(strMsg);
                String[] strLines = m_pTHREAD[runDev].m_pOpenCV.EXTRACT_MESG.PARSE_MESSAGE.Split('\n');
                foreach (String Line in strLines)
                {
                    if (Line.Length > 4)
                        lbVIEW.Items.Add(Line);
                }
                //lbVIEW.Items.Add("-----> ORG PARSE");
                //strLines = e.MyWorkResult.Split('\n');
                //foreach (String Line in strLines)
                //{
                //    if (Line.Length > 4)
                //        lbVIEW.Items.Add(Line);
                //}
            }
        }


        public void EventThreadHandler(Object sender, clsRunEventArgs e)
        {
            this.Invoke(new RunNotify(OnRunNotify), e);
        }

        public delegate void RunNotify(clsRunEventArgs e);
        public void OnRunNotify(clsRunEventArgs e)
        {
            if (e.DevNo == 0)
            {
                if (e.MsgType == 0)
                    lblSTEP.Text = e.MyWorkResult;
                else
                    lblDEVICE0.Text = e.MyWorkResult;
            }
            else
            {
                if (e.MsgType == 0)
                    lblRESOLUTION.Text = e.MyWorkResult;
                else
                    lblDEVICE1.Text = e.MyWorkResult;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// CALL BACK FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static int NotifyEventCallback(uint dwEventCode, IntPtr lpEventData, IntPtr lpUserData)
        //public int NotifyEventCallback(uint dwEventCode, IntPtr lpEventData, IntPtr lpUserData)
        {
            switch (dwEventCode)
            {
                case (uint)CAPTUREEVENT.EVENT_CAPTUREIMAGE:
                    {
                        if (lpUserData == null || lpEventData == null)
                            return 0;

                        GCHandle gchThis = GCHandle.FromIntPtr(lpUserData);
                        CAPTUREIMAGE_NOTIFY_INFO CaptureImageNotifyInfo = new CAPTUREIMAGE_NOTIFY_INFO();
                        CaptureImageNotifyInfo = (CAPTUREIMAGE_NOTIFY_INFO)Marshal.PtrToStructure(lpEventData, typeof(CAPTUREIMAGE_NOTIFY_INFO));
                        //if (((MainWindow)gchThis.Target).m_ShowCaptureImage != null)
                        //    ((MainWindow)gchThis.Target).m_ShowCaptureImage.ModifyName(ref CaptureImageNotifyInfo);
                    }
                    break;
                case (uint)CAPTUREEVENT.EVENT_CHECKCOPP:
                    {
                        uint plErrorID = (uint)Marshal.ReadInt32(lpEventData);
                        string strErrorID = "";
                        switch (plErrorID)
                        {
                            case (uint)COPPERRCODE.COPP_ERR_UNKNOWN:
                                strErrorID = "COPP_ERR_UNKNOWN";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_COPP_HW:
                                strErrorID = "COPP_ERR_NO_COPP_HW";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE:
                                strErrorID = "COPP_ERR_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_CERTIFICATE_CHAIN_FAILED:
                                strErrorID = "COPP_ERR_CERTIFICATE_CHAIN_FAILED";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_STATUS_LINK_LOST:
                                strErrorID = "COPP_ERR_STATUS_LINK_LOST";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_NO_HDCP_PROTECTION_TYPE:
                                strErrorID = "COPP_ERR_NO_HDCP_PROTECTION_TYPE";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_HDCP_REPEATER:
                                strErrorID = "COPP_ERR_HDCP_REPEATER";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_HDCP_PROTECTED_CONTENT:
                                strErrorID = "COPP_ERR_HDCP_PROTECTED_CONTENT";
                                break;
                            case (uint)COPPERRCODE.COPP_ERR_GET_CRL_FAILED:
                                strErrorID = "COPP_ERR_GET_CRL_FAILED";
                                break;
                        }
                        MessageBox.Show(strErrorID);
                    }
                    break;
                default:
                    return 0;
            }
            return 1;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// CALL BACK CAPTURE
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public int VideoCapture_CallBack0(VIDEO_SAMPLE_INFO VideoInfo, IntPtr pbData, int lLength, long tRefTime, LONGPTR lUserData)
        {
            return 1;
        }

        public int VideoCapture_CallBack1(VIDEO_SAMPLE_INFO VideoInfo, IntPtr pbData, int lLength, long tRefTime, LONGPTR lUserData)
        {
            return 1;
        }
    }
}
