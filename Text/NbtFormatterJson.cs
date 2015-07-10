using System;
using System.Collections.Generic;
using System.IO;
using copyNBTlib.Utility;

namespace copyNBTlib.Text
{
	/// <summary>
	/// Formatter for turning NBT tags into valid JSON value strings.
	/// This works for tags other than just compounds.
	/// </summary>
	public class NbtFormatterJson : INbtFormatter
	{
		static readonly int MultilineArrayMinElements = 16;
		static readonly int MultilineArrayElementsPerLine = 10;

		public static readonly NbtFormatterJson Minified = new NbtFormatterJson();
		public static readonly NbtFormatterJson Prettified = new NbtFormatterJson { Pretty = true, Multiline = true };

		string _indent = "  ";


		#region Public properties

		/// <summary>
		/// If true, prettifies the formatter's output by adding spacing between elements.
		/// </summary>
		public bool Pretty { get; set; }

		/// <summary>
		/// If true, outputs stuff in multiple lines.
		/// Requires Pretty to be set to true.
		/// </summary>
		public bool Multiline { get; set; }

		/// <summary>
		/// If true, multiple brackets on the same line.
		/// Requires Pretty and Multiline to be set to true.
		/// </summary>
		public bool CompactBrackets { get; set; }

		/// <summary>
		/// The string to use for indentation, 2 spaces by default.
		/// </summary>
		public string Indent {
			get { return _indent; }
			set {
				if (_indent == null)
					throw new ArgumentNullException("value");
				_indent = value;
			}
		}

		#endregion


		#region INBTFormatter implementation

		public void WriteTag(TextWriter writer, TagBase tag, string name = null)
		{
			WriteTagLookup(writer, tag, string.Empty);
		}

		#endregion


		#region Write arrays and lists

		void WriteValueArray<T>(TextWriter writer, TagBase tag, string curIndent)
		{
			var array = (T[])tag.Value;
			// (Only Pretty + Multiline:)
			// If array has enough elements, put them in multiple lines.
			// Otherwise just put them all on the same line.
			var elementsPerLine = ((array.Length >= MultilineArrayMinElements)
				? MultilineArrayElementsPerLine : -1);
			
			WriteArray(writer, array, elementsPerLine, curIndent, val => writer.Write(val));
		}

		void WriteValueList(TextWriter writer, TagList list, string curIndent)
		{
			// (Only Pretty + Multiline + CompactBrackets:)
			// If the list contains collection or array elements,
			// compact the brackets to be on the same line.
			var type = (list.ListType ?? TagType.Invalid);
			var elementsPerLine = ((CompactBrackets && (type.IsCollection() || type.IsArray())) ? -1 : 1);

			WriteArray(writer, list, elementsPerLine, curIndent,
				val => WriteTagLookup(writer, val, curIndent + Indent));
		}

		void WriteArray<T>(TextWriter writer, IList<T> elements, int elementsPerLine,
		                   string curIndent, Action<T> writeElement)
		{
			var multiline = (Multiline && (elementsPerLine > 0));

			writer.Write('[');
			if (Pretty && (elements.Count <= 0))
				writer.Write(' ');

			for (int i = 0; i < elements.Count; i++) {
				// If multiline and first element in line, insert linebreak.
				WriteIndentOrSpace(writer, (multiline && ((i % elementsPerLine) == 0)), curIndent + Indent);

				writeElement(elements[i]);

				// If not last element, include comma.
				if (i < (elements.Count - 1))
					writer.Write(',');
			}

			WriteIndentOrSpace(writer, (multiline && (elements.Count > 0)), curIndent);

			writer.Write(']');
		}

		#endregion

		#region Write compounds

		void WriteValueCompound(TextWriter writer, TagCompound compound, string curIndent)
		{
			writer.Write('{');
			if (Pretty && (compound.Count <= 0))
				writer.Write(' ');

			var i = 0;
			foreach (var nameTagPair in compound) {
				var name = nameTagPair.Key;
				var tag = nameTagPair.Value;

				WriteIndentOrSpace(writer, true, curIndent + Indent);

				WriteJsonString(writer, name);

				writer.Write(':');
				if (Pretty)
					writer.Write(' ');

				WriteTagLookup(writer, tag, curIndent + Indent);

				// If not last element, include comma.
				if (++i < compound.Count)
					writer.Write(',');
			}

			WriteIndentOrSpace(writer, (compound.Count > 0), curIndent);
			writer.Write('}');
		}

		#endregion


		#region Utility functions

		/// <summary>
		/// Writes a linebreak and indents if the formatter is set to Multiline and the line conditional is true.
		/// Otherwise writes just a space if the formatter is set to Pretty.
		/// Does nothing if the formatter is not set to Pretty.
		/// </summary>
		void WriteIndentOrSpace(TextWriter writer, bool line, string indent)
		{
			if (!Pretty) return;
			if (Multiline && line) {
				writer.WriteLine();
				writer.Write(indent);
			} else writer.Write(' ');
		}

		void WriteTagLookup(TextWriter writer, TagBase tag, string curIndent)
		{
			_writeTagLookup[(byte)tag.Type - 1](this, writer, tag, curIndent);
		}
		static readonly Action<NbtFormatterJson, TextWriter, TagBase, string>[] _writeTagLookup = {
			(f, w, tag, ind) => w.Write(tag.Value), // byte
			(f, w, tag, ind) => w.Write(tag.Value), // short
			(f, w, tag, ind) => w.Write(tag.Value), // int
			(f, w, tag, ind) => w.Write(tag.Value), // long
			(f, w, tag, ind) => w.Write(tag.Value), // float
			(f, w, tag, ind) => w.Write(tag.Value), // double
			(f, w, tag, ind) => f.WriteValueArray<byte>(w, tag, ind),
			(f, w, tag, ind) => WriteJsonString(w, (string)tag),
			(f, w, tag, ind) => f.WriteValueList(w, (TagList)tag, ind),
			(f, w, tag, ind) => f.WriteValueCompound(w, (TagCompound)tag, ind),
			(f, w, tag, ind) => f.WriteValueArray<int>(w, tag, ind),
		};

		/// <summary>
		/// Writes a JSON valid string, including quotes, from the source string.
		/// </summary>
		static void WriteJsonString(TextWriter writer, string str)
		{
			writer.Write('"');
			foreach (char c in str) {
				string repl;
				if (_jsonCharReplacement.TryGetValue(c, out repl))
					writer.Write(repl);
				else if (char.IsControl(c))
					writer.Write(@"\u{0:X4}", Convert.ToUInt16(c));
				else writer.Write(c);
			}
			writer.Write('"');
		}
		static readonly Dictionary<char, string> _jsonCharReplacement = new Dictionary<char, string> {
			{ '"', @"\""" }, { '\\', @"\\" }, { '\b', @"\b" }, { '\f', @"\f" },
			{ '\n', @"\n" }, { '\r', @"\r" }, { '\t', @"\t" }
		};

		#endregion
	}
}

