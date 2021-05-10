namespace CollisionFlow
{
	public class CollisionResult
	{
		public CollisionResult(CollisionPolygon main, Moved<LineFunction, Vector128> mainLine, CollisionPolygon other, Moved<Vector128, Vector128> otherPoint, double offset)
		{
			Main = main;
			MainLine = mainLine;
			Other = other;
			OtherPoint = otherPoint;
			Offset = offset;
		}

		public CollisionPolygon Main { get; }
		public Moved<LineFunction, Vector128> MainLine { get; }
		public CollisionPolygon Other { get; }
		public Moved<Vector128, Vector128> OtherPoint { get; }
		public double Offset { get; }
	}
}
