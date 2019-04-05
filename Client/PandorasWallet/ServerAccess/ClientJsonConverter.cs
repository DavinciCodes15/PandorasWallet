using Pandora.Client.ClientLib;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public class ClientJsonConverter : PandoraJsonConverter
    {
        private PandorasServer FPandoraServer;

        public ClientJsonConverter(PandorasServer aPandoraServer)
        {
            FPandoraServer = aPandoraServer;
            CreateConvertionEspecifications();
        }
    }
}