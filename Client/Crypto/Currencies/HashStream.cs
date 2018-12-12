﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Crypto
{
    public abstract class HashStreamBase : Stream
    {
        public abstract uint256 GetHash();
    }

    public class HashStream : HashStreamBase
    {
        public HashStream()
        {
        }

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int copied = 0;
            int toCopy = 0;
            while (copied != count)
            {
                toCopy = Math.Min(_Buffer.Length - _Pos, count - copied);
                Buffer.BlockCopy(buffer, offset + copied, _Buffer, _Pos, toCopy);
                copied += (byte)toCopy;
                _Pos += (byte)toCopy;
                ProcessBlockIfNeeded();
            }
        }

        private byte[] _Buffer = new byte[32];
        private byte _Pos;

        public override void WriteByte(byte value)
        {
            _Buffer[_Pos++] = value;
            ProcessBlockIfNeeded();
        }

        private void ProcessBlockIfNeeded()
        {
            if (_Pos == _Buffer.Length)
                ProcessBlock();
        }

#if (USEBC || WINDOWS_UWP || NETCORE)
		BouncyCastle.Crypto.Digests.Sha256Digest sha = new BouncyCastle.Crypto.Digests.Sha256Digest();
		private void ProcessBlock()
		{
			sha.BlockUpdate(_Buffer, 0, _Pos);
			_Pos = 0;
		}

		public uint256 GetHash()
		{
			ProcessBlock();
			sha.DoFinal(_Buffer, 0);
			_Pos = 32;
			ProcessBlock();
			sha.DoFinal(_Buffer, 0);
			return new uint256(_Buffer);
		}

#else
        private System.Security.Cryptography.SHA256Managed sha = new System.Security.Cryptography.SHA256Managed();

        private void ProcessBlock()
        {
            sha.TransformBlock(_Buffer, 0, _Pos, _Buffer, 0);
            _Pos = 0;
        }

        private static readonly byte[] Empty = new byte[0];

        public override uint256 GetHash()
        {
            ProcessBlock();
            sha.TransformFinalBlock(Empty, 0, 0);
            var hash1 = sha.Hash;
            Buffer.BlockCopy(sha.Hash, 0, _Buffer, 0, 32);
            sha.Initialize();
            sha.TransformFinalBlock(_Buffer, 0, 32);
            var hash2 = sha.Hash;
            return new uint256(hash2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                sha.Dispose();
            base.Dispose(disposing);
        }

#endif
    }

    /// <summary>
    /// Unoptimized hash stream, bufferize all the data
    /// </summary>
    public abstract class BufferedHashStream : HashStreamBase
    {
        private class FuncBufferedHashStream : BufferedHashStream
        {
            private Func<byte[], int, int, byte[]> _CalculateHash;

            public FuncBufferedHashStream(Func<byte[], int, int, byte[]> calculateHash, int capacity) : base(capacity)
            {
                if (calculateHash == null)
                    throw new ArgumentNullException(nameof(calculateHash));
                _CalculateHash = calculateHash;
            }

            protected override uint256 GetHash(byte[] data, int offset, int length)
            {
                return new uint256(_CalculateHash(data, offset, length), 0, 32);
            }
        }

        public static BufferedHashStream CreateFrom(Func<byte[], int, int, byte[]> calculateHash, int capacity = 0)
        {
            return new FuncBufferedHashStream(calculateHash, capacity);
        }

        private MemoryStream ms;

        public BufferedHashStream(int capacity)
        {
            ms = new MemoryStream(capacity);
        }

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ms.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            ms.WriteByte(value);
        }

        public override uint256 GetHash()
        {
#if NO_MEM_BUFFER
			var copy = ms.ToArray();
			return GetHash(copy, 0, (int)copy.Length);
#else
            return GetHash(ms.GetBuffer(), 0, (int)ms.Length);
#endif
        }

        protected abstract uint256 GetHash(byte[] data, int offset, int length);
    }
}