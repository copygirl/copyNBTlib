using System;
using System.Collections.Generic;
using System.IO;
using copyNBTlib.IO;

namespace copyNBTlib
{
	public class TagList : TagBase, IList<TagBase>
	{
		readonly List<TagBase> _tags = new List<TagBase>();

		TagType? _listType;

		#region Public properties

		/// <summary>
		/// Gets or sets whether this list changes its ListType
		/// dynamically based on which type of tags it contains.
		/// </summary>
		public bool Dynamic {
			get { return !_listType.HasValue; }
			set {
				if (!value && (Count <= 0))
					throw new InvalidOperationException("Can't set Dynamic to false while list is empty, set ListType directly instead");
				_listType = (value ? default(TagType?) : this[0].Type);
			}
		}

		/// <summary>
		/// Gets or sets the type of tags in the list.
		/// Returns the type of the list if it's not dynamic or the type of its elements if it's
		/// dynamic (null if empty). Can't change the type of the list while it contains elements.
		/// </summary>
		public TagType? ListType {
			get { return _listType ?? ((Count > 0) ? this[0].Type : default(TagType?)); }
			set {
				if (!value.HasValue)
					throw new ArgumentNullException("value", "Can't set ListType to null, set Dynamic to true instead");
				if (Count > 0)
					throw new InvalidOperationException("Can't change ListType while list isn't empty");
				_listType = value;
			}
		}

		#endregion


		public TagList(TagType? listType = null)
			: base(TagType.List)
		{
			_listType = listType;
		}


		#region TagBase implementation (read / write payload)

		public override void ReadPayload(NBTStreamReader reader)
		{
			Clear();

			var type = reader.ReadTagType();
			int count = reader.ReadInt();
			if ((type == TagType.End) && (Count > 0))
				throw new InvalidDataException("List tag with ListType of 'End' isn't empty");
			_tags.Capacity = count;

			_listType = ((Count > 0) ? type : default(TagType?));

			for (int i = 0; i < count; i++) {
				var tag = CreateTagFromType(type);
				tag.ReadPayload(reader);
				_tags.Add(tag);
			}
		}

		public override void WritePayload(NBTStreamWriter writer)
		{
			writer.Write(ListType ?? TagType.End);
			writer.Write(Count);
			foreach (var tag in this)
				tag.WritePayload(writer);
		}

		#endregion


		#region IList implementation

		// Note: This (incorrectly?) throws an ArgumentException if the list
		//       is dynamic and only has one element that's being replaced.
		public override TagBase this[int index] {
			get { return _tags[index]; }
			set { _tags[index] = VerifyTag(value); }
		}


		public int IndexOf(TagBase tag)
		{
			return _tags.IndexOf(tag);
		}

		public override void Insert(int index, TagBase tag)
		{
			_tags.Insert(index, VerifyTag(tag));
		}

		public override void RemoveAt(int index)
		{
			_tags.RemoveAt(index);
		}

		#endregion

		#region ICollection implementation

		public bool IsReadOnly { get { return false; } }

		public override int Count { get { return _tags.Count; } }


		public bool Contains(TagBase tag)
		{
			return _tags.Contains(tag);
		}

		public override void Add(TagBase tag)
		{
			_tags.Add(VerifyTag(tag));
		}

		public bool Remove(TagBase tag)
		{
			return _tags.Remove(tag);
		}

		public override void Clear()
		{
			_tags.Clear();
		}

		public void CopyTo(TagBase[] array, int arrayIndex)
		{
			_tags.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<TagBase> GetEnumerator()
		{
			return _tags.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion


		TagBase VerifyTag(TagBase tag)
		{
			if (tag == null)
				throw new ArgumentNullException("tag");
			if (ListType.HasValue && (ListType.Value != tag.Type))
				throw new ArgumentException(string.Format(
					"Can't add tag of type '{0}' to list of type '{1}'", tag.Type, ListType), "tag");
			return tag;
		}
	}
}

