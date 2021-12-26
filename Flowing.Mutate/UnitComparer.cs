namespace Flowing.Mutate
{
	public static class UnitComparer
	{
		public static INumberUnitComparer Position { get; } = new NumberUnitComparer(0.00001);
		public static INumberUnitComparer Time { get; } = new NumberUnitComparer(0.00001);
	}
}
