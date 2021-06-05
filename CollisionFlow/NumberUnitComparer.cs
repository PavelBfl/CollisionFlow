﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CollisionFlow
{
	public class NumberUnitComparer : IEqualityComparer<double>, IComparer<double>
	{
		private const long OFFSET_PRECISION = 10000;

		public static NumberUnitComparer Instance { get; } = new NumberUnitComparer();

		private static long Offset(double value) => (long)Math.Round(value * OFFSET_PRECISION);

		private NumberUnitComparer()
		{
			
		}

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