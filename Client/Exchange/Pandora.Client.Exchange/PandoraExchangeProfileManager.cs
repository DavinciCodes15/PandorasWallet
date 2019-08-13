using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class PandoraExchangeProfileManager : ISaveManagerConfigurable
    {
        private static PandoraExchangeProfileManager FInstance;
        private Dictionary<int, PandoraExchangeProfile> FPandoraExchangesProfiles;
        private IPandoraSaveManager FSaveManager;

        public Dictionary<uint, string> ExchangesInventory => PandoraExchangeFactoryProducer.GetInstance().Inventory;

        private PandoraExchangeProfileManager()
        {
            FPandoraExchangesProfiles = new Dictionary<int, PandoraExchangeProfile>();
            FSaveManager = PandoraSaveManagerFactory.GetSaveMangerFactory().GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
        }

        public static PandoraExchangeProfileManager GetProfiler()
        {
            if (FInstance == null)
                FInstance = new PandoraExchangeProfileManager();
            return FInstance;
        }

        public Dictionary<int, string> ProfileNames
        {
            get
            {
                Dictionary<int, string> lResult = new Dictionary<int, string>();
                lock (FPandoraExchangesProfiles)
                {
                    foreach (var lProfile in FPandoraExchangesProfiles)
                    {
                        var lProfileObject = lProfile.Value;
                        lResult.Add(lProfile.Key, $"(id:{lProfileObject.ProfileID}) {lProfileObject.Name}");
                    }
                }
                return lResult;
            }
        }

        public void LoadSavedProfiles()
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");
            var lSavedProfiles = FSaveManager.LoadProfiles();

            lock (FPandoraExchangesProfiles)
            {
                foreach (var lSavedProfile in lSavedProfiles)
                {
                    if (!FPandoraExchangesProfiles.ContainsKey(lSavedProfile.ProfileID))
                    {
                        lSavedProfile.ExchangeEntity = new PandoraExchangeEntity(lSavedProfile.ExchangeID);
                        FPandoraExchangesProfiles.Add(lSavedProfile.ProfileID, lSavedProfile);
                    }
                }
            }
        }

        public int AddNewExchangeProfile(uint aExchangeId, string aName)
        {
            if (aExchangeId <= 0)
                throw new ArgumentException("Exchange Id needs a value", nameof(aExchangeId));
            if (string.IsNullOrEmpty(aName))
                throw new ArgumentException("A profile Name needs to be provided", nameof(aName));
            int lProfileID = 51;
            var lNewProfile = new PandoraExchangeProfile
            {
                ExchangeID = aExchangeId,
                Name = aName,
                ProfileID = lProfileID,
                ExchangeEntity = new PandoraExchangeEntity(aExchangeId)
            };
            lock (FPandoraExchangesProfiles)
            {
                if (FPandoraExchangesProfiles.Any())
                    lProfileID = FPandoraExchangesProfiles.Keys.Max() + 3;
                FPandoraExchangesProfiles.Add(lProfileID, lNewProfile);
            }
            if (FSaveManager.Initialized)
                FSaveManager.SaveProfile(lNewProfile);
            else
                Universal.Log.Write(Universal.LogLevel.Warning, $"Saving exchange profile: {lNewProfile.ProfileID} - {lNewProfile.Name} only in memory. Saving manager not configured");
            return lProfileID;
        }

        public bool RemoveExchangeProfile(int aProfileID)
        {
            bool lResult = false;
            try
            {
                if (FSaveManager.Initialized)
                    FSaveManager.DeleteProfile(aProfileID);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, $"Unable to remove profile id {aProfileID} from Save store. Exception: {ex}");
            }
            lock (FPandoraExchangesProfiles)
            {
                if (FPandoraExchangesProfiles.TryGetValue(aProfileID, out PandoraExchangeProfile lProfile))
                {
                    lProfile.Close();
                    FPandoraExchangesProfiles.Remove(aProfileID);
                    lResult = true;
                }
            }
            return lResult;
        }

        public PandoraExchangeEntity GetProfileExchangeEntity(int aProfileID)
        {
            PandoraExchangeProfile lProfile;
            lock (FPandoraExchangesProfiles)
            {
                if (!FPandoraExchangesProfiles.TryGetValue(aProfileID, out lProfile))
                    throw new Exception("Profile not found");
            }
            return lProfile.ExchangeEntity;
        }

        public bool ConfigureSaveLocation(bool aForce = false, params string[] aSaveInitializingParams)
        {
            if (FSaveManager.Initialized && !aForce)
                return false;
            FSaveManager.Initialize(aSaveInitializingParams);
            return true;
        }
    }
}