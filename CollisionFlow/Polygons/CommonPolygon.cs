using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	class CommonPolygon : Polygon
	{
		public CommonPolygon(IEnumerable<Moved<LineFunction, Vector128>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			CourseQuadrant = Edges.Aggregate(Quadrant.None, (a, x) => a | GetQuadrant(x.Course));
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public override Quadrant CourseQuadrant { get; }

		public override Moved<LineFunction, Vector128>[] Edges { get; }
		public override Moved<Vector128, Vector128>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}
	}
}
