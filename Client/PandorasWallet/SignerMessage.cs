using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pandora.Client.Crypto.Currencies;
using Nethereum.Signer;

namespace Pandora.Client.ClientLib
{
    public class SignerMessage : IDisposable
    {
        //metodo 1
        //encriptar mensaje que ingrese el usuario en un label del dialog con la llave del usuario que entregue el usuario ese momento
        //metodo 2
        //desencriptar mensaje con la llave del usuario. (?)

        //metodo 3
        //firmar transacciones 


        //*recordar utilizar using para finalizar el uso del objeto despues de utilizarlo en el controlador
        public void Dispose()
        {
            FCryptoCurrencyAdvocacy = null;
        }
        private CryptoCurrencyAdvocacy FCryptoCurrencyAdvocacy;

        //cada instancia de la clase pedira advocacy_1 que es la actual llave de eth del usuario
        public SignerMessage(CryptoCurrencyAdvocacy aAdvocacy)
        {
            FCryptoCurrencyAdvocacy= aAdvocacy;
            string privateKey = FCryptoCurrencyAdvocacy.GetPrivateKey(1);
        }

        //message in text to encrypt with user hash of eth
        public string Fmsg {get; set;}    
       

        /*hashing message and then signing it.
         debo llamar esto desde el controlador, cierto luis?
        */
        public void HashAndSign()
        {
            if (Fmsg != null)
            {
                
                string FprivateKey = FCryptoCurrencyAdvocacy.GetPrivateKey(1);

                var LSigner = new EthereumMessageSigner();
                var LSignature = LSigner.HashAndSign(Fmsg, FprivateKey);
                //example "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7");
                HashMsgRecover(LSigner);
                
            }
            else {
                throw new ArgumentNullException("Fmsg");
            }
            
        }

        /*
         When receiving a signature that has been made with a hashed 
        file it’s necessary to start by hashing the file we want to verify and then recover the address that signed it.
         */
        public void HashMsgRecover(EthereumMessageSigner LSigner)
        {
            if (LSigner != null)
            {
                //that code, will be refactor..  mmm
                string FprivateKey = FCryptoCurrencyAdvocacy.GetPrivateKey(1);
                var LSignature = LSigner.HashAndSign(Fmsg, FprivateKey);

                //recover message
                var addressRec2 = LSigner.HashAndEcRecover(Fmsg, LSignature);

            }
            else { throw new ArgumentNullException("Lsigner"); }
            
        }
        //                                      FREE RODRIGO
        public void SignMsg()
        {
            //var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

            //who send the message
            string privateKey = FCryptoCurrencyAdvocacy.GetPrivateKey(1);

            if (this.Fmsg == null) { Fmsg = "Testing signature message"; }

            var signer1 = new EthereumMessageSigner();
            var signature1 = signer1.EncodeUTF8AndSign(this.Fmsg, new EthECKey(privateKey));

            //verify msg
            var addressRec1 = signer1.EncodeUTF8AndEcRecover(this.Fmsg, signature1);
        }


    }
}
