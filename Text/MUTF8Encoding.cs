using System.Text;

namespace copyNBTlib.Text
{
	/// <summary>
	/// Encoding class to support Java's "Modified UTF-8" encoding.
	/// 
	/// Null characters (U+0000) are encoded in two bytes (0xC0, 0x80).
	/// Characters above U+FFFF are treated as surrogate pairs, thus a single character takes up 3 bytes at most.
	/// 
	/// http://docs.oracle.com/javase/6/docs/api/java/io/DataInput.html#modified-utf-8
	/// </summary>
	public class MUTF8Encoding : Encoding
	{
		public static readonly MUTF8Encoding Instance = new MUTF8Encoding();

		public override string EncodingName { get { return "Modified UTF-8"; } }

		MUTF8Encoding() {  }


		#region GetByteCount, GetBytes and GetMaxByteCount

		public override int GetByteCount(char[] chars, int index, int count)
		{
			int byteCount = 0;
			for (int i = index; i < count; i++) {
				char c = chars[i];
				if ((c < '\u0080') && (c != '\u0000')) byteCount++;
				else if (c < '\u0800') byteCount += 2;
				else byteCount += 3;
			}
			return byteCount;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			int startByteIndex = byteIndex;
			for (int i = charIndex; i < charIndex + charCount; i++) {
				char c = chars[i];
				if ((c < '\u0080') && (c != '\u0000'))
					bytes[byteIndex++] = (byte)c;                      // 0xxxxxxx
				else if (c < '\u0800') {
					bytes[byteIndex++] = (byte)(0xC0 | c >> 6);        // 110xxxxx
					bytes[byteIndex++] = (byte)(0x80 | c & 0x3F);      // 10xxxxxx
				} else {
					bytes[byteIndex++] = (byte)(0xE0 | c >> 12);       // 1110xxxx
					bytes[byteIndex++] = (byte)(0x80 | c >> 6 & 0x3F); // 10xxxxxx
					bytes[byteIndex++] = (byte)(0x80 | c & 0x3F);      // 10xxxxxx
				}
			}
			return (byteIndex - startByteIndex);
		}

		public override int GetMaxByteCount(int charCount)
		{
			return (charCount * 3);
		}

		#endregion

		#region GetCharCount, GetChars and GetMaxCharCount

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			int charCount = 0;
			for (int i = index; i < index + count; i++) {
				byte b = bytes[i];
				if ((b & 0xF) == 0)
					charCount++;
				else if ((b & 0xE0) == 0xC0) {
					charCount += 2;
					i++;
				} else if ((b & 0xF0) == 0xE0) {
					charCount += 3;
					i += 2;
				}
			}
			return charCount;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			int startCharIndex = charIndex;
			for (int i = byteIndex; i < byteIndex + byteCount; i++) {
				byte b = bytes[i];
				if ((b & 0xF) == 0)
					chars[charIndex++] = (char)b;
				else if ((b & 0xE0) == 0xC0)
					chars[charIndex++] = (char)(
						(b & 0x1F) << 6 |
						bytes[++i] & 0x3F);
				else if ((b & 0xF0) == 0xE0)
					chars[charIndex++] = (char)(
						(b & 0x0F) << 12 |
						(bytes[++i] & 0x3F) << 6 |
						bytes[++i] & 0x3F);
				// else, the encoding was invalid.
			}
			return (charIndex - startCharIndex);
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}

		#endregion
	}
}

