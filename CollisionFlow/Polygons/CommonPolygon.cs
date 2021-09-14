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
		}

		public override Quadrant CourseQuadrant { get; }

		public override Moved<LineFunction, Vector128>[] Edges { get; }

		private Moved<Vector128, Vector128>[] vertices = null;
		public override Moved<Vector128, Vector128>[] Verticies => vertices ?? (vertices = GetVerticies(Edges));

		private Rect? bounds;
		public override Rect Bounds
		{
			get
			{
				if (bounds is null)
				{
					bounds = new Rect(Verticies.Select(x => x.Target));
				}
				return bounds.Value;
			}
		}

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = null;
		}
	}
}
