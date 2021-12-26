namespace Flowing.Mutate
{
	public struct TimeA
	{
		public TimeA(double result)
			: this(result, result)
		{

		}
		public TimeA(double result1, double result2)
		{
			Result1 = result1;
			Result2 = result2;
		}

		public double Result1 { get; }
		public double Result2 { get; }
	}
}
