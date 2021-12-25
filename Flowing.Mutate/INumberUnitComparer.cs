using System.Collections.Generic;

namespace Flowing.Mutate
{
	public interface INumberUnitComparer : IEqualityComparer<double>, IComparer<double>
	{
		double Min { get; }
		double Max { get; }
		double Epsilon { get; }
	}
}
