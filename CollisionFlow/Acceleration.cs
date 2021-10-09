using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow
{
	public struct Course
	{
		public Course(Vector<double> v, Vector<double> a)
		{
			V = v;
			A = a;
		}
		public Vector<double> V { get; }
		public Vector<double> A { get; }

		public Course Offset(double time) => new Course(V + A * time, A);
	}
	public class Acceleration
	{
		private static bool IsZero(double value) => Math.Abs(value) < 0.0000001;
		public static double? GetTime(double min, double vMin, double aMin, double max, double vMax, double aMax)
		{
			var v = vMin - vMax;
			var a = aMin - aMax;

			if ((IsZero(v) || v < 0) && (IsZero(a) || a < 0))
			{
				return null;
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
				if (result1 > 0)
				{
					return result1;
				}

				var result2 = (-v - Math.Sqrt(d)) / a;
				if (result2 > 0)
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
