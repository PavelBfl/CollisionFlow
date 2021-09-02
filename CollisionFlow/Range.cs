using System;

namespace CollisionFlow
{
	public struct Range
	{
		public static Range Auto(double first, double second) => NumberUnitComparer.Instance.Compare(first, second) < 0 ? new Range(first, second) : new Range(second, first);

		public Range(double min, double max)
		{
			if (NumberUnitComparer.Instance.Compare(min, max) > 0)
			{
				throw new InvalidOperationException();
			}

			Min = min;
			Max = max;
		}

		public double Min { get; }
		public double Max { get; }

		public bool Contains(double value)
		{
			return NumberUnitComparer.Instance.Compare(Min, value) < 0 && NumberUnitComparer.Instance.Compare(value, Max) <= 0;
		}
		public bool ContainsExEx(double value)
		{
			return NumberUnitComparer.Instance.Compare(Min, value) < 0 && NumberUnitComparer.Instance.Compare(value, Max) < 0;
		}
		public bool Contains(Range range)
		{
			return NumberUnitComparer.Instance.Compare(Min, range.Min) < 0 && NumberUnitComparer.Instance.Compare(range.Max, Max) <= 0;
		}
		public bool Intersect(Range range)
		{
			return NumberUnitComparer.Instance.Compare(Min, range.Max) < 0 && NumberUnitComparer.Instance.Compare(range.Min, Max) < 0;
		}
	}
}
