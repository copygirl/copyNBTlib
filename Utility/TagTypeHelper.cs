using System.Collections.Generic;

namespace copyNBTlib.Utility
{
	public static class TagTypeHelper
	{
		public static readonly IReadOnlyCollection<TagType> PrimitiveTypes = new List<TagType> {
			TagType.Byte, TagType.Int, TagType.Short, TagType.Long,
			TagType.Float, TagType.Double, TagType.String,
			TagType.ByteArray, TagType.IntArray
		}.AsReadOnly();

		public static readonly IReadOnlyCollection<TagType> ArrayTypes =
			new List<TagType> { TagType.ByteArray, TagType.IntArray }.AsReadOnly();

		public static readonly IReadOnlyCollection<TagType> CollectionTypes =
			new List<TagType> { TagType.List, TagType.Compound }.AsReadOnly();

		/// <summary>
		/// Returns if the tag type is a valid NBT tag type.
		/// </summary>
		public static bool IsValid(this TagType type, bool allowEndTag = false)
		{
			return ((type >= (allowEndTag ? TagType.End : TagType.Byte)) && (type <= TagType.IntArray));
		}

		/// <summary>
		/// Returns if the tag type is a primitive (Byte, Short, Int, Long, Float, Double, String, ByteArray or IntArray).
		/// </summary>
		public static bool IsPrimitive(this TagType type)
		{
			return ((type >= TagType.Byte) && ((type <= TagType.ByteArray) || (type == TagType.IntArray)));
		}

		/// <summary>
		/// Returns if the tag type is an array (ByteArray or IntArray).
		/// </summary>
		public static bool IsArray(this TagType type)
		{
			return ((type == TagType.ByteArray) || (type == TagType.IntArray));
		}

		/// <summary>
		/// Returns if the tag type is a collection (List or Compound).
		/// </summary>
		public static bool IsCollection(this TagType type)
		{
			return ((type == TagType.List) || (type == TagType.Compound));
		}
	}
}

