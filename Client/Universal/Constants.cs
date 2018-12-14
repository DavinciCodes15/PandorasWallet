namespace Pandora
{
    public static class Constants
    {
        public static readonly string REG_DavinciCodesRoot = @"SOFTWARE\Davinci Codes";
        public static readonly string PATH_PandorasSeverPathName = "Pandoras Wallet Server";
        public static readonly string PATH_PandorasClientPathName = "Pandoras Wallet";
        public static readonly string REG_PandorasServerKey = REG_DavinciCodesRoot + @"\" + PATH_PandorasSeverPathName;
        public static readonly string REG_PandorasClientKey = REG_DavinciCodesRoot + @"\" + PATH_PandorasClientPathName;
    }
}