using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CollisionFlow.Polygons
{
	class EdgesCollection : IList<Moved<LineFunction, Vector128>>
	{
		public EdgesCollection()
		{

		}
		public EdgesCollection(IEnumerable<Moved<LineFunction, Vector128>> edges)
		{
			Edges.AddRange(edges);
		}

		public Moved<LineFunction, Vector128> this[int index]
		{
			get => Edges[index];
			set
			{
				Edges[index] = value;
				IsChanged = true;
			}
		}
		private List<Moved<LineFunction, Vector128>> Edges { get; } = new List<Moved<LineFunction, Vector128>>();

		public bool IsChanged { get; private set; } = false;
		public void ChangesHandled() => IsChanged = false;

		public int Count => Edges.Count;
		public bool IsReadOnly => false;
		public void Add(Moved<LineFunction, Vector128> item)
		{
			Edges.Add(item);
			IsChanged = true;
		}

		public void Clear()
		{
			Edges.Clear();
			IsChanged = true;
		}

		public bool Contains(Moved<LineFunction, Vector128> item)
		{
			return Edges.Contains(item);
		}

		public void CopyTo(Moved<LineFunction, Vector128>[] array, int arrayIndex)
		{
			((ICollection<Moved<LineFunction, Vector128>>)Edges).CopyTo(array, arrayIndex);
		}

		public IEnumerator<Moved<LineFunction, Vector128>> GetEnumerator() => Edges.GetEnumerator();
		public int IndexOf(Moved<LineFunction, Vector128> item) => Edges.IndexOf(item);

		public void Insert(int index, Moved<LineFunction, Vector128> item)
		{
			Edges.Insert(index, item);
			IsChanged = true;
		}

		public bool Remove(Moved<LineFunction, Vector128> item)
		{
			var result = Edges.Remove(item);
			if (result)
			{
				IsChanged = true;
			}
			return result;
		}

		public void RemoveAt(int index)
		{
			Edges.RemoveAt(index);
			IsChanged = true;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
