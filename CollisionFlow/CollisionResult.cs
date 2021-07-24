namespace CollisionFlow
{
	public class CollisionResult
	{
		public CollisionResult(IPolygonHandler main, Moved<LineFunction, Vector128> mainLine, IPolygonHandler other, Moved<Vector128, Vector128> otherPoint, double offset)
		{
			Main = main;
			MainLine = mainLine;
			Other = other;
			OtherPoint = otherPoint;
			Offset = offset;
		}

		public IPolygonHandler Main { get; }
		public Moved<LineFunction, Vector128> MainLine { get; }
		public IPolygonHandler Other { get; }
		public Moved<Vector128, Vector128> OtherPoint { get; }
		public double Offset { get; private set; }

		public void Step(double value)
		{
			Offset -= value;
		}
	}
}
