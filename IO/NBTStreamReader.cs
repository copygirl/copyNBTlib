using System;
using System.IO;
using System.Text;
using copyNBTlib.Utility;

namespace copyNBTlib.IO
{
	/// <summary>
	/// Utility class for reading NBT tags from a stream.
	/// Allows specifying endianness of the source data.
	/// </summary>
	public class NBTStreamReader
	{
		readonly byte[] _buffer = new byte[256];

		public Stream BaseStream { get; private set; }
		public Encoding Encoding { get; private set; }
		public Endianness Endianness { get; private set; }

		public NBTStreamReader(Stream stream, Encoding encoding, Endianness endianness)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (encoding == null)
				throw new ArgumentNullException("encoding");
			if (!Enum.IsDefined(typeof(Endianness), endianness))
				throw new ArgumentException("Invalid Endianness", "endianness");
			
			if (!stream.CanRead)
				throw new ArgumentException("Stream is not readable", "stream");
			
			BaseStream = stream;
			Encoding = encoding;
			Endianness = endianness;
		}

		#region Basic read operations

		public byte[] Read(int count)
		{
			var buffer = _buffer;
			if (count > buffer.Length)
				buffer = new byte[count];
			if (BaseStream.Read(buffer, 0, count) < count)
				throw new EndOfStreamException();
			return buffer;
		}

		/// <summary>
		/// Reads a number of bytes, and if the host endianness is not
		/// the same as the source endianness, reverses the bytes read.
		/// </summary>
		byte[] ReadEndian(int count)
		{
			var buffer = Read(count);
			if (BitConverter.IsLittleEndian && (Endianness != Endianness.Little))
				Array.Reverse(buffer, 0, count);
			return buffer;
		}

		#endregion

		#region Read primitive values

		public   byte ReadByte()   { return Read(sizeof(byte))[0]; }
		public  short ReadShort()  { return BitConverter. ToInt16(ReadEndian(sizeof( short)), 0); }
		public    int ReadInt()    { return BitConverter. ToInt32(ReadEndian(sizeof(   int)), 0); }
		public   long ReadLong()   { return BitConverter. ToInt64(ReadEndian(sizeof(  long)), 0); }
		public  float ReadFloat()  { return BitConverter.ToSingle(ReadEndian(sizeof( float)), 0); }
		public double ReadDouble() { return BitConverter.ToDouble(ReadEndian(sizeof(double)), 0); }

		/// <summary>
		/// Reads an string in the reader's encoding, prefixed
		/// with its length in bytes as an unsigned short.
		/// </summary>
		public string ReadString()
		{
			var length = BitConverter.ToUInt16(ReadEndian(sizeof(ushort)), 0);
			return Encoding.GetString(Read(length), 0, length);
		}

		#endregion

		#region Read NBT tags

		public TagType ReadTagType(bool allowEndTag = false)
		{
			var type = (TagType)ReadByte();
			if (!type.IsValid(allowEndTag))
				throw new InvalidDataException(string.Format(
					"Invalid NBT tag type 0x{0:X}", type));
			return type;
		}

		/// <summary>
		/// Reads an NBT tag of a given tag type and its name.
		/// </summary>
		public TagBase ReadTag(TagType type, out string name)
		{
			if (!type.IsValid())
				throw new ArgumentException(string.Format(
					"Invalid NBT tag type 0x{0:X}", type), "type");
			name = ReadString();
			return ReadTag(type);
		}

		/// <summary>
		/// Reads an NBT tag of a given tag type.
		/// </summary>
		public TagBase ReadTag(TagType type)
		{
			var tag = TagBase.CreateTagFromType(type);
			tag.ReadPayload(this);
			return tag;
		}

		#endregion
	}
}

