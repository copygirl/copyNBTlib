using System;
using System.Collections.Generic;
using System.IO;

namespace copyNBTlib.Text
{
	/// <summary>
	/// Formatter for turning NBT tags into a string representation
	/// very similar to the one in Notch's original NBT specification.
	/// </summary>
	public class NbtFormatterNotch : INbtFormatter
	{
		public static readonly NbtFormatterNotch Single = new NbtFormatterNotch();
		public static readonly NbtFormatterNotch All = new NbtFormatterNotch(int.MaxValue);

		public int Depth { get; private set; }
		public string Indent { get; private set; }

		public NbtFormatterNotch(int depth = 0, string indent = "  ")
		{
			if (indent == null)
				throw new ArgumentNullException("indent");
			
			Depth = depth;
			Indent = indent;
		}

		#region INBTFormatter implementation

		public void WriteTag(TextWriter writer, TagBase tag, string name = null)
		{
			WriteTagInternal(writer, tag, name, Depth, string.Empty);
		}

		void WriteTagInternal(TextWriter writer, TagBase tag, string name, int depth, string curIndent)
		{
			writer.Write(curIndent);

			writer.Write(TypeToString(tag.Type));
			if (name != null) {
				writer.Write("(\"");
				writer.Write(name);
				writer.Write("\")");
			}
			writer.Write(": ");
			WriteTagLookup(writer, tag);

			if (!(tag is TagList) && !(tag is TagCompound) || (tag.Count <= 0)) return;

			if (depth > 0) {
				writer.WriteLine();
				writer.Write(curIndent);
				writer.WriteLine("{");

				var newIndent = curIndent + Indent;
				if (tag is TagList) {
					foreach (var childTag in (TagList)tag) {
						WriteTagInternal(writer, childTag, null, depth - 1, newIndent);
						writer.WriteLine();
					}
				} else foreach (var nameTagPair in (TagCompound)tag) {
					WriteTagInternal(writer, nameTagPair.Value, nameTagPair.Key, depth - 1, newIndent);
					writer.WriteLine();
				}

				writer.Write(curIndent);
				writer.Write("}");
			} else writer.Write(" { ... }");
		}

		#endregion

		#region Utility functions

		static string TypeToString(TagType type)
		{
			return _typeToString[(byte)type - 1];
		}
		static readonly string[] _typeToString = {
			"TAG_Byte", "TAG_Short", "TAG_Int", "TAG_Long",
			"TAG_Float", "TAG_Double", "TAG_Byte_Array", "TAG_String",
			"TAG_List", "TAG_Compound", "TAG_Int_Array"
		};

		static void WriteTagLookup(TextWriter writer, TagBase tag)
		{
			_writeTagLookup[(byte)tag.Type - 1](writer, tag);
		}
		static readonly Action<TextWriter, TagBase>[] _writeTagLookup = {
			(w, tag) => w.Write((byte)tag),
			(w, tag) => w.Write((short)tag),
			(w, tag) => w.Write((int)tag),
			(w, tag) => w.Write((long)tag),
			(w, tag) => w.Write((float)tag),
			(w, tag) => w.Write((double)tag),
			(w, tag) => w.Write("[{0} bytes]", ((byte[])tag).Length),
			(w, tag) => w.Write((string)tag),
			(w, tag) => TagListToString(w, (TagList)tag),
			(w, tag) => w.Write("{0} entr{1}", tag.Count, ((tag.Count == 1) ? "y" : "ies")),
			(w, tag) => w.Write("[{0} ints]", ((int[])tag).Length)
		};

		static void TagListToString(TextWriter writer, TagList list)
		{
			writer.Write("{0} entr{1}", list.Count, ((list.Count == 1) ? "y" : "ies"));
			if (list.ListType.HasValue)
				writer.Write(" of type {0}", TypeToString(list.ListType.Value));
		}

		#endregion
	}
}

