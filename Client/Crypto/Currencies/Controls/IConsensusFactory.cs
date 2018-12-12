using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public interface IConsensusFactory
    {
        bool TryCreateNew(Type type, out ICoinSerializable result);

        bool TryCreateNew<T>(out T result) where T : ICoinSerializable;

        Block CreateBlock();

        BlockHeader CreateBlockHeader();

        Transaction CreateTransaction();
    }
}