using System;
using System.Collections.Generic;
using System.Text;

namespace CollisionFlow
{
	class NumberUnitComparer : IEqualityComparer<double>
	{
		public const double PRECISION = 0.000001;
		public static NumberUnitComparer Instance { get; } = new NumberUnitComparer();

		private NumberUnitComparer()
		{

		}

		public bool Equals(double x, double y)
		{
			return Math.Abs(x - y) < PRECISION;
		}

		public int GetHashCode(double obj)
		{
			throw new NotImplementedException();
		}
	}
}
