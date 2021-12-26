using System.Collections.Generic;

namespace Flowing.Mutate
{
	public interface INumberUnitComparer : IEqualityComparer<double>, IComparer<double>
	{
		double Min { get; }
		double Max { get; }
		double Epsilon { get; }
	}

	public static class NumberUnitComparerExtensiond
	{
		public static bool InRange(this INumberUnitComparer comparer, double value) => comparer.Min < value && value < comparer.Max;
		public static bool IsZero(this IEqualityComparer<double> comparer, double value) => comparer.Equals(value, 0);
		public static double Increment(this INumberUnitComparer comparer, double value) => value + comparer.Epsilon;
		public static double Decrement(this INumberUnitComparer comparer, double value) => value - comparer.Epsilon;
	}
}
