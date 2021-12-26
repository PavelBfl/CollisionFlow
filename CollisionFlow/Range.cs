using Flowing.Mutate;
using System;

namespace CollisionFlow
{
	public struct Range
	{
		public static Range Auto(double first, double second) => UnitComparer.Position.Compare(first, second) < 0 ? new Range(first, second) : new Range(second, first);

		public Range(double min, double max)
		{
			if (UnitComparer.Position.Compare(min, max) > 0)
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
			return UnitComparer.Position.Compare(Min, value) < 0 && UnitComparer.Position.Compare(value, Max) <= 0;
		}
		public bool ContainsExEx(double value)
		{
			return UnitComparer.Position.Compare(Min, value) < 0 && UnitComparer.Position.Compare(value, Max) < 0;
		}
		public bool Contains(Range range)
		{
			return UnitComparer.Position.Compare(Min, range.Min) < 0 && UnitComparer.Position.Compare(range.Max, Max) <= 0;
		}
		public bool Intersect(Range range)
		{
			return UnitComparer.Position.Compare(Min, range.Max) < 0 && UnitComparer.Position.Compare(range.Min, Max) < 0;
		}
	}
}
