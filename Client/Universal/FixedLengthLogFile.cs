using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Pandora.Client.Universal.Threading;

namespace Pandora.Client.Universal
{
    public class FixedLengthLogFile : BaseLogfile
    {
        public delegate void ErrorEvent(object aSender, string aErrorMsg, string aLogText);
        public delegate void ReadWritePosition(object aSender, ref long aPos, bool aReadPos);
        ErrorEvent FOnErrorEvent;
        ReadWritePosition FOnReadWritePosition;

        public ushort LineLenght { get; private set; }

        public bool IncludeDate { get; private set; }

        public bool IncludeTime { get; private set; }

        public ushort MaxSize { get; private set; }

        public bool FileRollOver { get; private set; }


        public FixedLengthLogFile(string aFileName,
            ushort aLineLength = 80,
            ushort aMaxSize = 0,
            bool aDate = false,
            bool aTime = false,
            bool aFileRollOver = false,
            ErrorEvent aOnErrorEvent = null,
            ReadWritePosition aOnReadWritePosition = null)
            : base(aFileName)
        {
            if (aLineLength < 20)
                throw new InvalidDataException("LineLength must be greater than 20");
            LineLenght = aLineLength;
            MaxSize = aMaxSize;
            IncludeDate = aDate;
            IncludeTime = aTime;
            FileRollOver = aFileRollOver;
            FOnErrorEvent = aOnErrorEvent;
            FOnReadWritePosition = aOnReadWritePosition;
            this.OnErrorEvent += new ErrorHandlerDelegate(ErrorHandler);
        }

        private void ErrorHandler(object sender, Exception e, ref bool isHandled)
        {
            // NOTE: this event handler is called on the owner thread
            //       only if the SynchronzingObject is set it also blocks 
            //       the current thread from executing.
            string lLogText = "";
            isHandled = (e is ThreadLogError);
            if (isHandled) lLogText = (e as ThreadLogError).FLogText;
            FOnErrorEvent?.Invoke(this, e.Message, lLogText);
        }

        protected override void ThreadWrite(string aText, DateTime aTime)
        {
            if (aText.Length > 65535)
                aText = "Critical Log Error!  text to long";
            var lLines = new List<string>();
            lLines.AddRange(aText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            if (lLines.Count == 0)
                lLines.Add("");
            if (lLines.Count > 65535)
            {
                ThreadWrite("Critical Log Error!  too many lines", aTime);
                return;
            }
            WriteToStream(AddLineData(lLines, aTime));
        }

        protected override Stream MakeFileStream()
        {
            Stream lResult = base.MakeFileStream();
            if (MaxSize > 0)
            {
                long lStartPos = -1;
                const int OneMeg = 1048576;
                if (lResult is FileStream)
                {
                    ReadPosition(ref lStartPos);
                    if (lStartPos == -1)
                        lStartPos = lResult.Position;
                    if (lStartPos > MaxSize * OneMeg)
                        if (FileRollOver)
                            lResult = DoFileRollOver(lResult);
                        else
                            lStartPos = 0;
                    if (lStartPos > lResult.Length)
                        lStartPos = lResult.Length;
                    lResult.Position = lStartPos;
                }
            }
            return lResult;
        }

        private Stream DoFileRollOver(Stream aLogFileStream)
        {
            string lPath = Path.GetDirectoryName(this.FileName);
            string lNewFileName = Path.GetFileNameWithoutExtension(FileName);
            string lDate = DateTime.Now.ToString("' 'yyyy'-'MM'-'dd' 'HH'_'mm'_'ss");
            lNewFileName = string.Format("{0}{1}.log", lNewFileName, lDate);
            lPath = Path.Combine(lPath, "OldLogs");
            try
            {
                if (!Directory.Exists(lPath))
                    Directory.CreateDirectory(lPath);
                lNewFileName = Path.Combine(lPath, lNewFileName);
            }
            catch (Exception ex)
            {
                this.DoErrorHandler(ex);
                return aLogFileStream;
            }
            if (File.Exists(lNewFileName))
            {
                System.Threading.Thread.Sleep(2000);
                return DoFileRollOver(aLogFileStream);
            }
            try
            {
                File.Copy(FileName, lNewFileName);
            }
            catch (Exception ex)
            {
                DoErrorHandler(ex);
                if (File.Exists(lNewFileName))
                    File.Delete(lNewFileName);
                return aLogFileStream;
            }
            aLogFileStream.Close();
            aLogFileStream.Dispose();
            while (File.Exists(FileName))
                try
                {
                    File.Delete(FileName);
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(3000);
                    DoErrorHandler(ex);
                }
            return base.MakeFileStream();
        }

        protected override void CloseStream()
        {
            if (MaxSize != 0 && GetStream() is FileStream)
                WritePosition(GetStream().Position);
            base.CloseStream();
        }

        protected virtual void ReadPosition(ref long aPos)
        {
            FOnReadWritePosition(this, ref aPos, true);
        }

        protected virtual void WritePosition(long aPos)
        {
            FOnReadWritePosition(this, ref aPos, false);
        }

        private string LineFormat(string aLine, DateTime aTime)
        {
            return string.Format("{0}|{1}{2}{3}", aLine.PadRight(LineLenght), IncludeDate ? aTime.ToString("yyyy/MM/dd|") : "", IncludeTime ? aTime.ToString("HH:mm:ss.ffff|") : "", Environment.NewLine);
        }

        private string AddLineData(List<string> lLines, DateTime aTime)
        {
            var lStrBuilder = new StringBuilder();
            foreach (var lLine in lLines)
            {
                if (lLine.Length > LineLenght)
                    lStrBuilder.Append(CutLine(lLine, aTime));
                else
                    lStrBuilder.Append(LineFormat(lLine, aTime));
            }
            return lStrBuilder.ToString();
        }

        private string CutLine(string aLine, DateTime aTime)
        {
            if (aLine.Length <= LineLenght) return LineFormat(aLine, aTime);
            return LineFormat(aLine.Substring(0, LineLenght), aTime) + CutLine(aLine.Substring(LineLenght), aTime);
        }

        protected override void InternalFinalize()
        {
            base.InternalFinalize();
        }

    }
}
