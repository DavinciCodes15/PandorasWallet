using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Ask for transaction, block or merkle block
    /// </summary>
    [Payload("getdata")]
    public class GetDataPayload : Payload
    {
        public GetDataPayload()
        {
        }

        public GetDataPayload(params InventoryVector[] vectors)
        {
            inventory.AddRange(vectors);
        }

        private List<InventoryVector> inventory = new List<InventoryVector>();

        public List<InventoryVector> Inventory
        {
            set
            {
                inventory = value;
            }
            get
            {
                return inventory;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref inventory);
        }
    }
}