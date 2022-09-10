using System;

namespace Flowing.Mutate
{
	public static class Vector2Builder
	{
		public static Vector2<T> Create<T>(T x, T y) => new Vector2<T>(x, y);
		public static Vector2<T> Create<T>(T value) => Create(value, value);

		public static Vector2<double> ToNormal(this Vector2<double> vector)
		{
			var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			return Create(vector.X / length, vector.Y / length);
		}
	}
}
