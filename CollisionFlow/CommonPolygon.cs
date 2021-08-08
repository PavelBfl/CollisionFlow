using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow
{
	class CommonPolygon : Polygon
	{
		private const int POLYGOM_MIN_VERTICIES = 3;

		private static int counter = 0;
		public int StaticIndex { get; } = counter++;

		public CommonPolygon(IEnumerable<Moved<LineFunction, Vector128>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}

			var edgesInstance = edges.ToArray();
			if (edgesInstance.Length < POLYGOM_MIN_VERTICIES)
			{
				throw new InvalidCollisiopnException();
			}

			Edges = edgesInstance;
		}

		public override Moved<LineFunction, Vector128>[] Edges { get; }

		private Moved<Vector128, Vector128>[] points;
		public override Moved<Vector128, Vector128>[] Verticies
		{
			get
			{
				if (points is null)
				{
					points = new Moved<Vector128, Vector128>[Edges.Length];
					var prevIndex = Edges.Length - 1;
					for (var index = 0; index < Edges.Length; index++)
					{
						var prevLine = Edges[prevIndex];
						var currentLine = Edges[index];

						var prevLineOffset = prevLine.Target.OffsetByVector(prevLine.Course);
						var currentLineOffset = currentLine.Target.OffsetByVector(currentLine.Course);

						var currentPoint = prevLine.Target.Crossing(currentLine.Target);

						points[index] = Moved.Create(
							currentPoint,
							new Vector128(prevLineOffset.Crossing(currentLineOffset).ToVector() - currentPoint.ToVector())
						);
						prevIndex = index;
					}
				}
				return points;
			}
		}

		private Rect? bounds;
		public override Rect Bounds
		{
			get
			{
				if (bounds is null)
				{
					bounds = new Rect(Verticies.Select(x => x.Target));
				}
				return bounds.GetValueOrDefault();
			}
		}
		public override void Offset(double value)
		{
			for (int i = 0; i < Edges.Length; i++)
			{
				Edges[i] = Edges[i].Offset(value);
			}
			points = null;
			bounds = null;
		}
	}
}
