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
using Pandora.Client.ClientLib;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    /// <summary>
    /// Class used for managing Pandora's Wallet Cache
    /// </summary>
    public partial class PandorasCache : IDisposable
    {
        private PandorasServer FPandoraServer;

        private DBManager FDBManager;
        private ObjectCache FCache;

        private CachedObject<CurrencyAccount> FCurrencyAccounts;
        private CachedObject<CurrencyItem> FCurrencyItems;
        private CachedObject<CurrencyStatusItem> FCurrencyStatuses;
        private CachedObject<TransactionRecord> FTxs;

        public event Action<string> OnCacheExpired;

        public string DBFileName => FDBManager.FileName;

        public PandorasCache(PandorasServer aobject)
        {
            FPandoraServer = aobject;
            FPandoraServer.OnCurrencyItemMustUpdate += FPandoraServer_OnCurrencyItemMustUpdate;
            FCache = MemoryCache.Default;
            AppDataNameRecovery();
            FDBManager = new DBManager(FPandoraServer.DataPath, FPandoraServer.InstanceId);
            FCurrencyAccounts = new CachedObject<CurrencyAccount>(this, FCache, FDBManager);
            FCurrencyItems = new CachedObject<CurrencyItem>(this, FCache, FDBManager);
            FCurrencyStatuses = new CachedObject<CurrencyStatusItem>(this, FCache, FDBManager);
            FTxs = new CachedObject<TransactionRecord>(this, FCache, FDBManager);
            SetLifeTimes();
            FPandoraServer.SetCheckpoints(FDBManager.GetSavedCheckpoints());
        }

        private void SetLifeTimes()
        {
            FDBManager.AddOrReplaceLifeTimes("MonitoredAccounts", 3600);
            FDBManager.AddOrReplaceLifeTimes("CurrencyStatus", 60);
            FDBManager.AddOrReplaceLifeTimes("CurrencyItem", 300);
            FDBManager.AddOrReplaceLifeTimes("Tx", 10);
        }

        // TODO: Untested code occurs when the path
        // for storing cache data is changed.
        public void RefreshCacheFolder()
        {
            if (FDBManager == null)
            {
                return;
            }

            DBManager lOldDBManager = FDBManager;

            FDBManager = new DBManager(FPandoraServer.DataPath, FPandoraServer.InstanceId);

            SetLifeTimes();

            FCurrencyAccounts.SetDBManager(FDBManager);
            FCurrencyItems.SetDBManager(FDBManager);
            FCurrencyStatuses.SetDBManager(FDBManager);
            FTxs.SetDBManager(FDBManager);

            lOldDBManager.Dispose();
        }

        private bool FPandoraServer_OnCurrencyItemMustUpdate(long aId)
        {
            try
            {
                CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems, aId);
                FCurrencyItems.EmptyCache();
                return true;
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, "OnCurrencyItemMustUpdate error {0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace);
                return false;
            }
        }

        public CurrencyAccount[] GetCurrencyAccount(long aCurrencyAccountId)
        {
            return FCurrencyAccounts.Get(aCurrencyAccountId);
        }

        public CurrencyAccount[] GetMonitoredAccounts()
        {
            if (FDBManager.CheckLastRetrieval("MonitoredAccounts"))
                MonitoredAccountsDataUpdate(FPandoraServer, FCurrencyAccounts);
            return FCurrencyAccounts.Get();
        }

        public CurrencyStatusItem GetCurrencyStatusItem(long aID)
        {
            // get the Currency Status Item form the local Database
            CurrencyStatusItem[] lResult = FCurrencyStatuses.Get(aID);
            //it comes as an array so lets get the right value if one exists
            //if not return null
            if (lResult.Length == 0)
                return null;
            else
                return lResult[0];
        }

        public CurrencyStatusItem[] GetCurrencyStatuses(bool lForce = false)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyStatus") || lForce)
                CurrencyStatusDataUpdate(FPandoraServer, FCurrencyStatuses);

            return FCurrencyStatuses.Get();
        }

        public CurrencyItem GetCurrencyItem(long aCurrencyId)
        {
            // get the Currency Item form the local Database
            CurrencyItem[] lResult = FCurrencyItems.Get(aCurrencyId);
            //it comes as an array so lets get the right value if one exists
            //if not return null
            if (lResult.Length == 0)
            {
                var lJson = FPandoraServer.FServerAccess.GetCurrency(aCurrencyId);
                var lCurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(lJson, new PandoraJsonConverter());
                if (lCurrencyItem.Id == 0)
                {
                    Log.Write(LogLevel.Info, lJson);
                    throw new Exception(string.Format("Currency Id {0} does not exist.", aCurrencyId));
                }
                FDBManager.Write(lCurrencyItem);
                return lCurrencyItem;
            }
            else
                return lResult[0];
        }

        public CurrencyItem[] GetCurrencies(long? aID = null)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyItem"))
                CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems);
            if (aID.HasValue)
                return FCurrencyItems.Get(aID.Value);
            else
                return FCurrencyItems.Get();
        }

        public TransactionRecord[] GetTxs(long aID)
        {
            return FTxs.Get(aID);
        }

        public void Clear()
        {
            FCurrencyAccounts.EmptyCache();
            FCurrencyItems.EmptyCache();
            FCurrencyStatuses.EmptyCache();
            FTxs.EmptyCache();
        }

        public void ForceDataFetch()
        {
            FDBManager.ClearAll();
            Clear();
        }

        public void CheckfornewCurrencies()
        {
            CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems);
        }

        private void CurrencyItemDataUpdate(PandorasServer aPandorasServer, CachedObject<CurrencyItem> aCachedObject, long? aId = null)
        {
            if (FDBManager.Write(aPandorasServer.FetchCurrencies(aId)))
                aCachedObject.NewDataAlert = true;
        }

        private void CurrencyStatusDataUpdate(PandorasServer aPandorasServer, CachedObject<CurrencyStatusItem> aCachedObject, List<long> aListOfCurrencies = null)
        {
            List<CurrencyStatusItem> lStatuses = new List<CurrencyStatusItem>();

            foreach (long it in aPandorasServer.CurrencyIds)
            {
                List<CurrencyStatusItem> lCurrencyStatus = aPandorasServer.FetchCurrencyStatus(it);

                if (lCurrencyStatus.Any())
                {
                    aListOfCurrencies?.Add(it);
                    lStatuses.AddRange(lCurrencyStatus);
                }
            }

            if (FDBManager.Write(lStatuses))
            {
                aCachedObject.NewDataAlert = true;
            }
        }

        public void NewMonitoredAccountAdded(long aCurrencyId)
        {
            FTxs.EmptyCache();

            MonitoredAccountsDataUpdate(FPandoraServer, FCurrencyAccounts, true, aCurrencyId);
        }

        private void MonitoredAccountsDataUpdate(PandorasServer aPandorasServer, CachedObject<CurrencyAccount> aCachedObject, bool aFlagforCurrency = false, long aCurrencyId = 0)
        {
            bool lSomethingWrited = false;

            if (aFlagforCurrency)
            {
                if (FDBManager.Write(aPandorasServer.FetchMonitoredAccounts(aCurrencyId)))
                {
                    lSomethingWrited = true;
                }
            }
            else
            {
                List<CurrencyAccount> lListAccounts = new List<CurrencyAccount>();

                foreach (long it in aPandorasServer.CurrencyIds)
                {
                    lListAccounts.AddRange(aPandorasServer.FetchMonitoredAccounts(it));
                }
                if (FDBManager.Write(lListAccounts))
                {
                    lSomethingWrited = true;
                }
            }

            aCachedObject.NewDataAlert = lSomethingWrited;
        }

        private List<long> TransactionsDataUpdate(PandorasServer aPandorasServer, CachedObject<TransactionRecord> aCachedObject)
        {
            List<long> lCurrencyIdList = new List<long>();

            foreach (long lCurrencyId in aPandorasServer.CurrencyIds)
                if (FDBManager.WriteTransactionRecords(aPandorasServer.FetchTransactions(lCurrencyId), lCurrencyId))
                    lCurrencyIdList.Add(lCurrencyId);

            aCachedObject.NewDataAlert = lCurrencyIdList.Count > 0;

            return lCurrencyIdList;
        }

        public bool CheckforNewTransactions(out List<long> aListOfCurrencies)
        {
            if (FDBManager.CheckLastRetrieval("Tx"))
            {
                aListOfCurrencies = TransactionsDataUpdate(FPandoraServer, FTxs);

                if (aListOfCurrencies.Any())
                {
                    FTxs.EmptyCache();
                    return true;
                }
            }

            aListOfCurrencies = null;

            return false;
        }

        public bool CheckforNewCurrencyStatus(out List<long> aListOfCurrencies)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyStatus"))
            {
                aListOfCurrencies = new List<long>();
                CurrencyStatusDataUpdate(FPandoraServer, FCurrencyStatuses, aListOfCurrencies);

                if (aListOfCurrencies.Any())
                {
                    FCurrencyStatuses.EmptyCache();
                    return true;
                }
            }

            aListOfCurrencies = null;

            return false;
        }

        private void RaiseCacheExpiredEvent(string aKey)
        {
            OnCacheExpired(aKey);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PandorasCache() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        /// <summary>
        /// Serves as a bridge between system object cache and Sqlite HD database
        /// </summary>
        /// <typeparam name="T">Pandora item type</typeparam>
        private class CachedObject<T>
        {
            public bool NewDataAlert = false;

            private int FLifeTime;
            private List<object> FData = new List<object>();
            private ObjectCache FCacheRef;
            private DBManager FDBManager;
            private PandorasCache FPandoraCache;

            private T[] GetSetDBData(string aIdentifier, long? aId = null)
            {
                aIdentifier = aIdentifier + (aId.HasValue ? Convert.ToString(aId) : typeof(T).Name);
                T[] lResult;
                if (aId.HasValue && FCacheRef.Contains(aIdentifier))
                    lResult = (T[])FCacheRef.Get(aIdentifier);
                else
                {
                    switch (typeof(T).Name)
                    {
                        case "CurrencyAccount":
                            FDBManager.Read(out List<CurrencyAccount> lReturned1, aId);
                            FCacheRef.Set(aIdentifier, lReturned1.ToArray(), aId.HasValue ? SetSlidingPolicy() : SetAbsolutePolicy());
                            break;

                        case "CurrencyItem":
                            FDBManager.Read(out List<CurrencyItem> lReturned2, aId);
                            FCacheRef.Set(aIdentifier, lReturned2.ToArray(), aId.HasValue ? SetSlidingPolicy() : SetAbsolutePolicy());
                            break;

                        case "CurrencyStatusItem":
                            FDBManager.Read(out List<CurrencyStatusItem> lReturned3, aId);
                            FCacheRef.Set(aIdentifier, lReturned3.ToArray(), aId.HasValue ? SetSlidingPolicy() : SetAbsolutePolicy());
                            break;

                        case "TransactionRecord":
                            FDBManager.Read(out List<TransactionRecord> lReturned4, aId.Value);
                            FCacheRef.Set(aIdentifier, lReturned4.ToArray(), aId.HasValue ? SetSlidingPolicy() : SetAbsolutePolicy());
                            break;
                    }

                    lResult = (T[])FCacheRef.Get(aIdentifier);
                }
                if (lResult == null)
                    lResult = new T[0];

                return lResult;
            }

            private Dictionary<Type, string> FElementIdentifiers = new Dictionary<Type, string>
            {
                { typeof(CurrencyAccount), "1D"},
                { typeof(CurrencyItem), "2D"},
                { typeof(CurrencyStatusItem), "3D" },
                { typeof(TransactionRecord), "4D"},
            };

            /// <summary>
            /// Initializes bridge object
            /// </summary>
            /// <param name="aPandorasCache">Parent class</param>
            /// <param name="aObjectCache">Initialized .Net ObjectCache</param>
            /// <param name="aDBManager">Initialized DBManager object</param>
            /// <param name="aLifeTimeinMinutes">Lifespan of the cache object</param>
            public CachedObject(PandorasCache aPandorasCache, ObjectCache aObjectCache, DBManager aDBManager, int aLifeTimeinMinutes = 5)
            {
                FLifeTime = aLifeTimeinMinutes;
                FCacheRef = aObjectCache;
                FDBManager = aDBManager;
                FPandoraCache = aPandorasCache;
            }

            public void SetDBManager(DBManager aDBManager)
            {
                FDBManager = aDBManager;
            }

            private CacheItemPolicy SetSlidingPolicy()
            {
                CacheItemPolicy lSlidingCachepolicy = new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromMinutes(FLifeTime)
                };
                return lSlidingCachepolicy;
            }

            private CacheItemPolicy SetAbsolutePolicy()
            {
                CacheItemPolicy lAbsoluteCachepolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(FLifeTime * 2),
                    RemovedCallback = (x) => { FPandoraCache.RaiseCacheExpiredEvent(x.CacheItem.Key); }
                };
                return lAbsoluteCachepolicy;
            }

            public T[] Get()
            {
                if (typeof(T) == typeof(TransactionRecord))
                {
                    throw new Exception("Cannot get all transactions without an address and Currency ID");
                }

                if (FCacheRef.Contains("ALL" + typeof(T).ToString()) && !NewDataAlert)
                {
                    return (T[])FCacheRef.Get("ALL" + typeof(T).Name);
                }
                else
                {
                    NewDataAlert = false;
                    return GetSetDBData("ALL");
                }
            }

            public T[] Get(long aId)
            {
                return GetSetDBData(FElementIdentifiers[typeof(T)], aId);
            }

            public void EmptyCache()
            {
                List<string> LIdsToErase = new List<string>();
                foreach (KeyValuePair<string, object> it in FCacheRef)
                    LIdsToErase.Add(it.Key);
                foreach (string it in LIdsToErase)
                    FCacheRef.Remove(it);

                return;
            }
        }

        /// <summary>
        /// Scans the app data folder looking for bad or old file names, and then tries to perform a rename. This is used as a recovery system.
        /// </summary>
        private void AppDataNameRecovery()
        {
            try
            {
                if (TryRecoverAppDataNames(out string lWrongDataNames))
                    UpdateAppDataNames(lWrongDataNames);
            }
            catch (System.Security.SecurityException ex)
            {
                //This should never happend
                Universal.Log.Write(Universal.LogLevel.Error, ex.ToString());
                throw new Exception("Not enough permissions, please try restart the application with administrators rights");
            }
        }

        /// <summary>
        /// Scans app data folder and looks for missplaced names in data files.
        /// </summary>
        /// <param name="aWrongDataName">Output of bad filename found. Null when nothing found.</param>
        /// <returns></returns>
        /// <exception cref="System.Security.SecurityException">Thrown if app have inssuficient permissions to perform operations in app data folder</exception>
        internal bool TryRecoverAppDataNames(out string aWrongDataName)
        {
            bool lResult = false;
            aWrongDataName = null;
            List<System.IO.FileInfo> lWronDataNames = new List<System.IO.FileInfo>();
            System.IO.DirectoryInfo lDataDirectory = new System.IO.DirectoryInfo(FPandoraServer.DataPath);
            System.IO.FileInfo[] lDataFiles = lDataDirectory.GetFiles("*.sqlite");

            //First verify that there is no file already created with that isntanceid
            if (!(lDataFiles.Where(lFile => lFile.Name.Contains(FPandoraServer.InstanceId)).Any()))
            {
                PandoraJsonConverter lConverter = new PandoraJsonConverter();
                CurrencyAccount[] lUserMonitoredAccounts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CurrencyAccount>>(FPandoraServer.FServerAccess.GetMonitoredAcccounts(1, 0), lConverter).ToArray();
                CurrencyAccount lBaseAccount = lUserMonitoredAccounts.FirstOrDefault();
                if (lUserMonitoredAccounts.Any() && lBaseAccount != null)
                {
                    foreach (System.IO.FileInfo litem in lDataFiles)
                    {
                        try
                        {
                            // If the db file is usable the following method should not throw exception, otherwise just skip and test another
                            CurrencyAccount[] lAddresses = DBManager.GetUserAddressesFromFile(FPandoraServer.DataPath, litem.Name);
                            //Verify address to make sure that file belongs to user
                            if (lAddresses.Any() && lAddresses.Where(laccount => laccount.CurrencyId == 1 && laccount.Address == lBaseAccount.Address).Any())
                                lWronDataNames.Add(litem);
                        }
                        catch (ClientExceptions.CacheDBException ex)
                        {
                            Universal.Log.Write(Universal.LogLevel.Error, ex.ToString());
                        }
                    }
                    if (lWronDataNames.Any())
                    {
                        //If several files are found, take the most recent one as wright
                        aWrongDataName = lWronDataNames.OrderByDescending(lFile => lFile.LastWriteTimeUtc).First().Name.Replace(".sqlite", string.Empty);
                        lResult = true;
                    }
                }
            }
            return lResult;
        }

        /// <summary>
        /// By providing a bad file name, saves old files and try to rename them with a valid intanceid
        /// </summary>
        /// <param name="lWrongDataName">Incorrect file name to be renamed</param>
        /// <exception cref="System.Security.SecurityException">>Thrown if app have inssuficient permissions to perform operations in app data folder</exception>
        internal void UpdateAppDataNames(string lWrongDataName)
        {
            System.IO.DirectoryInfo lDataDir = new System.IO.DirectoryInfo(FPandoraServer.DataPath);
            System.IO.FileInfo[] lFiles = lDataDir.GetFiles();
            try
            {
                System.IO.FileInfo lSQliteFile = lFiles.Where(x => x.Name.Contains(lWrongDataName) && x.Extension == ".sqlite").FirstOrDefault();
                if (lSQliteFile != null)
                {
                    //Everything except .sqlite
                    System.IO.FileInfo[] lFilteredFiles = lFiles.Where(lf => lf.Name.Contains(lWrongDataName) && lf != lSQliteFile).ToArray();
                    foreach (System.IO.FileInfo lfile in lFilteredFiles)
                    {
                        System.IO.File.Copy(lfile.FullName, lfile.FullName.Replace(lWrongDataName, FPandoraServer.InstanceId));
                        System.IO.File.Delete(lfile.FullName);
                    }
                    System.IO.File.Copy(lSQliteFile.FullName, lSQliteFile.FullName.Replace(lWrongDataName, FPandoraServer.InstanceId));//And Finally do sqlite file
                    System.IO.File.Delete(lSQliteFile.FullName);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new System.Security.SecurityException(ex.Message);
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, ex.ToString());
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, ex.ToString());
            }
        }
    }
}