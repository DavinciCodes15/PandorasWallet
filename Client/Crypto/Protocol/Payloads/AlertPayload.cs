﻿using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Currencies.Crypto;
using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("alert")]
    public class AlertPayload : Payload, ICoinSerializable
    {
        /// <summary>
        /// Used for knowing if an alert is valid in past of future
        /// </summary>
        public DateTimeOffset? Now
        {
            get;
            set;
        }

        private VarString payload;
        private VarString signature;

        private int version;
        private long relayUntil;
        private long expiration;

        public DateTimeOffset Expiration
        {
            get
            {
                return Utils.UnixTimeToDateTime((uint)expiration);
            }
            set
            {
                expiration = Utils.DateTimeToUnixTime(value);
            }
        }

        private int id;
        private int cancel;
        private int[] setCancel = new int[0];
        private int minVer;
        private int maxVer;
        private VarString[] setSubVer = new VarString[0];
        private int priority;
        private VarString comment;
        private VarString statusBar;
        private VarString reserved;

        public string[] SetSubVer
        {
            get
            {
                List<string> messages = new List<string>();
                foreach (var v in setSubVer)
                {
                    messages.Add(Encoders.ASCII.EncodeData(v.GetString()));
                }
                return messages.ToArray();
            }
            set
            {
                List<VarString> messages = new List<VarString>();
                foreach (var v in value)
                {
                    messages.Add(new VarString(Encoders.ASCII.DecodeData(v)));
                }
                setSubVer = messages.ToArray();
            }
        }

        public string Comment
        {
            get
            {
                return Encoders.ASCII.EncodeData(comment.GetString());
            }
            set
            {
                comment = new VarString(Encoders.ASCII.DecodeData(value));
            }
        }

        public string StatusBar
        {
            get
            {
                return Encoders.ASCII.EncodeData(statusBar.GetString());
            }
            set
            {
                statusBar = new VarString(Encoders.ASCII.DecodeData(value));
            }
        }

        #region ICoinSerializable Members

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref payload);
            if (!stream.Serializing)
            {
                var payloadStream = new CoinStream(payload.GetString());
                payloadStream.CopyParameters(stream);

                ReadWritePayloadFields(payloadStream);
            }

            stream.ReadWrite(ref signature);
        }

        private void ReadWritePayloadFields(CoinStream payloadStream)
        {
            payloadStream.ReadWrite(ref version);
            payloadStream.ReadWrite(ref relayUntil);
            payloadStream.ReadWrite(ref expiration);
            payloadStream.ReadWrite(ref id);
            payloadStream.ReadWrite(ref cancel);
            payloadStream.ReadWrite(ref setCancel);
            payloadStream.ReadWrite(ref minVer);
            payloadStream.ReadWrite(ref maxVer);
            payloadStream.ReadWrite(ref setSubVer);
            payloadStream.ReadWrite(ref priority);
            payloadStream.ReadWrite(ref comment);
            payloadStream.ReadWrite(ref statusBar);
            payloadStream.ReadWrite(ref reserved);
        }

        private void UpdatePayload(CoinStream stream)
        {
            MemoryStream ms = new MemoryStream();
            var seria = new CoinStream(ms, true);
            seria.CopyParameters(stream);
            ReadWritePayloadFields(seria);
            payload = new VarString(ms.ToArray());
        }

        #endregion ICoinSerializable Members

        // FIXME: why do we need version parameter?
        // it shouldn't be called "version" because the it a field with the same name
        //public void UpdateSignature(Key key)
        //{
        //    if (key == null)
        //        throw new ArgumentNullException(nameof(key));
        //    UpdatePayload();
        //    signature = new VarString(key.Sign(Hashes.Hash256(payload.GetString())).ToDER());
        //}

        public void UpdatePayload(uint? protocolVersion = null)
        {
            UpdatePayload(new CoinStream(new byte[0])
            {
                ProtocolVersion = protocolVersion
            });
        }

        //public bool CheckSignature(Network network)
        //{
        //    if (network == null)
        //        throw new ArgumentNullException(nameof(network));
        //    return CheckSignature(network.AlertPubKey);
        //}

        public bool CheckSignature(PubKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return key.Verify(Hashes.Hash256(payload.GetString()), signature.GetString());
        }

        public bool IsInEffect
        {
            get
            {
                DateTimeOffset now = Now ?? DateTimeOffset.Now;
                return now < Expiration;
            }
        }

        public bool AppliesTo(int nVersion, string strSubVerIn)
        {
            return IsInEffect
                    && minVer <= nVersion && nVersion <= maxVer
                    && (SetSubVer.Length == 0 || SetSubVer.Contains(strSubVerIn));
        }

        public override string ToString()
        {
            return StatusBar;
        }
    }
}