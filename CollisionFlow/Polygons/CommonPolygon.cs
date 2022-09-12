using System.Collections.Generic;
using System;
using System.Linq;
using Flowing.Mutate;

namespace CollisionFlow.Polygons
{
	class CommonPolygon : Polygon
	{
		public CommonPolygon(IEnumerable<Mutated<LineFunction, Vector2<CourseA>>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.GetTarget()));
		}

		public override Mutated<LineFunction, Vector2<CourseA>>[] Edges { get; }
		public override Vector2<Mutated<double, CourseA>>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = new Rect(Verticies.Select(x => x.GetTarget()));
		}
	}
}
