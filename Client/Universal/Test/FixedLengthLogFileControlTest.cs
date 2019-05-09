using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pandora.Client.Universal.Test
{
    [TestClass]
    public class FixedLengthLogFileControlTest
    {
        [TestMethod]
        public void TestCreate()
        {
            new FixedLengthLogFileControl();
        }

        [TestMethod]
        public void TestActive()
        {
            var lLog = new FixedLengthLogFileControl();
            try
            {
                lLog.FileName = Path.GetTempFileName();
                lLog.Active = true;
            }
            finally
            {
                lLog.Active = false;
                if (File.Exists(lLog.FileName))
                    File.Delete(lLog.FileName);
            }
        }

        [TestMethod]
        public void FileRollover()
        {
            FErrorList.Clear();
            FPos = 0;
            var lPath = Path.GetTempPath();
            var lFileName = Path.Combine(lPath, "testrollover.log");
            var lLog = new FixedLengthLogFileControl();
            try
            {
                lLog.FileName = lFileName;
                lLog.IncludeDate = true;
                lLog.IncludeTime = true;
                lLog.LineLength = 132;
                lLog.MaxSize = 1;
                lLog.OnLogError += LLog_OnLogError;
                lLog.OnReadWritePosition += LLog_OnReadWritePosition;
                lLog.FileRollOver = true;
                lLog.Active = true;
                lLog.Write("**********************************************************************************************");
                for (int i = 0; i < 1025000 / 132; i++)
                {
                    lLog.Write("Line {0}", i);
                    System.Threading.Thread.Sleep(2);
                }
                System.Threading.Thread.Sleep(1000);
                if (FErrorList.Count > 0)
                    throw new Exception(FErrorList[FErrorList.Count - 1]);
                var files = Directory.GetFiles(Path.Combine(lPath,"OldLogs"), "testrollover*.log");
                Assert.IsTrue(files.Length >= 1, "Missing roll over file.");
                foreach (var file in files)
                    File.Delete(file);
            }
            finally
            {
                lLog.Active = false;
                if (File.Exists(lLog.FileName))
                    File.Delete(lLog.FileName);
            }
        }

        long FPos = 0;
        private void LLog_OnReadWritePosition(object aSender, ref long aPos, bool aReadPos)
        {
            if (aReadPos)
                aPos = FPos;
            else
                FPos = aPos;
        }

        List<string> FErrorList = new List<string>();
        private void LLog_OnLogError(object aSender, string aErrorMsg, string aLogText)
        {
            FErrorList.Add(aErrorMsg);  
        }
    }
}
