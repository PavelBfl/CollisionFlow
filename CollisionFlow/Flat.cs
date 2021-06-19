using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollisionFlow
{
	enum AllowedOffset
	{
		Allow,
		Never,
		Collision,
	}
	class Flat
	{
		private const ulong RIGHT = 1;
		private const ulong LEFT = 1UL << 63;
		private const int ACTIVE_RANGE = 62;

		private const double GLOBAL_BEGIN = -200;
		private const double GLOBAL_LENGTH = 400;

		public Flat(IEnumerable<Moved<double, double>> points)
		{
			if (points is null)
			{
				throw new ArgumentNullException(nameof(points));
			}

			Points = points.Distinct(PointComparer.Instance).ToArray();
			if (Points.Length == 0)
			{
				throw new InvalidCollisiopnException();
			}

			Group = GetGroup();
		}

		public Moved<double, double>[] Points { get; }
		public ulong Group { get; private set; }

		private ulong GetGroup()
		{
			return 0;
			var max = double.NegativeInfinity;
			var min = double.PositiveInfinity;
			foreach (var point in Points)
			{
				max = Math.Max(max, point.Target);
				min = Math.Min(min, point.Target);
			}

			return GetGroup(GLOBAL_BEGIN, GLOBAL_LENGTH, min, max - min);
		}

		public void Offset(double value)
		{
			for (int i = 0; i < Points.Length; i++)
			{
				Points[i] = Points[i].Offset(value);
			}
			Group = GetGroup();
		}
		public double GetAllowedOffset(Flat other, double value)
		{
			if (other is null)
			{
				throw new ArgumentNullException(nameof(other));
			}

			var result = value;
			foreach (var mainPoint in Points)
			{
				foreach (var otherPoint in other.Points)
				{
					result = Math.Min(result, Offset(mainPoint, otherPoint, result));

					if (NumberUnitComparer.Instance.Equals(result, 0))
					{
						return 0;
					}
				}
			}
			return result;
		}
		public AllowedOffset IsAllowedOffset(Flat other, double value)
		{
			if (other is null)
			{
				throw new ArgumentNullException(nameof(other));
			}

			var result = AllowedOffset.Never;
			foreach (var mainPoint in Points)
			{
				foreach (var otherPoint in other.Points)
				{
					switch (IsAllowedOffset(mainPoint, otherPoint, value))
					{
						case AllowedOffset.Allow:
							result = AllowedOffset.Allow;
							break;
						case AllowedOffset.Collision:
							return AllowedOffset.Collision;
					}
				}
			}
			return result;
		}
		private static AllowedOffset IsAllowedOffset(Moved<double, double> point1, Moved<double, double> point2, double value)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return NumberUnitComparer.Instance.Compare(localOffset, value) >= 0 ? AllowedOffset.Allow : AllowedOffset.Collision;
			}
			else
			{
				return AllowedOffset.Never;
			}
		}


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

		public static ulong GetGroup(double globalBegin, double globalLength, double rangeBegin, double rangeLength)
		{
			if (NumberUnitComparer.Instance.Compare(globalLength, 0) < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(globalLength));
			}
			if (NumberUnitComparer.Instance.Compare(rangeLength, 0) < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rangeLength));
			}

			var left = globalBegin <= rangeBegin ? GroupSide(rangeBegin - globalBegin, globalLength) : LEFT;

			var globalEnd = globalBegin + globalLength;
			var rangeEnd = rangeBegin + rangeLength;
			var right = rangeEnd <= globalEnd ? GroupSide(rangeEnd - globalBegin, globalLength) : RIGHT;

			return UnionGroup(left, right);
		}
		private static ulong GroupSide(double localLength, double globalLength)
		{
			var offset = (int)(localLength * ACTIVE_RANGE / globalLength);
			return RIGHT >> offset;
		}

		public static ulong UnionGroup(ulong group1, ulong group2)
		{
			if (group1 == 0 || group2 == 0)
			{
				throw new InvalidCollisiopnException();
			}
			else if ((group1 & group2) != 0)
			{
				return group1 | group2;
			}
			else
			{
				ulong gap = group1 | group2;
				ulong rightOffset = RIGHT;
				while ((gap & rightOffset) == 0)
				{
					gap |= rightOffset;
					rightOffset <<= 1;
				}
				ulong leftOffset = LEFT;
				while ((gap & leftOffset) == 0)
				{
					gap |= leftOffset;
					leftOffset >>= 1;
				}

				return group1 | group2 | ~gap;
			}
		}

		private class PointComparer : IEqualityComparer<Moved<double, double>>
		{
			public static PointComparer Instance { get; } = new PointComparer();

			private PointComparer()
			{

			}

			public bool Equals(Moved<double, double> x, Moved<double, double> y)
			{
				return NumberUnitComparer.Instance.Equals(x.Target, y.Target) && NumberUnitComparer.Instance.Equals(x.Course, y.Course);
			}

			public int GetHashCode(Moved<double, double> obj)
			{
				return NumberUnitComparer.Instance.GetHashCode(obj.Target) ^ NumberUnitComparer.Instance.GetHashCode(obj.Course);
			}
		}
	}
}
