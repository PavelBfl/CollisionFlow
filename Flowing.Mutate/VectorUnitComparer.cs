using System;
using System.Collections.Generic;
using System.Numerics;

namespace Flowing.Mutate
{
	internal class VectorUnitComparer : IEqualityComparer<Vector<double>>
	{
		public VectorUnitComparer(IEqualityComparer<double> unitComparer)
		{
			UnitComparer = unitComparer ?? throw new ArgumentNullException(nameof(unitComparer));
		}

		public IEqualityComparer<double> UnitComparer { get; }
		public bool Equals(Vector<double> x, Vector<double> y)
		{
			for (int i = 0; i < Vector<double>.Count; i++)
			{
				if (!UnitComparer.Equals(x[i], y[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(Vector<double> obj)
		{
			var result = 0;
			for (int i = 0; i < Vector<double>.Count; i++)
			{
				result ^= UnitComparer.GetHashCode(obj[i]);
			}
			return result;
		}
	}
}
