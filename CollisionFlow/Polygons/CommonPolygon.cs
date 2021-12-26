using System.Collections.Generic;
using System;
using System.Linq;
using Flowing.Mutate;

namespace CollisionFlow.Polygons
{
	class CommonPolygon : Polygon
	{
		public CommonPolygon(IEnumerable<Mutated<LineFunction, Course>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public override Mutated<LineFunction, Course>[] Edges { get; }
		public override Mutated<Vector128, Course>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}
	}
}
