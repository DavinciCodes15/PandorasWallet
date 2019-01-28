using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Buffers;

#if !NOSOCKET

using System.Net.Sockets;

#endif

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Protocol.Payloads;
using Pandora.Client.Crypto.Currencies.Controls;

namespace Pandora.Client.Crypto.Protocol
{
    public class Message : ICoinSerializable
    {
        private uint magic;

        public uint Magic
        {
            get
            {
                return magic;
            }
            set
            {
                magic = value;
            }
        }

        private byte[] command = new byte[12];

        public string Command
        {
            get
            {
                return Encoders.ASCII.EncodeData(command);
            }
            private set
            {
                command = Encoders.ASCII.DecodeData(value.Trim().PadRight(12, '\0'));
            }
        }

        private Payload _PayloadObject;

        public Payload Payload
        {
            get
            {
                return _PayloadObject;
            }
            set
            {
                _PayloadObject = value;
                Command = _PayloadObject.Command;
            }
        }

        public bool IfPayloadIs<TPayload>(Action<TPayload> action) where TPayload : Payload
        {
            var payload = Payload as TPayload;
            if (payload != null)
                action(payload);
            return payload != null;
        }

        #region ICoinSerializable Members

        // We use this for big blocks, because the default array pool would allocate a new array. We do not need lot's of bucket such arrays are short lived.
        private static readonly Lazy<ArrayPool<byte>> BigArrayPool = new Lazy<ArrayPool<byte>>(() => ArrayPool<byte>.Create(0x02000000, 5), false);

        private ArrayPool<byte> GetArrayPool(int size) => size < 1_048_576 ? ArrayPool<byte>.Shared : BigArrayPool.Value;

        public void ReadWrite(CoinStream stream)
        {
            if (Payload == null && stream.Serializing)
                throw new InvalidOperationException("Payload not affected");
            if (stream.Serializing || (!stream.Serializing && !_SkipMagic))
                stream.ReadWrite(ref magic);

            stream.ReadWrite(ref command);

            if (stream.Serializing)
            {
                // We can optimize by calculating the length at the same time we calculate the checksum
                if (stream.ProtocolCapabilities.SupportCheckSum)
                {
                    var hashStream = stream.ProtocolCapabilities.GetChecksumHashStream();
                    var bsStream = new CoinStream(hashStream, true);
                    bsStream.CopyParameters(stream);
                    Payload.ReadWrite(bsStream);
                    var length = (int)bsStream.Counter.WrittenBytes;
                    var checksum = hashStream.GetHash().GetLow32();
                    stream.ReadWrite(ref length);
                    stream.ReadWrite(ref checksum);
                }
                else
                {
                    var CoinStream = new CoinStream(Stream.Null, true);
                    CoinStream.CopyParameters(stream);
                    Payload.ReadWrite(CoinStream);
                    var length = (int)CoinStream.Counter.WrittenBytes;
                    stream.ReadWrite(ref length);
                }
                stream.ReadWrite(Payload);
            }
            else
            {
                int length = 0;
                stream.ReadWrite(ref length);
                if (length < 0 || length > 0x02000000) //MAX_SIZE 0x02000000 Serialize.h
                {
                    throw new FormatException("Message payload too big ( > 0x02000000 bytes)");
                }

                var arrayPool = GetArrayPool(length);
                var payloadBytes = arrayPool.Rent(length);
                try
                {
                    uint expectedChecksum = 0;
                    if (stream.ProtocolCapabilities.SupportCheckSum)
                        stream.ReadWrite(ref expectedChecksum);

                    stream.ReadWrite(ref payloadBytes, 0, length);

                    //  We do not verify the checksum anymore because for 1000 blocks, it takes 80 seconds.

                    CoinStream payloadStream = new CoinStream(new MemoryStream(payloadBytes, 0, length, false), false);
                    payloadStream.CopyParameters(stream);

                    var payloadType = PayloadAttribute.GetCommandType(Command);
                    var unknown = payloadType == typeof(UnknowPayload);
                    //if (unknown)
                    //    NodeServerTrace.Trace.TraceEvent(TraceEventType.Warning, 0, "Unknown command received : " + Command);
                    ICoinSerializable payload = null;
                    if (!stream.ConsensusFactory.TryCreateNew(payloadType, out payload))
                        payload = (ICoinSerializable)Activator.CreateInstance(payloadType);
                    payload.ReadWrite(payloadStream);
                    if (unknown)
                        ((UnknowPayload)payload)._Command = Command;
                    Payload = (Payload)payload;
                }
                finally
                {
                    arrayPool.Return(payloadBytes);
                }
            }
        }

        #endregion ICoinSerializable Members

        /// <summary>
        /// When parsing, maybe Magic is already parsed
        /// </summary>
        private bool _SkipMagic;

        public override string ToString()
        {
            return String.Format("{0} : {1}", Command, Payload);
        }

#if !NOSOCKET

        public static Message ReadNext(Socket socket, ProtocolData network, uint version, CancellationToken cancellationToken)
        {
            Currencies.PerformanceCounter counter;
            return ReadNext(socket, network, version, cancellationToken, out counter);
        }

        public static Message ReadNext(Socket socket, ProtocolData network, uint version, CancellationToken cancellationToken, out Currencies.PerformanceCounter counter)
        {
            return ReadNext(socket, network, version, cancellationToken, out counter);
        }

        [Obsolete("The buffer parameter is now ignored")]
        public static Message ReadNext(Socket socket, ProtocolData network, uint version, CancellationToken cancellationToken, byte[] buffer, out Currencies.PerformanceCounter counter)
        {
            var stream = new NetworkStream(socket, false);
            return ReadNext(stream, network, version, cancellationToken, out counter);
        }

#endif

        public static Message ReadNext(Stream stream, ProtocolData network, uint version, CancellationToken cancellationToken)
        {
            Currencies.PerformanceCounter counter;
            return ReadNext(stream, network, version, cancellationToken, out counter);
        }

        public static Message ReadNext(Stream stream, ProtocolData network, uint version, CancellationToken cancellationToken, out Currencies.PerformanceCounter counter)
        {
            CoinStream bitStream = new CoinStream(stream, false)
            {
                ProtocolVersion = version,
                ReadCancellationToken = cancellationToken,
                ConsensusFactory = network.Consensus,
                FProtocolData = network
            };

            if (!network.ReadMagic(stream, cancellationToken, true))
                throw new FormatException("Magic incorrect, the message comes from another network");

            Message message = new Message();
            using (message.SkipMagicScope(true))

            {
                message.Magic = network.Magic;
                message.ReadWrite(bitStream);
            }

            counter = bitStream.Counter;
            return message;
        }

        [Obsolete("The buffer parameter is now ignored")]
        public static Message ReadNext(Stream stream, ProtocolData network, uint version, CancellationToken cancellationToken, byte[] buffer, out Currencies.PerformanceCounter counter)
        {
            return ReadNext(stream, network, version, cancellationToken, out counter);
        }

        private IDisposable SkipMagicScope(bool value)
        {
            var old = _SkipMagic;
            return new Scope(() => _SkipMagic = value, () => _SkipMagic = old);
        }
    }
}