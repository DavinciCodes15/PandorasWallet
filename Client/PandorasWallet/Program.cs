//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
using Pandora.Client.PandorasWallet.Utils;
using Pandora.Client.Universal;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public static class Globals
    {
        public static bool Logout = false;
        public static bool Testnet = true;

        public static System.Drawing.Icon BytesToIcon(byte[] bytes)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                return new System.Drawing.Icon(ms);
            }
        }
    }

    internal static class Program
    {
        public static string UpgradeFileName = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        private static void Main()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            PandoraClientControl lControler = null;
#if DEBUG
            DebugCode();
#endif

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                lControler = PandoraClientControl.GetController(new AppMainForm());
                Application.Run(lControler.AppMainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error at initialization of Pandoras Wallet.\r\nDetails: \n{0}", ex));
            }
            finally
            {
                lControler?.Dispose();
            }
            if (UpgradeFileName != null && System.IO.File.Exists(UpgradeFileName))
            {
                try
                {
                    string lVbs = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upgrade.vbs");
                    Process.Start("wscript", $"\"{lVbs}\" \"{UpgradeFileName}\"");
                }
                catch
                {
                    
                }
            }
        }


        private static void LogInitialize()
        {
        }

#if DEBUG
        private static void DebugCode()
        {
            try
            {
                string lString = Environment.ExpandEnvironmentVariables(@"%PW_RootPath%Server\Wallet");
                (new IisExpressWebServer(lString, 20159)).Start();
            }
            catch (Exception ex)
            {
                //This will trigger if this is a installed debug version
                Log.Write(LogLevel.Warning, string.Format("Failed to initialize iisExpress. Details: {0}", ex));
            }
        }

        public class IisExpressWebServer
        {
            private int FPort;
            private string FFullPath;
            private static Process _webHostProcess;

            public IisExpressWebServer(string aFullPath, int aPortNumber)
            {
                FFullPath = aFullPath;
                FPort = aPortNumber;
            }

            public void Start()
            {
                ProcessStartInfo webHostStartInfo = InitializeIisExpress(FFullPath, FPort);
                _webHostProcess = Process.Start(webHostStartInfo);
            }

            public void Stop()
            {
                if (_webHostProcess == null)
                {
                    return;
                }

                if (!_webHostProcess.HasExited)
                {
                    _webHostProcess.Kill();
                }

                _webHostProcess.Dispose();
            }

            public string BaseUrl => string.Format("http://localhost:{0}", FPort);

            private static ProcessStartInfo InitializeIisExpress(string aFullPath, int aPortNumber)
            {
                // todo: grab stdout and/or stderr for logging purposes?
                string key = Environment.Is64BitOperatingSystem ? "programfiles(x86)" : "programfiles";
                string programfiles = Environment.GetEnvironmentVariable(key);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    ErrorDialog = true,
                    LoadUserProfile = true,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    Arguments = string.Format("/path:\"{0}\" /port:{1}", aFullPath, aPortNumber),
                    FileName = string.Format("{0}\\IIS Express\\iisexpress.exe", programfiles)
                };

                return startInfo;
            }
        }

#endif

    }
}