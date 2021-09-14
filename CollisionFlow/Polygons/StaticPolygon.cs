﻿using System.Collections.Generic;
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
			CourseQuadrant = Quadrant.None;
			Edges = edges.Select(x => Moved.Create(x, Vector128.Zero)).ToArray();
			Verticies = GetVerticies(Edges);
			Bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public override Quadrant CourseQuadrant { get; }
		public override Moved<LineFunction, Vector128>[] Edges { get; }
		public override Moved<Vector128, Vector128>[] Verticies { get; }
		public override Rect Bounds { get; }

		public override void Offset(double value)
		{
			
		}
	}
}