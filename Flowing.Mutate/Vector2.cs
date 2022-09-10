using System;
using System.Collections.Generic;
using System.Numerics;

namespace Flowing.Mutate
{

	public struct Vector2<T>
	{
		public Vector2(T x, T y)
		{
			X = x;
			Y = y;
		}

		public T X { get; }
		public T Y { get; }

		public bool Equals(Vector2<T> other, IEqualityComparer<T> itemComparer)
		{
			if (itemComparer is null)
			{
				throw new ArgumentNullException(nameof(itemComparer));
			}

			return itemComparer.Equals(X, other.X) && itemComparer.Equals(Y, other.Y);
		}

		public int GetHashCode(IEqualityComparer<T> itemComparer)
		{
			if (itemComparer is null)
			{
				throw new ArgumentNullException(nameof(itemComparer));
			}

			return itemComparer.GetHashCode(X) ^ itemComparer.GetHashCode(Y);
		}
	}
}
