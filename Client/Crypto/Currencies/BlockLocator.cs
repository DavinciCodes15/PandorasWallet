using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
    /// <summary>
    /// Compact representation of one's chain position which can be used to find forks with another chain
    /// </summary>
    public class BlockLocator : ICoinSerializable
    {
        public BlockLocator()
        {
        }

        private List<uint256> vHave = new List<uint256>();

        public List<uint256> Blocks
        {
            get
            {
                return vHave;
            }
            set
            {
                vHave = value;
            }
        }

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            stream.ReadWrite(ref vHave);
        }

        #endregion ICoinSerializable Members
    }
}