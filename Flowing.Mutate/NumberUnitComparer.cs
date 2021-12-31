using System;

namespace Flowing.Mutate
{
	public class NumberUnitComparer : INumberUnitComparer
	{
		public NumberUnitComparer(double min, double max, double epsilon)
		{
			Min = min;
			Max = max;
			Epsilon = epsilon;
		}
		public NumberUnitComparer(double epsilon)
			: this(long.MinValue / epsilon, long.MaxValue / epsilon, epsilon)
		{
			
		}

		public double Min { get; }
		public double Max { get; }
		public double Epsilon { get; }

		private long GetUnit(double value) => (long)(value / Epsilon + (value < 0 ? -0.5 : 0.5));

		public bool InRange(double value) => Min < value && value < Max;

		public bool IsZero(double value) => Equals(value, 0);

		public bool Equals(double x, double y) => GetUnit(x) == GetUnit(y);
		public int GetHashCode(double obj) => GetUnit(obj).GetHashCode();
		public int Compare(double x, double y)
		{
			var result = GetUnit(x) - GetUnit(y);
			if (result <= int.MinValue)
			{
				return int.MinValue;
			}
			else if (result >= int.MaxValue)
			{
				return int.MaxValue;
			}
			else
			{
				return (int)result;
			}
		}
	}
}
