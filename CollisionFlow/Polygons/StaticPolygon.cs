using System.Collections.Generic;
using System;
using System.Linq;
using Flowing.Mutate;

namespace CollisionFlow.Polygons
{
	class StaticPolygon : Polygon
	{
		public StaticPolygon(IEnumerable<LineFunction> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = edges.Select(x => Moved.Create(x, Vector2Extensions.CourseZero)).ToArray();
			Verticies = GetVerticies(Edges);
			Bounds = new Rect(Verticies.Select(x => x.GetTarget()));
		}

		public override Mutated<LineFunction, Vector2<CourseA>>[] Edges { get; }
		public override Vector2<Mutated<double, CourseA>>[] Verticies { get; }
		public override Rect Bounds { get; }

		public override void Offset(double value)
		{
			
		}
	}
}
