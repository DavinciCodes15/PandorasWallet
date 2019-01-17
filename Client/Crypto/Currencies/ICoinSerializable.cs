//using Pandora.Client.Crypto.Currencies.Protocol;
using Pandora.Client.Crypto.Currencies;
using System;
using System.IO;

namespace Pandora.Client.Crypto
{
    public interface ICoinSerializable
    {
        void ReadWrite(CoinStream stream);
    }

    public static class CoinSerializableExtensions
    {
        public static void ReadWrite(this ICoinSerializable serializable, Stream stream, bool serializing, uint version = 70012)
        {
            CoinStream s = new CoinStream(stream, serializing)
            {
                ProtocolVersion = version
            };
            serializable.ReadWrite(s);
        }

        public static int GetSerializedSize(this ICoinSerializable serializable, uint version, SerializationType serializationType)
        {
            CoinStream s = new CoinStream(Stream.Null, true)
            {
                Type = serializationType
            };
            s.ReadWrite(serializable);
            return (int)s.Counter.WrittenBytes;
        }

        public static int GetSerializedSize(this ICoinSerializable serializable, TransactionOptions options)
        {
            CoinStream bms = new CoinStream(Stream.Null, true)
            {
                TransactionOptions = options
            };
            serializable.ReadWrite(bms);
            return (int)bms.Counter.WrittenBytes;
        }

        public static int GetSerializedSize(this ICoinSerializable serializable, uint version = 70012)
        {
            return GetSerializedSize(serializable, version, SerializationType.Disk);
        }

        public static void ReadWrite(this ICoinSerializable serializable, byte[] bytes, uint version = 70012)
        {
            ReadWrite(serializable, new MemoryStream(bytes), false, version);
        }

        public static void FromBytes(this ICoinSerializable serializable, byte[] bytes, uint version = 70012)
        {
            serializable.ReadWrite(new CoinStream(bytes)
            {
                ProtocolVersion = version
            });
        }

        public static T Clone<T>(this T serializable, uint version) where T : ICoinSerializable, new()
        {
            T instance = new T();
            instance.FromBytes(serializable.ToBytes(version), version);
            return instance;
        }

        public static byte[] ToBytes(this ICoinSerializable serializable, uint version = 70012)
        {
            MemoryStream ms = new MemoryStream();
            serializable.ReadWrite(new CoinStream(ms, true)
            {
                ProtocolVersion = version,
            });
            return ToArrayEfficient(ms);
        }

        public static byte[] ToArrayEfficient(this MemoryStream ms)
        {
#if !(PORTABLE || NETCORE)
            byte[] bytes = ms.GetBuffer();
            Array.Resize(ref bytes, (int)ms.Length);
            return bytes;
#else
			return ms.ToArray();
#endif
        }
    }
}