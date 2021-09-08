namespace Pandora.Client.Exchange
{
    public interface ISaveManagerConfigurable
    {
        bool ConfigureSaveLocation(bool aForce = false, params string[] aSaveInitializingParams);
    }
}