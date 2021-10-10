using System.Collections.Generic;
using System;
using System.Linq;

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
			Edges = edges.Select(x => Moved.Create(x, Course.Zero)).ToArray();
			Verticies = GetVerticies(Edges);
			Bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public override Moved<LineFunction, Course>[] Edges { get; }
		public override Moved<Vector128, Course>[] Verticies { get; }
		public override Rect Bounds { get; }

		public override void Offset(double value)
		{
			
		}
	}
}
