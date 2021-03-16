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
using Pandora.Client.ClientLib;
using Pandora.Client.PandorasWallet.ClientExceptions;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public static class PandorasExt
    {
        public static string[] GetAddresses(this TransactionRecord aTxRecord)
        {
            List<string> lTransactionAddresses = new List<string>();

            if (aTxRecord.Inputs != null)
            {
                lTransactionAddresses.AddRange(aTxRecord.Inputs.Select(x => x.Address).ToArray());
            }

            if (aTxRecord.Outputs != null)
            {
                lTransactionAddresses.AddRange(aTxRecord.Outputs.Select(x => x.Address).ToArray());
            }

            return lTransactionAddresses.ToArray();
        }

        public static T GetEnumFromDescription<T>(this string description)
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new System.InvalidOperationException();
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

        //public static void StandardUnhandledErrorMsgBox(this Form aform, string aMsg, string aErrorTitle = "Unhandled System Error")
        //{
        //    var lDlg = Dialogs.ServerErrorDialog.GetInstance();
        //    Log.Write(LogLevel.Critical, "Error Dialog displayed.\r\nCaption: {0}\r\nMsg: {1}", aErrorTitle, aMsg);
        //    lDlg.ErrorTitle = aErrorTitle;
        //    lDlg.ErrorDetails = aMsg;
        //    lDlg.ParentWindow = aform;
        //    lDlg.Execute();
        //}

        public static void StandardExceptionMsgBox(this Form aform, Exception ex, string aErrorTitle = "Unhandled System Exception")
        {
            if (ex is LoginFailedException)
            {
                StandardErrorMsgBox(aform, "Login Failed", ex.Message);
                return;
            }

            using (var lDlg = Dialogs.ServerErrorDialog.GetInstance())
            {
                lDlg.ErrorMessage = ex.Message;
                lDlg.ErrorTitle = aErrorTitle;
                lDlg.ErrorDetails = string.Format("{0}\r\n----------------------------------------------------------------------------------\r\nUTC Date : {1}\r\n\r\n{2}", ex.Message, DateTime.Now.ToUniversalTime(), ex.StackTrace);
                lDlg.ParentWindow = aform;
                Log.Write(LogLevel.Critical, "Error Dialog displayed.\r\nCaption: {0}\r\nMsg: {1}", aErrorTitle, lDlg.ErrorDetails);
                lDlg.Execute();
            }
        }

        public static void StandardInfoMsgBox(this Form aform, string aTitle, string aDescription = null)
        {
            using (var lMsgBox = MessageBoxDialog.GetInfoBoxDialog(aTitle, aDescription, aform))
            {
                lMsgBox.Execute();
                Log.Write(LogLevel.Info, "Info Dialog displayed.\r\nMsg: {0}", aTitle);
            }
        }

        public static void StandardWarningMsgBox(this Form aform, string aTitle, string aDescription = null)
        {
            using (var lMsgBox = MessageBoxDialog.GetWarningBoxDialog(aTitle, aDescription, aform))
            {
                lMsgBox.Execute();
                Log.Write(LogLevel.Warning, "Warning Dialog displayed.\r\nMsg: {0}", aTitle);
            }
        }

        public static bool StandardWarningMsgBoxAsk(this Form aform, string aTitle, string aDescription = null)
        {
            using (var lMsgBox = MessageBoxDialog.GetWarningBoxDialog(aTitle, aDescription, aform))
            {                
                Log.Write(LogLevel.Warning, "Warning Dialog displayed.\r\nMsg: {0}", aTitle);
                return lMsgBox.Execute();
            }
        }

        public static void StandardErrorMsgBox(this Form aform, string aTitle, string aDescription = null)
        {
            using (var lMsgBox = MessageBoxDialog.GetErrorBoxDialog(aTitle, aDescription, aform))
            {
                lMsgBox.Execute();
                Log.Write(LogLevel.Error, "Error Dialog displayed.\r\nCaption: {0}\r\nMsg: {1}", aTitle, aDescription);
            }
        }

        public static bool StandardAskMsgBox(this Form aform, string aTitle, string aDescription = null)
        {
            using (var lMsgBox = MessageBoxDialog.GetAskBoxDialog(aTitle, aDescription, aform))
            {
                Log.Write(LogLevel.Info, "Question Dialog displayed.\r\nMsg: {0}", aTitle);
                return lMsgBox.Execute();
            }
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