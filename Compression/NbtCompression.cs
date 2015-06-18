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

		public static NbtCompression AutoDetect(Stream stream)
		{
			if (!stream.CanSeek)
				throw new NotSupportedException("Can't auto-detect NBT compression, stream doesn't support seeking");
			
			byte[] magic = new byte[2];
			int length = stream.Read(magic, 0, magic.Length);
			stream.Seek(-length, SeekOrigin.Current);

			if ((magic[0] == 0x1F) && (magic[1] == 0x8B)) return GZip;
			if ((magic[0] >= (byte)TagType.Byte) && (magic[0] <= (byte)TagType.IntArray)) return None;
 			throw new InvalidDataException("Couldn't auto-detect NBT compression, invalid magic numbers");
		}


		class NoCompression : NbtCompression
		{
			public override Stream Compress(Stream stream) { return stream; }
			public override Stream Decompress(Stream stream) { return stream; }
		}

		class GZipCompression : NbtCompression
		{
			public override Stream Compress(Stream stream)
			{
				return new GZipStream(stream, CompressionMode.Compress);
			}
			public override Stream Decompress(Stream stream)
			{
				return new GZipStream(stream, CompressionMode.Decompress);
			}
		}
	}
}

