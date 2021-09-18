using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	class UndeformablePolygon : Polygon
	{
		public UndeformablePolygon(IEnumerable<LineFunction> edges, Vector128 course)
		{
			Course = course;
			var edgesInstance = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Edges = new Moved<LineFunction, Vector128>[edgesInstance.Length];
			for (int i = 0; i < edgesInstance.Length; i++)
			{
				Edges[i] = Moved.Create(edgesInstance[i], Course);
			}
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
			BoundsCourse = new Rect(Course.X, Course.X, Course.Y, Course.Y);
		}

		public Vector128 Course { get; }
		public override Moved<LineFunction, Vector128>[] Edges { get; }
		public override Moved<Vector128, Vector128>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;
		public override Rect BoundsCourse { get; }

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = bounds.Offset((Course.ToVector() * value).ToVector128());
		}
	}
}
