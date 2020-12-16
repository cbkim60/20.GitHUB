using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;

using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
//using System.Drawing;


namespace enOpenCV
{
    public delegate void DataGetEventHandler(CaptureInfo eCapture);

    class clsThread
    {
        Thread m_pThread;
        public String m_pFileName = "";
        public bool m_bReadContiuos = false;
        public bool m_bReloadFlag = false;
        public int m_nRunCaptureIndex = 0;
        private clsOpenCvRun m_pOpenCv = new clsOpenCvRun();
        //private enUltima3Cap.openCvRUN m_pOpenCv = new enUltima3Cap.openCvRUN();

        public event EventHandler<clsEventArgs> ThreadEvent = (s, e) => { };
        public DataGetEventHandler DataSendEvent;


        public void runAgent()
        {
            m_pThread = new Thread(threadRun);
            m_pThread.Start();
        }

        public void stopAgent()
        {
            try
            {
                Thread.Sleep(100);
                m_pOpenCv.closeEngine();
                if (m_pThread != null)
                    m_pThread.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("!!!!!!!!!!!! ERROR HANDLING:" + ex.Message);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // THREAD
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected void threadRun()
        {
            String strFolder = "";
            List<String> fileList = new List<string>();
            m_pFileName = "";
            //clsOpenCvRun pOpenCv = new clsOpenCvRun();
            m_pOpenCv.initEngine();

            m_pOpenCv.m_JasonCapture.jasonRead2(0);
            Debug.WriteLine("JASON COUNT]" + m_pOpenCv.m_JasonCapture.m_pRectanges.Count);


            for (; ; )
            {
                Thread.Sleep(100);
                if (m_bReloadFlag == true)
                {
                    m_bReloadFlag = false;
                    m_pOpenCv.m_JasonCapture.jasonRead2(m_nRunCaptureIndex);
                }

                if (m_pFileName.Length > 0)
                {
                    fileList.Clear();
                    if ((m_bReadContiuos == true) && (m_pFileName.IndexOf("CaptureStudio") > 0))
                    {
                        strFolder = System.IO.Path.GetDirectoryName(m_pFileName) + "\\";

                        String strOrgPrefix = System.IO.Path.GetFileName(m_pFileName).Substring(0, 15);
                        String strExtension = System.IO.Path.GetExtension(m_pFileName);
                        // FOLDER=C:\Users\AMS302-2\Documents\captures\, Prefix=1_CaptureStudio Ext=.bmp
                        Debug.WriteLine("FOLDER=" + strFolder + String.Format(", Prefix={0} Ext={1}", strOrgPrefix, strExtension));
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strFolder);
                        foreach (System.IO.FileInfo File in di.GetFiles())
                        {
                            if ((File.Extension.ToLower().CompareTo(strExtension) == 0) && (File.Name.IndexOf(strOrgPrefix) >= 0))
                            {
                                fileList.Add(strFolder + File.Name);
                            }
                        }
                    }

                    if (fileList.Count > 1)
                    {
                        // C:\Users\AMS302-2\Documents\captures\1_CaptureStudio20201127105255_1_1.bmp
                        foreach(String fileName in fileList)
                        {
                            //Debug.WriteLine(fileName);
                            System.Diagnostics.Stopwatch sw = new Stopwatch();
                            sw.Start();
                            m_pOpenCv.handleContext(fileName);
                            ThreadEvent(this, new clsEventArgs(fileName, m_pOpenCv.ERROR_MESSAGE, m_pOpenCv.PARSE_RESULT, m_pOpenCv.BITMAP));
                            sw.Stop();
                            Debug.WriteLine(String.Format("FILE]{0} SW={1}", fileName, sw.ElapsedMilliseconds));
                            Thread.Sleep(800);
                        }
                    }
                    else
                    {
                        //pOpenCv.handleOpenCV(m_pFileName);
                        //ThreadEvent(this, new clsEventArgs(m_pFileName, pOpenCv.ERROR_MESSAGE, pOpenCv.PARSE_RESULT, pOpenCv.BITMAP));
                        m_pOpenCv.handleContext(m_pFileName);
                        ThreadEvent(this, new clsEventArgs(m_pFileName, m_pOpenCv.ERROR_MESSAGE, m_pOpenCv.PARSE_RESULT, m_pOpenCv.BITMAP));
                    }
                    m_pFileName = "";
                }
            }
        }

        
    }

    public class clsEventArgs : EventArgs
    {
        public String FileName { get; private set; }
        public String MyWorkResult { get; private set; }
        public String ErrorMsg { get; private set; }
        public System.Drawing.Bitmap MyBitmap { get; private set; }
        public clsEventArgs(String fileName, String errMsg, String parseResult, System.Drawing.Bitmap myBitmap)
        { FileName = fileName; MyBitmap = myBitmap; MyWorkResult = parseResult; ErrorMsg = errMsg; }
    }

    public class CaptureInfo
    {
        public String FileName { get; private set; }
        public String MyWorkResult { get; private set; }
        public String ErrorMsg { get; private set; }
        public System.Drawing.Bitmap MyBitmap { get; private set; }
        public CaptureInfo(String fileName, String errMsg, String parseResult, System.Drawing.Bitmap myBitmap)
        { FileName = fileName; MyBitmap = myBitmap; MyWorkResult = parseResult; ErrorMsg = errMsg; }
    }

    class clsOpenCvRun
    {
        public String PARSE_RESULT = "";
        public String ERROR_MESSAGE = "";
        public System.Drawing.Bitmap BITMAP = null;

        private TesseractEngine m_pEngine = null;
        public enUltima3Cap.jasonCapture m_JasonCapture = new enUltima3Cap.jasonCapture();

        public void initEngine()
        {
            try
            {
                m_pEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly);
                //m_pEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly, "psm=3");
                //m_pEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly, "psm=6");
                // https://stackoverflow.com/questions/9632044/tesseract-does-not-recognize-single-characters
                // The -psm sets the page segmentation mode, and mode 10 is for single characters
                //m_pEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly, "--psm 10");
                //m_pEngine.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ/-_[]:.");
                m_pEngine.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ/-_:.");
                // https://csharp.hotexamples.com/examples/Tesseract/TesseractEngine/SetVariable/php-tesseractengine-setvariable-method-examples.html
                m_pEngine.SetVariable("tessedit_unrej_any_wd", false);
                //m_pEngine.SetVariable("tessedit_ocr_engine_mode", "0");
                
                Debug.WriteLine("Engine INIT");
            }
            catch(Exception ex)
            {
                Debug.WriteLine("!!!!!!!!! " + ex.Message);
            }
        }

        public void closeEngine()
        {
            if (m_pEngine != null)
                m_pEngine.Dispose();
        }

        public void handleContext(String fileName)
        {
            
            PARSE_RESULT = "";
            ERROR_MESSAGE = "";
            BITMAP = null;

            if (m_pEngine == null)
                return;

            //Mat matOrg = Cv2.ImRead(fileName);
            Mat matRead = Cv2.ImRead(fileName, ImreadModes.Grayscale);
            Mat matThreashold = matRead.Threshold(180, 255, ThresholdTypes.Binary);
            //Mat matTemp = new Mat();
            //Cv2.FastNlMeansDenoisingColored(matOrg, matTemp, 10, 10, 7, 21);
            //Cv2.bit
            //matRead = Cv2.ColorChange
            //Mat matRead = matTemp.CvtColor(ColorConversionCodes.BayerBG2GRAY);
            Debug.WriteLine(String.Format("RECT={0},{1}  JASON={2}", matRead.Width, matRead.Height, m_JasonCapture.m_pRectanges.Count));
            bool bBitWiseEnable = true;
            if (bBitWiseEnable == true)
            {
                bool bMedian = false;
                Mat matCanny = new Mat();
                // edges = cv2.Canny(gray,600,1000)
                Cv2.Canny(matRead, matCanny, 600, 1000);
                if (bMedian == true)
                {
                    Mat matMedian = new Mat();
                    matThreashold = matRead.Threshold(0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    Cv2.MedianBlur(matThreashold, matMedian, 2);
                    matThreashold = matMedian.Threshold(127, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                }
                else
                {
                    matThreashold = matRead.Threshold(180, 255, ThresholdTypes.Binary);
                    //matThreashold = matRead.Threshold(0, 255, ThresholdTypes.Binary);
                }
                //Mat matDilate = new Mat();
                //Mat element = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
                //Cv2.Dilate(matThreashold, matDilate, element, new Point(0, 0), 2);
                Mat matBitWise = new Mat();
                Cv2.BitwiseAnd(matThreashold, matThreashold, matBitWise, matThreashold);
                //Cv2.BitwiseAnd(matDilate, matDilate, matBitWise, matDilate);
                matRead = matBitWise.Threshold(180, 255, ThresholdTypes.Binary);
            }

            // CROP: https://076923.github.io/posts/C-opencv-9/
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(452, 182, 1024, 728);
            if (m_JasonCapture.m_pCrop.Width > 0)
                rect = new OpenCvSharp.Rect(m_JasonCapture.m_pCrop.x, m_JasonCapture.m_pCrop.y, m_JasonCapture.m_pCrop.Width, m_JasonCapture.m_pCrop.Height);
            Mat subMat = matRead.SubMat(rect);

            //Cv2.ImShow(strFIle, matRead);
            BITMAP = BitmapConverter.ToBitmap(subMat);
            try
            {
                // System.DllNotFoundException: Failed to find library "leptonica-1.80.0.dll" for platform x86.
                if (m_JasonCapture.m_pRectanges.Count == 0)
                {
                    using (var page = m_pEngine.Process(BITMAP))
                        PARSE_RESULT = page.GetText();
                    Debug.WriteLine(PARSE_RESULT);
                }
                else
                {
                    Debug.WriteLine("###### BASE SCALE:" + m_JasonCapture.m_pBaseScale);
                    //m_pEngine.set
                    //int ResizeW, resizeH;
                    PARSE_RESULT = "";
                    String curText = "";
                    int nCount = 0;
                    System.Drawing.Pen pPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 1);
                    //  인덱싱된 픽셀 형식이 들어 있는 이미지로는 Graphics 개체를 만들 수 없습니다.
                    System.Drawing.Bitmap pRunBmp = new System.Drawing.Bitmap(BITMAP);
                    System.Drawing.Graphics curGraphics = System.Drawing.Graphics.FromImage(pRunBmp);

                    System.Drawing.Brush txtBrush = new System.Drawing.SolidBrush(System.Drawing.Color.YellowGreen);
                    System.Drawing.Font txtFont = new System.Drawing.Font(new System.Drawing.FontFamily("굴림"), 14, System.Drawing.FontStyle.Bold,
                        System.Drawing.GraphicsUnit.Pixel);
                    int lastLine = -1;
                    foreach (enUltima3Cap.CaptureRectangle pRect in m_JasonCapture.m_pRectanges)
                    {
                        curGraphics.DrawRectangle(pPen, pRect.x - m_JasonCapture.m_pCrop.x, pRect.y - m_JasonCapture.m_pCrop.y, pRect.Width, pRect.Height);
                        nCount += 1;
                        OpenCvSharp.Rect subRect = new OpenCvSharp.Rect(pRect.x, pRect.y, pRect.Width, pRect.Height);
                        Debug.WriteLine(String.Format("RECT={0},{1},{2},{3}", pRect.x, pRect.y, pRect.Width, pRect.Height));
                        Mat mapCont = matThreashold.SubMat(subRect);
                        Mat mapChange = mapCont.CvtColor(ColorConversionCodes.BayerBG2GRAY);
                        Mat mapResize = new Mat();
                        //ResizeW = (int)(mapChange * 1.6);
                        //Cv2.Resize(mapChange, mapResize, new Size(0, 0), 1.6, 1.6);
                        Debug.WriteLine(String.Format("\t--->{0},{1}", mapChange.Width, mapChange.Height));
                        //Pix pix = PixConverter.ToPix(mapResize);
                        //m_pEngine.SetImage(_mat.data, _mat.cols, _mat.rows, 4, 4 * _mat.cols);
                        //char* outtext = Cv2.GetUTF8Text();
                        // http://kimstar.kr/1580/
                        System.Drawing.Bitmap bmpText = new System.Drawing.Bitmap(pRect.Width, pRect.Height + 2);
                        System.Drawing.Graphics txtGraphics = System.Drawing.Graphics.FromImage(bmpText);
                        //txtGraphics.Clear(System.Drawing.Color.Gray);
                        
                        System.Drawing.PointF startPoint = new System.Drawing.PointF(pRect.x - m_JasonCapture.m_pCrop.x + 8, pRect.y - m_JasonCapture.m_pCrop.y + pRect.Height + 2);
                        startPoint = new System.Drawing.PointF(pRect.x - m_JasonCapture.m_pCrop.x + pRect.Width + 8, pRect.y - m_JasonCapture.m_pCrop.y + 2);

                        mapCont = matRead.SubMat(subRect);
                        mapChange = mapCont.CvtColor(ColorConversionCodes.BayerBG2GRAY);
                        switch(pRect.dataType)
                        {
                            case enUltima3Cap.jasonCapture.DATA_TYPE_FLOAT:
                                m_pEngine.SetVariable("tessedit_char_whitelist", "0123456789.");
                                break;
                            case enUltima3Cap.jasonCapture.DATA_TYPE_TEXT:
                                m_pEngine.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ/-_:.");
                                break;
                            default:
                                m_pEngine.SetVariable("tessedit_char_whitelist", "0123456789");
                                break;
                        }
                        curText = parseFrame(mapChange, mapChange, pRect);
                        if(pRect.locMain != lastLine)
                        {
                            if(lastLine >= 0)
                                PARSE_RESULT += "\n";
                            lastLine = pRect.locMain;
                        }
                        PARSE_RESULT += (curText + "\t");

                        //if (pRect.picType >= 1)
                        //{
                        //    if (nCount < 2)
                        //        Cv2.Resize(mapChange, mapResize, new OpenCvSharp.Size(0, 0), 2.0, 2.0);
                        //    else
                        //        Cv2.Resize(mapChange, mapResize, new Size(0, 0), 4.0, 4.0);
                        //    Cv2.Resize(mapChange, mapResize, new Size(0, 0), pRect.Resize, pRect.Resize);
                        //    // https://stackoverflow.com/questions/64704144/how-can-i-add-psm-n-option-to-tesseract-engine-constructor-c
                        //    using (var bitMap = BitmapConverter.ToBitmap(mapResize))
                        //    {
                        //        curText = "";
                        //        using (var pix = PixConverter.ToPix(bitMap))
                        //        {
                        //            using (var page = m_pEngine.Process(pix))
                        //                curText += page.GetText();
                        //        }
                        //        //curText = System.Text.RegularExpressions.Regex.Replace(curText, @"[^\u0020-\u007E]", string.Empty);
                        //        Debug.WriteLine("THRESHOLD]" + curText);
                        //        String[] parseLines = curText.Split('\n');
                        //        foreach (String line in parseLines)
                        //        {
                        //            if (line.Length > 0)
                        //                PARSE_RESULT += (line + " ");
                        //        }
                        //        PARSE_RESULT = PARSE_RESULT.TrimEnd(' ') + "\n";
                        //    }
                        //}

                        //if (pRect.picType != 1)
                        //{
                        //    mapCont = matRead.SubMat(subRect);
                        //    mapChange = mapCont.CvtColor(ColorConversionCodes.BayerBG2GRAY);
                        //    //Cv2.Resize(mapChange, mapResize, new Size(0, 0), 1.6, 1.6);
                        //    Cv2.Resize(mapChange, mapResize, new Size(0, 0), pRect.Resize, pRect.Resize);
                        //    // PPID, VALVE
                        //    //if ((nCount == 5) || (nCount == 2))
                        //    //    Cv2.Resize(mapChange, mapResize, new Size(0, 0), 4.8, 4.8);
                        //    //if (nCount < 2)
                        //    //    Cv2.Resize(mapChange, mapResize, new Size(0, 0), 1.6, 1.6);
                        //    //else
                        //    //    Cv2.Resize(mapChange, mapResize, new Size(0, 0), 4.0, 4.0);
                            
                        //    using (var bitMap = BitmapConverter.ToBitmap(mapResize))
                        //    {
                        //        curText = "";
                        //        using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleBlock))
                        //            curText += page.GetText();
                        //        //using (var pix = PixConverter.ToPix(bitMap))
                        //        //{
                        //        //    using (var page = m_pEngine.Process(pix))
                        //        //        curText += page.GetText();
                        //        //}
                        //        //curText = System.Text.RegularExpressions.Regex.Replace(curText, @"[^\u0020-\u007E]", string.Empty);
                        //        Debug.WriteLine("GRAY---]" + curText);
                        //        String[] parseLines = curText.Split('\n');
                        //        foreach (String line in parseLines)
                        //        {
                        //            if (line.Length > 0)
                        //                PARSE_RESULT += (line + " ");
                        //        }
                        //        PARSE_RESULT = PARSE_RESULT.TrimEnd(' ') + "\n";
                                
                        //    }
                        //}
                        curGraphics.DrawString(curText, txtFont, txtBrush, startPoint);
                    }
                    BITMAP = pRunBmp;
                }
                Debug.WriteLine("BITMAP=" + BITMAP.Width);
                Debug.WriteLine(PARSE_RESULT);
            }
            catch (Exception ex)
            {
                ERROR_MESSAGE = ex.ToString();
                Debug.WriteLine(ERROR_MESSAGE);
            }
        }

        private const double MAX_PARSE_RESIZE = 5.1;
        private double m_ResizeRun = 0;
        public String parseFrame(Mat matGray, Mat matTrh, enUltima3Cap.CaptureRectangle pRect)
        {
            String parseString = "", strReadText = "";
            int n, nValue = 0;
            double fValue = 0;
            Mat mapResize = new Mat();
            Mat mapRun = matGray;
            if (pRect.picType == 1)
                mapRun = matTrh;
            if (pRect.dataType == enUltima3Cap.jasonCapture.DATA_TYPE_INT)
            {
                Mat matDilate = new Mat();
                Mat matErode = new Mat();
                Mat marMake = new Mat();
                Mat element = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(2, 2));
                Cv2.Dilate(mapRun, matDilate, element, new Point(0, 0), 3);
                Cv2.Erode(mapRun, matErode, element, new Point(-1, -1), 3);
                Cv2.HConcat(new Mat[] { matDilate, matErode }, marMake);
                //Mat matBitWise = new Mat();
                //Cv2.BitwiseAnd(matDilate, matDilate, matBitWise, matDilate);
                //mapRun = matBitWise.Threshold(180, 255, ThresholdTypes.Binary);
            }

            Cv2.Resize(mapRun, mapResize, new Size(0, 0), pRect.Resize, pRect.Resize);
            m_ResizeRun = 1.0;
            using (var bitMap = BitmapConverter.ToBitmap(mapResize))
            {
                strReadText = "";
                if (pRect.dataType == enUltima3Cap.jasonCapture.DATA_TYPE_INT)
                {
                    using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleWord))
                        strReadText += page.GetText();
                }
                else
                {
                    using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleLine))
                        strReadText += page.GetText();
                }
            }
            strReadText = strReadText.Replace("\n", "");
            switch (pRect.dataType)
            {
                case enUltima3Cap.jasonCapture.DATA_TYPE_FLOAT:
                    do
                    {
                        strReadText = strReadText.Replace("\n", "").Replace(" ", "");
                        if (double.TryParse(strReadText, out fValue) == true)
                        {
                            parseString = strReadText;
                            break;
                        }
                        if ((pRect.dataSub > 0) && (strReadText.Length == 0))
                            break;
                        if (pRect.picType == 1)
                            Cv2.Resize(matTrh, mapResize, new Size(0, 0), m_ResizeRun, m_ResizeRun);
                        else
                            Cv2.Resize(matGray, mapResize, new Size(0, 0), m_ResizeRun, m_ResizeRun);
                        strReadText = "";
                        using (var bitMap = BitmapConverter.ToBitmap(mapResize))
                        {
                            using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleWord))
                                strReadText += page.GetText();
                        }
                        m_ResizeRun += 0.2;
                    } while (m_ResizeRun < MAX_PARSE_RESIZE);
                    break;
                case enUltima3Cap.jasonCapture.DATA_TYPE_TEXT:
                    do
                    {
                        strReadText = strReadText.Replace("\n", "").Trim();
                        if (strReadText.Length > 0)
                        {
                            if (pRect.dataSub == 1)
                            {
                                // check digit
                                nValue = 0;
                                strReadText = strReadText.Replace("O", "0");
                                for (n = 0; n < strReadText.Length; n++)
                                {
                                    if (int.TryParse(strReadText.Substring(n, 1), out nValue) == true)
                                        break;
                                }
                                if (n < strReadText.Length)
                                {
                                    parseString = strReadText;
                                    break;
                                }
                            }
                            else if (pRect.dataSub == 2)
                            {
                                strReadText = strReadText.Replace(" ", "");
                                if (strReadText.Length > 0)
                                {
                                    parseString = strReadText;
                                    break;
                                }
                            }
                            else
                            {
                                parseString = strReadText;
                                break;
                            }
                        }
                        if ((pRect.dataSub > 0) && (strReadText.Length == 0))
                            break;
                        if (pRect.picType == 1)
                            Cv2.Resize(matTrh, mapResize, new Size(0, 0), m_ResizeRun, m_ResizeRun);
                        else
                            Cv2.Resize(matGray, mapResize, new Size(0, 0), m_ResizeRun, m_ResizeRun);
                        strReadText = "";
                        using (var bitMap = BitmapConverter.ToBitmap(mapResize))
                        {
                            using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleWord))
                                strReadText += page.GetText();
                        }
                        m_ResizeRun += 0.2;
                    } while (m_ResizeRun < MAX_PARSE_RESIZE);
                    break;
                case enUltima3Cap.jasonCapture.DATA_TYPE_INT:
                default:
                    do
                    {
                        strReadText = strReadText.Replace("\n", "").Replace(" ", "").Replace("O", "0");
                        if (int.TryParse(strReadText, out nValue) == true)
                        {
                            parseString = strReadText;
                            break;
                        }
                        //else if (strReadText.Length > 0)
                        //    Debug.WriteLine(String.Format("---> CURRENT={0} LAST={1}", strReadText, m_ResizeRun));
                        if ((pRect.dataSub > 0) && (strReadText.Length == 0))
                            break;
                        if (pRect.picType == 1)
                            Cv2.Resize(matTrh, mapResize, new Size(0, 0), m_ResizeRun, m_ResizeRun);
                        else
                            Cv2.Resize(matGray, mapResize, new Size(0, 0), m_ResizeRun * 1.2, m_ResizeRun);
                        strReadText = "";
                        using (var bitMap = BitmapConverter.ToBitmap(mapResize))
                        {
                            using (var page = m_pEngine.Process(bitMap, PageSegMode.SingleWord))
                                strReadText += page.GetText();
                        }
                        m_ResizeRun += 0.2;
                    } while (m_ResizeRun < MAX_PARSE_RESIZE);
                    break;
            }

            Debug.WriteLine(String.Format("RESULT={0} LAST={1}", parseString, m_ResizeRun));
            return (parseString);
        }


        public void handleOpenCV(String fileName)
        {
            PARSE_RESULT = "";
            ERROR_MESSAGE = "";
            BITMAP = null;

            Mat matRead = Cv2.ImRead(fileName, ImreadModes.Grayscale);

            // CROP: https://076923.github.io/posts/C-opencv-9/
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(452, 182, 1024, 728);
            Mat subMat = matRead.SubMat(rect);

            //Cv2.ImShow(strFIle, matRead);
            BITMAP = BitmapConverter.ToBitmap(subMat);
            try
            {
                // System.DllNotFoundException: Failed to find library "leptonica-1.80.0.dll" for platform x86.
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly))
                {
                    using (var page = engine.Process(BITMAP))
                        PARSE_RESULT = page.GetText();
                    Debug.WriteLine(PARSE_RESULT);
                }
            }
            catch (Exception ex)
            {
                ERROR_MESSAGE = ex.ToString();
                Debug.WriteLine(ERROR_MESSAGE);
            }
        }
    }
}
