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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;

using System.Diagnostics;

namespace enOpenCV
{
    public partial class Form1 : Form
    {
        // not openCV, use MS: https://medium.com/dotnetdev/c-%EC%9C%BC%EB%A1%9C-windows-10-ocr-%EC%82%AC%EC%9A%A9%ED%95%98%EA%B8%B0-ebc3b82c5f30

        // base code: https://shalchoong.tistory.com/19
        // tesseract: https://m.blog.naver.com/PostView.nhn?blogId=rhukjin&logNo=222052759608&proxyReferer=https:%2F%2Fwww.google.com%2F

        private clsThread m_pThread = new clsThread();
        private String runFileName = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            btnOpen.Click += BtnOpen_Click;
            btnReload.Click += BtnReload_Click;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            cmbDEV.Items.Add("Device0");
            cmbDEV.Items.Add("Device1");
            cmbDEV.SelectedIndex = 0;

            m_pThread.ThreadEvent += EventServerHandler;
            m_pThread.DataSendEvent += new DataGetEventHandler(this.CaptureMessage);
            m_pThread.runAgent();

            String setVal = "0.0";
            double dValue;
            if (double.TryParse(setVal, out dValue) == true)
                Debug.WriteLine("INPUT]" + setVal + "---> " + dValue);
            else
                Debug.WriteLine("INPUT]" + setVal + "---> FAILED");
            setVal = "0. 0";
            if (double.TryParse(setVal, out dValue) == true)
                Debug.WriteLine("INPUT]" + setVal + "---> " + dValue);
            else
                Debug.WriteLine("INPUT]" + setVal + "---> FAILED");
        }

        private void BtnReload_Click(object sender, EventArgs e)
        {
            m_pThread.m_nRunCaptureIndex = cmbDEV.SelectedIndex;
            m_pThread.m_bReloadFlag = true;
            m_pThread.m_bReadContiuos = false;
            m_pThread.m_pFileName = runFileName;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_pThread.stopAgent();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
       + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                txtFILE.Text = System.IO.Path.GetFileName(openDlg.FileName);
                runFileName = openDlg.FileName;
                //handleImage(strFileName);
                if (checkContinue.Checked == true)
                    m_pThread.m_bReadContiuos = true;
                else
                    m_pThread.m_bReadContiuos = false;
                m_pThread.m_pFileName = openDlg.FileName;
            }
        }

        private void handleImage(String strFIle)
        {
            Mat matRead = Cv2.ImRead(strFIle, ImreadModes.Grayscale);

            // CROP: https://076923.github.io/posts/C-opencv-9/
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(452, 182, 1024, 728);
            Mat subMat = matRead.SubMat(rect);

            //Cv2.ImShow(strFIle, matRead);
            Bitmap changeBitmap = BitmapConverter.ToBitmap(subMat);
            pictureBox1.Image = changeBitmap;

            String strText = OcrProcess(changeBitmap);
            String[] strLines = strText.Split('\n');
            lbCONVERT.Items.Clear();
            foreach(String line in strLines)
            {
                lbCONVERT.Items.Add(line);
            }
        }

        private String OcrProcess(Bitmap oc)
        {
            String capString = "";
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly))
                {
                    using (var page = engine.Process(oc))
                        capString = page.GetText();
                }
            }
            catch(Exception ex)
            {
                capString = ex.ToString();
                Debug.WriteLine(capString);
            }
            return capString;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CALL-BACK from server Side
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void EventServerHandler(Object sender, clsEventArgs e)
        {
            Debug.WriteLine("--->MESG]" + e.FileName);
            this.Invoke(new ServerMessage(OnServerMessage), e);

            //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ServerMessage(this.OnServerMessage), e.MyWorkResult);
            //PictureBox picShow = picSHOW0;
            //if(e.DevNo == 1)
            //    picShow = picSHOW1;
            //picShow.Image = e.MyBitmap;
        }

        public delegate void ServerMessage(clsEventArgs e);
        public void OnServerMessage(clsEventArgs e)
        {
            String[] strLines = e.MyWorkResult.Split('\n');
            if (e.ErrorMsg.Length > 0)
                strLines = e.ErrorMsg.Split('\n');
            if(e.MyBitmap != null)
            {
                pictureBox1.Image = e.MyBitmap;
            }
            lbCONVERT.Items.Clear();
            foreach (String line in strLines)
            {
                lbCONVERT.Items.Add(line);
            }
        }

        private void CaptureMessage(CaptureInfo e)
        {
            String[] strLines = e.MyWorkResult.Split('\n');
            if (e.ErrorMsg.Length > 0)
                strLines = e.ErrorMsg.Split('\n');
            else
            {

            }
            lbCONVERT.Items.Clear();
            foreach (String line in strLines)
            {
                lbCONVERT.Items.Add(line);
            }
        }
    }
}
