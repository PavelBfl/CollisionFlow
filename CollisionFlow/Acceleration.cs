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

	public struct TimeA
	{
		public TimeA(double result)
			: this(result, result)
		{

		}
		public TimeA(double result1, double result2)
		{
			Result1 = result1;
			Result2 = result2;
		}

		public double Result1 { get; }
		public double Result2 { get; }
	}
	public class Acceleration
	{
		private static bool IsZero(double value) => Math.Abs(value) < 0.000000000001;

		public static TimeA? GetTime(double first, double vFirst, double aFirst, double second, double vSecond, double aSecond)
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
		private static TimeA? GetTimeMinMax(double min, double vMin, double aMin, double max, double vMax, double aMax)
		{
			var v = vMin - vMax;
			var a = aMin - aMax;

			if ((IsZero(v) || v < 0) && (IsZero(a) || a < 0))
			{
				return null;
			}
			else if (IsZero(a))
			{
				var time = Relation.GetTime(Moved.Create(min, vMin), Moved.Create(max, vMax));

				return time is null ? new TimeA?() : new TimeA(time.Value);
			}

			var s = max - min;
			// (a/2)t^2 + vt - s = 0
			var d = (v * v) - 2 * a * (-s);
			if (d == 0)
			{
				var result = -v / a;
				return new TimeA(result);
			}
			else if (d > 0)
			{
				var sqrtD = Math.Sqrt(d);
				var result1 = (-v + sqrtD) / a;
				var result2 = (-v - sqrtD) / a;

				return new TimeA(result1, result2);
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
