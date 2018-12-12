﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Logging
{
    public class ThreadSafeFileWriter: TextWriter
    {
        private static readonly object openedFilesLock = new object();
        private static readonly List<string> openedFiles = new List<string>();

        private StreamWriter logWriter;
        private readonly object writeLock;

        public override Encoding Encoding => Encoding.ASCII;

        public ThreadSafeFileWriter(string path)
        {
            logWriter = new StreamWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {AutoFlush = true};
            writeLock = new object();

            lock(openedFilesLock)
            {
                if (openedFiles.Contains(path))
                    throw new System.Exception("Can't have multiple ThreadSafeFileWriters for the same file, reuse a single instance");

                openedFiles.Add(path);
            }
        }

        public override void WriteLine(string logMessage)
        {
            lock(writeLock)
                logWriter.WriteLine(logMessage);            
        }

        protected override void Dispose(bool disposing)
        {
            lock (writeLock)
            {
                logWriter.Close();
                logWriter = null;
            }
        }
    }
}
