using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandora.Client.Crypto.Currencies.BlockHeader;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Announce the hash of a transaction or block
    /// </summary>
    [Payload("inv")]
    public class InvPayload : Payload, ICoinSerializable, IEnumerable<InventoryVector>
    {
        public InvPayload()
        {
        }

        public InvPayload(params Transaction[] transactions)
            : this(transactions.Select(tx => new InventoryVector(InventoryType.MSG_TX, tx.GetHash())).ToArray())
        {
        }

        public InvPayload(params Block[] blocks)
            : this(blocks.Select(b => new InventoryVector(InventoryType.MSG_BLOCK, b.GetHash())).ToArray())
        {
        }

        public InvPayload(InventoryType type, params uint256[] hashes)
            : this(hashes.Select(h => new InventoryVector(type, h)).ToArray())
        {
        }

        public InvPayload(params InventoryVector[] invs)
        {
            _Inventory.AddRange(invs);
        }

        private List<InventoryVector> _Inventory = new List<InventoryVector>();

        public List<InventoryVector> Inventory
        {
            get
            {
                return _Inventory;
            }
        }

        #region ICoinSerializable Members

        public const int MAX_INV_SZ = 50000;

        public override void ReadWriteCore(CoinStream stream)
        {
            var old = stream.MaxArraySize;
            stream.MaxArraySize = MAX_INV_SZ;
            stream.ReadWrite(ref _Inventory);
            stream.MaxArraySize = old;
        }

        #endregion ICoinSerializable Members

        public override string ToString()
        {
            return "Count: " + Inventory.Count.ToString();
        }

        #region IEnumerable<uint256> Members

        public IEnumerator<InventoryVector> GetEnumerator()
        {
            return Inventory.GetEnumerator();
        }

        #endregion IEnumerable<uint256> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}