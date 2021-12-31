using Xunit;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Flowing.Mutate.Test
{
	public class NumberUnitComparerTest
	{
		private static IEnumerable<double> LessHalf { get; } = new[] { 0, 0.1, 0.4, 0.49999999 };
		private static double Half { get; } = 0.5;
		private static IEnumerable<double> GreatHalf { get; } = new[] { 0.50001, 1, 2 };
		private static IEnumerable<double> Epsilons { get; } = new[] { 0.1, 0.25, 0.0001 };
		private static IEnumerable<int> ValuesCounters { get; } = new[] { 1, 2, 10, 1000 };

		public static IEnumerable<object[]> GetZeroEqual()
		{
			foreach (var @params in from epsilon in Epsilons
								 from offset in LessHalf.Concat(LessHalf.Select(x => -x))
								 select new object[] { epsilon, offset })
			{
				yield return @params;
			}
		}
		public static IEnumerable<object[]> GetZeroNotEqual()
		{
			var offsets = GreatHalf.Prepend(Half);
			foreach (var @params in from epsilon in Epsilons
									from offset in offsets.Concat(offsets.Select(x => -x))
									select new object[] { epsilon, offset })
			{
				yield return @params;
			}
		}
		public static IEnumerable<object[]> GetValuesEquals()
		{
			foreach (var @params in from offset in LessHalf.Select(x => -x).Concat(LessHalf)
								 from epsilon in Epsilons
								 from valueCount in ValuesCounters
								 select new { offset, epsilon, valueCount })
			{
				var value = @params.epsilon * @params.valueCount;
				yield return new object[] { @params.epsilon, value, value + @params.offset * @params.epsilon };
			}
		}

		[Theory]
		[MemberData(nameof(GetZeroEqual))]
		public void Equals_ZeroCompare_Success(double epsilon, double offset)
		{
			var comparer = new NumberUnitComparer(epsilon);

			Assert.Equal(0, epsilon * offset, comparer);
		}

		[Theory]
		[MemberData(nameof(GetZeroNotEqual))]
		public void Equals_ZeroCompare_Fail(double epsilon, double offset)
		{
			var comparer = new NumberUnitComparer(epsilon);

			Assert.NotEqual(0, epsilon * offset, comparer);
		}

		[Theory]
		[MemberData(nameof(GetValuesEquals))]
		public void Equals_Compare_Success(double epsilon, double mainValue, double otherValue)
		{
			var comparer = new NumberUnitComparer(epsilon);

			Assert.Equal(mainValue, otherValue, comparer);
		}
	}
}