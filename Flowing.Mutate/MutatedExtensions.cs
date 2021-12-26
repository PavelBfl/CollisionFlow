using System;
using System.Collections.Generic;
using System.Text;

namespace Flowing.Mutate
{
	public static class MutatedExtensions
	{
		public static TimeA? GetTimeCollision(this Mutated<double, CourseA> main, Mutated<double, CourseA> other)
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
		private static TimeA? GetTimeMinMaxCollision(Mutated<double, CourseA> min, Mutated<double, CourseA> max)
		{
			var v = min.Course.V - max.Course.V;
			var a = min.Course.A - max.Course.A;

			if ((IsZero(v) || v < 0) && (IsZero(a) || a < 0))
			{
				return null;
			}
			else if (IsZero(a))
			{
				var time = GetTimeCollision(min.SetCourse(min.Course.V), max.SetCourse(max.Course.V));

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

		public static double? GetTimeCollision(Mutated<double, double> point1, Mutated<double, double> point2)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localTime = distance / speed;
				return UnitComparer.Time.InRange(localTime) ? localTime : new double?();
			}
			else
			{
				return null;
			}
		}
	}
}
