using System;
using System.Collections.Generic;
using System.Text;

namespace CollisionFlow
{
	struct Flat
	{
		public static double Offset(Moved<double, double> point1, Moved<double, double> point2, double offset)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return localOffset < offset ? localOffset : offset;
			}
			else
			{
				return offset;
			}
		}

		private static bool TryOffset(Moved<double, double> point1, Moved<double, double> point2, double offset)
		{
			return point1.Target < point2.Target ? TryOffsetMinMax(point1, point2, offset) : TryOffsetMinMax(point2, point1, offset);
		}
		private static bool TryOffsetMinMax(Moved<double, double> min, Moved<double, double> max, double offset)
		{
			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return NumberUnitComparer.Instance.Compare(localOffset, offset) >= 0;
			}
			else
			{
				return true;
			}
		}

		public static double Offset(IEnumerable<Moved<double, double>> points1, IEnumerable<Moved<double, double>> points2, double offset)
		{
			var result = offset;
			foreach (var point1 in points1)
			{
				foreach (var point2 in points2)
				{
					result = Math.Min(result, Offset(point1, point2, result));

					if (NumberUnitComparer.Instance.Equals(result, 0))
					{
						return 0;
					}
				}
			}
			return result;
		}
		public static bool TryOffset(Moved<Vector128, Vector128>[] points1, Moved<Vector128, Vector128>[] points2, double offset)
		{
			foreach (var point1 in points1)
			{
				foreach (var point2 in points2)
				{
					var xResult = TryOffset(Moved.Create(point1.Target.X, point1.Course.X), Moved.Create(point2.Target.X, point2.Course.X), offset);
					if (!xResult)
					{
						return false;
					}
					var yResult = TryOffset(Moved.Create(point1.Target.Y, point1.Course.Y), Moved.Create(point2.Target.Y, point2.Course.Y), offset);
					if (!yResult)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static long Group(double gloabalMin, double globalMax, double rangeMin, double rangeMax)
		{
			var globalSize = globalMax - gloabalMin;
			var groupSize = globalSize / 64;

			var result = 0L;
			for (double i = rangeMin - gloabalMin; i < rangeMax - gloabalMin; i += groupSize)
			{
				result |= 1L << (int)(i / groupSize);
			}
			result |= 1L << (int)((rangeMax - gloabalMin) / groupSize);

			return result;
		}
	}
}
