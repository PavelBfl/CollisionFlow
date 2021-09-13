using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	enum Quadrant
	{
		None = 0x0,
		Left = 0x1,
		Right = 0x2,
		Top = 0x4,
		Bottom = 0x8,
	}
	abstract class Polygon : IPolygonHandler
	{
		protected static Quadrant GetQuadrant(Vector128 course)
		{
			var xCompare = NumberUnitComparer.Instance.Compare(course.X, 0);
			var yCompare = NumberUnitComparer.Instance.Compare(course.Y, 0);

			var result = Quadrant.None;
			if (xCompare < 0)
			{
				result |= Quadrant.Left;
			}
			else if (xCompare > 0)
			{
				result |= Quadrant.Right;
			}

			if (yCompare < 0)
			{
				result |= Quadrant.Bottom;
			}
			else if (yCompare > 0)
			{
				result |= Quadrant.Top;
			}
			return result;
		}
		protected static Moved<Vector128, Vector128>[] GetVerticies(Moved<LineFunction, Vector128>[] edges)
		{
			var vertices = new Moved<Vector128, Vector128>[edges.Length];
			var prevIndex = edges.Length - 1;
			for (var index = 0; index < edges.Length; index++)
			{
				var prevLine = edges[prevIndex];
				var currentLine = edges[index];

				var prevLineOffset = prevLine.Target.OffsetByVector(prevLine.Course);
				var currentLineOffset = currentLine.Target.OffsetByVector(currentLine.Course);

				var currentPoint = prevLine.Target.Crossing(currentLine.Target);

				vertices[index] = Moved.Create(
					currentPoint,
					new Vector128(prevLineOffset.Crossing(currentLineOffset).ToVector() - currentPoint.ToVector())
				);
				prevIndex = index;
			}
			return vertices;
		}

		public static Polygon Create(IEnumerable<Moved<LineFunction, Vector128>> edges)
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
		public abstract Quadrant CourseQuadrant { get; }

		public abstract Moved<LineFunction, Vector128>[] Edges { get; }
		public abstract Moved<Vector128, Vector128>[] Verticies { get; }
		public abstract Rect Bounds { get; }
		public virtual void Offset(double value)
		{
			for (int i = 0; i < Edges.Length; i++)
			{
				Edges[i] = Edges[i].Offset(value);
			}
			for (int i = 0; i < Verticies.Length; i++)
			{
				Verticies[i] = Verticies[i].Offset(value);
			}
		}

		public Moved<Vector128, Vector128> GetBeginVertex(int edgeIndex) => Verticies[edgeIndex];
		public Moved<Vector128, Vector128> GetEndVertex(int edgeIndex) => Verticies[edgeIndex + 1 < Verticies.Length ? edgeIndex + 1 : 0];

		IReadOnlyList<Moved<LineFunction, Vector128>> IPolygonHandler.Edges => Edges;
		IReadOnlyList<Moved<Vector128, Vector128>> IPolygonHandler.Vertices => Verticies;

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
	class CommonPolygon : Polygon
	{
		public CommonPolygon(IEnumerable<Moved<LineFunction, Vector128>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			CourseQuadrant = Edges.Aggregate(Quadrant.None, (a, x) => a | GetQuadrant(x.Course));
		}

		public override Quadrant CourseQuadrant { get; }

		public override Moved<LineFunction, Vector128>[] Edges { get; }

		private Moved<Vector128, Vector128>[] vertices = null;
		public override Moved<Vector128, Vector128>[] Verticies => vertices ?? (vertices = GetVerticies(Edges));

		private Rect? bounds;
		public override Rect Bounds
		{
			get
			{
				if (bounds is null)
				{
					bounds = new Rect(Verticies.Select(x => x.Target));
				}
				return bounds.Value;
			}
		}

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = null;
		}
	}
	class UndeformablePolygon : Polygon
	{
		public UndeformablePolygon(IEnumerable<LineFunction> edges, Vector128 course)
		{
			Course = course;
			CourseQuadrant = GetQuadrant(Course);
			var edgesInstance = edges?.ToArray() ?? throw new ArgumentNullException(nameof(edges));
			Edges = new Moved<LineFunction, Vector128>[edgesInstance.Length];
			for (int i = 0; i < edgesInstance.Length; i++)
			{
				Edges[i] = Moved.Create(edgesInstance[i], Course);
			}
			Verticies = GetVerticies(Edges);
			bounds = new Rect(Verticies.Select(x => x.Target));
		}

		public override Quadrant CourseQuadrant { get; }
		public Vector128 Course { get; }
		public override Moved<LineFunction, Vector128>[] Edges { get; }
		public override Moved<Vector128, Vector128>[] Verticies { get; }

		private Rect bounds;
		public override Rect Bounds => bounds;

		public override void Offset(double value)
		{
			base.Offset(value);
			bounds = bounds.Offset((Course.ToVector() * value).ToVector128());
		}
	}
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
