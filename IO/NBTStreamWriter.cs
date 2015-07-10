using System;
using System.IO;
using System.Text;
using copyNBTlib.Utility;

namespace copyNBTlib.IO
{
	/// <summary>
	/// Utility class for writing NBT tags to a stream.
	/// Allows specifying the endianness of the output data.
	/// </summary>
	public class NBTStreamWriter
	{
		public Stream BaseStream { get; private set; }
		public Encoding Encoding { get; private set; }
		public Endianness Endianness { get; private set; }

		public NBTStreamWriter(Stream stream, Encoding encoding, Endianness endianness)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (encoding == null)
				throw new ArgumentNullException("encoding");
			if (!Enum.IsDefined(typeof(Endianness), endianness))
				throw new ArgumentException("Invalid Endianness", "endianness");

			if (!stream.CanWrite)
				throw new ArgumentException("Stream is not writable", "stream");

			BaseStream = stream;
			Encoding = encoding;
			Endianness = endianness;
		}

		#region Base write operations

		public void Write(byte[] array)
		{
			BaseStream.Write(array, 0, array.Length);
		}

		/// <summary>
		/// Writes some bytes. If the output endianness is not
		/// the same as the host endianness, reverses the bytes.
		/// </summary>
		void WriteEndian(byte[] array)
		{
			if (BitConverter.IsLittleEndian && (Endianness != Endianness.Little))
				Array.Reverse(array, 0, array.Length);
			Write(array);
		}

		#endregion

		#region Write primitive values

		public void Write(  byte value) { BaseStream.WriteByte(value); }
		public void Write( short value) { WriteEndian(BitConverter.GetBytes(value)); }
		public void Write(   int value) { WriteEndian(BitConverter.GetBytes(value)); }
		public void Write(  long value) { WriteEndian(BitConverter.GetBytes(value)); }
		public void Write( float value) { WriteEndian(BitConverter.GetBytes(value)); }
		public void Write(double value) { WriteEndian(BitConverter.GetBytes(value)); }

		/// <summary>
		/// Writes an string in the writer's encoding, prefixed
		/// with its length in bytes as an unsigned short.
		/// </summary>
		public void Write(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			
			byte[] buffer = Encoding.GetBytes(value);
			WriteEndian(BitConverter.GetBytes((ushort)buffer.Length));
			Write(buffer);
		}

		#endregion

		#region Write NBT tags

		public void Write(TagType type, bool allowEndTag = false)
		{
			if (!type.IsValid(allowEndTag))
				throw new ArgumentException(string.Format(
					"Invalid NBT tag type 0x{0:X}", type), "type");
			Write((byte)type);
		}

		/// <summary>
		/// Writes a full NBT tag including the type, name and actual payload.
		/// </summary>
		public void Write(TagBase tag, string name)
		{
			if (tag == null)
				throw new ArgumentNullException("tag");
			if (name == null)
				throw new ArgumentNullException("name");
			
			Write(tag.Type);
			Write(name);
			tag.WritePayload(this);
		}

		#endregion
	}
}

