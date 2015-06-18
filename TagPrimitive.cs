using System.IO;

namespace copyNBTlib
{
	public abstract class TagPrimitive<T> : TagPrimitiveBase<T>
	{
		public new T Value {
			get { return _value; }
			set { _value = value; }
		}

		internal TagPrimitive(TagType type, T value)
			: base(type, value) {  }
	}

	#region TagPrimitiveBase definiton

	/// <summary>
	/// This class functions as an in-between for TagPrimitive and TagBase,
	/// allowing simultaneous overriding and hiding of the Value property.
	/// </summary>
	public abstract class TagPrimitiveBase<T> : TagBase
	{
		protected T _value;

		public override object Value { get { return _value; } }

		internal TagPrimitiveBase(TagType type, T value)
			: base(type) { _value = value; }
	}

	#endregion

	#region Number type class definitons (byte, short, int, long, float, double)

	public class TagByte : TagPrimitive<byte>
	{
		public TagByte(byte value = 0) : base(TagType.Byte, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadByte(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}
	public class TagShort : TagPrimitive<short>
	{
		public TagShort(short value = 0) : base(TagType.Short, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadInt16(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}
	public class TagInt : TagPrimitive<int>
	{
		public TagInt(int value = 0) : base(TagType.Int, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadInt32(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}
	public class TagLong : TagPrimitive<long>
	{
		public TagLong(long value = 0) : base(TagType.Long, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadInt64(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}
	public class TagFloat : TagPrimitive<float>
	{
		public TagFloat(float value = 0) : base(TagType.Float, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadSingle(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}
	public class TagDouble : TagPrimitive<double>
	{
		public TagDouble(double value = 0) : base(TagType.Double, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = reader.ReadDouble(); }
		public override void WritePayload(BinaryWriter writer) { writer.Write(Value); }
	}

	#endregion

	#region Complex primary type class definitions (string, byte[], int[])

	public class TagString : TagPrimitive<string>
	{
		public TagString(string value = "") : base(TagType.String, value) {  }
		public override void ReadPayload(BinaryReader reader) { Value = ReadString(reader); }
		public override void WritePayload(BinaryWriter writer) { WriteString(writer, Value); }
	}

	public class TagByteArray : TagPrimitive<byte[]>
	{
		public TagByteArray(params byte[] value)
			: base(TagType.ByteArray, value) {  }

		public override void ReadPayload(BinaryReader reader)
		{
			int length = reader.ReadInt32();
			Value = reader.ReadBytes(length);
		}
		
		public override void WritePayload(BinaryWriter writer)
		{
			writer.Write(Value.Length);
			writer.Write(Value);
		}
	}

	public class TagIntArray : TagPrimitive<int[]>
	{
		public TagIntArray(params int[] value)
			: base(TagType.IntArray, value) {  }

		public override void ReadPayload(BinaryReader reader)
		{
			int length = reader.ReadInt32();
			Value = new int[length];
			for (int i = 0; i < length; i++)
				Value[i] = reader.ReadInt32();
		}
		
		public override void WritePayload(BinaryWriter writer)
		{
			writer.Write(Value.Length);
			foreach (int i in Value)
				writer.Write(i);
		}
	}

	#endregion
}

