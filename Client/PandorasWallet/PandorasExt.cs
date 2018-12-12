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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public static class PandorasExt
    {
        public static T GetEnumFromDescription<T>(this string description)
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException();
            }

            foreach (FieldInfo field in type.GetFields())
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
        }

        public static void StandardErrorMsgBox(this Form aform, string aString)
        {
            MessageBox.Show(aString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void StandardInfoMsgBox(this Form aform, string aString)
        {
            MessageBox.Show(aString, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public static void SetWaitCursor(this Form aForm)
        {
            WaitingEnabled(true, aForm);
            Application.DoEvents();
        }

        public static void SetArrowCursor(this Form aForm)
        {
            WaitingEnabled(false, aForm);
            Application.DoEvents();
        }

        private static void WaitingEnabled(bool value, Form aForm)
        {
            if (value == Application.UseWaitCursor)
            {
                return;
            }

            Application.UseWaitCursor = value;
            Cursor.Current = value ? Cursors.WaitCursor : Cursors.Default;
            Form f = aForm;
            f.Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            if (f != null && f.Handle != IntPtr.Zero)
            {
                SendMessage(f.Handle, 0x20, f.Handle, (IntPtr)1);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}