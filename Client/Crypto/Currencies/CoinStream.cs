using Pandora.Client.Crypto.Currencies.Controls;
using Pandora.Client.Crypto.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pandora.Client.Crypto.Currencies
{
    public enum SerializationType
    {
        Disk,
        Network,
        Hash
    }

    public class Scope : IDisposable
    {
        private Action close;

        public Scope(Action open, Action close)
        {
            this.close = close;
            open();
        }

        #region IDisposable Members

        public void Dispose()
        {
            close();
        }

        #endregion IDisposable Members

        public static IDisposable Nothing => new Scope(() =>
                                                           {
                                                           }, () =>
                                                           {
                                                           });
    }

    public class CoinStream
    {
        private int _MaxArraySize = 1024 * 1024;

        public int MaxArraySize
        {
            get => _MaxArraySize;
            set => _MaxArraySize = value;
        }

        //ReadWrite<T>(ref T data)
        private static MethodInfo _ReadWriteTyped;

        static CoinStream()
        {
            _ReadWriteTyped = typeof(CoinStream)
            .GetTypeInfo()
            .DeclaredMethods
            .Where(m => m.Name == "ReadWrite")
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters().Any(p => p.ParameterType.IsByRef && p.ParameterType.HasElementType && !p.ParameterType.GetElementType().IsArray))
            .First();
        }

        private readonly Stream _Inner;

        public Stream Inner => _Inner;

        private readonly bool _Serializing;

        public bool Serializing => _Serializing;

        private IConsensusFactory _ConsensusFactory = new BaseConsensusFactory();

        /// <summary>
        /// Set the format to use when serializing and deserializing consensus related types.
        /// </summary>
        public IConsensusFactory ConsensusFactory
        {
            get => _ConsensusFactory;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _ConsensusFactory = value;
            }
        }

        public CoinStream(Stream inner, bool serializing)
        {
            _Serializing = serializing;
            _Inner = inner;
        }

        public CoinStream(byte[] bytes)
            : this(new MemoryStream(bytes), false)
        {
        }

        public Script ReadWrite(Script data)
        {
            if (Serializing)
            {
                byte[] bytes = data == null ? Script.Empty.ToBytes(true) : data.ToBytes(true);
                ReadWriteAsVarString(ref bytes);
                return data;
            }
            else
            {
                VarString varString = new VarString();
                varString.ReadWrite(this);
                return Script.FromBytesUnsafe(varString.GetString(true));
            }
        }

        public void ReadWrite(ref Script script)
        {
            if (Serializing)
            {
                ReadWrite(script);
            }
            else
            {
                script = ReadWrite(script);
            }
        }

        public T ReadWrite<T>(T data) where T : ICoinSerializable
        {
            ReadWrite<T>(ref data);
            return data;
        }

        public void ReadWriteAsVarString(ref byte[] bytes)
        {
            if (Serializing)
            {
                VarString str = new VarString(bytes);
                str.ReadWrite(this);
            }
            else
            {
                VarString str = new VarString();
                str.ReadWrite(this);
                bytes = str.GetString(true);
            }
        }

        public void ReadWrite(Type type, ref object obj)
        {
            try
            {
                object[] parameters = new object[] { obj };
                _ReadWriteTyped.MakeGenericMethod(type).Invoke(this, parameters);
                obj = parameters[0];
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public void ReadWrite(ref byte data)
        {
            ReadWriteByte(ref data);
        }

        public byte ReadWrite(byte data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref bool data)
        {
            byte d = data ? (byte)1 : (byte)0;
            ReadWriteByte(ref d);
            data = (d == 0 ? false : true);
        }

        public void ReadWriteStruct<T>(ref T data) where T : struct, ICoinSerializable
        {
            data.ReadWrite(this);
        }

        public void ReadWriteStruct<T>(T data) where T : struct, ICoinSerializable
        {
            data.ReadWrite(this);
        }

        public void ReadWrite<T>(ref T data) where T : ICoinSerializable
        {
            T obj = data;
            if (obj == null)
            {
                if (!ConsensusFactory.TryCreateNew<T>(out obj))
                {
                    obj = Activator.CreateInstance<T>();
                }
            }

            obj.ReadWrite(this);
            if (!Serializing)
            {
                data = obj;
            }
        }

        public void ReadWrite<T>(ref List<T> list) where T : ICoinSerializable, new()
        {
            ReadWriteList<List<T>, T>(ref list);
        }

        public void ReadWrite<TList, TItem>(ref TList list)
            where TList : List<TItem>, new()
            where TItem : ICoinSerializable, new()
        {
            ReadWriteList<TList, TItem>(ref list);
        }

        private void ReadWriteList<TList, TItem>(ref TList data)
            where TList : List<TItem>, new()
            where TItem : ICoinSerializable, new()
        {
            TItem[] dataArray = data == null ? null : data.ToArray();
            if (Serializing && dataArray == null)
            {
                dataArray = new TItem[0];
            }
            ReadWriteArray(ref dataArray);
            if (!Serializing)
            {
                if (data == null)
                {
                    data = new TList();
                }
                else
                {
                    data.Clear();
                }

                data.AddRange(dataArray);
            }
        }

        public void ReadWrite(ref byte[] arr)
        {
            ReadWriteBytes(ref arr);
        }

        public void ReadWrite(ref byte[] arr, int offset, int count)
        {
            ReadWriteBytes(ref arr, offset, count);
        }

        public void ReadWrite<T>(ref T[] arr) where T : ICoinSerializable, new()
        {
            ReadWriteArray<T>(ref arr);
        }

        private void ReadWriteNumber(ref long value, int size)
        {
            ulong uvalue = unchecked((ulong)value);
            ReadWriteNumber(ref uvalue, size);
            value = unchecked((long)uvalue);
        }

        private void ReadWriteNumber(ref ulong value, int size)
        {
            byte[] bytes = new byte[size];

            for (int i = 0; i < size; i++)
            {
                bytes[i] = (byte)(value >> i * 8);
            }
            if (IsBigEndian)
            {
                Array.Reverse(bytes);
            }

            ReadWriteBytes(ref bytes);
            if (IsBigEndian)
            {
                Array.Reverse(bytes);
            }

            ulong valueTemp = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                ulong v = bytes[i];
                valueTemp += v << (i * 8);
            }
            value = valueTemp;
        }

        private void ReadWriteBytes(ref byte[] data, int offset = 0, int count = -1)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                return;
            }

            count = count == -1 ? data.Length : count;

            if (count == 0)
            {
                return;
            }

            if (Serializing)
            {
                Inner.Write(data, offset, count);
                Counter.AddWritten(count);
            }
            else
            {
                int readen = Inner.ReadEx(data, offset, count, ReadCancellationToken);
                if (readen == 0)
                {
                    throw new EndOfStreamException("No more byte to read");
                }

                Counter.AddReaden(readen);
            }
        }

        private PerformanceCounter _Counter;

        public PerformanceCounter Counter
        {
            get
            {
                if (_Counter == null)
                {
                    _Counter = new PerformanceCounter();
                }

                return _Counter;
            }
        }

        private void ReadWriteByte(ref byte data)
        {
            if (Serializing)
            {
                Inner.WriteByte(data);
                Counter.AddWritten(1);
            }
            else
            {
                int readen = Inner.ReadByte();
                if (readen == -1)
                {
                    throw new EndOfStreamException("No more byte to read");
                }

                data = (byte)readen;
                Counter.AddReaden(1);
            }
        }

        public bool IsBigEndian
        {
            get;
            set;
        }

        public IDisposable BigEndianScope()
        {
            bool old = IsBigEndian;
            return new Scope(() =>
            {
                IsBigEndian = true;
            },
            () =>
            {
                IsBigEndian = old;
            });
        }

        private ProtocolCapabilities _ProtocolCapabilities;

        public ProtocolCapabilities ProtocolCapabilities
        {
            get
            {
                try
                {
                    ProtocolCapabilities capabilities = _ProtocolCapabilities;
                    if (capabilities == null)
                    {
                        capabilities = ProtocolVersion.HasValue ? FProtocolData.GetProtocolCapabilities(ProtocolVersion.Value) : ProtocolCapabilities.CreateSupportAll();
                        _ProtocolCapabilities = capabilities;
                    }
                    return capabilities;
                }
                catch
                {
                    throw;
                }
            }
        }

        private uint? _ProtocolVersion = null;

        public uint? ProtocolVersion
        {
            get => _ProtocolVersion;
            set
            {
                _ProtocolVersion = value;
                _ProtocolCapabilities = null;
            }
        }

        private TransactionOptions _TransactionSupportedOptions = TransactionOptions.All;

        public TransactionOptions TransactionOptions
        {
            get => _TransactionSupportedOptions;
            set => _TransactionSupportedOptions = value;
        }

        public IDisposable ProtocolVersionScope(uint version)
        {
            uint? old = ProtocolVersion;
            return new Scope(() =>
            {
                ProtocolVersion = version;
            },
            () =>
            {
                ProtocolVersion = old;
            });
        }

        public void CopyParameters(CoinStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            ProtocolVersion = stream.ProtocolVersion;
            IsBigEndian = stream.IsBigEndian;
            MaxArraySize = stream.MaxArraySize;
            Type = stream.Type;
            ConsensusFactory = stream.ConsensusFactory;
            FProtocolData = stream.FProtocolData;
        }

        public SerializationType Type
        {
            get;
            set;
        }

        public IDisposable SerializationTypeScope(SerializationType value)
        {
            SerializationType old = Type;
            return new Scope(() =>
            {
                Type = value;
            }, () =>
            {
                Type = old;
            });
        }

        public System.Threading.CancellationToken ReadCancellationToken
        {
            get;
            set;
        }

        public ProtocolData FProtocolData { get; set; }

        public void ReadWriteAsVarInt(ref uint val)
        {
            ulong vallong = val;
            ReadWriteAsVarInt(ref vallong);
            if (!Serializing)
            {
                val = (uint)vallong;
            }
        }

        public void ReadWriteAsVarInt(ref ulong val)
        {
            VarInt value = new VarInt(val);
            ReadWrite(ref value);
            if (!Serializing)
            {
                val = value.ToLong();
            }
        }

        public void ReadWriteAsCompactVarInt(ref uint val)
        {
            CompactVarInt value = new CompactVarInt(val, sizeof(uint));
            ReadWrite(ref value);
            if (!Serializing)
            {
                val = (uint)value.ToLong();
            }
        }

        public void ReadWriteAsCompactVarInt(ref ulong val)
        {
            CompactVarInt value = new CompactVarInt(val, sizeof(ulong));
            ReadWrite(ref value);
            if (!Serializing)
            {
                val = value.ToLong();
            }
        }

        private VarInt _VarInt = new VarInt(0);

        private void ReadWriteArray<T>(ref T[] data) where T : ICoinSerializable
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new T[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                T obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref ulong[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new ulong[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                ulong obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref ushort[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new ushort[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                ushort obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref uint[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new uint[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                uint obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref byte[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new byte[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                byte obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref long[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new long[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                long obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref short[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new short[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                short obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        private void ReadWriteArray(ref int[] data)
        {
            if (data == null && Serializing)
            {
                throw new ArgumentNullException("Impossible to serialize a null array");
            }

            _VarInt.SetValue(data == null ? 0 : (ulong)data.Length);
            ReadWrite(ref _VarInt);

            if (_VarInt.ToLong() > (uint)MaxArraySize)
            {
                throw new ArgumentOutOfRangeException("Array size not big");
            }

            if (!Serializing)
            {
                data = new int[_VarInt.ToLong()];
            }

            for (int i = 0; i < data.Length; i++)
            {
                int obj = data[i];
                ReadWrite(ref obj);
                data[i] = obj;
            }
        }

        public void ReadWrite(ref ulong[] data)
        {
            ReadWriteArray(ref data);
        }

        public void ReadWrite(ref ushort[] data)
        {
            ReadWriteArray(ref data);
        }

        public void ReadWrite(ref uint[] data)
        {
            ReadWriteArray(ref data);
        }

        public void ReadWrite(ref long[] data)
        {
            ReadWriteArray(ref data);
        }

        public void ReadWrite(ref short[] data)
        {
            ReadWriteArray(ref data);
        }

        public void ReadWrite(ref int[] data)
        {
            ReadWriteArray(ref data);
        }

        private uint256.MutableUint256 _MutableUint256 = new uint256.MutableUint256(uint256.Zero);

        public void ReadWrite(ref uint256 value)
        {
            value = value ?? uint256.Zero;
            _MutableUint256.Value = value;
            ReadWrite(ref _MutableUint256);
            value = _MutableUint256.Value;
        }

        public void ReadWrite(uint256 value)
        {
            value = value ?? uint256.Zero;
            _MutableUint256.Value = value;
            ReadWrite(ref _MutableUint256);
            value = _MutableUint256.Value;
        }

        public void ReadWrite(ref List<uint256> value)
        {
            if (Serializing)
            {
                List<uint256.MutableUint256> list = value == null ? null : value.Select(v => v.AsCoinSerializable()).ToList();
                ReadWrite(ref list);
            }
            else
            {
                List<uint256.MutableUint256> list = null;
                ReadWrite(ref list);
                value = list.Select(l => l.Value).ToList();
            }
        }

        private uint160.MutableUint160 _MutableUint160 = new uint160.MutableUint160(uint160.Zero);

        public void ReadWrite(ref uint160 value)
        {
            value = value ?? uint160.Zero;
            _MutableUint160.Value = value;
            ReadWrite(ref _MutableUint160);
            value = _MutableUint160.Value;
        }

        public void ReadWrite(uint160 value)
        {
            value = value ?? uint160.Zero;
            _MutableUint160.Value = value;
            ReadWrite(ref _MutableUint160);
            value = _MutableUint160.Value;
        }

        public void ReadWrite(ref List<uint160> value)
        {
            if (Serializing)
            {
                List<uint160.MutableUint160> list = value == null ? null : value.Select(v => v.AsCoinSerializable()).ToList();
                ReadWrite(ref list);
            }
            else
            {
                List<uint160.MutableUint160> list = null;
                ReadWrite(ref list);
                value = list.Select(l => l.Value).ToList();
            }
        }

        public void ReadWrite(ref ulong data)
        {
            ulong l = data;
            ReadWriteNumber(ref l, sizeof(ulong));
            if (!Serializing)
            {
                data = l;
            }
        }

        public ulong ReadWrite(ulong data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref ushort data)
        {
            ulong l = data;
            ReadWriteNumber(ref l, sizeof(ushort));
            if (!Serializing)
            {
                data = (ushort)l;
            }
        }

        public ushort ReadWrite(ushort data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref uint data)
        {
            ulong l = data;
            ReadWriteNumber(ref l, sizeof(uint));
            if (!Serializing)
            {
                data = (uint)l;
            }
        }

        public uint ReadWrite(uint data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref long data)
        {
            long l = data;
            ReadWriteNumber(ref l, sizeof(long));
            if (!Serializing)
            {
                data = l;
            }
        }

        public long ReadWrite(long data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref short data)
        {
            long l = data;
            ReadWriteNumber(ref l, sizeof(short));
            if (!Serializing)
            {
                data = (short)l;
            }
        }

        public short ReadWrite(short data)
        {
            ReadWrite(ref data);
            return data;
        }

        public void ReadWrite(ref int data)
        {
            long l = data;
            ReadWriteNumber(ref l, sizeof(int));
            if (!Serializing)
            {
                data = (int)l;
            }
        }

        public int ReadWrite(int data)
        {
            ReadWrite(ref data);
            return data;
        }
    }
}