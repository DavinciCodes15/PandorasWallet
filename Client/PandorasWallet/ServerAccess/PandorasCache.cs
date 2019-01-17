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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public partial class PandorasCache : IDisposable
    {
        private PandorasServer FPandoraServer;

        private DBManager FDBManager;
        private ObjectCache FCache;

        private CachedObject FCurrencyAccounts, FCurrencyItems, FCurrencyStatuses, FTxs;

        public event Action<string> OnCacheExpired;

        public PandorasCache(PandorasServer aobject)
        {
            FPandoraServer = aobject;
            FPandoraServer.OnCurrencyItemMustUpdate += FPandoraServer_OnCurrencyItemMustUpdate;

            FCache = MemoryCache.Default;

            FDBManager = new DBManager(FPandoraServer.DataPath, FPandoraServer.InstanceId);

            FCurrencyAccounts = new CachedObject(this, FCache, FDBManager, typeof(CurrencyAccount));
            FCurrencyItems = new CachedObject(this, FCache, FDBManager, typeof(CurrencyItem));
            FCurrencyStatuses = new CachedObject(this, FCache, FDBManager, typeof(CurrencyStatusItem));
            FTxs = new CachedObject(this, FCache, FDBManager, typeof(TransactionRecord));

            SetLifeTimes();

            FPandoraServer.SetCheckpoints(FDBManager.GetSavedCheckpoints());
        }

        public void SetLifeTimes()
        {
            FDBManager.AddOrReplaceLifeTimes("MonitoredAccounts", 3600);
            FDBManager.AddOrReplaceLifeTimes("CurrencyStatus", 60);
            FDBManager.AddOrReplaceLifeTimes("CurrencyItem", 300);
            FDBManager.AddOrReplaceLifeTimes("Tx", 10);
        }

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

        private bool FPandoraServer_OnCurrencyItemMustUpdate(ulong obj)
        {
            try
            {
                CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems);
                FCurrencyItems.EmptyCache();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public CurrencyAccount[] GetMonitoredAccountByID(uint aID)
        {
            if (FDBManager.CheckLastRetrieval("MonitoredAccounts"))
            {
                MonitoredAccountsDataUpdate(FPandoraServer, FCurrencyAccounts);
            }

            return (CurrencyAccount[])FCurrencyAccounts.Get(aID);
        }

        public List<CurrencyAccount> GetAllMonitoredAccounts()
        {
            if (FDBManager.CheckLastRetrieval("MonitoredAccounts"))
            {
                MonitoredAccountsDataUpdate(FPandoraServer, FCurrencyAccounts);
            }

            return (List<CurrencyAccount>)FCurrencyAccounts.Get();
        }

        public CurrencyStatusItem GetCurrencyStatusByID(uint aID)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyStatus"))
            {
                CurrencyStatusDataUpdate(FPandoraServer, FCurrencyStatuses);
            }

            return (CurrencyStatusItem)FCurrencyStatuses.Get(aID);
        }

        public CurrencyStatusItem[] GetAllCurrencyStatuses()
        {
            if (FDBManager.CheckLastRetrieval("CurrencyStatus"))
            {
                CurrencyStatusDataUpdate(FPandoraServer, FCurrencyStatuses);
            }

            return (CurrencyStatusItem[])FCurrencyStatuses.Get();
        }

        public CurrencyItem GetCurrencyByID(uint aID)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyItem"))
            {
                CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems);
            }

            return (CurrencyItem)FCurrencyItems.Get(aID);
        }

        public CurrencyItem[] GetAllCurrencies()
        {
            if (FDBManager.CheckLastRetrieval("CurrencyItem"))
            {
                CurrencyItemDataUpdate(FPandoraServer, FCurrencyItems);
            }

            return FCurrencyItems.Get();
        }

        public TransactionRecord[] GetTxs(uint aID)
        {
            return (TransactionRecord[])FTxs.Get(aID);
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

        private void CurrencyItemDataUpdate(PandorasServer aPandorasServer, CachedObject aCachedObject)
        {
            if (FDBManager.Write(aPandorasServer.FetchCurrencies()))
            {
                aCachedObject.NewDataAlert = true;
                return;
            }
        }

        private void CurrencyStatusDataUpdate(PandorasServer aPandorasServer, CachedObject aCachedObject, List<uint> aListOfCurrencies = null)
        {
            bool lSomethingWrited = false;

            foreach (uint it in aPandorasServer.CurrencyNumber)
            {
                if (FDBManager.Write(aPandorasServer.FetchCurrencyStatus(it)))
                {
                    lSomethingWrited = true;
                    aListOfCurrencies?.Add(it);
                }
            }

            aCachedObject.NewDataAlert = lSomethingWrited;
        }

        public void NewMonitoredAccountAdded(uint aCurrencyId)
        {
            FTxs.EmptyCache();

            MonitoredAccountsDataUpdate(FPandoraServer, FCurrencyAccounts, true, aCurrencyId);
        }

        private void MonitoredAccountsDataUpdate(PandorasServer aPandorasServer, CachedObject aCachedObject, bool aFlagforCurrency = false, uint aCurrencyId = 0)
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
                foreach (uint it in aPandorasServer.CurrencyNumber)
                {
                    if (FDBManager.Write(aPandorasServer.FetchMonitoredAccounts(it)))
                    {
                        lSomethingWrited = true;
                    }
                }
            }

            aCachedObject.NewDataAlert = lSomethingWrited;
        }

        private List<uint> TransactionsDataUpdate(PandorasServer aPandorasServer, CachedObject aCachedObject)
        {
            bool lSomethingWrited = false;

            List<uint> lCurrencyNumbers = new List<uint>();

            foreach (uint it in aPandorasServer.CurrencyNumber)
            {
                if (FDBManager.Write(aPandorasServer.FetchTransactions(it), it))
                {
                    lSomethingWrited = true;
                    lCurrencyNumbers.Add(it);
                }
            }

            aCachedObject.NewDataAlert = lSomethingWrited;

            return lCurrencyNumbers;
        }

        public bool CheckforNewTransactions(PandorasServer aPandorasServer, out List<uint> aListOfCurrencies)
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

        public bool CheckforNewCurrencyStatus(PandorasServer aPandorasServer, out List<uint> aListOfCurrencies)
        {
            if (FDBManager.CheckLastRetrieval("CurrencyStatus"))
            {
                aListOfCurrencies = new List<uint>();
                CurrencyStatusDataUpdate(FPandoraServer, FTxs, aListOfCurrencies);

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

        private class CachedObject

        {
            private int FLifeTime;
            private List<object> FData = new List<object>();
            private Type FType;
            private ObjectCache FCacheRef;
            private DBManager FDBManager;
            private PandorasCache FPandoraCache;

            public bool NewDataAlert = false;

            private Dictionary<Type, Func<uint, CachedObject, dynamic>> FOneElementDict = new Dictionary<Type, Func<uint, CachedObject, dynamic>>
            {
                { typeof(CurrencyAccount),(uint aCurrencyId, CachedObject ACachedObject) => {if(ACachedObject.FCacheRef.Contains("1D" + Convert.ToString(aCurrencyId))) { return (CurrencyAccount[])ACachedObject.FCacheRef.Get("1D" + Convert.ToString(aCurrencyId)); } else{ACachedObject.FDBManager.Read(out List<CurrencyAccount>returned, true, aCurrencyId);  ACachedObject.FCacheRef.Set("1D" + Convert.ToString(aCurrencyId), returned.ToArray(), ACachedObject.SetSlidingPolicy()); return (CurrencyAccount[])ACachedObject.FCacheRef.Get("1D" + Convert.ToString(aCurrencyId)); } } }, //TODO:LUIS- MAKE THIS SUPPORT POSSIBLE ERRORS AND BUILD LOGICS
                { typeof(CurrencyItem), (uint aId, CachedObject ACachedObject) => {if(ACachedObject.FCacheRef.Contains("2D" + Convert.ToString(aId))) { return (CurrencyItem) ACachedObject.FCacheRef.Get("2D" + Convert.ToString(aId)); } else {ACachedObject.FDBManager.Read(out List<CurrencyItem>returned, true, aId); ACachedObject.FCacheRef.Set("2D" + Convert.ToString(aId), returned.FirstOrDefault(), ACachedObject.SetSlidingPolicy()); return (CurrencyItem) ACachedObject.FCacheRef.Get("2D" + Convert.ToString(aId)); } } },
                { typeof(CurrencyStatusItem), (uint aId, CachedObject ACachedObject) => {if(ACachedObject.FCacheRef.Contains("3D" + Convert.ToString(aId))) { return (CurrencyStatusItem)ACachedObject.FCacheRef.Get("3D" + Convert.ToString(aId)); } else {ACachedObject.FDBManager.Read(out List<CurrencyStatusItem>returned, true, aId); ACachedObject.FCacheRef.Set("3D" + Convert.ToString(aId), returned.FirstOrDefault(), ACachedObject.SetSlidingPolicy());  return (CurrencyStatusItem)ACachedObject.FCacheRef.Get("3D" + Convert.ToString(aId)); } } },
                { typeof(TransactionRecord), (uint aId, CachedObject ACachedObject) => {if(ACachedObject.FCacheRef.Contains("4D" + aId)) { return (TransactionRecord[])ACachedObject.FCacheRef.Get("4D" + aId); } else { ACachedObject.FDBManager.Read(out List<TransactionRecord>returned, aId); ACachedObject.FCacheRef.Set("4D" + aId, returned.ToArray(), ACachedObject.SetSlidingPolicy()); return (TransactionRecord[])ACachedObject.FCacheRef.Get("4D" + aId); } } },
            };

            private Dictionary<Type, Func<CachedObject, dynamic>> FAllElementsDict = new Dictionary<Type, Func<CachedObject, dynamic>>
                {
                    { typeof(CurrencyAccount),(CachedObject ACachedObject)=>{ACachedObject.FDBManager.Read(out List<CurrencyAccount>returned);  ACachedObject.FCacheRef.Set("ALL" + ACachedObject.ToString(), returned, ACachedObject.SetAbsolutePolicy()); return ((List<CurrencyAccount>) ACachedObject.FCacheRef.Get("ALL" + ACachedObject.ToString())); } },
                    { typeof(CurrencyItem),(CachedObject ACachedObject)=>{ACachedObject.FDBManager.Read(out List<CurrencyItem>returned);  ACachedObject.FCacheRef.Set("ALL" + ACachedObject.ToString(), returned.ToArray(), ACachedObject.SetAbsolutePolicy()); return ((CurrencyItem[]) ACachedObject.FCacheRef.Get("ALL" + ACachedObject.ToString())); } },
                    { typeof(CurrencyStatusItem),(CachedObject ACachedObject)=>{ ACachedObject.FDBManager.Read(out List<CurrencyStatusItem>returned); ACachedObject.FCacheRef.Set("ALL" + ACachedObject.ToString(), returned.ToArray(), ACachedObject.SetAbsolutePolicy()); return ((CurrencyStatusItem[]) ACachedObject.FCacheRef.Get("ALL" + ACachedObject.ToString())); } },
                 };

            public CachedObject(PandorasCache aPandorasCache, ObjectCache aObjectCache, DBManager aDBManager, Type aType, int aLifeTimeinMinutes = 5)
            {
                FType = aType;
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

            public dynamic Get()
            {
                if (FType == typeof(TransactionRecord))
                {
                    throw new Exception("Cannot get all transactions without an address and Currency ID");
                }

                if (FCacheRef.Contains("ALL" + FType.ToString()) && !NewDataAlert)
                {
                    return Convert.ChangeType(FCacheRef.Get("ALL" + FType.Name), FType == typeof(CurrencyAccount) ? typeof(List<>).MakeGenericType(FType) : typeof(Array).MakeGenericType(FType));
                }
                else
                {
                    NewDataAlert = false;
                    return FAllElementsDict[FType](this);
                }
            }

            public dynamic Get(uint aCurrencyId)
            {
                return FOneElementDict[FType](aCurrencyId, this);
            }

            public void EmptyCache()
            {
                List<string> LIdsToErase = new List<string>();

                foreach (KeyValuePair<string, object> it in FCacheRef)
                {
                    LIdsToErase.Add(it.Key);
                }

                foreach (string it in LIdsToErase)
                {
                    FCacheRef.Remove(it);
                }

                return;
            }
        }
    }
}