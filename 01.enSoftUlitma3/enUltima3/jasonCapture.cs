using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace enUltima3Cap
{
    class jasonCapture
    {
        public List<CaptureRectangle> m_pRectanges = new List<CaptureRectangle>();
        public CaptureRectangle m_pCrop = new CaptureRectangle();
        public double m_pBaseScale = 1.6;

        public const int DATA_TYPE_INT = 0;
        public const int DATA_TYPE_FLOAT = 1;
        public const int DATA_TYPE_TEXT = 2;

        public void jasonRead(int nDevNo)
        {
            String fileName = @".\parseJason.txt";
            if (System.IO.File.Exists(fileName) == false)
                return;
            bool bSuccess = false, bEnable = false;
            int n = 0;
            String devToken = "", tokenName = "";
            m_pRectanges.Clear();
            m_pCrop.Width = m_pCrop.Height = 0;
            try
            {
                //String allRead = System.IO.File.ReadAllText(fileName).Replace("\t", "");
                String allText = System.IO.File.ReadAllText(fileName).Replace("\t", "");
                String allRead = allText.Replace(" ", "");
                devToken = String.Format("DEVICE{0}", nDevNo);
                Debug.WriteLine(allRead);

                JObject jObj = JObject.Parse(allRead);
                Debug.WriteLine(jObj.Count);
                JToken idToken = jObj[devToken];
                String capRead = idToken.ToString();
                Debug.WriteLine(capRead);
                JObject jDevObj = JObject.Parse(capRead);

                idToken = jDevObj["BaseScale"];
                if (idToken != null)
                {
                    m_pBaseScale = Convert.ToDouble(idToken.ToString());
                }
                idToken = jDevObj["Enable"];
                if (idToken != null)
                {
                    if (idToken.ToString() == "1")
                        bEnable = true;
                    Debug.WriteLine("########################## Enabled=" + bEnable);
                }

                for (n = 1; n < 20; n++)
                {
                    tokenName = "";
                    switch (n)
                    {
                        case 1: tokenName = "auto-idle"; break;
                        case 2: tokenName = "PPID"; break;
                        case 3: tokenName = "PRESS"; break;
                        case 4: tokenName = "STEP"; break;
                        case 5: tokenName = "VALVE"; break;
                        case 6: tokenName = "RF"; break;
                        case 7: tokenName = "RFr"; break;
                        case 8: tokenName = "GAS1"; break;
                        case 9: tokenName = "GAS2"; break;
                        case 10: tokenName = "TEMP1"; break;
                        case 11: tokenName = "TEMP2"; break;
                        case 12: tokenName = "TEMP3"; break;
                        case 13: tokenName = "EFD"; break;
                        case 14: tokenName = "TIME"; break;
                        case 15: tokenName = "CYCLE"; break;
                        case 16: tokenName = "auto-end"; break;
                        case 17: tokenName = "last"; break;
                    }
                    if (tokenName.Length == 0)
                    {
                        bSuccess = true;
                        break;
                    }
                    idToken = jDevObj[tokenName];
                    String readValue = idToken.ToString();
                    if (n == 0)
                    {
                        if (readValue == "1")
                            bEnable = true;
                    }
                    else
                    {
                        CaptureRectangle pRect = new CaptureRectangle();
                        String[] strSplits = readValue.Split(',');
                        Debug.WriteLine(n + "]" + strSplits.Length + "<==" + tokenName);
                        if (strSplits.Length < 4)
                            break;
                        pRect.x = Convert.ToInt32(strSplits[0]);
                        pRect.y = Convert.ToInt32(strSplits[1]);
                        pRect.Width = Convert.ToInt32(strSplits[2]);
                        pRect.Height = Convert.ToInt32(strSplits[3]);
                        if (strSplits.Length > 4)
                            pRect.picType = Convert.ToInt32(strSplits[4]);
                        else
                            pRect.picType = 2;
                        if (strSplits.Length > 5)
                            pRect.Resize = Convert.ToDouble(strSplits[5]);
                        else
                            pRect.Resize = m_pBaseScale;
                        if (((n >= 3) && (n < 14) && (n != 4) && (pRect.x < 700)) || ((tokenName == "CYCLE")))
                        {
                            // "PRESS":"566,65,404,22,0",
                            // "VALVE":"822,91,146,20,0,1.2",
                            pRect.Width = 146;
                            m_pRectanges.Add(pRect);
                            CaptureRectangle pRectMon = new CaptureRectangle();
                            pRectMon.x = 822;
                            pRectMon.y = pRect.y;
                            pRectMon.y = pRect.y;
                            pRectMon.Width = pRect.Width;
                            pRectMon.Height = pRect.Height;
                            pRectMon.picType = pRect.picType;
                            pRectMon.Resize = pRect.Resize;
                            m_pRectanges.Add(pRectMon);
                        }
                        else if (tokenName == "TIME")
                        {
                            pRect.Width = 178;
                            m_pRectanges.Add(pRect);
                            CaptureRectangle pRectMon = new CaptureRectangle();
                            pRectMon.x = 792;
                            pRectMon.y = pRect.y;
                            pRectMon.y = pRect.y;
                            pRectMon.Width = pRect.Width;
                            pRectMon.Height = pRect.Height;
                            pRectMon.picType = pRect.picType;
                            pRectMon.Resize = pRect.Resize;
                            m_pRectanges.Add(pRectMon);
                        }
                        else
                            m_pRectanges.Add(pRect);
                    }
                }
                idToken = jDevObj["CropRect"];
                if (idToken != null)
                {
                    String readValue = idToken.ToString();
                    String[] strSplits = readValue.Split(',');
                    // "CropRect":"286,32,1278,400",
                    if (strSplits.Length >= 4)
                    {
                        m_pCrop.x = Convert.ToInt32(strSplits[0]);
                        m_pCrop.y = Convert.ToInt32(strSplits[1]);
                        m_pCrop.Width = Convert.ToInt32(strSplits[2]);
                        m_pCrop.Height = Convert.ToInt32(strSplits[3]);
                    }
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine("!!!!!!!!!!!! jasonRead()] " + ex.Message);
                Debug.WriteLine(String.Format("IDX={0} DEV={1} TOKEN={2}", n, devToken, tokenName));
            }
            Debug.WriteLine("############ COUNT]" + m_pRectanges.Count + ", Enable=" + bEnable);

            if ((bSuccess == false) || (bEnable == false))
                m_pRectanges.Clear();
        }

        public void jasonRead2(int nDevNo)
        {
            String fileName = @".\parseJason.txt";
            if (System.IO.File.Exists(fileName) == false)
                return;
            bool bSuccess = false, bEnable = false;
            int n = 0;
            String devToken = "", tokenName = "";
            m_pRectanges.Clear();
            m_pCrop.Width = m_pCrop.Height = 0;
            try
            {
                String allText = System.IO.File.ReadAllText(fileName).Replace("\t", "");
                String allRead = allText.Replace(" ", "");
                devToken = String.Format("DEVICE{0}", nDevNo);
                //Debug.WriteLine(allRead);

                JObject jObj = JObject.Parse(allRead);
                Debug.WriteLine(jObj.Count);
                JToken idToken = jObj[devToken];
                String capRead = idToken.ToString();
                Debug.WriteLine(capRead);
                JObject jDevObj = JObject.Parse(capRead);

                idToken = jDevObj["BaseScale"];
                if (idToken != null)
                {
                    m_pBaseScale = Convert.ToDouble(idToken.ToString());
                }

                idToken = jDevObj["Enable"];
                if (idToken != null)
                {
                    if (idToken.ToString() == "1")
                        bEnable = true;
                    Debug.WriteLine("########################## Enabled=" + bEnable);
                }

                idToken = jDevObj["CropRect"];
                if (idToken != null)
                {
                    String readValue = idToken.ToString();
                    String[] strSplits = readValue.Split(',');
                    // "CropRect":"286,32,1278,400",
                    if (strSplits.Length >= 4)
                    {
                        m_pCrop.x = Convert.ToInt32(strSplits[0]);
                        m_pCrop.y = Convert.ToInt32(strSplits[1]);
                        m_pCrop.Width = Convert.ToInt32(strSplits[2]);
                        m_pCrop.Height = Convert.ToInt32(strSplits[3]);
                    }
                }

                for (n = 0; n < 40; n++)
                {
                    tokenName = "";
                    switch (n)
                    {
                        case 0: tokenName = "auto-idle"; break;
                        case 1: tokenName = "PPID"; break;
                        case 2: tokenName = "PRESS-SET"; break;
                        case 3: tokenName = "PRESS-MON"; break;
                        case 4: tokenName = "STEP"; break;
                        case 5: tokenName = "VALVE-SET"; break;
                        case 6: tokenName = "VALVE-MON"; break;
                        case 7: tokenName = "RF-SET"; break;
                        case 8: tokenName = "RF-MON"; break;
                        case 9: tokenName = "RFr-SET"; break;
                        case 10: tokenName = "RFr-MON"; break;
                        case 11: tokenName = "GAS1-SET"; break;
                        case 12: tokenName = "GAS1-MON"; break;
                        case 13: tokenName = "GAS2-SET"; break;
                        case 14: tokenName = "GAS2-MON"; break;
                        case 15: tokenName = "TEMP1-SET"; break;
                        case 16: tokenName = "TEMP1-MON"; break;
                        case 17: tokenName = "TEMP2-SET"; break;
                        case 18: tokenName = "TEMP2-MON"; break;
                        case 19: tokenName = "TEMP3-SET"; break;
                        case 20: tokenName = "TEMP3-MON"; break;
                        case 21: tokenName = "EFD-SET"; break;
                        case 22: tokenName = "EFD-MON"; break;
                        case 23: tokenName = "TIME-SET"; break;
                        case 24: tokenName = "TIME-MON"; break;
                        case 25: tokenName = "CYCLE-SET"; break;
                        case 26: tokenName = "CYCLE-MON"; break;
                        case 27: tokenName = "auto-end"; break;
                        case 28: tokenName = "last"; break;
                    }
                    if (tokenName.Length == 0)
                    {
                        bSuccess = true;
                        break;
                    }
                    idToken = jDevObj[tokenName];
                    String readValue = idToken.ToString();

                    JObject objInfos = JObject.Parse(readValue);
                    CaptureRectangle pRect = new CaptureRectangle();
                    pRect.x = Convert.ToInt32(objInfos["x"].ToString());
                    pRect.y = Convert.ToInt32(objInfos["y"].ToString());
                    pRect.Width = Convert.ToInt32(objInfos["Width"].ToString());
                    pRect.Height = Convert.ToInt32(objInfos["Height".ToString()]);
                    pRect.picType = Convert.ToInt32(objInfos["picType"].ToString());
                    int nValue = Convert.ToInt32(objInfos["dataType".ToString()]);
                    pRect.dataType = nValue / 100;
                    pRect.dataSub = nValue % 100;
                    nValue = Convert.ToInt32(objInfos["dataLoc"].ToString());
                    pRect.locMain = nValue / 100;
                    pRect.locSub = nValue % 100;
                    pRect.Resize = m_pBaseScale;
                    if(objInfos["Scale"] != null)
                        pRect.Resize = Convert.ToDouble(objInfos["Scale"].ToString());
                    m_pRectanges.Add(pRect);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("!!!!!!!!!!!! jasonRead()] " + ex.Message);
                Debug.WriteLine(String.Format("IDX={0} DEV={1} TOKEN={2}", n, devToken, tokenName));
            }

            if ((bSuccess == false) || (bEnable == false))
                m_pRectanges.Clear();
        }
    }

    public class CaptureRectangle
    {
        public int x { get; set; }
        public int y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int picType { get; set; }
        public int dataType { get; set; }
        public int dataSub { get; set; }
        public int locMain { get; set; }
        public int locSub { get; set; }
        public double Resize { get; set; }
    }
}
