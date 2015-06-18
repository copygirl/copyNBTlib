using System.IO;

namespace copyNBTlib.Text
{
	/// <summary>
	/// Interface for formatting NBT tags to a string using a TextWriter.
	/// </summary>
	public interface INbtFormatter
	{
		void WriteTag(TextWriter writer, TagBase tag, string name = null);
	}
}

