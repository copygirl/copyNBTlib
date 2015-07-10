using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using copyNBTlib.Compression;
using copyNBTlib.IO;
using copyNBTlib.Text;
using copyNBTlib.Utility;

namespace copyNBTlib
{
	public abstract class TagBase
	{
		public TagType Type { get; private set; }


		internal TagBase(TagType type)
		{
			Type = type;
		}


		#region Shorthand properties / methods

		// TODO: Allow setting value?
		/// <summary>
		/// Gets the value of a primitive. Throws a NotSupportedException for non-primitive tags.
		/// Note that for getting the value of a specific tag type, there's an explicit cast available.
		/// </summary>
		public virtual object Value { get { throw MakeShorthandNSE(TagTypeHelper.PrimitiveTypes); } }

		/// <summary>
		/// Returns the number of tags in a collection. Throws a NotSupportedException for non-collection tags.
		/// If you want to iterate the collection, simply do it after casting it to TagList or TagCompound.
		/// </summary>
		public virtual int Count { get { throw MakeShorthandNSE(TagTypeHelper.CollectionTypes); } }

		/// <summary>
		/// Clears all tags in this collection. Throws a NotSupportedException for non-collection tags.
		/// </summary>
		public virtual void Clear() { throw MakeShorthandNSE(TagTypeHelper.CollectionTypes); }

		// List

		public virtual TagBase this[int index] {
			get { throw MakeShorthandNSE(TagType.List); }
			set { throw MakeShorthandNSE(TagType.List); }
		}

		public virtual void Add(TagBase tag) { throw MakeShorthandNSE(TagType.List); }
		public virtual void Insert(int index, TagBase tag) { throw MakeShorthandNSE(TagType.List); }
		public virtual void RemoveAt(int index) { throw MakeShorthandNSE(TagType.List); }

		// Compound

		public virtual TagBase this[string name] {
			get { throw MakeShorthandNSE(TagType.Compound); }
			set { throw MakeShorthandNSE(TagType.Compound); }
		}

		public virtual void Add(string name, TagBase tag) { throw MakeShorthandNSE(TagType.Compound); }
		public virtual bool Remove(string name) { throw MakeShorthandNSE(TagType.Compound); }

		#endregion

		#region Casting primitive tags

		// Explicitly casting primitive tags to their values.
		// Examples: var playerName = (string)playerTag["Name"];
		//           var yPosition = (float)playerTag["Position"][1];

		public static explicit operator   byte(TagBase tag) { return ((TagByte)tag).Value; }
		public static explicit operator  short(TagBase tag) { return ((TagShort)tag).Value; }
		public static explicit operator    int(TagBase tag) { return ((TagInt)tag).Value; }
		public static explicit operator   long(TagBase tag) { return ((TagLong)tag).Value; }
		public static explicit operator  float(TagBase tag) { return ((TagFloat)tag).Value; }
		public static explicit operator double(TagBase tag) { return ((TagDouble)tag).Value; }
		public static explicit operator string(TagBase tag) { return ((TagString)tag).Value; }
		public static explicit operator byte[](TagBase tag) { return ((TagByteArray)tag).Value; }
		public static explicit operator  int[](TagBase tag) { return ((TagIntArray)tag).Value; }

		// Implicitly casting primitive data types to tags.
		// Examples: playerNamesList.Add("copygirl");
		//           new TagList { 42, 1337, 9001 };
		//           new TagCompound { { "someInt", 1 }, { "someByte": (byte)2 } };

		public static implicit operator TagBase(  byte value) { return new TagByte(value); }
		public static implicit operator TagBase( short value) { return new TagShort(value); }
		public static implicit operator TagBase(   int value) { return new TagInt(value); }
		public static implicit operator TagBase(  long value) { return new TagLong(value); }
		public static implicit operator TagBase( float value) { return new TagFloat(value); }
		public static implicit operator TagBase(double value) { return new TagDouble(value); }
		public static implicit operator TagBase(string value) { return new TagString(value); }
		public static implicit operator TagBase(byte[] value) { return new TagByteArray(value); }
		public static implicit operator TagBase( int[] value) { return new TagIntArray(value); }

		#endregion


		#region Loading from files or streams

		/// <summary>
		/// Loads an NBT tag from a file.
		/// </summary>
		/// <param name="file">File to load from.</param>
		/// <param name="name">Will be set to the name of the root tag.</param>
		/// <param name="compression">The compression to use, null or default to auto-detect.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public static TagBase Load(string file, out string name, NbtCompression compression = null,
			                       bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			using (var stream = File.OpenRead(file))
				return Load(stream, out name, compression, ensureCompound, endianness);
		}
		/// <summary>
		/// Loads an NBT tag from a file.
		/// Shorthand for ignoring the tag name.
		/// </summary>
		/// <param name="file">File to load from.</param>
		/// <param name="compression">The compression to use, null or default to auto-detect.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public static TagBase Load(string file, NbtCompression compression = null,
		                           bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			string name;
			return Load(file, out name, compression, ensureCompound, endianness);
		}

		/// <summary>
		/// Loads an NBT tag from a stream.
		/// </summary>
		/// <param name="stream">The stream to load from.</param>
		/// <param name="name">Will be set to the name of the root tag.</param>
		/// <param name="compression">The compression to use, null or default to auto-detect.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public static TagBase Load(Stream stream, out string name, NbtCompression compression = null,
		                           bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			compression = compression ?? NbtCompression.AutoDetect(stream);
			using (stream = compression.Decompress(stream))
			using (var reader = new EndianBinaryReader(stream, endianness)) {
				var type = ReadTagType(reader);
				ValidateTagTypeIDE(type);
				if (ensureCompound && (type != TagType.Compound))
					throw new InvalidDataException("Root tag is not a compound");
				name = ReadString(reader);
				return Read(reader, type);
			}
		}
		/// <summary>
		/// Loads an NBT tag from a stream.
		/// Shorthand for ignoring the tag name.
		/// </summary>
		/// <param name="stream">The stream to load from.</param>
		/// <param name="compression">The compression to use, null or default to auto-detect.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public static TagBase Load(Stream stream, NbtCompression compression = null,
			bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			string name;
			return Load(stream, out name, compression, ensureCompound, endianness);
		}

		#endregion

		#region Saving to files or streams

		/// <summary>
		/// Saves an NBT tag to a file.
		/// </summary>
		/// <param name="file">File to save to.</param>
		/// <param name="name">Name of the root tag.</param>
		/// <param name="compression">The compression to use.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public void Save(string file, string name, NbtCompression compression,
		                 bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			using (var stream = File.Open(file, FileMode.OpenOrCreate))
				Save(stream, name, compression, ensureCompound, endianness);
		}

		/// <summary>
		/// Saves an NBT tag to a stream.
		/// </summary>
		/// <param name="stream">Stream to save to.</param>
		/// <param name="name">Name of the root tag.</param>
		/// <param name="compression">The compression to use.</param>
		/// <param name="ensureCompound">If true, throws an exception if the root tag isn't a compound.</param>
		/// <param name="endianness">The endianness to use. PC = Big, Pocket Edition = Little.</param>
		public void Save(Stream stream, string name, NbtCompression compression,
			bool ensureCompound = true, Endianness endianness = Endianness.Big)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (ensureCompound && (Type != TagType.Compound))
				throw new NotSupportedException("Root tag is not a compound");

			using (stream = compression.Compress(stream))
			using (var writer = new EndianBinaryWriter(stream, endianness))
				Write(writer, name);
		}

		#endregion


		#region Reading NBT tags

		/// <summary>
		/// Reads a full NBT tag, including its name.
		/// </summary>
		public static TagBase Read(BinaryReader reader, out string name)
		{
			var type = ReadTagType(reader);
			name = ReadString(reader);
			return Read(reader, type);
		}

		/// <summary>
		/// Reads an NBT tag of a given tag type.
		/// </summary>
		public static TagBase Read(BinaryReader reader, TagType type)
		{
			ValidateTagTypeIDE(type);
			var tag = CreateTagFromType(type);
			tag.ReadPayload(reader);
			return tag;
		}

		public static TagType ReadTagType(BinaryReader reader)
		{
			return (TagType)reader.ReadByte();
		}

		/// <summary>
		/// Reads the payload (data) of this NBT tag.
		/// </summary>
		public abstract void ReadPayload(BinaryReader reader);

		/// <summary>
		/// Reads an NBT-conform (UTF-8) string, prefixed
		/// with its length in bytes as an unsigned short.
		/// </summary>
		internal static string ReadString(BinaryReader reader)
		{
			var length = reader.ReadUInt16();
			var bytes = reader.ReadBytes(length);
			return MUTF8Encoding.Instance.GetString(bytes);
		}

		#endregion

		#region Writing NBT tags

		/// <summary>
		/// Writes a full NBT tag including the type, name and actual payload.
		/// </summary>
		public void Write(BinaryWriter writer, string name)
		{
			WriteTagType(writer);
			WriteString(writer, name);
			WritePayload(writer);
		}

		public static void WriteTagType(BinaryWriter writer, TagType type)
		{
			writer.Write((byte)type);
		}
		public void WriteTagType(BinaryWriter writer)
		{
			WriteTagType(writer, Type);
		}

		/// <summary>
		/// Writes the payload (data) of this NBT tag.
		/// </summary>
		public abstract void WritePayload(BinaryWriter writer);

		/// <summary>
		/// Writes an NBT-conform (Modified UTF-8) string, prefixed
		/// with its length in bytes as an unsigned short.
		/// </summary>
		internal static void WriteString(BinaryWriter writer, string str)
		{
			var bytes = MUTF8Encoding.Instance.GetBytes(str);
			writer.Write((ushort)bytes.Length);
			writer.Write(bytes);
		}

		#endregion


		#region ToString and ToJson

		/// <summary>
		/// Returns a human-readable string representing this tag, not including sub-tags.
		/// </summary>
		public override string ToString()
		{
			return ToString(null);
		}
		/// <summary>
		/// Returns a human-readable string representing this tag,
		/// including the tag name but without its sub-tags.
		/// </summary>
		public string ToString(string name)
		{
			return ToString(NbtFormatterNotch.Single, name);
		}
		/// <summary>
		/// Returns a string representing this tag, formatted using the specified formatter.
		/// Optionally includes a tag name, though some formatters may not be able to use it.
		/// </summary>
		public string ToString(INbtFormatter formatter, string name = null)
		{
			using (var writer = new StringWriter()) {
				formatter.WriteTag(writer, this, name);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Returns a JSON string containing the value for this tag.
		/// Simple tag primitives will simply return the value.
		/// Tag strings will return a quoted, escaped string.
		/// Tag arrays and lists will return JSON arrays.
		/// Tag compounds will return JSON objects.
		/// </summary>
		public string ToJson(bool pretty = false)
		{
			return ToString(pretty ? NbtFormatterJson.Prettified : NbtFormatterJson.Minified);
		}

		#endregion


		#region Utility functions

		/// <summary>
		/// Creates an empty / default value tag from a given tag type.
		/// </summary>
		public static TagBase CreateTagFromType(TagType type)
		{
			ValidateTagTypeAE(type);
			return _constructorLookup[(byte)type - 1]();
		}
		static readonly Func<TagBase>[] _constructorLookup = {
			() => new TagByte(),
			() => new TagShort(),
			() => new TagInt(),
			() => new TagLong(),
			() => new TagFloat(),
			() => new TagDouble(),
			() => new TagByteArray(),
			() => new TagString(),
			() => new TagList(),
			() => new TagCompound(),
			() => new TagIntArray(),
		};


		/// <summary>
		/// Thrown an ArgumentException if the tag type isn't a valid NBT tag type.
		/// </summary>
		public static TagType ValidateTagTypeAE(TagType type, bool allowEnd = false, string paramName = "type")
		{
			if (!type.IsValid(allowEnd))
				throw new ArgumentException(paramName, string.Format("Invalid NBT tag type 0x{0:X2}", type));
			return type;
		}
		/// <summary>
		/// Thrown an InvalidDataException if the tag type isn't a valid NBT tag type.
		/// </summary>
		public static TagType ValidateTagTypeIDE(TagType type, bool allowEnd = false)
		{
			if (!type.IsValid(allowEnd))
				throw new InvalidDataException(string.Format("Invalid NBT tag type 0x{0:X2}", type));
			return type;
		}


		/// <summary>
		/// Returns an new instance of a NotSupportedException to be
		/// thrown in shorthand methods if the tag type is incorrect.
		/// </summary>
		Exception MakeShorthandNSE(IEnumerable<TagType> expectedTypes)
		{
			return new NotSupportedException(string.Format(
				"Tag expected to be {0} for this operation, but is '{1}'",
				string.Join(" or ", expectedTypes.Select(t => "'" + t + "'")), Type));
		}
		/// <summary>
		/// Returns an new instance of a NotSupportedException to be
		/// thrown in shorthand methods if the tag type is incorrect.
		/// </summary>
		Exception MakeShorthandNSE(params TagType[] expectedTypes)
		{
			return MakeShorthandNSE(expectedTypes);
		}

		#endregion
	}
}

