using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	abstract class Polygon : IPolygonHandler
	{
		protected static Moved<Vector128, Course>[] GetVerticies(Moved<LineFunction, Course>[] edges)
		{
			var vertices = new Moved<Vector128, Course>[edges.Length];
			var prevIndex = edges.Length - 1;
			for (var index = 0; index < edges.Length; index++)
			{
				var prevLine = edges[prevIndex];
				var currentLine = edges[index];

				var currentPoint = prevLine.Target.Crossing(currentLine.Target);

				var prevLineOffsetV = prevLine.Target.OffsetByVector(prevLine.Course.V.ToVector128());
				var currentLineOffsetV = currentLine.Target.OffsetByVector(currentLine.Course.V.ToVector128());
				var v = prevLineOffsetV.Crossing(currentLineOffsetV).ToVector() - currentPoint.ToVector();

				var prevLineOffsetA = prevLine.Target.OffsetByVector(prevLine.Course.A.ToVector128());
				var currentLineOffsetA = currentLine.Target.OffsetByVector(currentLine.Course.A.ToVector128());
				var a = prevLineOffsetA.Crossing(currentLineOffsetA).ToVector() - currentPoint.ToVector();

				vertices[index] = Moved.Create(
					currentPoint,
					new Course(v, a)
				);
				prevIndex = index;
			}
			return vertices;
		}

		public static Polygon Create(IEnumerable<Moved<LineFunction, Course>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			try
			{
				var course = edges.Select(x => x.Course).Distinct().Single();
				if (course.Equals(Vector128.Zero))
				{
					return new StaticPolygon(edges.Select(x => x.Target));
				}
				else
				{
					return new UndeformablePolygon(edges.Select(x => x.Target), course);
				}
			}
			catch (InvalidOperationException)
			{
				return new CommonPolygon(edges);
			}
		}

		public int GlobalIndex { get; set; }

		public abstract Moved<LineFunction, Course>[] Edges { get; }
		public abstract Moved<Vector128, Course>[] Verticies { get; }
		public abstract Rect Bounds { get; }
		public virtual void Offset(double time)
		{
			for (int i = 0; i < Edges.Length; i++)
			{
				var edge = Edges[i];
				var offset = Acceleration.Offset(
					time,
					edge.Target.Offset,
					edge.Target.GetCourseOffset(edge.Course.V.ToVector128()),
					edge.Target.GetCourseOffset(edge.Course.A.ToVector128())
				);
				var line = new LineFunction(edge.Target.Slope, offset);
				var course = edge.Course.Offset(time);

				Edges[i] = Moved.Create(line, course);
			}
			for (int i = 0; i < Verticies.Length; i++)
			{
				var vertex = Verticies[i];
				var point = new Vector128(
					Acceleration.Offset(time, vertex.Target.X, vertex.Course.V.GetX(), vertex.Course.A.GetX()),
					Acceleration.Offset(time, vertex.Target.Y, vertex.Course.V.GetY(), vertex.Course.A.GetY())
				);
				var course = vertex.Course.Offset(time);

				Verticies[i] = Moved.Create(point, course);
			}
		}

		public Moved<Vector128, Course> GetBeginVertex(int edgeIndex) => Verticies[edgeIndex];
		public Moved<Vector128, Course> GetEndVertex(int edgeIndex) => Verticies[edgeIndex + 1 < Verticies.Length ? edgeIndex + 1 : 0];

		IReadOnlyList<Moved<LineFunction, Course>> IPolygonHandler.Edges => Edges;
		IReadOnlyList<Moved<Vector128, Course>> IPolygonHandler.Vertices => Verticies;

		public object AttachetData { get; set; }

		public bool IsCollision(Polygon other)
		{
			if (other is null)
			{
				throw new ArgumentNullException(nameof(other));
			}

			if (!Bounds.Intersect(other.Bounds))
			{
				return false;
			}

			return IsCollision(
				Verticies.Select(x => x.Target),
				other.Verticies.Select(x => x.Target)
			);
		}

		private static bool IsCrossing(double begin1, double end1, double begin2, double end2)
		{
			var range1 = Range.Auto(begin1, end1);
			var range2 = Range.Auto(begin2, end2);
			return range1.Intersect(range2);
		}
		private static bool IsCrossing(Vector128 begin1, Vector128 end1, Vector128 begin2, Vector128 end2)
		{
			if (!IsCrossing(begin1.X, end1.X, begin2.X, end2.X) || !IsCrossing(begin1.Y, end1.Y, begin2.Y, end2.Y))
			{
				return false;
			}

			return IsCrossingTo(begin1, end1, begin2, end2) && IsCrossingTo(begin2, end2, begin1, end1);
		}
		private static bool IsCrossingTo(Vector128 mainBegin1, Vector128 mainEnd1, Vector128 subBegin2, Vector128 subEnd2)
		{
			var line = new LineFunction(mainBegin1, mainEnd1);
			var projectLine = line.Perpendicular();

			var projectPoint = line.Crossing(projectLine);

			var projectBegin = line.OffsetToPoint(subBegin2).Crossing(projectLine);
			var projectEnd = line.OffsetToPoint(subEnd2).Crossing(projectLine);

			return projectLine.GetOptimalProjection() == LineState.Horisontal ?
				Range.Auto(projectBegin.X, projectEnd.X).ContainsExEx(projectPoint.X) :
				Range.Auto(projectBegin.Y, projectEnd.Y).ContainsExEx(projectPoint.Y);
		}
		private static bool IsContainsPoint(IEnumerable<Vector128> polygon, Vector128 point)
		{
			var result = false;
			var polygonInstance = polygon.ToArray();
			var prevIndex = polygonInstance.Length - 1;
			for (int i = 0; i < polygonInstance.Length; i++)
			{
				var prevPoint = polygonInstance[prevIndex];
				var currentPoint = polygonInstance[i];
				if (currentPoint.Y < point.Y && prevPoint.Y >= point.Y || prevPoint.Y < point.Y && currentPoint.Y >= point.Y)
				{
					if (currentPoint.X + (point.Y - currentPoint.Y) / (prevPoint.Y - currentPoint.Y) * (prevPoint.X - currentPoint.X) < point.X)
					{
						result = !result;
					}
				}
				prevIndex = i;
			}
			return result;
		}
		public static bool IsCollision(IEnumerable<Vector128> polygon1, IEnumerable<Vector128> polygon2)
		{
			var polygon1Instance = polygon1?.ToArray() ?? throw new ArgumentNullException(nameof(polygon1));
			if (polygon1Instance.Length < 3)
			{
				throw new InvalidOperationException();
			}
			var polygon2Instance = polygon2?.ToArray() ?? throw new ArgumentNullException(nameof(polygon2));
			if (polygon2Instance.Length < 3)
			{
				throw new InvalidOperationException();
			}

			if (polygon1Instance.Any(x => IsContainsPoint(polygon2Instance, x)) || polygon2Instance.Any(x => IsContainsPoint(polygon1Instance, x)))
			{
				return true;
			}

			var prev1 = polygon1Instance.Length - 1;
			for (int i = 0; i < polygon1Instance.Length; i++)
			{
				var prev2 = polygon2Instance.Length - 1;
				for (int j = 0; j < polygon2Instance.Length; j++)
				{
					if (IsCrossing(polygon1Instance[i], polygon1Instance[prev1], polygon2Instance[j], polygon2Instance[prev2]))
					{
						return true;
					}
					prev2 = j;
				}
				prev1 = i;
			}
			return false;
		}
	}
}
