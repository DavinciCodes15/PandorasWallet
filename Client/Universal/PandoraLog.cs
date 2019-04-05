using System;
using System.IO;
using Pandora.Client.Universal;

namespace Pandora.Client.Universal
{
    public class PandoraLog : FixedLengthLogFileControl
    {
        static PandoraLog FLog;
        PandoraLog()
        {
            OnReadWritePosition += ReadWirtePosition;
        }

        public static PandoraLog GetPandoraLog()
        {
            if (FLog == null)
                FLog = new PandoraLog();
            return FLog;
        }

        const string LOG_PosFile = "pos.dat";

        void ReadWirtePosition(object aSender, ref long aPos, bool aReadPos)
        {
            var lPath = FileName + LOG_PosFile;

            if (aReadPos)
            {
                if (File.Exists(lPath))
                    try
                    {
                        aPos = Convert.ToInt64(File.ReadAllText(lPath));
                    }
                    catch
                    {
                    }
                else
                    aPos = 0;
            }
            else
            {
                File.WriteAllText(lPath, aPos.ToString());

            }
        }

 #if DEBUG
       public override void Write(string aText)
        {
            base.Write(aText);
            System.Diagnostics.Debug.WriteLine(aText);
        }
#endif

    }
}