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
                Utils.PandoraLog.GetPandoraLog().Dispose();
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
            lLog.Active = true;
            Log.SystemLog = PandoraLog.GetPandoraLog();
            Log.Write("*********** Pandora Client Started!");
            Pandora.Client.Universal.Log.WriteAppEvent("Pandora Client log started.", System.Diagnostics.EventLogEntryType.Information, 6005);
        }
    }
}