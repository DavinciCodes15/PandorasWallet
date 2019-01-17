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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Pandora.Client.PandorasWallet.Utils
{
    public class SettingsManager
    {
        private string SettingsPath { get; }
        private ReaderWriterLockSlim rwLock { get; }

        private JObject settings;

        public SettingsManager(string settingsPath)
        {
            rwLock = new ReaderWriterLockSlim();
            SettingsPath = settingsPath;
            LoadSettings();
        }

        public void CreateUserSettings()
        {
            if (!GetSetting("DefaultCoin", out uint value1))
            {
                AddSetting("DefaultCoin", (uint)0);
            }

            if (!GetSetting("SelectedCoins", out List<uint> value2))
            {
                AddSetting("SelectedCoins", new List<uint>());
            }

            if (!GetSetting("AddressNumber", out uint value3))
            {
                AddSetting("AddressNumber", (uint)1);
            }

            SaveSettings();
        }

        /// <summary>
        /// Creates a JSON file for settings at <see cref="SettingsPath"/>
        /// </summary>
        private void CreateSettingsFile()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                    File.Create(SettingsPath).Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException($"The provided {nameof(SettingsPath)} was invalid", ex);
            }
        }

        /// <summary>
        /// Loads the contents of the settings file into <see cref="settings"/>
        /// </summary>
        public void LoadSettings()
        {
            if (settings == null)
            {
                rwLock.EnterWriteLock();

                CreateSettingsFile();
                string fileContents = File.ReadAllText(SettingsPath);
                settings = string.IsNullOrWhiteSpace(fileContents) ? new JObject() : JObject.Parse(fileContents);

                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Add a new setting or update existing setting with the same name
        /// </summary>
        /// <param name="setting">Setting name</param>
        /// <param name="value">Setting value</param>
        public void AddSetting(string setting, object value)
        {
            LoadSettings();
            rwLock.EnterWriteLock();

            if (settings[setting] == null && value != null)
            {
                settings.Add(setting, JToken.FromObject(value));
            }
            else if (value == null && settings[setting] != null)
            {
                settings[setting] = null;
            }
            else if (value != null)
            {
                settings[setting] = JToken.FromObject(value);
            }

            rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Adds a batch of new settings, replacing already existing values with the same name
        /// </summary>
        /// <param name="settings"></param>
        public void AddSettings(Dictionary<string, object> settings)
        {
            foreach (KeyValuePair<string, object> setting in settings)
            {
                AddSetting(setting.Key, setting.Value);
            }
        }

        /// <summary>
        /// Gets the value of the setting with the provided name as type <c>T</c>
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="setting">Setting name</param>
        /// <param name="result">Result value</param>
        /// <returns>True when setting was successfully found, false when setting is not found</returns>
        public bool GetSetting<T>(string setting, out T result)
        {
            result = default(T);

            try
            {
                LoadSettings();

                rwLock.EnterReadLock();
                {
                    if (settings[setting] == null) { rwLock.ExitReadLock(); return false; }

                    result = settings[setting].ToObject<T>();
                }
                rwLock.ExitReadLock();
                return true;
            }
            catch (Exception)
            {
                try
                {
                    rwLock.ExitReadLock();
                    rwLock.EnterReadLock();

                    result = settings[setting].Value<T>();

                    rwLock.ExitReadLock();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a dictionary of all settings
        /// </summary>
        /// <returns></returns>
        public bool GetSettings(Dictionary<string, object> values)
        {
            return GetSettings(out values);
        }

        /// <summary>
        /// Returns a dictionary of all settings, where all settings are of the same type
        /// </summary>
        /// <typeparam name="TValue">The type of all of the settings</typeparam>
        /// <returns></returns>
        public bool GetSettings<TValue>(out Dictionary<string, TValue> values)
        {
            try
            {
                LoadSettings();
                rwLock.EnterReadLock();

                values = settings.ToObject<Dictionary<string, TValue>>();
                rwLock.ExitReadLock();
                return true;
            }
            catch
            {
                values = null;
                rwLock.ExitReadLock();
                return false;
            }
        }

        /// <summary>
        /// Saves all queued settings to <see cref="SettingsPath"/>
        /// </summary>
        public void SaveSettings()
        {
            rwLock.EnterWriteLock();

            File.WriteAllText(SettingsPath, settings.ToString(Formatting.Indented));

            rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Completely delete the settings file at <see cref="SettingsPath"/>. This action is irreversable.
        /// </summary>
        public void DeleteSettings()
        {
            rwLock.EnterWriteLock();

            File.Delete(SettingsPath);
            settings = null;

            rwLock.ExitWriteLock();
        }
    }
}