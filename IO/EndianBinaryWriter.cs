using System;
using System.IO;

namespace copyNBTlib.IO
{
	/// <summary>
	/// BinaryWriter which ensures that outgoing data is in
	/// the target endianness, converting data if necessary.
	/// </summary>
	public class EndianBinaryWriter : BinaryWriter
	{
		public Endianness Endianness { get; private set; }

		public EndianBinaryWriter(Stream stream, Endianness endianness)
			: base(stream)
		{
			Endianness = endianness;
		}

		public override void Write( short value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write(ushort value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write(   int value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write(  uint value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write(  long value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write( ulong value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write( float value) { Write(Write, BitConverter.GetBytes, value); }
		public override void Write(double value) { Write(Write, BitConverter.GetBytes, value); }


		/// <summary>
		/// Writes a value to the underlying stream, converting
		/// it to the correct endianness if necessary.
		/// </summary>
		void Write<T>(Action<T> defaultFunc, Func<T, byte[]> convertFunc, T value)
		{
			if (BitConverter.IsLittleEndian && (Endianness == Endianness.Little))
				defaultFunc(value);
			else Write(Reverse(convertFunc(value)));
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

