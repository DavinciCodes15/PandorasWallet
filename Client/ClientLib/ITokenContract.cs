using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{
    //This interface will become usefull if later we try to implement other tooken standars differents than ERC20 by also using a Factory.
    // In the meantime, no factory has been made
    public interface ITokenContract
    {
        string GetNameMethodCode();

        string GetSymbolMethodCode();

        string GetPrecisionMethodCode();

        string GetBalanceMethodCode(string aAddress);

        string GetBalanceTransferMethodCode(string aDestinationAddress, BigInteger aAmount);
    }
}