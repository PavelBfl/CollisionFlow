using System.Collections.Generic;
using System.Numerics;

namespace Flowing.Mutate
{
	public static class UnitComparer
	{
		public static INumberUnitComparer Position { get; } = new NumberUnitComparer(0.00001);
		public static IEqualityComparer<Vector<double>> VectorPosition { get; } = new VectorUnitComparer(Position);
		public static INumberUnitComparer Time { get; } = new NumberUnitComparer(0.00001);
	}
}
