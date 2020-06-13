using Pandora.Client.Crypto.Protocol;
using System;
using System.IO;
using System.Threading;

namespace Pandora.Client.Crypto.Currencies
{
    [Flags]
    public enum ProtocolFlags
    {
        None = 0, UsesInvPayloadforGetHeader = 1, FailsToVerAck = 2, UseRPC = 4, UsesOldVersionPayload = 8, SegwitSupport = 16, EthereumProtocol = 32
    }

    public class ProtocolData
    {
        public long ParamsVersion { get; private set; }
        public uint MaxP2PVersion { get; private set; }

        public int DefaultPort { get; private set; }

        public uint Magic { get; private set; }

        public IConsensusFactory Consensus { get; private set; }
        public string DNSSeed { get; private set; }

        private ProtocolFlags FPeculiarities;

        private byte[] _MagicBytes;

        public byte[] MagicBytes
        {
            get
            {
                if (_MagicBytes == null)
                {
                    byte[] bytes = new byte[]
                    {
                        (byte)Magic,
                        (byte)(Magic >> 8),
                        (byte)(Magic >> 16),
                        (byte)(Magic >> 24)
                    };
                    _MagicBytes = bytes;
                }
                return _MagicBytes;
            }
        }

        public ProtocolData(long aParamsVersion, uint aMaxP2PVersion, int aDefaultPort, uint aMagic, IConsensusFactory aConsensus, ProtocolFlags aPeculiarities = ProtocolFlags.None)
        {
            ParamsVersion = aParamsVersion;
            MaxP2PVersion = aMaxP2PVersion;
            DefaultPort = aDefaultPort;
            Magic = aMagic;
            Consensus = aConsensus;
            FPeculiarities = aPeculiarities;
        }

        public bool Checkif(ProtocolFlags aPeculiarity)
        {
            return (FPeculiarities & aPeculiarity) == aPeculiarity;
        }

        public virtual ProtocolCapabilities GetProtocolCapabilities(uint protocolVersion)
        {
            return new ProtocolCapabilities()
            {
                PeerTooOld = false,
                SupportCheckSum = true,
                SupportCompactBlocks = true,
                SupportGetBlock = true,
                SupportMempoolQuery = true,
                SupportNodeBloom = true,
                SupportPingPong = true,
                SupportReject = true,
                SupportSendHeaders = true,
                SupportTimeAddress = true,
                SupportUserAgent = true,
                SupportWitness = FPeculiarities.HasFlag(ProtocolFlags.SegwitSupport),
                SupportVersionRelay = !Checkif(ProtocolFlags.UsesOldVersionPayload)

                //PeerTooOld = protocolVersion < 209U,
                //SupportTimeAddress = protocolVersion >= 31402U,
                //SupportGetBlock = protocolVersion < 32000U || protocolVersion > 32400U,
                //SupportPingPong = protocolVersion > 60000U,
                //SupportMempoolQuery = protocolVersion >= 60002U,
                //SupportReject = protocolVersion >= 70002U,
                //SupportNodeBloom = protocolVersion >= 70011U,
                //SupportSendHeaders = protocolVersion >= 70012U,
                //SupportWitness = protocolVersion >= 70012U,
                //SupportCompactBlocks = protocolVersion >= 70014U,
                //SupportCheckSum = protocolVersion >= 60002,
                //SupportUserAgent = protocolVersion >= 60002,
                //SupportVersionRelay = protocolVersion >= 70001U && !Checkif(ProtocolFlags.UsesOldVersionPayload)
            };
        }

        public bool ReadMagic(Stream stream, CancellationToken cancellation, bool throwIfEOF = false)
        {
            byte[] bytes = new byte[1];
            for (int i = 0; i < MagicBytes.Length; i++)
            {
#if DEBUG
                string hex = BitConverter.ToString(MagicBytes);
#endif

                i = Math.Max(0, i);
                cancellation.ThrowIfCancellationRequested();

                int read = stream.ReadEx(bytes, 0, bytes.Length, cancellation);
                if (read == 0)
                {
                    if (throwIfEOF)
                    {
                        throw new EndOfStreamException("No more bytes to read");
                    }
                    else
                    {
                        return false;
                    }
                }

                if (read != 1)
                {
                    i--;
                }
                else if (_MagicBytes[i] != bytes[0])
                {
                    i = _MagicBytes[0] == bytes[0] ? 0 : -1;
                }
            }
            return true;
        }
    }
}