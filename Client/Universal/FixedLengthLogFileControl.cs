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
using System;
using System.IO;

namespace Pandora.Client.Universal
{
    public class FixedLengthLogFileControl : IDisposable, ISystemLog
    {
        protected string FFileName = "";
        private bool FIncludeTime;
        private bool FIncludeDate;
        private ushort FLineLength;
        protected FixedLengthLogFile FLogFile;
        protected FixedLengthLogFile.ErrorEvent FOnLogError;
        protected FixedLengthLogFile.ReadWritePosition FOnReadWritePosition;

        public FixedLengthLogFileControl()
        {
            FLineLength = 80;
            FIncludeTime = true;
            FIncludeDate = true;
        }

        public event FixedLengthLogFile.ErrorEvent OnLogError
        {
            add
            {
                lock (this)
                {
                    FOnLogError += value;
                }
            }
            remove
            {
                lock (this)
                {
                    FOnLogError -= value;
                }
            }
        }

        public event FixedLengthLogFile.ReadWritePosition OnReadWritePosition
        {
            add
            {
                lock (this)
                {
                    FOnReadWritePosition += value;
                }
            }
            remove
            {
                lock (this)
                {
                    FOnReadWritePosition -= value;
                }
            }
        }

        protected virtual void ThreadTerminate(object sender, EventArgs e)
        {
            lock (this)
            {
                FLogFile = null;
            }
        }

        protected void TestActive()
        {
            if (Active)
            {
                throw new ArgumentException("Property can not be set while log is active");
            }
        }

        public virtual void Write(string aText)
        {
            FixedLengthLogFile lLogFile = FLogFile;
            if (lLogFile != null)
            {
                lLogFile.Write(aText);
            }
            else
            {
                throw new ArgumentException("Log must be active before write command is executed");
            }
        }

        public ushort MaxSize { get; set; }

        public void Write(string aText, params object[] aArgs)
        {
            Write(string.Format(aText, aArgs));
        }

        public virtual bool Active
        {
            get
            {
                lock (this)
                {
                    return FLogFile != null;
                }
            }
            set
            {
                if (Active == value)
                {
                    return;
                }

                FixedLengthLogFile lLogFile;
                lock (this)
                {
                    lLogFile = FLogFile;
                }

                if (!value)
                {
                    lLogFile.Terminate();
                    lLogFile.ActiveThread.Join(10000);
                    lock (this)
                    {
                        FLogFile = null;
                    }
                }
                else
                {
                    string lPath;
                    try
                    {
                        lPath = Path.GetDirectoryName(FFileName);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("Invalid file name... '{0}'", FFileName));
                    }
                    if (!string.IsNullOrEmpty(lPath) && !Directory.Exists(lPath))
                    {
                        throw new ArgumentException("Log directory does not exist. " + lPath);
                    }

                    lock (this)
                    {
                        FLogFile = NewFixedLenthLogfile();
                        FLogFile.OnTerminated += new EventHandler(ThreadTerminate);
                    }
                }
                lLogFile = null;
            }
        }

        protected virtual FixedLengthLogFile NewFixedLenthLogfile()
        {
            return new FixedLengthLogFile(FileName, LineLength, MaxSize, IncludeDate, IncludeTime, FOnLogError, FOnReadWritePosition);
        }

        public string FileName
        {
            get => FFileName;
            set
            {
                TestActive();
                FFileName = value;
            }
        }

        public ushort LineLength
        {
            get => FLineLength;
            set
            {
                TestActive();
                FLineLength = value;
            }
        }

        public bool IncludeTime
        {
            get => FIncludeTime;
            set
            {
                TestActive();
                FIncludeTime = value;
            }
        }

        public bool IncludeDate
        {
            get => FIncludeDate;
            set
            {
                TestActive();
                FIncludeDate = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Active = false;
        }

        #endregion IDisposable Members
    }
}