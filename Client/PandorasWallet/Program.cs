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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if DEBUG
            string lString = Environment.ExpandEnvironmentVariables(@"%PW_RootPath%Server\Wallet");
            (new IisExpressWebServer(lString, 20159)).Start();
#endif
            PandoraClientControl lControler = null;
            LogInitialize();
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                lControler = PandoraClientControl.GetController(new PandoraClientMainWindow());
                Application.Run(lControler.MainForm);
            }
            catch (Exception ex)
            {
                lControler?.MainForm.StandardErrorMsgBox("Error at initialization of Pandoras Wallet. Details: " + ex.Message + " on " + ex.Source);
                Utils.PandoraLog.GetPandoraLog().Write("Error at initialization of Pandoras Wallet. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                try
                {
                    lControler.Dispose();
                }
                catch(Exception ex)
                {
                    Log.Write(ex.Message);
                }
                (Log.SystemLog as PandoraLog).Active = false;
            }
        }

        private static void LogInitialize()
        {
            PandoraLog lLog = PandoraLog.GetPandoraLog();
            Log.LogLevelFlag = LogLevelFlags.All;
            lLog.FileName = Properties.Settings.Default.LogFileName;
            Pandora.Client.Universal.Log.WriteAppEvent("PandoraClient log file is '" + lLog.FileName + "'", System.Diagnostics.EventLogEntryType.Information, 6004);
            Pandora.Client.Universal.Log.WriteAppEvent("Pandora current working folder '" + System.IO.Directory.GetCurrentDirectory() + "'", System.Diagnostics.EventLogEntryType.Information, 6004);
            lLog.LineLength = 120;
            lLog.MaxSize = 5;
            lLog.Active = true;
            Log.SystemLog = lLog;
            Log.Write("*********** Pandora Client Started!");
            Pandora.Client.Universal.Log.WriteAppEvent("Pandora Client log started.", System.Diagnostics.EventLogEntryType.Information, 6005);
        }
    }

#if DEBUG

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