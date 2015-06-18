using System;
using System.IO;

namespace copyNBTlib.IO
{
	/// <summary>
	/// BinaryReader which treats data read to be in a specific endianness
	/// and converts them to the host endianness, if necessary.
	/// </summary>
	public class EndianBinaryReader : BinaryReader
	{
		public Endianness Endianness { get; private set; }

		public EndianBinaryReader(Stream stream, Endianness endianness)
			: base(stream)
		{
			Endianness = endianness;
		}

		public override  short ReadInt16()  { return Read(ReadInt16,  BitConverter.ToInt16,  sizeof( short)); }
		public override ushort ReadUInt16() { return Read(ReadUInt16, BitConverter.ToUInt16, sizeof(ushort)); }
		public override    int ReadInt32()  { return Read(ReadInt32,  BitConverter.ToInt32,  sizeof(   int)); }
		public override   uint ReadUInt32() { return Read(ReadUInt32, BitConverter.ToUInt32, sizeof(  uint)); }
		public override   long ReadInt64()  { return Read(ReadInt64,  BitConverter.ToInt64,  sizeof(  long)); }
		public override  ulong ReadUInt64() { return Read(ReadUInt64, BitConverter.ToUInt64, sizeof( ulong)); }
		public override  float ReadSingle() { return Read(ReadSingle, BitConverter.ToSingle, sizeof( float)); }
		public override double ReadDouble() { return Read(ReadDouble, BitConverter.ToDouble, sizeof(double)); }


		/// <summary>
		/// Reads a value from the underlying stream,
		/// converting it to host endianness if necessary.
		/// </summary>
		T Read<T>(Func<T> defaultFunc, Func<byte[], int, T> convertFunc, int size)
		{
			return ((BitConverter.IsLittleEndian && (Endianness == Endianness.Little))
				? defaultFunc() : convertFunc(Reverse(ReadBytes(size)), 0));
		}

		/// <summary>
		/// Reverses the array and returns it.
		/// This modifies the array, it doesn't create a copy.
		/// </summary>
		static T[] Reverse<T>(T[] array)
		{
			Array.Reverse(array);
			return array;
		}
	}
}

