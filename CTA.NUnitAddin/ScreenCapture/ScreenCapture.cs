using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.IO;
using CTA.NUnitAddin.Service;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using log4net;

namespace CTA.NUnitAddin
{
    public class ScreenCapture
    {
        private ILog Logger { get { return LogManager.GetLogger(this.GetType()); } }

        private Image _screenShot = null;
        public Image ScreenShot
        {
            get
            {
                return _screenShot;
            }

            set
            {
                _screenShot = value;
            }
        }

        public bool HasTook
        {
            get
            {
                return _screenShot != null;
            }
        }

        private ImageFormat DefaultScreenshotFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }

        private string DefaultPath
        {
            get
            {
                return Path.Combine(CTAService.CtaExecutionDir, "DesktopImg_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png");
            }
        }

        public void TakeAndSave(string savePath, ImageFormat format)
        {
            try
            {
                Image img = TakeScreenshot();
                img.Save(savePath, format);
            }
            catch (Exception e)
            {
                Logger.Error("Cannot take and save screenshot in following path : " + savePath);
                Logger.Error(e.Message);
                Logger.Error(e.InnerException);
            }
        }

        public void TakeAndSave(string savePath)
        {
            TakeAndSave(savePath, DefaultScreenshotFormat);
        }

        public void TakeAndSave()
        {
            TakeAndSave(DefaultPath, DefaultScreenshotFormat);
        }

        public void Take()
        {
            ScreenShot = TakeScreenshot();
        }

        public void Save(string savePath)
        {
            if (ScreenShot != null)
            {
                try
                {
                    ScreenShot.Save(savePath, DefaultScreenshotFormat);
                    ScreenShot = null;
                }
                catch (Exception e)
                {
                    Logger.Error("Cannot save screenshot in following path : " + savePath);
                    Logger.Error(e.Message);
                    Logger.Error(e.InnerException);
                }
            }
        }

        public string GetEtapScreenshotPath(string testCaseID)
        {
            string name = Regex.Replace(testCaseID, @"\s", "");
            if (name.Trim().StartsWith("Eikon.MonitoringTests.Tests."))
            {
                name = Regex.Replace(name, @"Eikon.MonitoringTests.Tests.", "");
            }
            else if (name.Trim().StartsWith("Eikon.OPSConfidenceTests.Tests."))
            {
                name = Regex.Replace(name, @"Eikon.OPSConfidenceTests.Tests.", "");
            }
            try
            {
                string path = Path.Combine(CTAService.CtaExecutionDir, Regex.Replace(name, @"\([^\)]*\)", "") + "_" + DateTime.Now.ToString("HH_mm_ss") + ".png");
            }
            catch (Exception e)
            {
                Logger.Error("Cannot combine paths to create screenshot path for testCaseID : " + testCaseID);
                Logger.Error("String 1 : " + CTAService.CtaExecutionDir);
                Logger.Error("String 2 : " + Regex.Replace(name, @"\([^\)]*\)", "") + "_" + DateTime.Now.ToString("HH_mm_ss") + ".png");
                Logger.Error(e.Message);
                Logger.Error(e.InnerException);
                return "";
            }
            Logger.Info("The screenshot Path is :" + path);
            return path;
        }


        private Image TakeScreenshot()
        {
            WIN32_API.SIZE size;

            IntPtr hDC = WIN32_API.GetDC(WIN32_API.GetDesktopWindow());
            IntPtr hMemDC = WIN32_API.CreateCompatibleDC(hDC);

            size.cx = WIN32_API.GetSystemMetrics(WIN32_API.SM_CXSCREEN);
            size.cy = WIN32_API.GetSystemMetrics(WIN32_API.SM_CYSCREEN);

            m_HBitmap = WIN32_API.CreateCompatibleBitmap(hDC, size.cx, size.cy);

            if (m_HBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)WIN32_API.SelectObject(hMemDC, m_HBitmap);
                WIN32_API.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC, 0, 0, WIN32_API.SRCCOPY | WIN32_API.CAPTUREBLT);
                WIN32_API.SelectObject(hMemDC, hOld);
                WIN32_API.DeleteDC(hMemDC);
                WIN32_API.ReleaseDC(WIN32_API.GetDesktopWindow(), hDC);
                return System.Drawing.Image.FromHbitmap(m_HBitmap);
            }
            return null;
        }

        protected IntPtr m_HBitmap;
    }

    public class WIN32_API
    {
        public struct SIZE
        {
            public int cx;
            public int cy;
        }
        public const int SRCCOPY = 13369376;
        public const int CAPTUREBLT = 1073741824;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, int RasterOp);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
    }
}

