using System;
using System.Numerics;

namespace CollisionFlow
{
	public struct Vector128 : IEquatable<Vector128>
	{
		private const int X_INDEX = 0;
		private const int Y_INDEX = 1;

		public static Vector<double> Create(double x, double y) => new Vector<double>(new[] { x, y, 0, 0 });
		public static Vector128 Zero { get; } = new Vector128(Vector<double>.Zero);
		public static Vector128 One { get; } = new Vector128(Vector<double>.One);

		public Vector128(double x, double y)
		{
			X = x;
			Y = y;
		}
		public Vector128(Vector<double> vector)
			: this(vector[X_INDEX], vector[Y_INDEX])
		{
			
		}

		public double X { get; }
		public double Y { get; }

		public Vector<double> ToVector() => Create(X, Y);

		public bool Equals(Vector128 other)
		{
			return X == other.X && Y == other.Y;
		}
		public override bool Equals(object obj)
		{
			return obj is Vector128 other && Equals(other);
		}
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
	}
}
