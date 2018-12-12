﻿using Pandora.Client.Crypto.Currencies.BouncyCastle.Math;
using Pandora.Client.Crypto.Currencies.Controls;
using Pandora.Client.Crypto.Currencies.Crypto;
using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pandora.Client.Crypto.Currencies
{
    /// <summary>
    /// Nodes collect new transactions into a block, hash them into a hash tree,
    /// and scan through nonce values to make the block's hash satisfy proof-of-work
    /// requirements.  When they solve the proof-of-work, they broadcast the block
    /// to everyone and the block is added to the block chain.  The first transaction
    /// in the block is a special one that creates a new coin owned by the creator
    /// of the block.
    /// </summary>
    public class BlockHeader : ICoinSerializable
    {
        internal const int Size = 80;

        public static BlockHeader Parse(string hex, IConsensusFactory consensusFactory)
        {
            if (consensusFactory == null)
            {
                throw new ArgumentNullException(nameof(consensusFactory));
            }

            return new BlockHeader(Encoders.Hex.DecodeData(hex), consensusFactory);
        }

        public BlockHeader()
        {
            SetNull();
        }

        public BlockHeader(string hex, IConsensusFactory consensusFactory)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            if (consensusFactory == null)
            {
                throw new ArgumentNullException(nameof(consensusFactory));
            }

            CoinStream bs = new CoinStream(Encoders.Hex.DecodeData(hex))
            {
                ConsensusFactory = consensusFactory
            };
            ReadWrite(bs);
        }

        [Obsolete("Use new BlockHeader(string hex, Network|Consensus|ConsensusFactory) instead")]
        public BlockHeader(string hex)
            : this(Encoders.Hex.DecodeData(hex))
        {
        }

        public BlockHeader(byte[] data, IConsensusFactory consensusFactory)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (consensusFactory == null)
            {
                throw new ArgumentNullException(nameof(consensusFactory));
            }

            CoinStream bs = new CoinStream(data)
            {
                ConsensusFactory = consensusFactory
            };
            ReadWrite(bs);
        }

        [Obsolete("Use new BlockHeader(byte[] hex, Network|Consensus|ConsensusFactory) instead")]
        public BlockHeader(byte[] bytes)
        {
            this.ReadWrite(bytes);
        }

        // header
        private const int CURRENT_VERSION = 3;

        protected uint256 hashPrevBlock;

        public uint256 HashPrevBlock
        {
            get => hashPrevBlock;
            set => hashPrevBlock = value;
        }

        public uint256 Hash
        {
            get => GetHash();

            set
            {
                if (_Hashes == null)
                {
                    _Hashes = new uint256[1];
                }

                _Hashes[0] = value;
            }
        }

        protected uint256 hashMerkleRoot;

        protected uint nTime;
        protected uint nBits;

        public Target Bits
        {
            get => nBits;
            set => nBits = value;
        }

        protected int nVersion;

        public int Version
        {
            get => nVersion;
            set => nVersion = value;
        }

        protected uint nNonce;

        public uint Nonce
        {
            get => nNonce;
            set => nNonce = value;
        }

        public uint256 HashMerkleRoot
        {
            get => hashMerkleRoot;
            set => hashMerkleRoot = value;
        }

        internal void SetNull()
        {
            nVersion = CURRENT_VERSION;
            hashPrevBlock = 0;
            hashMerkleRoot = 0;
            nTime = 0;
            nBits = 0;
            nNonce = 0;
        }

        public bool IsNull => (nBits == 0);

        #region CoinSerializable Members

        public virtual void ReadWrite(CoinStream stream)
        {
            stream.ReadWrite(ref nVersion);

#if DEBUG
            byte[] lbytes = BitConverter.GetBytes(nVersion);
#endif

            stream.ReadWrite(ref hashPrevBlock);
            stream.ReadWrite(ref hashMerkleRoot);
            stream.ReadWrite(ref nTime);
            stream.ReadWrite(ref nBits);
            stream.ReadWrite(ref nNonce);
        }

        #endregion CoinSerializable Members

        public virtual uint256 GetPoWHash()
        {
            return GetHash();
        }

        public virtual uint256 GetHash()
        {
            uint256 h = null;
            uint256[] hashes = _Hashes;
            if (hashes != null)
            {
                h = hashes[0];
            }
            if (h != null)
            {
                return h;
            }

            using (HashStreamBase hs = CreateHashStream())
            {
                ReadWrite(new CoinStream(hs, true));
                h = hs.GetHash();
            }

            hashes = _Hashes;
            if (hashes != null)
            {
                hashes[0] = h;
            }
            return h;
        }

        protected virtual HashStreamBase CreateHashStream()
        {
            return new HashStream();
        }

        [Obsolete("Call PrecomputeHash(true, true) instead")]
        public void CacheHashes()
        {
            PrecomputeHash(true, true);
        }

        /// <summary>
        /// Precompute the block header hash so that later calls to GetHash() will returns the precomputed hash
        /// </summary>
        /// <param name="invalidateExisting">If true, the previous precomputed hash is thrown away, else it is reused</param>
        /// <param name="lazily">If true, the hash will be calculated and cached at the first call to GetHash(), else it will be immediately</param>
        public void PrecomputeHash(bool invalidateExisting, bool lazily)
        {
            _Hashes = invalidateExisting ? new uint256[1] : _Hashes ?? new uint256[1];
            if (!lazily && _Hashes[0] == null)
            {
                _Hashes[0] = GetHash();
            }
        }

        private uint256[] _Hashes;

        public DateTimeOffset BlockTime
        {
            get => Utils.UnixTimeToDateTime(nTime);
            set => nTime = Utils.DateTimeToUnixTime(value);
        }

        private static BigInteger Pow256 = BigInteger.ValueOf(2).Pow(256);

        public bool CheckProofOfWork()
        {
            BigInteger bits = Bits.ToBigInteger();
            if (bits.CompareTo(BigInteger.Zero) <= 0 || bits.CompareTo(Pow256) >= 0)
            {
                return false;
            }
            // Check proof of work matches claimed amount
            return GetPoWHash() <= Bits.ToUInt256();
        }
    }

    public class Block : ICoinSerializable
    {
        private BlockHeader header;

        //FIXME: it needs to be changed when Gavin Andresen increase the max block size.
        public const uint MAX_BLOCK_SIZE = 1000 * 1000;

        public Block() : this((new BaseConsensusFactory()).CreateBlockHeader())
        {
        }

        public Block(BlockHeader blockHeader)
        {
            if (blockHeader == null)
            {
                throw new ArgumentNullException(nameof(blockHeader));
            }

            SetNull();
            header = blockHeader;
        }

        // network and disk
        private List<Transaction> vtx = new List<Transaction>();

        public List<Transaction> Transactions
        {
            get => vtx;
            set => vtx = value;
        }

        public MerkleNode GetMerkleRoot()
        {
            return MerkleNode.GetRoot(Transactions.Select(t => t.GetHash()));
        }

        public void ReadWrite(CoinStream stream)
        {
            stream.ReadWrite(ref header);
            stream.ReadWrite(ref vtx);
        }

        public bool HeaderOnly => vtx == null || vtx.Count == 0;

        /// <summary>
        /// Get the coinbase height as specified by the first tx input of this block (BIP 34)
        /// </summary>
        /// <returns>Null if block has been created before BIP34 got enforced, else, the height</returns>+
        public int? GetCoinbaseHeight()
        {
            if (Header.Version < 2 || Transactions.Count == 0 || Transactions[0].Inputs.Count == 0)
            {
                return null;
            }

            return Transactions[0].Inputs[0].ScriptSig.ToOps().FirstOrDefault()?.GetInt();
        }

        private void SetNull()
        {
            if (header != null)
            {
                header.SetNull();
            }

            vtx.Clear();
        }

        public BlockHeader Header => header;

        public uint256 GetHash()
        {
            //Block's hash is his header's hash
            return header.GetHash();
        }

        public void ReadWrite(byte[] array, int startIndex)
        {
            MemoryStream ms = new MemoryStream(array);
            ms.Position += startIndex;
            CoinStream bitStream = new CoinStream(ms, false);
            ReadWrite(bitStream);
        }

        public Transaction AddTransaction(Transaction tx)
        {
            Transactions.Add(tx);
            return tx;
        }

        /// <summary>
        /// Create a block with the specified option only. (useful for stripping data from a block)
        /// </summary>
        /// <param name="options">Options to keep</param>
        /// <returns>A new block with only the options wanted</returns>
        public Block WithOptions(TransactionOptions options)
        {
            if (Transactions.Count == 0)
            {
                return this;
            }

            if (options == TransactionOptions.Witness && Transactions[0].HasWitness)
            {
                return this;
            }

            if (options == TransactionOptions.None && !Transactions[0].HasWitness)
            {
                return this;
            }

            Block instance = GetConsensusFactory().CreateBlock();
            MemoryStream ms = new MemoryStream();
            CoinStream bms = new CoinStream(ms, true)
            {
                TransactionOptions = options
            };
            ReadWrite(bms);
            ms.Position = 0;
            bms = new CoinStream(ms, false)
            {
                TransactionOptions = options
            };
            instance.ReadWrite(bms);
            return instance;
        }

        public virtual IConsensusFactory GetConsensusFactory()
        {
            return new BaseConsensusFactory();
        }

        public void UpdateMerkleRoot()
        {
            Header.HashMerkleRoot = GetMerkleRoot().Hash;
        }

        /// <summary>
        /// Check proof of work and merkle root
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            return CheckMerkleRoot() && Header.CheckProofOfWork();
        }

        public bool CheckProofOfWork()
        {
            return Header.CheckProofOfWork();
        }

        public bool CheckMerkleRoot()
        {
            return Header.HashMerkleRoot == GetMerkleRoot().Hash;
        }

        public static Block Parse(string hex, BaseConsensusFactory consensusFactory)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            if (consensusFactory == null)
            {
                throw new ArgumentNullException(nameof(consensusFactory));
            }

            Block block = consensusFactory.CreateBlock();
            block.ReadWrite(Encoders.Hex.DecodeData(hex));
            return block;
        }

        public static Block Load(byte[] hex, BaseConsensusFactory consensusFactory)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            if (consensusFactory == null)
            {
                throw new ArgumentNullException(nameof(consensusFactory));
            }

            Block block = consensusFactory.CreateBlock();
            block.ReadWrite(hex);
            return block;
        }

        public MerkleBlock Filter(params uint256[] txIds)
        {
            return new MerkleBlock(this, txIds);
        }

        public MerkleBlock Filter(BloomFilter filter)
        {
            return new MerkleBlock(this, filter);
        }
    }
}