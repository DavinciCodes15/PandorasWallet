using Pandora.Client.Crypto.Currencies.Crypto;
using Pandora.Client.Crypto.Currencies.DataEncoders;
//using Pandora.Client.Crypto.Currencies.Protocol;
using Pandora.Client.Crypto.Currencies.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{


    public class TxIn : ICoinSerializable
    {
        public TxIn()
        {

        }
        public TxIn(Script scriptSig)
        {
            this.scriptSig = scriptSig;
        }
        public TxIn(OutPoint prevout, Script scriptSig)
        {
            this.prevout = prevout;
            this.scriptSig = scriptSig;
        }
        public TxIn(OutPoint prevout)
        {
            this.prevout = prevout;
        }
        OutPoint prevout = new OutPoint();
        Script scriptSig = Script.Empty;
        uint nSequence = uint.MaxValue;

        public Sequence Sequence
        {
            get
            {
                return nSequence;
            }
            set
            {
                nSequence = value.Value;
            }
        }
        public OutPoint PrevOut
        {
            get
            {
                return prevout;
            }
            set
            {
                prevout = value;
            }
        }


        public Script ScriptSig
        {
            get
            {
                return scriptSig;
            }
            set
            {
                scriptSig = value;
            }
        }

        public IDestination GetSigner()
        {
            return scriptSig.GetSigner();// ?? witScript.GetSigner();
        }

        public string GetAddress(Network aNetwork)
        {
            var lItem = PayToPubkeyTemplate.Instance.ExtractScriptPubKeyParameters(this.ScriptSig);
            return lItem.GetAddress(aNetwork).ToString();
        }


        //WitScript witScript = WitScript.Empty;

        ///// <summary>
        ///// The witness script (Witness script is not serialized and deserialized at the TxIn level, but at the Transaction level)
        ///// </summary>
        //public WitScript WitScript
        //{
        //	get
        //	{
        //		return witScript;
        //	}
        //	set
        //	{
        //		witScript = value;
        //	}
        //}

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            stream.ReadWrite(ref prevout);
            stream.ReadWrite(ref scriptSig);
            stream.ReadWrite(ref nSequence);
        }

        #endregion

        public bool IsFrom(PubKey pubKey)
        {
            var result = PayToPubkeyHashTemplate.Instance.ExtractScriptSigParameters(ScriptSig);
            return result != null && result.PublicKey == pubKey;
        }

        public bool IsFinal
        {
            get
            {
                return (nSequence == uint.MaxValue);
            }
        }

        public TxIn Clone()
        {
            var txin = CoinSerializableExtensions.Clone(this, 0);
            //			txin.WitScript = (witScript ?? WitScript.Empty).Clone();
            return txin;
        }

        public static TxIn CreateCoinbase(int height)
        {
            var txin = new TxIn();
            txin.ScriptSig = new Script(Op.GetPushOp(height)) + OpcodeType.OP_0;
            return txin;
        }
    }

    public class TxOutCompressor : ICoinSerializable
    {
        // Amount compression:
        // * If the amount is 0, output 0
        // * first, divide the amount (in base units) by the largest power of 10 possible; call the exponent e (e is max 9)
        // * if e<9, the last digit of the resulting number cannot be 0; store it as d, and drop it (divide by 10)
        //   * call the result n
        //   * output 1 + 10*(9*n + d - 1) + e
        // * if e==9, we only know the resulting number is not zero, so output 1 + 10*(n - 1) + 9
        // (this is decodable, as d is in [1-9] and e is in [0-9])

        ulong CompressAmount(ulong n)
        {
            if (n == 0)
                return 0;
            int e = 0;
            while (((n % 10) == 0) && e < 9)
            {
                n /= 10;
                e++;
            }
            if (e < 9)
            {
                int d = (int)(n % 10);
                n /= 10;
                return 1 + (n * 9 + (ulong)(d - 1)) * 10 + (ulong)e;
            }
            else
            {
                return 1 + (n - 1) * 10 + 9;
            }
        }

        ulong DecompressAmount(ulong x)
        {
            // x = 0  OR  x = 1+10*(9*n + d - 1) + e  OR  x = 1+10*(n - 1) + 9
            if (x == 0)
                return 0;
            x--;
            // x = 10*(9*n + d - 1) + e
            int e = (int)(x % 10);
            x /= 10;
            ulong n = 0;
            if (e < 9)
            {
                // x = 9*n + d - 1
                int d = (int)((x % 9) + 1);
                x /= 9;
                // x = n
                n = (x * 10 + (ulong)d);
            }
            else
            {
                n = x + 1;
            }
            while (e != 0)
            {
                n *= 10;
                e--;
            }
            return n;
        }


        private TxOut _TxOut = new TxOut();
        public TxOut TxOut
        {
            get
            {
                return _TxOut;
            }
        }
        public TxOutCompressor()
        {

        }
        public TxOutCompressor(TxOut txOut)
        {
            _TxOut = txOut;
        }

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            if (stream.Serializing)
            {
                ulong val = CompressAmount((ulong)_TxOut.Value.Satoshi);
                stream.ReadWriteAsCompactVarInt(ref val);
            }
            else
            {
                ulong val = 0;
                stream.ReadWriteAsCompactVarInt(ref val);
                _TxOut.Value = new Money(DecompressAmount(val));
            }
            ScriptCompressor cscript = new ScriptCompressor(_TxOut.ScriptPubKey);
            stream.ReadWrite(ref cscript);
            if (!stream.Serializing)
                _TxOut.ScriptPubKey = new Script(cscript.ScriptBytes);
        }

        #endregion
    }

    public class TxOut : ICoinSerializable, IDestination
    {
        Script publicKey = Script.Empty;
        public Script ScriptPubKey
        {
            get
            {
                return this.publicKey;
            }
            set
            {
                this.publicKey = value;
            }
        }

        public TxOut()
        {

        }

        public TxOut(Money value, IDestination destination)
        {
            Value = value;
            if (destination != null)
                ScriptPubKey = destination.ScriptPubKey;
        }

        public TxOut(Money value, Script scriptPubKey)
        {
            Value = value;
            ScriptPubKey = scriptPubKey;
        }

        readonly static Money NullMoney = new Money(-1);
        Money _Value = NullMoney;
        public Money Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _Value = value;
            }
        }


        public bool IsDust(FeeRate minRelayTxFee)
        {
            return (Value < GetDustThreshold(minRelayTxFee));
        }

        public Money GetDustThreshold(FeeRate minRelayTxFee)
        {
            if (minRelayTxFee == null)
                throw new ArgumentNullException("minRelayTxFee");
            int nSize = this.GetSerializedSize() + 148;
            return 3 * minRelayTxFee.GetFee(nSize);
        }

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            long value = Value.Satoshi;
            stream.ReadWrite(ref value);
            if (!stream.Serializing)
                _Value = new Money(value);
            stream.ReadWrite(ref publicKey);
        }

        #endregion
        public string GetAddress(Network aNetwork)
        {
            return ScriptPubKey.GetDestination().GetAddress(aNetwork).ToString();
        }

        public bool IsTo(IDestination destination)
        {
            return ScriptPubKey == destination.ScriptPubKey;
        }

        public static TxOut Parse(string hex)
        {
            var ret = new TxOut();
            ret.FromBytes(Encoders.Hex.DecodeData(hex));
            return ret;
        }
    }

    public class IndexedTxIn
    {
        public TxIn TxIn
        {
            get;
            set;
        }

        /// <summary>
        /// The index of this TxIn in its transaction
        /// </summary>
        public uint Index
        {
            get;
            set;
        }

        public OutPoint PrevOut
        {
            get
            {
                return TxIn.PrevOut;
            }
            set
            {
                TxIn.PrevOut = value;
            }
        }

        public Script ScriptSig
        {
            get
            {
                return TxIn.ScriptSig;
            }
            set
            {
                TxIn.ScriptSig = value;
            }
        }


        //public WitScript WitScript
        //{
        //	get
        //	{
        //		return TxIn.WitScript;
        //	}
        //	set
        //	{
        //		TxIn.WitScript = value;
        //	}
        //}
        public Transaction Transaction
        {
            get;
            set;
        }

        public bool VerifyScript(Script scriptPubKey, ScriptVerify scriptVerify = ScriptVerify.Standard)
        {
            ScriptError unused;
            return VerifyScript(scriptPubKey, scriptVerify, out unused);
        }
        public bool VerifyScript(Script scriptPubKey, out ScriptError error)
        {
            return Script.VerifyScript(scriptPubKey, Transaction, (int)Index, null, out error);
        }
        public bool VerifyScript(Script scriptPubKey, ScriptVerify scriptVerify, out ScriptError error)
        {
            return Script.VerifyScript(scriptPubKey, Transaction, (int)Index, null, scriptVerify, SigHash.Undefined, out error);
        }
        public bool VerifyScript(Script scriptPubKey, Money value, ScriptVerify scriptVerify, out ScriptError error)
        {
            return Script.VerifyScript(scriptPubKey, Transaction, (int)Index, value, scriptVerify, SigHash.Undefined, out error);
        }

        public bool VerifyScript(ICoin coin, ScriptVerify scriptVerify = ScriptVerify.Standard)
        {
            ScriptError error;
            return VerifyScript(coin, scriptVerify, out error);
        }

        public bool VerifyScript(ICoin coin, ScriptVerify scriptVerify, out ScriptError error)
        {
            return Script.VerifyScript(coin.TxOut.ScriptPubKey, Transaction, (int)Index, coin.TxOut.Value, scriptVerify, SigHash.Undefined, out error);
        }
        public bool VerifyScript(ICoin coin, out ScriptError error)
        {
            return VerifyScript(coin, ScriptVerify.Standard, out error);
        }

        public TransactionSignature Sign(CCKey key, ICoin coin, SigHash sigHash)
        {
            var hash = GetSignatureHash(coin, sigHash);
            return new TransactionSignature(key.Sign(hash), sigHash);// key.Sign(hash, sigHash);
        }

        public uint256 GetSignatureHash(ICoin coin, SigHash sigHash = SigHash.All)
        {
            return Script.SignatureHash(coin.GetScriptCode(), Transaction, (int)Index, sigHash, coin.TxOut.Value, coin.GetHashVersion());
        }

    }

    public interface ICoin
    {
        IMoney Amount
        {
            get;
        }
        OutPoint Outpoint
        {
            get;
        }
        TxOut TxOut
        {
            get;
        }

        /// <summary>
        /// Returns the script actually signed and executed
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Additional information needed to get the ScriptCode</exception>
        /// <returns>The executed script</returns>
        Script GetScriptCode();
        void OverrideScriptCode(Script scriptCode);
        bool CanGetScriptCode
        {
            get;
        }
        HashVersion GetHashVersion();
    }


    public class UnsignedList<T> : List<T>
    where T : ICoinSerializable, new()
    {
        public UnsignedList()
        {

        }
        public UnsignedList(Transaction parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            Transaction = parent;
        }

        public Transaction Transaction
        {
            get;
            internal set;
        }

        public UnsignedList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public UnsignedList(int capacity)
            : base(capacity)
        {
        }

        public T this[uint index]
        {
            get
            {
                return base[(int)index];
            }
            set
            {
                base[(int)index] = value;
            }
        }
    }

    public class TxInList : UnsignedList<TxIn>
    {
        public TxInList()
        {

        }
        public TxInList(Transaction parent)
            : base(parent)
        {

        }
        public TxIn this[OutPoint outpoint]
        {
            get
            {
                return this[outpoint.N];
            }
            set
            {
                this[outpoint.N] = value;
            }
        }

        public IEnumerable<IndexedTxIn> AsIndexedInputs()
        {
            // We want i as the index of txIn in Intputs[], not index in enumerable after where filter
            return this.Select((r, i) => new IndexedTxIn()
            {
                TxIn = r,
                Index = (uint)i,
                Transaction = Transaction
            });
        }
    }

    public class IndexedTxOut
    {
        public TxOut TxOut
        {
            get;
            set;
        }
        public uint N
        {
            get;
            set;
        }

        public Transaction Transaction
        {
            get;
            set;
        }
        public Coin ToCoin()
        {
            return new Coin(this);
        }
    }

    public class TxOutList : UnsignedList<TxOut>
    {
        public TxOutList()
        {

        }
        public TxOutList(Transaction parent)
            : base(parent)
        {

        }
        public IEnumerable<TxOut> To(IDestination destination)
        {
            return this.Where(r => r.IsTo(destination));
        }
        public IEnumerable<TxOut> To(Script scriptPubKey)
        {
            return this.Where(r => r.ScriptPubKey == scriptPubKey);
        }

        public IEnumerable<IndexedTxOut> AsIndexedOutputs()
        {
            // We want i as the index of txOut in Outputs[], not index in enumerable after where filter
            return this.Select((r, i) => new IndexedTxOut()
            {
                TxOut = r,
                N = (uint)i,
                Transaction = Transaction
            });
        }

        public IEnumerable<Coin> AsCoins()
        {
            var txId = Transaction.GetHash();
            for (int i = 0; i < Count; i++)
            {
                yield return new Coin(new OutPoint(txId, i), this[i]);
            }
        }
    }

    public class WitScript
    {
        byte[][] _Pushes;
        public WitScript(string script)
        {
            var parts = script.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _Pushes = new byte[parts.Length][];
            for (int i = 0; i < parts.Length; i++)
            {
                _Pushes[i] = Encoders.Hex.DecodeData(parts[i]);
            }
        }

        /// <summary>
        /// Create a new WitnessScript
        /// </summary>
        /// <param name="script">Scripts</param>
        /// <param name="unsafe">If false, make a copy of the input script array</param>
        public WitScript(byte[][] script, bool @unsafe = false)
        {
            if (@unsafe)
                _Pushes = script;
            else
            {
                _Pushes = script.ToArray();
                for (int i = 0; i < _Pushes.Length; i++)
                    _Pushes[i] = script[i].ToArray();
            }
        }

        /// <summary>
        /// Create a new WitnessScript
        /// </summary>
        /// <param name="script">Scripts</param>
        public WitScript(IEnumerable<byte[]> script, bool @unsafe = false)
            : this(script.ToArray(), @unsafe)
        {

        }

        public WitScript(params Op[] ops)
        {
            List<byte[]> pushes = new List<byte[]>();
            foreach (var op in ops)
            {
                if (op.PushData == null)
                    throw new ArgumentException("Non push operation unsupported in WitScript", "ops");
                pushes.Add(op.PushData);
            }
            _Pushes = pushes.ToArray();
        }

        public WitScript(byte[] script)
        {
            if (script == null)
                throw new ArgumentNullException("script");
            var ms = new MemoryStream(script);
            CoinStream stream = new CoinStream(ms, false);
            ReadCore(stream);
        }
        WitScript()
        {

        }

        public WitScript(Script scriptSig)
        {
            List<byte[]> pushes = new List<byte[]>();
            foreach (var op in scriptSig.ToOps())
            {
                if (op.PushData == null)
                    throw new ArgumentException("A WitScript can only contains push operations", "script");
                pushes.Add(op.PushData);
            }
            _Pushes = pushes.ToArray();
        }

        public static WitScript Load(CoinStream stream)
        {
            WitScript script = new WitScript();
            script.ReadCore(stream);
            return script;
        }
        void ReadCore(CoinStream stream)
        {
            List<byte[]> pushes = new List<byte[]>();
            uint pushCount = 0;
            stream.ReadWriteAsVarInt(ref pushCount);
            for (int i = 0; i < (int)pushCount; i++)
            {
                byte[] push = ReadPush(stream);
                pushes.Add(push);
            }
            _Pushes = pushes.ToArray();
        }
        private static byte[] ReadPush(CoinStream stream)
        {
            byte[] push = null;
            stream.ReadWriteAsVarString(ref push);
            return push;
        }

        public byte[] this[int index]
        {
            get
            {
                return _Pushes[index];
            }
        }

        public IEnumerable<byte[]> Pushes
        {
            get
            {
                return _Pushes;
            }
        }

        static WitScript _Empty = new WitScript(new byte[0][], true);

        public static WitScript Empty
        {
            get
            {
                return _Empty;
            }
        }

        public override bool Equals(object obj)
        {
            WitScript item = obj as WitScript;
            if (item == null)
                return false;
            return EqualsCore(item);
        }

        private bool EqualsCore(WitScript item)
        {
            if (_Pushes.Length != item._Pushes.Length)
                return false;
            for (int i = 0; i < _Pushes.Length; i++)
            {
                if (!Utils.ArrayEqual(_Pushes[i], item._Pushes[i]))
                    return false;
            }
            return true;
        }
        public static bool operator ==(WitScript a, WitScript b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.EqualsCore(b);
        }

        public static bool operator !=(WitScript a, WitScript b)
        {
            return !(a == b);
        }
        public static WitScript operator +(WitScript a, WitScript b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;
            return new WitScript(a._Pushes.Concat(b._Pushes).ToArray());
        }
        public static implicit operator Script(WitScript witScript)
        {
            if (witScript == null)
                return null;
            return witScript.ToScript();
        }
        public override int GetHashCode()
        {
            return Utils.GetHashCode(ToBytes());
        }

        public byte[] ToBytes()
        {
            var ms = new MemoryStream();
            CoinStream stream = new CoinStream(ms, true);
            uint pushCount = (uint)_Pushes.Length;
            stream.ReadWriteAsVarInt(ref pushCount);
            foreach (var push in Pushes)
            {
                var localpush = push;
                stream.ReadWriteAsVarString(ref localpush);
            }
            return ms.ToArrayEfficient();
        }

        public override string ToString()
        {
            return ToScript().ToString();
        }

        public Script ToScript()
        {
            return new Script(_Pushes.Select(p => Op.GetPushOp(p)).ToArray());
        }

        public int PushCount
        {
            get
            {
                return _Pushes.Length;
            }
        }

        public byte[] GetUnsafePush(int i)
        {
            return _Pushes[i];
        }

        public WitScript Clone()
        {
            return new WitScript(ToBytes());
        }

        public TxDestination GetSigner()
        {
            throw new NotImplementedException();
            //var pubKey = PayToWitPubKeyHashTemplate.Instance.ExtractWitScriptParameters(this);
            //if(pubKey != null)
            //{
            //	return pubKey.PublicKey.WitHash;
            //}
            //var p2sh = PayToWitScriptHashTemplate.Instance.ExtractWitScriptParameters(this);
            //return p2sh != null ? p2sh.WitHash : null;
        }
    }

    //[Flags]
    //public enum TransactionOptions : uint
    //{
    //	None = 0x00000000,
    //	Witness = 0x40000000,
    //	All = Witness
    //}
    class Witness
    {
        TxInList _Inputs;
        public Witness(TxInList inputs)
        {
            _Inputs = inputs;
        }

        internal bool IsNull()
        {
            throw new NotImplementedException();
            //return _Inputs.All(i => i.WitScript.PushCount == 0);
        }

        internal void ReadWrite(CoinStream stream)
        {
            throw new NotImplementedException();
            //         for (int i = 0; i < _Inputs.Count; i++)
            //{
            //	if(stream.Serializing)
            //	{
            //		var bytes = (_Inputs[i].WitScript ?? WitScript.Empty).ToBytes();
            //		stream.ReadWrite(ref bytes);
            //	}
            //	else
            //	{
            //		_Inputs[i].WitScript = WitScript.Load(stream);
            //	}
            //}

            //if(IsNull())
            //	throw new FormatException("Superfluous witness record");
        }
    }

    //https://en.bitcoin.it/wiki/Transactions
    //https://en.bitcoin.it/wiki/Protocol_specification
    public class Transaction : ICoinSerializable
    {
        public bool RBF
        {
            get
            {
                return Inputs.Any(i => i.Sequence < 0xffffffff - 1);
            }
        }

        uint nVersion = 1;

        public uint Version
        {
            get
            {
                return nVersion;
            }
            set
            {
                nVersion = value;
            }
        }
        TxInList vin;
        TxOutList vout;
        LockTime nLockTime;

        public Transaction()
        {
            vin = new TxInList(this);
            vout = new TxOutList(this);
        }

        public Transaction(string hex, uint version = 70012)//ProtocolVersion version = ProtocolVersion.PROTOCOL_VERSION)
            : this()
        {
            this.FromBytes(Encoders.Hex.DecodeData(hex), version);
        }

        public Transaction(byte[] bytes)
            : this()
        {
            this.FromBytes(bytes);
        }

        public Money TotalOut
        {
            get
            {
                return Outputs.Sum(v => v.Value);
            }
        }

        public LockTime LockTime
        {
            get
            {
                return nLockTime;
            }
            set
            {
                nLockTime = value;
            }
        }

        public TxInList Inputs
        {
            get
            {
                return vin;
            }
        }
        public TxOutList Outputs
        {
            get
            {
                return vout;
            }
        }

        //Since it is impossible to serialize a transaction with 0 input without problems during deserialization with wit activated, we fit a flag in the version to workaround it
        const uint NoDummyInput = (1 << 27);

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            var witSupported = (((uint)stream.TransactionOptions & (uint)TransactionOptions.Witness) != 0) &&
                                false; //stream.ProtocolVersion >= ProtocolVersion.WITNESS_VERSION;

            byte flags = 0;
            if (!stream.Serializing)
            {
                stream.ReadWrite(ref nVersion);
                /* Try to read the vin. In case the dummy is there, this will be read as an empty vector. */
                stream.ReadWrite<TxInList, TxIn>(ref vin);

                var hasNoDummy = (nVersion & NoDummyInput) != 0 && vin.Count == 0;
                if (witSupported && hasNoDummy)
                    nVersion = nVersion & ~NoDummyInput;

                if (vin.Count == 0 && witSupported && !hasNoDummy)
                {
                    /* We read a dummy or an empty vin. */
                    stream.ReadWrite(ref flags);
                    if (flags != 0)
                    {
                        /* Assume we read a dummy and a flag. */
                        stream.ReadWrite<TxInList, TxIn>(ref vin);
                        vin.Transaction = this;
                        stream.ReadWrite<TxOutList, TxOut>(ref vout);
                        vout.Transaction = this;
                    }
                    else
                    {
                        /* Assume read a transaction without output. */
                        vout = new TxOutList();
                        vout.Transaction = this;
                    }
                }
                else
                {
                    /* We read a non-empty vin. Assume a normal vout follows. */
                    stream.ReadWrite<TxOutList, TxOut>(ref vout);
                    vout.Transaction = this;
                }
                if (((flags & 1) != 0) && witSupported)
                {
                    /* The witness flag is present, and we support witnesses. */
                    flags ^= 1;
                    Witness wit = new Witness(Inputs);
                    wit.ReadWrite(stream);
                }
                if (flags != 0)
                {
                    /* Unknown flag in the serialization */
                    throw new FormatException("Unknown transaction optional data");
                }
            }
            else
            {
                var version = (witSupported && (vin.Count == 0 && vout.Count > 0)) ? nVersion | NoDummyInput : nVersion;
                stream.ReadWrite(ref version);

                if (witSupported)
                {
                    /* Check whether witnesses need to be serialized. */
                    if (HasWitness)
                    {
                        flags |= 1;
                    }
                }
                if (flags != 0)
                {
                    /* Use extended format in case witnesses are to be serialized. */
                    TxInList vinDummy = new TxInList();
                    stream.ReadWrite<TxInList, TxIn>(ref vinDummy);
                    stream.ReadWrite(ref flags);
                }
                stream.ReadWrite<TxInList, TxIn>(ref vin);
                vin.Transaction = this;
                stream.ReadWrite<TxOutList, TxOut>(ref vout);
                vout.Transaction = this;
                if ((flags & 1) != 0)
                {
                    Witness wit = new Witness(this.Inputs);
                    wit.ReadWrite(stream);
                }
            }
            stream.ReadWriteStruct(ref nLockTime);
        }

        #endregion

        public uint256 GetHash()
        {
            uint256 h = null;
            var hashes = _Hashes;
            if (hashes != null)
            {
                h = hashes[0];
            }
            if (h != null)
                return h;

            using (HashStream hs = new HashStream())
            {
                this.ReadWrite(new CoinStream(hs, true)
                {
                    TransactionOptions = TransactionOptions.None
                });
                h = hs.GetHash();
            }

            hashes = _Hashes;
            if (hashes != null)
            {
                hashes[0] = h;
            }
            return h;
        }

        /// <summary>
        /// If called, GetHash and GetWitHash become cached, only use if you believe the instance will not be modified after calculation. Calling it a second type invalidate the cache.
        /// </summary>
        public void CacheHashes()
        {
            _Hashes = new uint256[2];
        }

        public Transaction Clone(bool cloneCache)
        {
            var clone = CoinSerializableExtensions.Clone(this, Version);
            if (cloneCache)
                clone._Hashes = _Hashes.ToArray();
            return clone;
        }

        uint256[] _Hashes = null;

        public uint256 GetWitHash()
        {
            if (!HasWitness)
                return GetHash();

            uint256 h = null;
            var hashes = _Hashes;
            if (hashes != null)
            {
                h = hashes[1];
            }
            if (h != null)
                return h;

            using (HashStream hs = new HashStream())
            {
                this.ReadWrite(new CoinStream(hs, true)
                {
                    TransactionOptions = TransactionOptions.Witness
                });
                h = hs.GetHash();
            }

            hashes = _Hashes;
            if (hashes != null)
            {
                hashes[1] = h;
            }
            return h;
        }
        public uint256 GetSignatureHash(ICoin coin, SigHash sigHash = SigHash.All)
        {
            return Inputs.AsIndexedInputs().ToArray()[GetIndex(coin)].GetSignatureHash(coin, sigHash);
        }
        public TransactionSignature SignInput(ISecret secret, ICoin coin, SigHash sigHash = SigHash.All)
        {
            return SignInput(secret.PrivateKey, coin, sigHash);
        }
        public TransactionSignature SignInput(CCKey key, ICoin coin, SigHash sigHash = SigHash.All)
        {
            return Inputs.AsIndexedInputs().ToArray()[GetIndex(coin)].Sign(key, coin, sigHash);
        }

        private int GetIndex(ICoin coin)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].PrevOut == coin.Outpoint)
                    return i;
            }
            throw new ArgumentException("The coin is not being spent by this transaction", "coin");
        }

        public bool IsCoinBase
        {
            get
            {
                return (Inputs.Count == 1 && Inputs[0].PrevOut.IsNull);
            }
        }

        public static uint CURRENT_VERSION = 2;
        public static uint MAX_STANDARD_TX_SIZE = 100000;

        public TxOut AddOutput(Money money, IDestination destination)
        {
            return AddOutput(new TxOut(money, destination));
        }
        public TxOut AddOutput(Money money, Script scriptPubKey)
        {
            return AddOutput(new TxOut(money, scriptPubKey));
        }
        public TxOut AddOutput(TxOut @out)
        {
            this.vout.Add(@out);
            return @out;
        }
        public TxIn AddInput(TxIn @in)
        {
            this.vin.Add(@in);
            return @in;
        }

        internal static readonly int WITNESS_SCALE_FACTOR = 4;
        /// <summary>
        /// Size of the transaction discounting the witness (Used for fee calculation)
        /// </summary>
        /// <returns>Transaction size</returns>
        public int GetVirtualSize()
        {
            var totalSize = this.GetSerializedSize(TransactionOptions.Witness);
            var strippedSize = this.GetSerializedSize(TransactionOptions.None);
            // This implements the weight = (stripped_size * 4) + witness_size formula,
            // using only serialization with and without witness data. As witness_size
            // is equal to total_size - stripped_size, this formula is identical to:
            // weight = (stripped_size * 3) + total_size.
            var weight = strippedSize * (WITNESS_SCALE_FACTOR - 1) + totalSize;
            return (weight + WITNESS_SCALE_FACTOR - 1) / WITNESS_SCALE_FACTOR;
        }

        public TxIn AddInput(Transaction prevTx, int outIndex)
        {
            if (outIndex >= prevTx.Outputs.Count)
                throw new InvalidOperationException("Output " + outIndex + " is not present in the prevTx");
            var @in = new TxIn();
            @in.PrevOut.Hash = prevTx.GetHash();
            @in.PrevOut.N = (uint)outIndex;
            AddInput(@in);
            return @in;
        }


        /// <summary>
        /// Sign a specific coin with the given secret
        /// </summary>
        /// <param name="secrets">Secrets</param>
        /// <param name="coins">Coins to sign</param>
        public void Sign(ISecret[] secrets, params ICoin[] coins)
        {
            Sign(secrets.Select(s => s.PrivateKey).ToArray(), coins);
        }

        /// <summary>
        /// Sign a specific coin with the given secret
        /// </summary>
        /// <param name="key">Private keys</param>
        /// <param name="coins">Coins to sign</param>
        public void Sign(CCKey[] keys, params ICoin[] coins)
        {
            TransactionBuilder builder = new TransactionBuilder();
            builder.AddKeys(keys);
            builder.AddCoins(coins);
            builder.SignTransactionInPlace(this);
        }
        /// <summary>
        /// Sign a specific coin with the given secret
        /// </summary>
        /// <param name="secret">Secret</param>
        /// <param name="coins">Coins to sign</param>
        public void Sign(ISecret secret, params ICoin[] coins)
        {
            Sign(new[] { secret }, coins);
        }

        /// <summary>
        /// Sign a specific coin with the given secret
        /// </summary>
        /// <param name="key">Private key</param>
        /// <param name="coins">Coins to sign</param>
        public void Sign(CCKey key, params ICoin[] coins)
        {
            Sign(new[] { key }, coins);
        }

        /// <summary>
        /// Sign the transaction with a private key
        /// <para>ScriptSigs should be filled with previous ScriptPubKeys</para>
        /// <para>For more complex scenario, use TransactionBuilder</para>
        /// </summary>
        /// <param name="secret"></param>
        public void Sign(ISecret secret, bool assumeP2SH)
        {
            Sign(secret.PrivateKey, assumeP2SH);
        }

        /// <summary>
        /// Sign the transaction with a private key
        /// <para>ScriptSigs should be filled with either previous scriptPubKeys or redeem script (for P2SH)</para>
        /// <para>For more complex scenario, use TransactionBuilder</para>
        /// </summary>
        /// <param name="secret"></param>
        public void Sign(CCKey key, bool assumeP2SH)
        {
            List<Coin> coins = new List<Coin>();
            for (int i = 0; i < Inputs.Count; i++)
            {
                var txin = Inputs[i];
                if (Script.IsNullOrEmpty(txin.ScriptSig))
                    throw new InvalidOperationException("ScriptSigs should be filled with either previous scriptPubKeys or redeem script (for P2SH)");
                if (assumeP2SH)
                {
                    var p2shSig = PayToScriptHashTemplate.Instance.ExtractScriptSigParameters(txin.ScriptSig);
                    if (p2shSig == null)
                    {
                        coins.Add(new ScriptCoin(txin.PrevOut, new TxOut()
                        {
                            ScriptPubKey = txin.ScriptSig.PaymentScript,
                        }, txin.ScriptSig));
                    }
                    else
                    {
                        coins.Add(new ScriptCoin(txin.PrevOut, new TxOut()
                        {
                            ScriptPubKey = p2shSig.RedeemScript.PaymentScript
                        }, p2shSig.RedeemScript));
                    }
                }
                else
                {
                    coins.Add(new Coin(txin.PrevOut, new TxOut()
                    {
                        ScriptPubKey = txin.ScriptSig
                    }));
                }

            }
            Sign(key, coins.ToArray());
        }

        //public TxPayload CreatePayload()
        //{
        //	return new TxPayload(this.Clone());
        //}

#if !NOJSONNET
        public static Transaction Parse(string tx, RawFormat format, Network network = null)
        {
            return GetFormatter(format, network).ParseJson(tx);
        }
#endif

        public static Transaction Parse(string hex)
        {
            return new Transaction(Encoders.Hex.DecodeData(hex));
        }

        public string ToHex()
        {
            return Encoders.Hex.EncodeData(this.ToBytes());
        }
#if !NOJSONNET
        public override string ToString()
        {
            return ToString(RawFormat.BlockExplorer, null);
        }

        public string ToString(RawFormat rawFormat, Network network)
        {
            var formatter = GetFormatter(rawFormat, network);
            return ToString(formatter);
        }

        static private RawFormatter GetFormatter(RawFormat rawFormat, Network network)
        {
            RawFormatter formatter = null;
            switch (rawFormat)
            {
                case RawFormat.Satoshi:
                    formatter = new SatoshiFormatter(network);
                    break;
                case RawFormat.BlockExplorer:
                    formatter = new BlockExplorerFormatter(network);
                    break;
                default:
                    throw new NotSupportedException(rawFormat.ToString());
            }
            formatter.Network = network ?? formatter.Network;
            return formatter;
        }

        internal string ToString(RawFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");
            return formatter.ToString(this);
        }
#endif
        /// <summary>
        /// Calculate the fee of the transaction
        /// </summary>
        /// <param name="spentCoins">Coins being spent</param>
        /// <returns>Fee or null if some spent coins are missing or if spentCoins is null</returns>
        public Money GetFee(ICoin[] spentCoins)
        {
            if (IsCoinBase)
                return Money.Zero;
            spentCoins = spentCoins ?? new ICoin[0];

            Money fees = -TotalOut;
            foreach (var input in this.Inputs)
            {
                var coin = spentCoins.FirstOrDefault(s => s.Outpoint == input.PrevOut);
                if (coin == null)
                    return null;
                fees += coin.TxOut.Value;
            }
            return fees;
        }

        /// <summary>
        /// Calculate the fee rate of the transaction
        /// </summary>
        /// <param name="spentCoins">Coins being spent</param>
        /// <returns>Fee or null if some spent coins are missing or if spentCoins is null</returns>
        public FeeRate GetFeeRate(ICoin[] spentCoins)
        {
            var fee = GetFee(spentCoins);
            if (fee == null)
                return null;
            return new FeeRate(fee, this.GetSerializedSize(this.Version));
        }

        //public bool IsFinal(ChainedBlock block)
        //{
        //	if(block == null)
        //		return IsFinal(Utils.UnixTimeToDateTime(0), 0);
        //	return IsFinal(block.Header.BlockTime, block.Height);
        //}
        //public bool IsFinal(DateTimeOffset blockTime, int blockHeight)
        //{
        //	var nBlockTime = Utils.DateTimeToUnixTime(blockTime);
        //	if(nLockTime == 0)
        //		return true;
        //	if((long)nLockTime < ((long)nLockTime < LockTime.LOCKTIME_THRESHOLD ? (long)blockHeight : nBlockTime))
        //		return true;
        //	foreach(var txin in Inputs)
        //		if(!txin.IsFinal)
        //			return false;
        //	return true;
        //}

        [Flags]
        public enum LockTimeFlags : int
        {
            None = 0,
            /// <summary>
            /// Interpret sequence numbers as relative lock-time constraints.
            /// </summary>
            VerifySequence = (1 << 0),

            /// <summary>
            ///  Use GetMedianTimePast() instead of nTime for end point timestamp.
            /// </summary>
            MedianTimePast = (1 << 1),
        }


        ///// <summary>
        ///// Calculates the block height and time which the transaction must be later than
        ///// in order to be considered final in the context of BIP 68.  It also removes
        ///// from the vector of input heights any entries which did not correspond to sequence
        ///// locked inputs as they do not affect the calculation.
        ///// </summary>		
        ///// <param name="prevHeights">Previous Height</param>
        ///// <param name="block">The block being evaluated</param>
        ///// <param name="flags">If VerifySequence is not set, returns always true SequenceLock</param>
        ///// <returns>Sequence lock of minimum SequenceLock to satisfy</returns>
        //public bool CheckSequenceLocks(int[] prevHeights, ChainedBlock block, LockTimeFlags flags = LockTimeFlags.VerifySequence)
        //{
        //	return CalculateSequenceLocks(prevHeights, block, flags).Evaluate(block);
        //}

        /// <summary>
        /// Calculates the block height and time which the transaction must be later than
        /// in order to be considered final in the context of BIP 68.  It also removes
        /// from the vector of input heights any entries which did not correspond to sequence
        /// locked inputs as they do not affect the calculation.
        /// </summary>		
        /// <param name="prevHeights">Previous Height</param>
        /// <param name="block">The block being evaluated</param>
        /// <param name="flags">If VerifySequence is not set, returns always true SequenceLock</param>
        /// <returns>Sequence lock of minimum SequenceLock to satisfy</returns>
        //public SequenceLock CalculateSequenceLocks(int[] prevHeights, ChainedBlock block, LockTimeFlags flags = LockTimeFlags.VerifySequence)
        //{
        //	if(prevHeights.Length != Inputs.Count)
        //		throw new ArgumentException("The number of element in prevHeights should be equal to the number of inputs", "prevHeights");

        //	// Will be set to the equivalent height- and time-based nLockTime
        //	// values that would be necessary to satisfy all relative lock-
        //	// time constraints given our view of block chain history.
        //	// The semantics of nLockTime are the last invalid height/time, so
        //	// use -1 to have the effect of any height or time being valid.
        //	int nMinHeight = -1;
        //	long nMinTime = -1;

        //	// tx.nVersion is signed integer so requires cast to unsigned otherwise
        //	// we would be doing a signed comparison and half the range of nVersion
        //	// wouldn't support BIP 68.
        //	bool fEnforceBIP68 = Version >= 2
        //					  && (flags & LockTimeFlags.VerifySequence) != 0;

        //	// Do not enforce sequence numbers as a relative lock time
        //	// unless we have been instructed to
        //	if(!fEnforceBIP68)
        //	{
        //		return new SequenceLock(nMinHeight, nMinTime);
        //	}

        //	for(var txinIndex = 0; txinIndex < Inputs.Count; txinIndex++)
        //	{
        //		TxIn txin = Inputs[txinIndex];

        //		// Sequence numbers with the most significant bit set are not
        //		// treated as relative lock-times, nor are they given any
        //		// consensus-enforced meaning at this point.
        //		if((txin.Sequence & Sequence.SEQUENCE_LOCKTIME_DISABLE_FLAG) != 0)
        //		{
        //			// The height of this input is not relevant for sequence locks
        //			prevHeights[txinIndex] = 0;
        //			continue;
        //		}

        //		int nCoinHeight = prevHeights[txinIndex];

        //		if((txin.Sequence & Sequence.SEQUENCE_LOCKTIME_TYPE_FLAG) != 0)
        //		{
        //			long nCoinTime = (long)Utils.DateTimeToUnixTimeLong(block.GetAncestor(Math.Max(nCoinHeight - 1, 0)).GetMedianTimePast());

        //			// Time-based relative lock-times are measured from the
        //			// smallest allowed timestamp of the block containing the
        //			// txout being spent, which is the median time past of the
        //			// block prior.
        //			nMinTime = Math.Max(nMinTime, nCoinTime + (long)((txin.Sequence & Sequence.SEQUENCE_LOCKTIME_MASK) << Sequence.SEQUENCE_LOCKTIME_GRANULARITY) - 1);
        //		}
        //		else
        //		{
        //			// We subtract 1 from relative lock-times because a lock-
        //			// time of 0 has the semantics of "same block," so a lock-
        //			// time of 1 should mean "next block," but nLockTime has
        //			// the semantics of "last invalid block height."
        //			nMinHeight = Math.Max(nMinHeight, nCoinHeight + (int)(txin.Sequence & Sequence.SEQUENCE_LOCKTIME_MASK) - 1);
        //		}
        //	}

        //	return new SequenceLock(nMinHeight, nMinTime);
        //}


        private DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b)
        {
            return a > b ? a : b;
        }

        /// <summary>
        /// Create a transaction with the specified option only. (useful for stripping data from a transaction)
        /// </summary>
        /// <param name="options">Options to keep</param>
        /// <returns>A new transaction with only the options wanted</returns>
        public Transaction WithOptions(TransactionOptions options)
        {
            if (options == TransactionOptions.Witness && HasWitness)
                return this;
            if (options == TransactionOptions.None && !HasWitness)
                return this;
            var instance = new Transaction();
            var ms = new MemoryStream();
            var bms = new CoinStream(ms, true);
            bms.TransactionOptions = options;
            this.ReadWrite(bms);
            ms.Position = 0;
            bms = new CoinStream(ms, false);
            bms.TransactionOptions = options;
            instance.ReadWrite(bms);
            return instance;
        }

        public bool HasWitness
        {
            get
            {
                return false;//Inputs.Any(i => i.WitScript != WitScript.Empty && i.WitScript != null);
            }
        }

        private static readonly uint MAX_BLOCK_SIZE = 1000000;
        private static readonly ulong MAX_MONEY = 21000000ul * Money.COIN;

        /// <summary>
        /// Context free transaction check
        /// </summary>
        /// <returns>The error or success of the check</returns>
        public TransactionCheckResult Check()
        {
            // Basic checks that don't depend on any context
            if (Inputs.Count == 0)
                return TransactionCheckResult.NoInput;
            if (Outputs.Count == 0)
                return TransactionCheckResult.NoOutput;
            // Size limits
            if (this.GetSerializedSize(Version) > MAX_BLOCK_SIZE)
                return TransactionCheckResult.TransactionTooLarge;

            // Check for negative or overflow output values
            long nValueOut = 0;
            foreach (var txout in Outputs)
            {
                if (txout.Value < 0)
                    return TransactionCheckResult.NegativeOutput;
                if (txout.Value > MAX_MONEY)
                    return TransactionCheckResult.OutputTooLarge;
                nValueOut += txout.Value;
                if (!((nValueOut >= 0 && nValueOut <= (long)MAX_MONEY)))
                    return TransactionCheckResult.OutputTotalTooLarge;
            }

            // Check for duplicate inputs
            var vInOutPoints = new HashSet<OutPoint>();
            foreach (var txin in Inputs)
            {
                if (vInOutPoints.Contains(txin.PrevOut))
                    return TransactionCheckResult.DuplicateInputs;
                vInOutPoints.Add(txin.PrevOut);
            }

            if (IsCoinBase)
            {
                if (Inputs[0].ScriptSig.Length < 2 || Inputs[0].ScriptSig.Length > 100)
                    return TransactionCheckResult.CoinbaseScriptTooLarge;
            }
            else
            {
                foreach (var txin in Inputs)
                    if (txin.PrevOut.IsNull)
                        return TransactionCheckResult.NullInputPrevOut;
            }

            return TransactionCheckResult.Success;
        }
    }

    public class FeeRate : IEquatable<FeeRate>, IComparable<FeeRate>
    {
        private readonly Money _FeePerK;
        /// <summary>
        /// Fee per KB
        /// </summary>
        public Money FeePerK
        {
            get
            {
                return _FeePerK;
            }
        }

        readonly static FeeRate _Zero = new FeeRate(Money.Zero);
        public static FeeRate Zero
        {
            get
            {
                return _Zero;
            }
        }

        public FeeRate(Money feePerK)
        {
            if (feePerK == null)
                throw new ArgumentNullException("feePerK");
            if (feePerK.Satoshi < 0)
                throw new ArgumentOutOfRangeException("feePerK");
            _FeePerK = feePerK;
        }

        public FeeRate(Money feePaid, int size)
        {
            if (feePaid == null)
                throw new ArgumentNullException("feePaid");
            if (feePaid.Satoshi < 0)
                throw new ArgumentOutOfRangeException("feePaid");
            if (size > 0)
                _FeePerK = feePaid * 1000 / size;
            else
                _FeePerK = 0;
        }

        /// <summary>
        /// Get fee for the size
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns></returns>
        public Money GetFee(int size)
        {
            Money nFee = _FeePerK.Satoshi * size / 1000;
            if (nFee == 0 && _FeePerK.Satoshi > 0)
                nFee = _FeePerK.Satoshi;
            return nFee;
        }
        public Money GetFee(Transaction tx)
        {
            return GetFee(tx.GetSerializedSize(tx.Version));
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;
            if (((object)this == null) || (obj == null))
                return false;
            var left = this;
            var right = obj as FeeRate;
            if (right == null)
                return false;
            return left._FeePerK == right._FeePerK;
        }

        public override string ToString()
        {
            return String.Format("{0} BTC/kB", _FeePerK.ToString());
        }

        #region IEquatable<FeeRate> Members

        public bool Equals(FeeRate other)
        {
            return other != null && _FeePerK.Equals(other._FeePerK);
        }

        public int CompareTo(FeeRate other)
        {
            return other == null
                ? 1
                : _FeePerK.CompareTo(other._FeePerK);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            var m = obj as FeeRate;
            if (m != null)
                return _FeePerK.CompareTo(m._FeePerK);
#if !(PORTABLE || NETCORE)
            return _FeePerK.CompareTo(obj);
#else
			return _FeePerK.CompareTo((long)obj);
#endif
        }

        #endregion

        public static bool operator <(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left._FeePerK < right._FeePerK;
        }
        public static bool operator >(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left._FeePerK > right._FeePerK;
        }
        public static bool operator <=(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left._FeePerK <= right._FeePerK;
        }
        public static bool operator >=(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left._FeePerK >= right._FeePerK;
        }

        public static bool operator ==(FeeRate left, FeeRate right)
        {
            if (Object.ReferenceEquals(left, right))
                return true;
            if (((object)left == null) || ((object)right == null))
                return false;
            return left._FeePerK == right._FeePerK;
        }

        public static bool operator !=(FeeRate left, FeeRate right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _FeePerK.GetHashCode();
        }

        public static FeeRate Min(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left <= right
                ? left
                : right;
        }

        public static FeeRate Max(FeeRate left, FeeRate right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            return left >= right
                ? left
                : right;
        }
    }

    public enum TransactionCheckResult
    {
        Success,
        NoInput,
        NoOutput,
        NegativeOutput,
        OutputTooLarge,
        OutputTotalTooLarge,
        TransactionTooLarge,
        DuplicateInputs,
        NullInputPrevOut,
        CoinbaseScriptTooLarge,
    }
}
