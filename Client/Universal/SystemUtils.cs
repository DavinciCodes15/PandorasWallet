using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

#if MONO
#else

using System.Drawing;

#endif

namespace Pandora.Client.Universal
{
    public static class SystemUtils
    {
        public static BigInteger ConvertDoubleLongToBigInteger(long aAmount, long aExAmount)
        {
            BigInteger lBigAmount = aAmount;
            if (aExAmount > 0)
            {
                string lLessSigHex = aAmount.ToString("X");
                if (lLessSigHex.Length > 15) throw new Exception($"Unable to interpretate double long. Value to big: {lLessSigHex}");
                if (lLessSigHex.Length < 15)
                {
                    int lLeadingZeros = 15 - lLessSigHex.Length;
                    for (var lCounter = 0; lCounter < lLeadingZeros; lCounter++)
                        lLessSigHex = string.Concat('0', lLessSigHex);
                }
                string lMoreSigHex = aExAmount.ToString("X");
                string lFinalHex = string.Concat("0", lMoreSigHex, lLessSigHex);
                lBigAmount = BigInteger.Parse(lFinalHex, System.Globalization.NumberStyles.HexNumber);
            }
            return lBigAmount;
        }

        public static Tuple<long, long?> ConvertHexToDoubleLong(string aHexNumber)
        {
            Tuple<long, long?> lResult;
            string lValue = (aHexNumber?.ToString())?.Replace("0x", string.Empty);
            if (lValue.Length > 15)
            {
                var lValueArray = new string[2];
                lValueArray[0] = lValue.Substring(lValue.Length - 15, 15);
                lValueArray[1] = lValue.Substring(0, lValue.Length - 15);
                lResult = new Tuple<long, long?>(ConvertHexStringToDecimal(lValueArray[0]), ConvertHexStringToDecimal(lValueArray[1]));
            }
            else
                lResult = new Tuple<long, long?>(ConvertHexStringToDecimal(lValue), null);
            return lResult;
        }

        public static long ConvertHexStringToDecimal(dynamic aHexNumber)
        {
            long lResult = 0;
            if (string.IsNullOrEmpty(aHexNumber) || !long.TryParse($"0{aHexNumber}", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out lResult))
                throw new Exception($"Unable to convert {aHexNumber ?? "NULL"} to decimal");
            return lResult;
        }

        public static Type[] GetChildrenFromBaseClass<T>(this Assembly aAssembly) where T : class
        {
            var lTypes = aAssembly.GetTypes()
                            .Where(lType => lType.IsClass && !lType.IsAbstract && lType.IsSubclassOf(typeof(T)))
                            .ToArray();
            return lTypes;
        }

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

        public static byte[] IconToBytes(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

#endif

        public static string GetAssemblyVersion()
        {
            Assembly lAssembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo lFileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(lAssembly.Location);
            return lFileVersion.FileVersion;
        }

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
            if (string.IsNullOrEmpty(aProcessName)) throw new ArgumentException("aProcessName can not be null or empty.");
            Process lResult = null;
            try
            {
                Process[] lList = Process.GetProcessesByName(aProcessName);
                if (lList == null || lList.Length == 0)
                    lList = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(aProcessName));
                if (lList == null || lList.Length == 0)
                {
                    lList = new Process[0];
                    aProcessName = aProcessName.ToLowerInvariant();
                    var lProcesses = Process.GetProcesses();
                    if (lProcesses != null)
                        foreach (var lProc in lProcesses)
                        {
                            //NOTE: in Mono some case this throws an error because this list is
                            //      directly connected to the existing process and not a copy of the
                            //      about the process.  So we need to take a copy of the name
                            //      and catch any excptions cause because the process ended.
                            string lProcessName;
                            try
                            {
                                lProcessName = lProc.ProcessName.ToLowerInvariant();
                            }
                            catch
                            {
                                // we ignore the error here
                                lProcessName = "-";
                            }
                            if (lProcessName.Contains(aProcessName))
                            {
                                lList = new Process[1] { lProc };
                                break;
                            }
                        }
                }
                if (lList.Length > 0)
                    lResult = lList[0];
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "SystemUtils.GetProcess failed for finding process '{0}'\nWith error: {1}\nStack: {2}", aProcessName, ex.Message, ex.StackTrace);
            }
            return lResult;
        }

        public static bool IsProcessRunning(string aProcessName)
        {
            return GetProcess(aProcessName) != null;
        }

        public static bool SendAlertEmail(string aFrom, string aToList, string aSubject, string aBody, string aServer, int aPort, string aUsername, string aPassword, bool aUseSSL, string aCertFile = null)
        {
            var lEmailModel = new EmailModel(aFrom, aToList, aSubject, aBody, aServer, aPort, aUsername, aPassword, aUseSSL, aCertFile);
            //sync method
            return BatchEmailService.ProcessEmail(lEmailModel);
        }

        public static bool SendAlertEmail(string aSubject, string aBody)
        {
            var lEmailModel = new EmailModel(aSubject, aBody);
            //async method
            return BatchEmailService.ProcessEmail(lEmailModel);
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
            if (e is ThreadAbortException) return; // not important
            string s = string.Format("{0}\n{1}.{2}\nServer Name: {3}\n{4}", aMessage, aClassName, aMethodName, Environment.MachineName, FormatException(e));
            Log.Write(LogLevel.Critical, s);
            //SendEmail(SubjectLine, s);
            SendAlertEmail(SubjectLine, s);
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
            Enabled = aEmailSystem.Enabled;
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
            Task.Factory.StartNew((() => SendThread(aSubject, aBody, new EmailSystem(this))))
                .ContinueWith((aTask)
                =>
                {
                    if (aTask.IsFaulted)
                        Log.Write(LogLevel.Error, $"Error sending email async with subject {aSubject}. Exception: {aTask.Exception?.Flatten()}");
                });
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

    public class BatchEmailService
    {
        private static Timer FTimerAlerts;
        private static int FNumbersMails = 0;
        private const int FMaxMailsAlertsByInterval = 3;
        private const int FTimeIntervalAlerts = 300000;//5 minutes;
        private static Dictionary<string, List<EmailModel>> FDictMailAlerts = new Dictionary<string, List<EmailModel>>();

        internal static Dictionary<string, List<EmailModel>> DictMailAlerts
        {
            get => FDictMailAlerts;
            set
            {
                if (value == null)
                {
                    FDictMailAlerts = new Dictionary<string, List<EmailModel>>();
                    FNumbersMails = 0;
                    FTimerAlerts.Dispose();
                    FTimerAlerts = null;
                }
                else
                {
                    FDictMailAlerts = value;
                }
            }
        }

        private static string ConsolidateMailsMessage(List<EmailModel> aMails)
        {
            var lMailBody = new StringBuilder();
            aMails.ForEach(lMail => lMailBody.Append($"mail orginal date: {lMail.DateOriginal.ToString("yyyy/MM/dd HH:mm:ss.fff")})\n-----------------------------------------\n{lMail.Body}\n\n"));
            return lMailBody.ToString();
        }

        private static List<EmailModel> GenerateListEmails()
        {
            var lListMails = new List<EmailModel>();
            foreach (var lMails in FDictMailAlerts.Values)
            {
                var lFirstMail = lMails[0];
                var lMailBody = ConsolidateMailsMessage(lMails);
                if (lFirstMail.Async)
                    lListMails.Add(new EmailModel($"Grouped message: {lFirstMail.Subject}", lMailBody));
                else
                    lListMails.Add(new EmailModel(lFirstMail.From, lFirstMail.ToList, $"Grouped message: {lFirstMail.Subject}", lMailBody, lFirstMail.Server, lFirstMail.Port, lFirstMail.UserName, lFirstMail.Password, lFirstMail.UseSSL, lFirstMail.CertFile));
            }
            return lListMails;
        }

        private static void SendBatchEmail(object aState)
        {
            Log.Write(LogLevel.Info, "On init SendBatchEmail");
            //Get all emails saved
            var lListMails = GenerateListEmails();

            //Recycle vars used in loop
            DictMailAlerts = null;

            //send mails
            foreach (var lMail in lListMails)
            {
                Log.Write(LogLevel.Info, $"Sending mail {lMail.Subject}");
                if (lMail.Async)
                    SystemUtils.SendEmail(lMail.Subject, lMail.Body);
                else
                    SystemUtils.SendEmail(lMail.From, lMail.ToList, lMail.Subject, lMail.Body, lMail.Server, lMail.Port, lMail.UserName, lMail.Password, lMail.UseSSL, lMail.CertFile);
            }
        }

        private static void CheckTimer()
        {
            if (FTimerAlerts == null)
            {
                Log.Write(LogLevel.Info, "Create new interval of timer");
                FTimerAlerts = new Timer(new TimerCallback(SendBatchEmail), null, FTimeIntervalAlerts, Timeout.Infinite);
            }
        }

        private static void AddEmailToDictionary(EmailModel aEmail)
        {
            if (DictMailAlerts.ContainsKey(aEmail.Subject))
                FDictMailAlerts[aEmail.Subject].Add(aEmail);
            else
                FDictMailAlerts.Add(aEmail.Subject, new List<EmailModel> { aEmail });
        }

        public static bool ProcessEmail(EmailModel aEmail)
        {
            Log.Write(LogLevel.Info, "On init ProcessEmail");
            CheckTimer();

            if (FNumbersMails++ < FMaxMailsAlertsByInterval)
            {
                Log.Write(LogLevel.Info, "The limit of emails sent in the interval has not been exceeded. Therefore, send mail directly without queue");
                if (aEmail.Async)
                {
                    if (SystemUtils.EmailSystem != null)
                        SystemUtils.EmailSystem.SendAsync(aEmail.Subject, aEmail.Body);
                    return true;
                }
                else
                {
                    return SystemUtils.SendEmail(aEmail.From, aEmail.ToList, aEmail.Subject, aEmail.Body, aEmail.Server, aEmail.Port, aEmail.UserName, aEmail.Password, aEmail.UseSSL, aEmail.CertFile);
                }
            }

            Log.Write(LogLevel.Info, "The limit of emails sent in the interval has been exceeded. Therefore, add mail to the interval queue");
            AddEmailToDictionary(aEmail);
            return true;
        }
    }

    public class EmailModel
    {
        internal string From { get; set; }
        internal string ToList { get; set; }
        internal string Subject { get; set; }
        internal string Body { get; set; }
        internal string Server { get; set; }
        internal int Port { get; set; }
        internal string UserName { get; set; }
        internal string Password { get; set; }
        internal bool UseSSL { get; set; }
        internal string CertFile { get; set; }

        internal bool Async { get; set; }
        internal DateTime DateOriginal { get; set; }

        /// <summary>
        /// Constructor used to specify all fields of a new email message. Internally the system mark the message as sync
        /// </summary>
        /// <param name="aFrom">address from</param>
        /// <param name="aToList">a list of address to </param>
        /// <param name="aSubject">subject of message</param>
        /// <param name="aBody">content of message</param>
        /// <param name="aServer">server used to send mail</param>
        /// <param name="aPort">port used</param>
        /// <param name="aUsername">username of credencials of service mail</param>
        /// <param name="aPassword">password of credencials of service mail</param>
        /// <param name="aUseSSL">boolean valu indicating if the service mail use SLL</param>
        /// <param name="aCertFile">path of file certificate</param>
        internal EmailModel(string aFrom, string aToList, string aSubject, string aBody, string aServer, int aPort, string aUsername, string aPassword, bool aUseSSL, string aCertFile = null)
        {
            From = aFrom;
            ToList = aToList;
            Subject = aSubject;
            Body = aBody;
            Server = aServer;
            Port = aPort;
            UserName = aUsername;
            Password = aPassword;
            UseSSL = aUseSSL;
            CertFile = aCertFile;
            DateOriginal = DateTime.Now;
            Async = false;

            Log.Write(LogLevel.Info, "Created a new instance of EmailModel sync");
        }

        /// <summary>
        /// Constructor used to specify only subject and body of a new email message. Internally the system mark the message as async
        /// </summary>
        /// <param name="aSubject">subject of message</param>
        /// <param name="aBody">content of message</param>
        internal EmailModel(string aSubject, string aBody)
        {
            Subject = aSubject;
            Body = aBody;
            DateOriginal = DateTime.Now;
            Async = true;

            Log.Write(LogLevel.Info, "Created a new instance of EmailModel async");
        }
    }
}