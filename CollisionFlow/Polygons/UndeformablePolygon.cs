using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	class UndeformablePolygon : Polygon
	{
		public UndeformablePolygon(IEnumerable<LineFunction> edges, Course course)
		{
			Course = course;
			var edgesInstance = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Edges = new Moved<LineFunction, Course>[edgesInstance.Length];
			for (int i = 0; i < edgesInstance.Length; i++)
			{
				Edges[i] = Moved.Create(edgesInstance[i], Course);
			}
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public Course Course { get; }
		public override Moved<LineFunction, Course>[] Edges { get; }
		public override Moved<Vector128, Course>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double time)
		{
			base.Offset(time);
			bounds = Course.Offset(bounds, time);
		}
	}
}
