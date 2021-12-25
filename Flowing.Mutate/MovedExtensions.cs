using System;
using System.Collections.Generic;
using System.Text;

namespace Flowing.Mutate
{
	public static class MovedExtensions
	{
		public static TimeA? GetTimeCollision(this Moved<double, CourseA> main, Moved<double, CourseA> other)
		{
			if (main.Target < other.Target)
			{
				return GetTimeMinMaxCollision(main, other);
			}
			else
			{
				return GetTimeMinMaxCollision(other, main);
			}
		}

		private static bool IsZero(double value) => Math.Abs(value) < 0.000000000001;
		private static TimeA? GetTimeMinMaxCollision(Moved<double, CourseA> min, Moved<double, CourseA> max)
		{
			var v = min.Course.V - max.Course.V;
			var a = min.Course.A - max.Course.A;

			if ((IsZero(v) || v < 0) && (IsZero(a) || a < 0))
			{
				return null;
			}
			else if (IsZero(a))
			{
				var time = GetTimeCollision(new Moved<double, double>(min.Target, min.Course.V), new Moved<double, double>(max.Target, max.Course.V));

				return time is null ? new TimeA?() : new TimeA(time.Value);
			}

			var s = max.Target - min.Target;
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

		public static double? GetTimeCollision(Moved<double, double> point1, Moved<double, double> point2)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return NumberUnitComparer.Instance.InRange(localOffset) ? localOffset : new double?();
			}
			else
			{
				return null;
			}
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
}
