using System;
using System.Collections.Generic;
using System.Text;

namespace CollisionFlow
{
	public class NumberUnitComparer : IEqualityComparer<double>, IComparer<double>
	{
		private const long OFFSET_PRECISION = 10000;
		private const long MIN = long.MinValue / OFFSET_PRECISION;
		private const long MAX = long.MaxValue / OFFSET_PRECISION;

		public static NumberUnitComparer Instance { get; } = new NumberUnitComparer();

		private static long Offset(double value) => (long)Math.Round(value * OFFSET_PRECISION);

		private NumberUnitComparer()
		{
			
		}

		public double Epsilon => 1d / OFFSET_PRECISION;

		public bool InRange(double value) => MIN < value && value < MAX;

		public bool IsZero(double value) => Equals(value, 0);

		public bool Equals(double x, double y) => Offset(x) == Offset(y);

		public int GetHashCode(double obj) => Offset(obj).GetHashCode();

		public int Compare(double x, double y)
		{
			var result = Offset(x) - Offset(y);
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
