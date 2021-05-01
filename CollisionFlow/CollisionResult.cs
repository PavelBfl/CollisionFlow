namespace CollisionFlow
{
	public struct CollisionResult
	{
		public CollisionResult(double offset)
			: this(null, null, offset)
		{

		}
		public CollisionResult(CollisionPolygon polygon1, CollisionPolygon polygon2, double offset)
		{
			Polygon1 = polygon1;
			Polygon2 = polygon2;
			Offset = offset;
		}

		public CollisionPolygon Polygon1 { get; }
		public CollisionPolygon Polygon2 { get; }
		public double Offset { get; }
	}
}
