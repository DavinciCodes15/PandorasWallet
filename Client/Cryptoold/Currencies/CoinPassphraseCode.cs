using Pandora.Client.Crypto.Currencies.Crypto;
using Pandora.Client.Crypto.Currencies.DataEncoders;
using Pandora.Client.Crypto.Currencies.BouncyCastle.Math;
using Pandora.Client.Crypto.Currencies.BouncyCastle.Math.EC;
using System;
using System.Linq;
using System.Text;


namespace Pandora.Client.Crypto.Currencies
{
    public class EncryptedKeyResult
	{
		public EncryptedKeyResult(CoinEncryptedSecretEC key, CoinAddress address, byte[] seed, Func<CoinConfirmationCode> calculateConfirmation)
		{
			_EncryptedKey = key;
			_GeneratedAddress = address;
			_CalculateConfirmation = calculateConfirmation;
			_Seed = seed;
		}

		private readonly CoinEncryptedSecretEC _EncryptedKey;
		public CoinEncryptedSecretEC EncryptedKey
		{
			get
			{
				return _EncryptedKey;
			}
		}

		Func<CoinConfirmationCode> _CalculateConfirmation;
		private CoinConfirmationCode _ConfirmationCode;
		public CoinConfirmationCode ConfirmationCode
		{
			get
			{
				if(_ConfirmationCode == null)
				{
					_ConfirmationCode = _CalculateConfirmation();
					_CalculateConfirmation = null;
				}
				return _ConfirmationCode;
			}
		}
		private readonly CoinAddress _GeneratedAddress;
		public CoinAddress GeneratedAddress
		{
			get
			{
				return _GeneratedAddress;
			}
		}

		private readonly byte[] _Seed;
		public byte[] Seed
		{
			get
			{
				return _Seed;
			}
		}
	}

    public class LotSequence
	{
		public LotSequence(int lot, int sequence)
		{
			if(lot > 1048575 || lot < 0)
				throw new ArgumentOutOfRangeException("lot");
			if(sequence > 1024 || sequence < 0)
				throw new ArgumentOutOfRangeException("sequence");

			_Lot = lot;
			_Sequence = sequence;
			uint lotSequence = (uint)lot * 4096 + (uint)sequence;
			_Bytes =
				new[]
					{
						(byte)(lotSequence >> 24),
						(byte)(lotSequence >> 16),
						(byte)(lotSequence >> 8),
						(byte)(lotSequence)
					};
		}
		public LotSequence(byte[] bytes)
		{
			_Bytes = bytes.ToArray();
			uint lotSequence =
				((uint)_Bytes[0] << 24) +
				((uint)_Bytes[1] << 16) +
				((uint)_Bytes[2] << 8) +
				((uint)_Bytes[3] << 0);

			_Lot = (int)(lotSequence / 4096);
			_Sequence = (int)(lotSequence - _Lot);
		}

		private readonly int _Lot;
		public int Lot
		{
			get
			{
				return _Lot;
			}
		}
		private readonly int _Sequence;
		public int Sequence
		{
			get
			{
				return _Sequence;
			}
		}

		readonly byte[] _Bytes;
		public byte[] ToBytes()
		{
			return _Bytes.ToArray();
		}

		private int Id
		{
			get
			{
				return Utils.ToInt32(_Bytes, 0, true);
			}
		}

		public override bool Equals(object obj)
		{
			LotSequence item = obj as LotSequence;
			return item != null && Id.Equals(item.Id);
		}
		public static bool operator ==(LotSequence a, LotSequence b)
		{
			if(ReferenceEquals(a, b))
				return true;
			if(((object)a == null) || ((object)b == null))
				return false;
			return a.Id == b.Id;
		}

		public static bool operator !=(LotSequence a, LotSequence b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

    public class CoinPassphraseCode : Base58Data
	{

		public CoinPassphraseCode(string passphrase, Network network, LotSequence lotsequence, byte[] ownersalt = null)
			: base(GenerateWif(passphrase, network, lotsequence, ownersalt), network)
		{
		}
		private static string GenerateWif(string passphrase, Network network, LotSequence lotsequence, byte[] ownersalt)
		{
			bool hasLotSequence = lotsequence != null;

			//ownersalt is 8 random bytes
			ownersalt = ownersalt ?? RandomUtils.GetBytes(8);
			var ownerEntropy = ownersalt;

			if(hasLotSequence)
			{
				ownersalt = ownersalt.Take(4).ToArray();
				ownerEntropy = ownersalt.Concat(lotsequence.ToBytes()).ToArray();
			}


			var prefactor = SCrypt.CoinComputeDerivedKey(Encoding.UTF8.GetBytes(passphrase), ownersalt, 32);
			var passfactor = prefactor;
			if(hasLotSequence)
			{
				passfactor = Hashes.Hash256(prefactor.Concat(ownerEntropy).ToArray()).ToBytes();
			}

			var passpoint = new CCKey(passfactor, fCompressedIn: true).PubKey.ToBytes();

			var bytes =
				network.GetVersionBytes(Network.BASE58_PASSPHRASE_CODE, true)
				.Concat(new[] { hasLotSequence ? (byte)0x51 : (byte)0x53 })
				.Concat(ownerEntropy)
				.Concat(passpoint)
				.ToArray();
			return Encoders.Base58Check.EncodeData(bytes);
		}

		public CoinPassphraseCode(string wif, Network expectedNetwork = null)
			: base(wif, expectedNetwork)
		{
		}

		LotSequence _LotSequence;
		public LotSequence LotSequence
		{
			get
			{
				var hasLotSequence = (vchData[0]) == 0x51;
				if(!hasLotSequence)
					return null;
				return _LotSequence ?? (_LotSequence = new LotSequence(OwnerEntropy.Skip(4).Take(4).ToArray()));
			}
		}

		public EncryptedKeyResult GenerateEncryptedSecret(bool isCompressed = true, byte[] seedb = null)
		{
			//Set flagbyte.
			byte flagByte = 0;
			//Turn on bit 0x20 if the Coin address will be formed by hashing the compressed public key
			flagByte |= isCompressed ? (byte)0x20 : (byte)0x00;
			flagByte |= LotSequence != null ? (byte)0x04 : (byte)0x00;

			//Generate 24 random bytes, call this seedb. Take SHA256(SHA256(seedb)) to yield 32 bytes, call this factorb.
			seedb = seedb ?? RandomUtils.GetBytes(24);

			var factorb = Hashes.Hash256(seedb).ToBytes();

			//ECMultiply passpoint by factorb.
			var curve = ECKey.Secp256k1;
			var passpoint = curve.Curve.DecodePoint(Passpoint);
			var pubPoint = passpoint.Multiply(new BigInteger(1, factorb));

			//Use the resulting EC point as a public key
			var pubKey = new PubKey(pubPoint.GetEncoded());

			//and hash it into a Coin address using either compressed or uncompressed public key
			//This is the generated Coin address, call it generatedaddress.
			pubKey = isCompressed ? pubKey.Compress() : pubKey.Decompress();

			//call it generatedaddress.
			var generatedaddress = pubKey.GetAddress(Network);

			//Take the first four bytes of SHA256(SHA256(generatedaddress)) and call it addresshash.
			var addresshash = CoinEncryptedSecretEC.HashAddress(generatedaddress);

			//Derive a second key from passpoint using scrypt
			//salt is addresshash + ownerentropy
			var derived = CoinEncryptedSecretEC.CalculateDecryptionKey(Passpoint, addresshash, OwnerEntropy);

			//Now we will encrypt seedb.
			var encrypted = CoinEncryptedSecret.EncryptSeed
							(seedb,
							derived);

			//0x01 0x43 + flagbyte + addresshash + ownerentropy + encryptedpart1[0...7] + encryptedpart2 which totals 39 bytes
			var bytes =
				new[] { flagByte }
				.Concat(addresshash)
				.Concat(this.OwnerEntropy)
				.Concat(encrypted.Take(8).ToArray())
				.Concat(encrypted.Skip(16).ToArray())
				.ToArray();

			var encryptedSecret = new CoinEncryptedSecretEC(bytes, Network);

			return new EncryptedKeyResult(encryptedSecret, generatedaddress, seedb, () =>
			{
				//ECMultiply factorb by G, call the result pointb. The result is 33 bytes.
				var pointb = new CCKey(factorb).PubKey.ToBytes();
				//The first byte is 0x02 or 0x03. XOR it by (derivedhalf2[31] & 0x01), call the resulting byte pointbprefix.
				var pointbprefix = (byte)(pointb[0] ^ (byte)(derived[63] & 0x01));
				var pointbx = CoinEncryptedSecret.EncryptKey(pointb.Skip(1).ToArray(), derived);
				var encryptedpointb = new byte[] { pointbprefix }.Concat(pointbx).ToArray();

				var confirmBytes =
					Network.GetVersionBytes(Network.BASE58_CONFIRMATION_CODE, true)
					.Concat(new[] { flagByte })
					.Concat(addresshash)
					.Concat(OwnerEntropy)
					.Concat(encryptedpointb)
					.ToArray();

				return new CoinConfirmationCode(Encoders.Base58Check.EncodeData(confirmBytes), Network);
			});
		}


		byte[] _OwnerEntropy;
		public byte[] OwnerEntropy
		{
			get
			{
				return _OwnerEntropy ?? (_OwnerEntropy = vchData.Skip(1).Take(8).ToArray());
			}
		}
		byte[] _Passpoint;
		public byte[] Passpoint
		{
			get
			{
				return _Passpoint ?? (_Passpoint = vchData.Skip(1).Skip(8).ToArray());
			}
		}

		protected override bool IsValid
		{
			get
			{
				return 1 + 8 + 33 == vchData.Length && (vchData[0] == 0x53 || vchData[0] == 0x51);
			}
		}


		public override int Type
		{
			get
			{
				return Network.BASE58_PASSPHRASE_CODE;
			}
		}
	}

    public class CoinConfirmationCode : Base58Data
    {

        public CoinConfirmationCode(string wif, Network expectedNetwork = null)
            : base(wif, expectedNetwork)
        {
        }
        public CoinConfirmationCode(byte[] rawBytes, Network network)
            : base(rawBytes, network)
        {
        }

        byte[] _AddressHash;
        public byte[] AddressHash
        {
            get
            {
                return _AddressHash ?? (_AddressHash = vchData.SafeSubarray(1, 4));
            }
        }
        public bool IsCompressed
        {
            get
            {
                return (vchData[0] & 0x20) != 0;
            }
        }
        byte[] _OwnerEntropy;
        public byte[] OwnerEntropy
        {
            get
            {
                return _OwnerEntropy ?? (_OwnerEntropy = vchData.SafeSubarray(5, 8));
            }
        }
        LotSequence _LotSequence;
        public LotSequence LotSequence
        {
            get
            {
                var hasLotSequence = (vchData[0] & 0x04) != 0;
                if (!hasLotSequence)
                    return null;
                if (_LotSequence == null)
                {
                    _LotSequence = new LotSequence(OwnerEntropy.SafeSubarray(4, 4));
                }
                return _LotSequence;
            }
        }

        byte[] _EncryptedPointB;
        byte[] EncryptedPointB
        {
            get
            {
                return _EncryptedPointB ?? (_EncryptedPointB = vchData.SafeSubarray(13));
            }
        }

        public override int Type
        {
            get
            {
                return Network.BASE58_CONFIRMATION_CODE;
            }
        }

        protected override bool IsValid
        {
            get
            {
                return vchData.Length == 1 + 4 + 8 + 33;
            }
        }


        public bool Check(string passphrase, CoinAddress expectedAddress)
        {
            //Derive passfactor using scrypt with ownerentropy and the user's passphrase and use it to recompute passpoint 
            byte[] passfactor = CoinEncryptedSecretEC.CalculatePassFactor(passphrase, LotSequence, OwnerEntropy);
            //Derive decryption key for pointb using scrypt with passpoint, addresshash, and ownerentropy
            byte[] passpoint = CoinEncryptedSecretEC.CalculatePassPoint(passfactor);
            byte[] derived = CoinEncryptedSecretEC.CalculateDecryptionKey(passpoint, AddressHash, OwnerEntropy);

            //Decrypt encryptedpointb to yield pointb
            var pointbprefix = EncryptedPointB[0];
            pointbprefix = (byte)(pointbprefix ^ (byte)(derived[63] & (byte)0x01));

            //Optional since ArithmeticException will catch it, but it saves some times
            if (pointbprefix != 0x02 && pointbprefix != 0x03)
                return false;
            var pointb = CoinEncryptedSecret.DecryptKey(EncryptedPointB.Skip(1).ToArray(), derived);
            pointb = new byte[] { pointbprefix }.Concat(pointb).ToArray();

            //4.ECMultiply pointb by passfactor. Use the resulting EC point as a public key
            var curve = ECKey.Secp256k1;
            ECPoint pointbec;
            try
            {
                pointbec = curve.Curve.DecodePoint(pointb);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (ArithmeticException)
            {
                return false;
            }
            PubKey pubkey = new PubKey(pointbec.Multiply(new BigInteger(1, passfactor)).GetEncoded());

            //and hash it into address using either compressed or uncompressed public key methodology as specifid in flagbyte.
            pubkey = IsCompressed ? pubkey.Compress() : pubkey.Decompress();

            var actualhash = CoinEncryptedSecretEC.HashAddress(pubkey.GetAddress(Network));
            var expectedhash = CoinEncryptedSecretEC.HashAddress(expectedAddress);

            return Utils.ArrayEqual(actualhash, expectedhash);
        }
    }
}
