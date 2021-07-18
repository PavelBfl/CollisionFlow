using System;
using System.Numerics;

namespace CollisionFlow
{
	public struct Vector128 : IEquatable<Vector128>
	{
		private const int X_INDEX = 0;
		private const int Y_INDEX = 1;
		private const string DISPLAY_FORMAT = "{0}; {1}";

		private static double[] container = new double[4];
		public static Vector<double> Create(double x, double y)
		{
			container[0] = x;
			container[1] = y;
			return new Vector<double>(container);
		}

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

		public Vector128 ToNormal()
		{
			var length = Math.Sqrt(X * X + Y * Y);
			return new Vector128(X / length, Y / length);
		}

		public Vector<double> ToVector() => Create(X, Y);

		public bool Equals(Vector128 other)
		{
			return NumberUnitComparer.Instance.Equals(X ,other.X) && NumberUnitComparer.Instance.Equals(Y, other.Y);
		}
		public override bool Equals(object obj)
		{
			return obj is Vector128 other && Equals(other);
		}
		public override int GetHashCode() => NumberUnitComparer.Instance.GetHashCode(X) ^ NumberUnitComparer.Instance.GetHashCode(Y);

		public override string ToString()
		{
			return string.Format(DISPLAY_FORMAT, X, Y);
		}
	}
}
