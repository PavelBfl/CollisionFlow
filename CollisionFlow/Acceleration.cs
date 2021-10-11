using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow
{
	public struct Course : IEquatable<Course>
	{
		public static Course Zero { get; } = new Course(Vector<double>.Zero, Vector<double>.Zero);

		public Course(Vector<double> v, Vector<double> a)
		{
			V = v;
			A = a;
		}
		public Vector<double> V { get; }
		public Vector<double> A { get; }

		public Course SetV(double x, double y)
		{
			return new Course(Vector128.Create(x, y), A);
		}
		public Course SetA(double x, double y)
		{
			return new Course(V, Vector128.Create(x, y));
		}

		public bool Equals(Course other)
		{
			return NumberUnitComparer.Instance.Equals(V.GetX(), other.V.GetX()) &&
				NumberUnitComparer.Instance.Equals(V.GetY(), other.V.GetY()) &&
				NumberUnitComparer.Instance.Equals(A.GetX(), other.A.GetX()) &&
				NumberUnitComparer.Instance.Equals(A.GetY(), other.A.GetY());
		}

		public Course Offset(double time) => new Course(V + A * time, A);

		public Vector<double> Offset(Vector<double> vector, double time)
		{
			return Vector128.Create(
				Acceleration.Offset(time, vector.GetX(), V.GetX(), A.GetX()),
				Acceleration.Offset(time, vector.GetY(), V.GetY(), A.GetY())
			);
		}
		public Rect Offset(Rect rect, double time)
		{
			var leftTop = Offset(Vector128.Create(rect.Left, rect.Top), time);

			return Rect.CreateLeftTop(
				leftTop.GetX(),
				leftTop.GetY(),
				rect.Right - rect.Left,
				rect.Top - rect.Bottom
			);
		}
	}
	public class Acceleration
	{
		private static bool IsZero(double value) => Math.Abs(value) < 0.0000001;

		public static double? GetTime(double first, double vFirst, double aFirst, double second, double vSecond, double aSecond)
		{
			if (first < second)
			{
				return GetTimeMinMax(first, vFirst, aFirst, second, vSecond, aSecond);
			}
			else
			{
				return GetTimeMinMax(second, vSecond, aSecond, first, vFirst, aFirst);
			}
		}
		private static double? GetTimeMinMax(double min, double vMin, double aMin, double max, double vMax, double aMax)
		{
			var v = vMin - vMax;
			var a = aMin - aMax;

			if ((IsZero(v) || v < 0) && (IsZero(a) || a < 0))
			{
				return null;
			}
			else if (IsZero(a))
			{
				return Relation.GetTime(Moved.Create(min, vMin), Moved.Create(max, vMax));
			}

			var s = max - min;

			var d = (v * v) - 2 * a * (-s);
			if (IsZero(d))
			{
				var result = -v / a;
				return result;
			}
			else if (d > 0)
			{
				var result1 = (-v + Math.Sqrt(d)) / a;
				if (result1 >= 0)
				{
					return result1;
				}

				var result2 = (-v - Math.Sqrt(d)) / a;
				if (result2 >= 0)
				{
					return result2;
				}

				throw new InvalidOperationException();
			}
			else
			{
				return null;
			}
		}
		public static double Offset(double time, double value, double v, double a)
		{
			return value + v * time + (a * time * time) / 2;
		}
	}
}
