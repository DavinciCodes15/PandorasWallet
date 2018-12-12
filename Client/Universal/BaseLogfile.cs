﻿//   Copyright 2017-2019 Davinci Codes
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
using Pandora.Client.Universal.Threading;
using System;
using System.IO;
using System.Threading;

namespace Pandora.Client.Universal
{
    public class BaseLogfile : MethodJetThread
    {
        internal class ThreadLogError : GenericException
        {
            public string FLogText;

            public ThreadLogError(string aMessage) : base(aMessage)
            {
                FLogText = null;
            }

            public ThreadLogError(string aLogText, string aMsg, Exception aInner)
                : base(aMsg, aInner)
            {
                FLogText = aLogText;
            }
        }

        protected delegate void ThreadWriteEvent(string aText, DateTime aTime);

        private Stream FStream;
        private MemoryStream FBackupStream = new MemoryStream();
        protected ThreadWriteEvent OnThreadWriteEvent;

        public string FileName { get; private set; }

        public BaseLogfile(string aFileName)
            : base()
        {
            FileName = aFileName;
            Initialize();
            FStream = MakeFileStream();
            CloseStream();
            OnThreadWriteEvent += new ThreadWriteEvent(ThreadWrite);
            Run();
        }

        protected virtual void Initialize()
        {
        }

        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            ActiveThread.Priority = System.Threading.ThreadPriority.Lowest;
        }

        protected virtual void CloseStream()
        {
            if (FStream != FBackupStream)
            {
                FStream.Close();
            }

            FStream = null;
        }

        protected virtual Stream GetStream()
        {
            if (FStream == null)
            {
                try
                {
                    FStream = MakeFileStream();
                    SaveBackup();
                }
                catch
                {
                    Log.WriteAppEvent("Log Stream Error", System.Diagnostics.EventLogEntryType.Error, Log.SE_ID_Stream_Error);
                    FStream = FBackupStream;
                }
            }

            return FStream;
        }

        protected virtual Stream MakeFileStream()
        {
            FileStream lResult = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            try
            {
                lResult.Position = lResult.Length;
            }
            catch
            {
                lResult.Dispose();
                lResult = null;
                throw;
            }
            return lResult;
        }

        private bool SaveBackup()
        {
            FStream = GetStream();
            if (FBackupStream.Length > 0 && FStream != FBackupStream)
            {
                try
                {
                    FBackupStream.Position = 0;
                    FStream.Write(FBackupStream.ToArray(), 0, Convert.ToInt32(FBackupStream.Length));
                    FBackupStream.SetLength(0);
                }
                catch
                {
                    FBackupStream.Position = FBackupStream.Length;
                    FStream = null;
                    return false;
                }
            }

            return true;
        }

        protected override void InternalFinalize()
        {
            base.InternalFinalize();
            if (FBackupStream.Length > 0)
            {
                try
                {
                    int lRetryCount = 0;
                    while (!SaveBackup())
                    {
                        lRetryCount++;
                        Thread.Sleep(10);
                        if (lRetryCount > 200)
                        {
                            throw new ThreadLogError(
                                string.Format(
                                System.Globalization.CultureInfo.CurrentCulture,
                                "Unable able to access {0}.  Logging information has been lost.", FileName));
                        }
                    }
                }
                finally
                {
                    CloseStream();
                }
            }
        }

        protected virtual void ThreadWrite(string aText, DateTime aTime)
        {
            WriteToStream(string.Format("{0}|{1}" + Environment.NewLine, aText, aTime.ToString()));
        }

        protected void WriteToStream(string aText)
        {
            try
            {
                try
                {
                    StreamWriter lStreamWriter = new StreamWriter(GetStream());
                    lStreamWriter.Write(aText);
                    lStreamWriter.Flush();
                }
                catch (Exception E)
                {
                    throw new ThreadLogError(aText, E.Message, E);
                }
            }
            finally
            {
                CloseStream();
            }
        }

        public void Write(string aText)
        {
            BeginInvoke(OnThreadWriteEvent, aText, DateTime.Now);
        }
    }
}