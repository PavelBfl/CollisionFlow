using System.Collections.Generic;
using System;
using System.Linq;
using Flowing.Mutate;

namespace CollisionFlow.Polygons
{
	abstract class Polygon : IPolygonHandler
	{
		protected static Vector2<Mutated<double, CourseA>>[] GetVerticies(Mutated<LineFunction, Vector2<CourseA>>[] edges)
		{
			var vertices = new Vector2<Mutated<double, CourseA>>[edges.Length];
			var prevIndex = edges.Length - 1;
			for (var index = 0; index < edges.Length; index++)
			{
				var prevLine = edges[prevIndex];
				var currentLine = edges[index];

				var currentPoint = prevLine.Target.Crossing(currentLine.Target);

				var prevLineOffsetV = prevLine.Target.OffsetByVector(prevLine.Course.GetV());
				var currentLineOffsetV = currentLine.Target.OffsetByVector(currentLine.Course.GetV());
				var v = prevLineOffsetV.Crossing(currentLineOffsetV).ToVector() - currentPoint.ToVector();

				var prevLineOffsetA = prevLine.Target.OffsetByVector(prevLine.Course.GetA());
				var currentLineOffsetA = currentLine.Target.OffsetByVector(currentLine.Course.GetA());
				var a = prevLineOffsetA.Crossing(currentLineOffsetA).ToVector() - currentPoint.ToVector();

				vertices[index] = Vector2Builder.Create(
					Moved.Create(currentPoint.X, new CourseA(v.GetX(), a.GetX())),
					Moved.Create(currentPoint.Y, new CourseA(v.GetY(), a.GetY()))
				);
				prevIndex = index;
			}
			return vertices;
		}

		public static Polygon Create(IEnumerable<Mutated<LineFunction, Vector2<CourseA>>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			try
			{
				var course = edges.Select(x => x.Course).Distinct().Single();
				if (course.Equals(Vector2Builder.Create(CourseA.Zero)))
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

		public abstract Mutated<LineFunction, Vector2<CourseA>>[] Edges { get; }
		public abstract Vector2<Mutated<double, CourseA>>[] Verticies { get; }
		public abstract Rect Bounds { get; }
		public virtual void Offset(double time)
		{
			for (int i = 0; i < Edges.Length; i++)
			{
				var edge = Edges[i];
				var offset = new CourseA(
					edge.Target.GetCourseOffset(edge.Course.GetV()),
					edge.Target.GetCourseOffset(edge.Course.GetA())
				).OffsetValue(edge.Target.Offset, time);
				var line = new LineFunction(edge.Target.Slope, offset);
				var course = edge.Course.Offset(time);

				Edges[i] = Moved.Create(line, course);
			}
			for (int i = 0; i < Verticies.Length; i++)
			{
				var vertex = Verticies[i];

				Verticies[i] = Vector2Builder.Create(
					vertex.X.Offset(time),
					vertex.Y.Offset(time)
				);
			}
		}

		public Vector2<Mutated<double, CourseA>> GetBeginVertex(int edgeIndex) => Verticies[edgeIndex];
		public Vector2<Mutated<double, CourseA>> GetEndVertex(int edgeIndex) => Verticies[edgeIndex + 1 < Verticies.Length ? edgeIndex + 1 : 0];

		IReadOnlyList<Mutated<LineFunction, Vector2<CourseA>>> IPolygonHandler.Edges => Edges;
		IReadOnlyList<Vector2<Mutated<double, CourseA>>> IPolygonHandler.Vertices => Verticies;

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
				Verticies.Select(x => x.GetTarget()),
				other.Verticies.Select(x => x.GetTarget())
			);
		}

		private static bool IsCrossing(double begin1, double end1, double begin2, double end2)
		{
			var range1 = Range.Auto(begin1, end1);
			var range2 = Range.Auto(begin2, end2);
			return range1.Intersect(range2);
		}
		private static bool IsCrossing(Vector2<double> begin1, Vector2<double> end1, Vector2<double> begin2, Vector2<double> end2)
		{
			if (!IsCrossing(begin1.X, end1.X, begin2.X, end2.X) || !IsCrossing(begin1.Y, end1.Y, begin2.Y, end2.Y))
			{
				return false;
			}

			return IsCrossingTo(begin1, end1, begin2, end2) && IsCrossingTo(begin2, end2, begin1, end1);
		}
		private static bool IsCrossingTo(Vector2<double> mainBegin1, Vector2<double> mainEnd1, Vector2<double> subBegin2, Vector2<double> subEnd2)
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
		private static bool IsContainsPoint(IEnumerable<Vector2<double>> polygon, Vector2<double> point)
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
		public static bool IsCollision(IEnumerable<Vector2<double>> polygon1, IEnumerable<Vector2<double>> polygon2)
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
