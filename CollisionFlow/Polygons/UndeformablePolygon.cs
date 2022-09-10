﻿using System.Collections.Generic;
using System;
using System.Linq;
using Flowing.Mutate;

namespace CollisionFlow.Polygons
{
	class UndeformablePolygon : Polygon
	{
		public UndeformablePolygon(IEnumerable<LineFunction> edges, Vector2<CourseA> course)
		{
			Course = course;
			var edgesInstance = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Edges = new Mutated<LineFunction, Course>[edgesInstance.Length];
			for (int i = 0; i < edgesInstance.Length; i++)
			{
				Edges[i] = Moved.Create(edgesInstance[i], Course);
			}
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public Vector2<CourseA> Course { get; private set; }
		public override Mutated<LineFunction, Vector2<CourseA>>[] Edges { get; }
		public override Mutated<Vector2<double>, Vector2<CourseA>>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double time)
		{
			base.Offset(time);
			bounds = Course.Offset(bounds, time);
			Course = Course.Offset(time);
		}
	}
}
