using System;
using System.IO;
using Pandora.Client.Universal;

namespace Pandora.Client.PandorasWallet
{
    public class CoreSettings
    {
        public string DataPath { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool RequestWalletPassword { get; set; }
        public string ServerName { get; set; }
        public int Port { get; set; }
        public bool EncryptedConnection { get; set; }
        public bool SaveLogingPasswords { get; set; }
        public string LoginHistory { get; set; } //email,username,passwordencrypted:email,username,password
        public string LogFileName { get; set; }
        public ushort LogMaxSize;
        public LogLevelFlags LogLevelFlags;
        public ushort LogLineLength;
        public bool LogRollOver;

        public static CoreSettings LoadSettings(string aFileName)
        {
            var lConfigFile = new ConfigFile();
            lConfigFile.LoadFromFile(aFileName);
            var lResult = new CoreSettings()
            {
                DataPath = lConfigFile.ReadStringValue("DataPath", ""),
                UserName = lConfigFile.ReadStringValue("UserName", ""),
                Email = lConfigFile.ReadStringValue("Email", ""),
                RequestWalletPassword = lConfigFile.ReadBoolValue("RequestWalletPassword", true),
                ServerName = lConfigFile.ReadStringValue("ServerName", Properties.Settings.Default.ServerName),
                Port = lConfigFile.ReadIntValue("Port", Properties.Settings.Default.Port),
                EncryptedConnection = lConfigFile.ReadBoolValue("EncryptedConnection", Properties.Settings.Default.EncyptedConnection),
                SaveLogingPasswords = lConfigFile.ReadBoolValue("SaveLogingPasswords", false),
                LoginHistory = lConfigFile.ReadStringValue("LoginHistory", ""),
                LogFileName = lConfigFile.ReadStringValue("LogFileName", ""),
                LogMaxSize = (ushort)lConfigFile.ReadIntValue("LogMaxSize", 100),
                LogLevelFlags = (LogLevelFlags)Enum.Parse(typeof(LogLevelFlags), lConfigFile.ReadStringValue("LogLevelFlags", "All")),
                LogLineLength = (ushort)lConfigFile.ReadIntValue("LogLineLength", 120),
                LogRollOver = lConfigFile.ReadBoolValue("LogRollOver", true)

            };
            return lResult;
        }

        public static void SaveSettings(CoreSettings aSettings, string aFileName)
        {
            var lConfigFile = new ConfigFile();
            lConfigFile.WriteStringValue("DataPath", aSettings.DataPath);
            lConfigFile.WriteStringValue("UserName", aSettings.UserName);
            lConfigFile.WriteStringValue("Email", aSettings.Email);
            lConfigFile.WriteStringValue("ServerName", aSettings.ServerName);
            lConfigFile.WriteIntValue("Port", aSettings.Port);
            lConfigFile.WriteBoolValue("RequestWalletPassword", aSettings.RequestWalletPassword);
            lConfigFile.WriteBoolValue("EncryptedConnection", aSettings.EncryptedConnection);
            lConfigFile.WriteBoolValue("SaveLogingPasswords", aSettings.SaveLogingPasswords);
            lConfigFile.WriteStringValue("LoginHistory", aSettings.LoginHistory);
            lConfigFile.WriteStringValue("LogFileName", aSettings.LogFileName);
            lConfigFile.WriteIntValue("LogMaxSize", aSettings.LogMaxSize);
            lConfigFile.WriteStringValue("LogLevelFlags", aSettings.LogLevelFlags.ToString());
            lConfigFile.WriteIntValue("LogLineLength", aSettings.LogLineLength);
            lConfigFile.WriteBoolValue("LogRollOver", aSettings.LogRollOver);
            lConfigFile.SaveToFile(aFileName);
        }

        public static string DefaultPath()
        {
            var lSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
            lSettingsPath = Path.Combine(lSettingsPath, "PandorasWallet.ini");
            return lSettingsPath;
        }
    }
}
