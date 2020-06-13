using Pandora.Client.Crypto.Currencies.Bitcoin;
using Pandora.Client.Crypto.Currencies.Crypto;
using Pandora.Client.Crypto.Currencies.HashX11;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Pandora.Client.Crypto.Currencies
{
    public class BaseConsensusFactory : IConsensusFactory
    {
        private ConcurrentDictionary<Type, bool> _IsAssignableFromBlockHeader = new ConcurrentDictionary<Type, bool>();
        private TypeInfo BlockHeaderType = typeof(BlockHeader).GetTypeInfo();

        private ConcurrentDictionary<Type, bool> _IsAssignableFromBlock = new ConcurrentDictionary<Type, bool>();
        private TypeInfo BlockType = typeof(Block).GetTypeInfo();

        private ConcurrentDictionary<Type, bool> _IsAssignableFromTransaction = new ConcurrentDictionary<Type, bool>();
        private TypeInfo TransactionType = typeof(Transaction).GetTypeInfo();

        public BaseConsensusFactory()
        {
            FNetwork = null;
        }

        public BaseConsensusFactory(IServerChainParams aServerChainParams)
        {
            ServerChainParams = aServerChainParams;
            FNetwork = new Bitcoin.BitcoinNetwork(aServerChainParams);
        }

        public IServerChainParams ServerChainParams { get; private set; }

        private BitcoinNetwork FNetwork;

        protected bool IsBlockHeader(Type type)
        {
            return IsAssignable(type, BlockHeaderType, _IsAssignableFromBlockHeader);
        }

        protected bool IsBlock(Type type)
        {
            return IsAssignable(type, BlockType, _IsAssignableFromBlock);
        }

        protected bool IsTransaction(Type type)
        {
            return IsAssignable(type, TransactionType, _IsAssignableFromTransaction);
        }

        private bool IsAssignable(Type type, TypeInfo baseType, ConcurrentDictionary<Type, bool> cache)
        {
            if (!cache.TryGetValue(type, out bool isAssignable))
            {
                isAssignable = baseType.IsAssignableFrom(type.GetTypeInfo());
                cache.TryAdd(type, isAssignable);
            }
            return isAssignable;
        }

        public virtual bool TryCreateNew(Type type, out ICoinSerializable result)
        {
            result = null;
            if (IsBlock(type))
            {
                result = CreateBlock();
                return true;
            }
            if (IsBlockHeader(type))
            {
                result = CreateBlockHeader();
                return true;
            }
            if (IsTransaction(type))
            {
                result = CreateTransaction();
                return true;
            }
            return false;
        }

        public bool TryCreateNew<T>(out T result) where T : ICoinSerializable
        {
            result = default(T);
            bool success = TryCreateNew(typeof(T), out ICoinSerializable r);
            if (success)
            {
                result = (T)r;
            }

            return success;
        }

        public virtual Block CreateBlock()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new Block(CreateBlockHeader());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public virtual BlockHeader CreateBlockHeader()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new BlockHeader();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public virtual Transaction CreateTransaction()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new Transaction(FNetwork);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete

    public class AltBlockHeader : BlockHeader
    {
        public override uint256 GetPoWHash()
        {
            byte[] headerBytes = this.ToBytes();
            byte[] h = SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
            return new uint256(h);
        }
    }

    public class AltX11BlockHeader : AltBlockHeader
    {
        // https://github.com/dashpay/dash/blob/e596762ca22d703a79c6880a9d3edb1c7c972fd3/src/primitives/block.cpp#L13
        private static byte[] CalculateHash(byte[] data, int offset, int count)
        {
            return new X11().ComputeBytes(data.Skip(offset).Take(count).ToArray());
        }

        protected override HashStreamBase CreateHashStream()
        {
            return BufferedHashStream.CreateFrom(CalculateHash);
        }
    }

    public class AltQuarkBlockHeader : AltBlockHeader
    {
        private static byte[] CalculateHash(byte[] data, int offset, int count)
        {
            return new Quark().ComputeBytes(data.Skip(offset).Take(count).ToArray());
        }

        protected override HashStreamBase CreateHashStream()
        {
            return BufferedHashStream.CreateFrom(CalculateHash);
        }
    }

    public class AltBlock : Block
    {
        private IConsensusFactory FMainConsensusFactory;

        public AltBlock(BlockHeader header, IConsensusFactory aMainConsensusFactory) : base(header)
        {
            FMainConsensusFactory = aMainConsensusFactory;
        }

        public override IConsensusFactory GetConsensusFactory()
        {
            return FMainConsensusFactory;
        }
    }
}