using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Pandora.Client.Universal
{
    public class ConfigFile
    {
        private Dictionary<string, object> FValues = new Dictionary<string, object>();

        protected virtual void AddSetting(string aKeyName, object aValue)
        {
            FValues.Add(aKeyName, aValue);
        }

        protected virtual bool DeleteSetting(string aKeyName)
        {
            FValues.Remove(aKeyName);
            return true;
        }

        protected virtual void ModifySetting(string aKeyName, object aReplacementValue)
        {
            FValues[aKeyName] = aReplacementValue;
        }

        public virtual void Clear()
        {
            FValues.Clear();
        }

        public virtual void LoadFromFile(string aFileName)
        {
            using (FileStream lFile = new FileStream(aFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadFromStream(lFile);
                lFile.Close();
            }
        }

        public virtual void LoadFromStream(Stream aStream)
        {
            using (StreamReader lStreamReader = new StreamReader(aStream))
            {
                this.Clear();
                while (!lStreamReader.EndOfStream)
                {
                    string lLine = lStreamReader.ReadLine();
                    string s = lLine.Replace('\t', ' ').Trim();

                    if (s.Length > 3 && s[0] != ';' && s[0] != '#')
                    {
                        if (s[0] == '[')
                        {
                            throw new Exception("not implemented [] sections");
                        }
                        else if (Char.IsLetter(s[0]))
                        {
                            s = lLine;
                            int lEqualIndex = s.IndexOf('=');
                            if (lEqualIndex >= 1)
                                this.AddSetting(s.Substring(0, lEqualIndex), s.Substring(lEqualIndex + 1));
                        }
                    }
                }
            }
        }

        public virtual void SaveToFile(string aFileName)
        {
            using (FileStream lFile = new FileStream(aFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SaveToStream(lFile);
                lFile.Close();
            }
        }

        public virtual void SaveToStream(Stream aStream)
        {
            using (StreamWriter lStreamWriter = new StreamWriter(aStream))
            {
                var lKeys = GetKeyNames();
                foreach (string lKey in lKeys)
                {
                    object lValue = ReadObjectValue(lKey);
                    if (lValue == null)
                        lValue = "";
                    else
                    {
                        if (lValue is String)
                            lValue = lValue.ToString();
                        else if (lValue is DateTime)
                            lValue = ((DateTime)lValue).ToString("yyyy'/'MM'/'dd HH':'mm':'ss");
                        else if (lValue is Decimal)
                            lValue = lValue.ToString();
                        else
                        {
                            if (!lValue.GetType().IsPrimitive)
                                throw new GenericException("Only primitive types and DateTime can be used.");
                            lValue = lValue.ToString();
                        }
                    }
                    lStreamWriter.WriteLine("{0}={1}", lKey, lValue);
                }
            }
        }

        public void WriteIntValue(string aKeyName, int aValue)
        {
            WriteObjectValue(aKeyName, aValue);
        }

        public void WriteStringValue(string aKeyName, string aValue)
        {
            WriteObjectValue(aKeyName, aValue);
        }

        public void WriteObjectValue(string aKeyName, object aValue)
        {
            if (this.KeyExists(aKeyName))
                this.ModifySetting(aKeyName, aValue);
            else
                this.AddSetting(aKeyName, aValue);
        }

        public void WriteBoolValue(string aKeyName, bool aValue)
        {
            WriteObjectValue(aKeyName, aValue);
        }

        public void WriteByteArray(string aKeyName, byte[] aValue)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < aValue.Length; i++)
                sb.AppendFormat("{0:X2}", aValue[i]);
            WriteStringValue(aKeyName, sb.ToString());
        }

        public int ReadIntValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToInt32(this.ReadObjectValue(aKeyName, aDefaultValue));
        }

        public long ReadLongValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToInt64(this.ReadObjectValue(aKeyName, aDefaultValue));
        }

        public string ReadStringValue(string aKeyName, string aDefaultValue)
        {
            return Convert.ToString(this.ReadObjectValue(aKeyName, aDefaultValue));
        }

        public object ReadObjectValue(string aKeyName, object aDefaultValue)
        {
            if (KeyExists(aKeyName))
                return FValues[aKeyName];
            else
            {
                if (aDefaultValue == null)
                    throw new ArgumentNullException(aKeyName + " is not found and defualt value is null");
                return aDefaultValue;
            }
        }

        public object ReadObjectValue(string aKeyName)
        {
            return ReadObjectValue(aKeyName, null);
        }

        public bool ReadBoolValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToBoolean(this.ReadObjectValue(aKeyName, aDefaultValue));
        }

        public byte[] ReadByteArray(string aKeyName)
        {
            string s = ReadStringValue(aKeyName, null);
            if (String.IsNullOrEmpty(s))
                return null;
            if (s.Length % 2 != 0)
                throw new GenericException("Invalid byte array data.");
            byte[] lArray = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                lArray[i / 2] = Byte.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            return lArray;
        }

        public bool KeyExists(string aKeyName)
        {
            return FValues.ContainsKey(aKeyName);
        }

        public string[] GetKeyNames()
        {
            List<string> lList = new List<string>();
            foreach (KeyValuePair<string, object> lKeyValue in FValues)
                lList.Add(lKeyValue.Key);
            return lList.ToArray();
        }
    }
}