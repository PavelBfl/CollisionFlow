using System.Numerics;

namespace CollisionFlow
{
	public static class VectorExtensions
	{
		private const int X_INDEX = 0;
		private const int Y_INDEX = 1;

		public static T GetX<T>(this Vector<T> vector) where T : struct
			=> vector[X_INDEX];
		public static T GetY<T>(this Vector<T> vector) where T : struct
			=> vector[Y_INDEX];

		public static Vector128 ToVector128(this Vector<double> vector) => new Vector128(vector);
	}
}
