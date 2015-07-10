using System;
using System.IO;
using System.IO.Compression;

namespace copyNBTlib.Compression
{
	public abstract class NbtCompression
	{
		public static readonly NbtCompression None = new NoCompression();
		public static readonly NbtCompression GZip = new GZipCompression();
		// TODO: Add ZLibCompression. Where is that even used? Network streams?

		public abstract Stream Compress(Stream stream);
		public abstract Stream Decompress(Stream stream);

		/// <summary>
		/// Attempts to auto-detect the compression used.
		/// </summary>
		/// <exception cref="NotSupportedException">Thrown when stream doesn't support seeking.</exception>
		public static NbtCompression AutoDetect(Stream stream)
		{
			if (!stream.CanSeek)
				throw new NotSupportedException("Can't auto-detect NBT compression, stream doesn't support seeking");
			
			var magic = new byte[2];
			var length = stream.Read(magic, 0, magic.Length);
			stream.Seek(-length, SeekOrigin.Current);

			if ((magic[0] == 0x1F) && (magic[1] == 0x8B)) return GZip;
			if ((magic[0] >= (byte)TagType.Byte) && (magic[0] <= (byte)TagType.IntArray)) return None;
 			throw new InvalidDataException("Couldn't auto-detect NBT compression, invalid magic numbers");
		}

		#region NoCompression class

		class NoCompression : NbtCompression
		{
			public override Stream Compress(Stream stream) { return new LeaveOpenWrapper(stream); }
			public override Stream Decompress(Stream stream) { return new LeaveOpenWrapper(stream); }

			// Wrapper is needed so the wrapped stream isn't
			// closed when the compression stream is disposed.
			class LeaveOpenWrapper : Stream
			{
				readonly Stream _base;

				internal LeaveOpenWrapper(Stream stream) { _base = stream; }

				public override bool CanRead { get { return _base.CanRead; } }
				public override bool CanSeek { get { return _base.CanSeek; } }
				public override bool CanWrite { get { return _base.CanWrite; } }
				public override long Length { get { return _base.Length; } }
				public override long Position {
					get { return _base.Position; }
					set { _base.Position = value; }
				}

				public override void Flush() { _base.Flush(); }
				public override long Seek(long offset, SeekOrigin origin) { return _base.Seek(offset, origin); }
				public override void SetLength(long value) { _base.SetLength(value); }
				public override int Read(byte[] buffer, int offset, int count) { return _base.Read(buffer, offset, count); }
				public override void Write(byte[] buffer, int offset, int count) { _base.Write(buffer, offset, count); }
			}
		}

		#endregion

		#region GZipCompression class

		class GZipCompression : NbtCompression
		{
			public override Stream Compress(Stream stream)
			{
				return new GZipStream(stream, CompressionMode.Compress, false);
			}
			public override Stream Decompress(Stream stream)
			{
				return new GZipStream(stream, CompressionMode.Decompress, false);
			}
		}

		#endregion
	}
}

