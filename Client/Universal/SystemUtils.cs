using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

#if MONO
#else

using System.Drawing;

#endif

namespace Pandora.Client.Universal
{
    public static class SystemUtils
    {
        public static string StandardDateTimeToStringDate(this DateTime aDateTime)
        {
            return aDateTime.ToString("yyyy'/'MM'/'dd HH':'mm':'ss");
        }

#if MONO
#else

        public static Icon BytesToIcon(byte[] bytes)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                return new Icon(ms);
            }
        }

#endif

        public static DateTime StandardStringDateToDateTime(this string aDateTime)
        {
            if (aDateTime == null)
            {
                aDateTime = string.Empty;
            }
            if (!DateTime.TryParseExact(aDateTime, "yyyy/MM/dd HH:mm:ss.ffff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime lResult))
            {
                lResult = DateTime.ParseExact(aDateTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }

            return lResult;
        }

        public static EmailSystem EmailSystem;

        public static Process GetProcess(string aProcessName)
        {
            Process lResult = null;
            Process[] lList = Process.GetProcessesByName(aProcessName);
            if (lList.Length == 0)
            {
                lList = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(aProcessName));
            }

            if (lList.Length > 0)
            {
                lResult = lList[0];
            }

            return lResult;
        }

        public static bool IsProcessRunning(string aProcessName)
        {
            return GetProcess(aProcessName) != null;
        }

        public static bool SendEmail(string aFrom, string aToList, string aSubject, string aBody, string aServer, int aPort, string aUsername, string aPassword, bool aUseSSL, string aCertFile = null)
        {
            EmailSystem lEmail = new EmailSystem
            {
                Enabled = true
            };
            string[] lList = aFrom.Split(';');
            if (lList.Length > 1)
            {
                lEmail.FromAddress = new EmailSystem.Address(lList[0], lList[1]);
            }
            else
            {
                lEmail.FromAddress.Email = lList[0];
            }

            lList = aToList.Split(';');
            for (int i = 0; i < lList.Length; i += 2)
            {
                lEmail.ToAddress.Add(new EmailSystem.Address(lList[i], lList[i + 1]));
            }

            lEmail.ServerName = aServer;
            lEmail.Port = aPort;
            lEmail.UserName = aUsername;
            lEmail.Password = aPassword;
            lEmail.UseSSL = aUseSSL;
            lEmail.CertificateFileName = aCertFile;

            return lEmail.Send(aSubject, aBody);
        }

        public static void SendEmail(string aSubject, string aBody)
        {
            if (EmailSystem != null)
            {
                EmailSystem.SendAsync(aSubject, aBody);
            }
        }

        public static string SubjectLine = "Pandora's Application Critical Error";

        public static void CriticalError(string aMethodName, string aClassName, Exception e, string aMessage)
        {
            string s = string.Format("{0}\n{1}.{2}\n{3}", aMessage, aClassName, aMethodName, FormatException(e));
            Log.Write(LogLevel.Critical, s);
            SendEmail(SubjectLine, s);
        }

        private static string FormatException(Exception e)
        {
            if (e != null)
            {
                return string.Format("\nException Message: {0}\n\nSource: {1}\nStack Trace :{2}", e.Message, e.Source, e.StackTrace, FormatException(e.InnerException));
            }
            else
            {
                return "";
            }
        }
    }

    public class EmailSystem
    {
        public bool Enabled { get; set; }

        public class Address
        {
            private string FEmail;

            public Address(Address aAddress)
            {
                Name = aAddress.Name;
                FEmail = aAddress.Email;
            }

            public Address(string aName = null, string aEmail = null)
            {
                Name = aName;
                Email = aEmail;
            }

            public string Email
            {
                get => FEmail;

                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!EmailSystem.IsValidEmail(value))
                        {
                            throw new ArgumentException("Invalid email format.");
                        }
                    }

                    FEmail = value;
                }
            }

            public string Name { get; set; }
        }

        public EmailSystem()
        {
            ToAddress = new List<Address>();
            FromAddress = new Address();
        }

        public EmailSystem(EmailSystem aEmailSystem)
        {
            FromAddress = new Address(aEmailSystem.FromAddress);
            ToAddress = new List<Address>();
            foreach (Address lAddress in aEmailSystem.ToAddress)
            {
                ToAddress.Add(new Address(lAddress));
            }

            Port = aEmailSystem.Port;
            ServerName = aEmailSystem.ServerName;
            UserName = aEmailSystem.UserName;
            Password = aEmailSystem.Password;
            CertificateFileName = aEmailSystem.CertificateFileName;
            UseSSL = aEmailSystem.UseSSL;
        }

        public int Port { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CertificateFileName { get; set; }

        public string LastError
        {
            get;
            private set;
        }

        public bool UseSSL { get; set; }
        public Address FromAddress { get; set; }
        public List<Address> ToAddress { get; private set; }

        public bool Send(string aSubject, string aBody)
        {
            if (!Enabled)
            {
                return false;
            }

            LastError = "";
            Pandora.Client.Universal.Log.Write(LogLevel.Debug, "Sending email");
            System.Net.Mail.MailMessage lMailMessage = new System.Net.Mail.MailMessage
            {
                Subject = aSubject,
                Body = aBody + "\n" + DateTime.Now.ToString(),
                From = new System.Net.Mail.MailAddress(FromAddress.Email, FromAddress.Name)
            };
            if (ToAddress.Count == 0)
            {
                throw new Exception("No ToAddress has been provided.");
            }

            foreach (Address lAddress in ToAddress)
            {
                lMailMessage.To.Add(string.Format("{0} <{1}>", lAddress.Name, lAddress.Email));
            }

            System.Net.Mail.SmtpClient lSmtp = new System.Net.Mail.SmtpClient(ServerName, Port)
            {
                EnableSsl = UseSSL
            };
            if (!string.IsNullOrEmpty(CertificateFileName))
            {
                lSmtp.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateFileName));
            }

            lSmtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            lSmtp.UseDefaultCredentials = false;

            lSmtp.Credentials = new System.Net.NetworkCredential(UserName, Password);
            try
            {
                Log.Write(LogLevel.Debug, "        Port : {0}", Port);
                Log.Write(LogLevel.Debug, "        Server : {0}", ServerName);
                Log.Write(LogLevel.Debug, "        Subject : {0}", aSubject);
                Log.Write(LogLevel.Debug, "        Username : {0}", UserName);
                lSmtp.Send(lMailMessage);
                Pandora.Client.Universal.Log.Write(LogLevel.Info, "Email Sent");
            }
            catch (Exception Ex)
            {
                LastError = Ex.Message;
                Pandora.Client.Universal.Log.Write(LogLevel.Error, "Unable to send email : {0}", Ex.Message);
                return false;
            }
            return true;
        }

        public void SendAsync(string aSubject, string aBody)
        {
            Thread lNewThread = new Thread(() => SendThread(aSubject, aBody, new EmailSystem(this)));
            lNewThread.Start();
        }

        private void SendThread(string aSubject, string aBody, EmailSystem aEmailSystem)
        {
            try
            {
                if (!aEmailSystem.Send(aSubject, aBody))
                {
                    LastError = aEmailSystem.LastError;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Pandora.Client.Universal.Log.Write(LogLevel.Error, "Error sending email : {0}", ex.Message);
            }
        }

        public static bool IsValidEmail(string aEmail)
        {
            try
            {
                System.Net.Mail.MailAddress lAddr = new System.Net.Mail.MailAddress(aEmail);
                return lAddr.Address == aEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}