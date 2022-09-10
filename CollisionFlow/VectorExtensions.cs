using System.Numerics;
using Flowing.Mutate;

namespace CollisionFlow
{
	public static class VectorExtensions
	{
		private const int X_INDEX = 0;
		private const int Y_INDEX = 1;

		public static Vector<T> Create2d<T>(T x, T y)
			where T : struct
			=> new Vector<T>(new T[] { x, y, default, default });

		public static T GetX<T>(this Vector<T> vector) where T : struct
			=> vector[X_INDEX];
		public static T GetY<T>(this Vector<T> vector) where T : struct
			=> vector[Y_INDEX];

		public static Vector<T> ToVector<T>(this Vector2<T> vector)
			where T : struct
			=> Create2d(vector.X, vector.Y);

		public static Vector2<T> ToVector2<T>(this Vector<T> vector)
			where T : struct
			=> new Vector2<T>(vector.GetX(), vector.GetY());
	}
}
