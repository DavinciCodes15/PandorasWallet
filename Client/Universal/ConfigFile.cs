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
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                Clear();
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
                        else if (char.IsLetter(s[0]))
                        {
                            s = lLine;
                            int lEqualIndex = s.IndexOf('=');
                            if (lEqualIndex >= 1)
                            {
                                AddSetting(s.Substring(0, lEqualIndex), s.Substring(lEqualIndex + 1));
                            }
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
                string[] lKeys = GetKeyNames();
                foreach (string lKey in lKeys)
                {
                    object lValue = ReadObjectValue(lKey);
                    if (lValue == null)
                    {
                        lValue = "";
                    }
                    else
                    {
                        if (lValue is string)
                        {
                            lValue = lValue.ToString();
                        }
                        else if (lValue is DateTime)
                        {
                            lValue = ((DateTime)lValue).ToString("yyyy'/'MM'/'dd HH':'mm':'ss");
                        }
                        else if (lValue is decimal)
                        {
                            lValue = lValue.ToString();
                        }
                        else
                        {
                            if (!lValue.GetType().IsPrimitive)
                            {
                                throw new GenericException("Only primitive types and DateTime can be used.");
                            }

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
            if (KeyExists(aKeyName))
            {
                ModifySetting(aKeyName, aValue);
            }
            else
            {
                AddSetting(aKeyName, aValue);
            }
        }

        public void WriteBoolValue(string aKeyName, bool aValue)
        {
            WriteObjectValue(aKeyName, aValue);
        }

        public void WriteByteArray(string aKeyName, byte[] aValue)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < aValue.Length; i++)
            {
                sb.AppendFormat("{0:X2}", aValue[i]);
            }

            WriteStringValue(aKeyName, sb.ToString());
        }

        public int ReadIntValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToInt32(ReadObjectValue(aKeyName, aDefaultValue));
        }

        public long ReadLongValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToInt64(ReadObjectValue(aKeyName, aDefaultValue));
        }

        public string ReadStringValue(string aKeyName, string aDefaultValue)
        {
            return Convert.ToString(ReadObjectValue(aKeyName, aDefaultValue));
        }

        public object ReadObjectValue(string aKeyName, object aDefaultValue)
        {
            if (KeyExists(aKeyName))
            {
                return FValues[aKeyName];
            }
            else
            {
                if (aDefaultValue == null)
                {
                    throw new ArgumentNullException(aKeyName + " is not found and defualt value is null");
                }

                return aDefaultValue;
            }
        }

        public object ReadObjectValue(string aKeyName)
        {
            return ReadObjectValue(aKeyName, null);
        }

        public bool ReadBoolValue(string aKeyName, object aDefaultValue)
        {
            return Convert.ToBoolean(ReadObjectValue(aKeyName, aDefaultValue));
        }

        public byte[] ReadByteArray(string aKeyName)
        {
            string s = ReadStringValue(aKeyName, null);
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            if (s.Length % 2 != 0)
            {
                throw new GenericException("Invalid byte array data.");
            }

            byte[] lArray = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                lArray[i / 2] = byte.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }

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
            {
                lList.Add(lKeyValue.Key);
            }

            return lList.ToArray();
        }
    }
}