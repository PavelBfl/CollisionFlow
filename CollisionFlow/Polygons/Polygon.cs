using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow.Polygons
{
	enum PolygonState
	{
		None,
		Static,
		Undeformable,
	}
	enum Quadrant
	{
		None = 0x0,
		Left = 0x1,
		Right = 0x2,
		Top = 0x4,
		Bottom = 0x8,
	}
	class Polygon : IPolygonHandler
	{
		private static Quadrant GetQuadrant(Vector128 course)
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

		public Polygon(IEnumerable<Moved<LineFunction, Vector128>> edges)
		{
			if (edges is null)
			{
				throw new ArgumentNullException(nameof(edges));
			}
			Edges = new EdgesCollection(edges);
			CourseQuadrant = Edges.Aggregate(Quadrant.None, (a, x) => a | GetQuadrant(x.Course));
		}
		
		public int GlobalIndex { get; set; }

		public bool ChangeRefresh()
		{
			if (Edges.IsChanged)
			{
				try
				{
					var course = Edges.Select(x => x.Course).Distinct().Single();
					if (course.Equals(Vector128.Zero))
					{
						State = PolygonState.Static;
						CourseQuadrant = Quadrant.None;
					}
					else
					{
						State = PolygonState.Undeformable;
						CourseQuadrant = GetQuadrant(course);
					}
				}
				catch (InvalidOperationException)
				{
					State = PolygonState.None;
					CourseQuadrant = Edges.Aggregate(Quadrant.None, (a, x) => a | GetQuadrant(x.Course));
				}
				vertices = null;
				bounds = null;

				Edges.ChangesHandled();
				return true;
			}
			else
			{
				return false;
			}
		}
		public PolygonState State { get; private set; } = PolygonState.None;
		public Quadrant CourseQuadrant { get; private set; }

		public EdgesCollection Edges { get; }
		IList<Moved<LineFunction, Vector128>> IPolygonHandler.Edges => Edges;

		private Moved<Vector128, Vector128>[] vertices = null;
		public Moved<Vector128, Vector128>[] Verticies
		{
			get
			{
				if (vertices is null)
				{
					vertices = new Moved<Vector128, Vector128>[Edges.Count];
					var prevIndex = Edges.Count - 1;
					for (var index = 0; index < Edges.Count; index++)
					{
						var prevLine = Edges[prevIndex];
						var currentLine = Edges[index];

						var prevLineOffset = prevLine.Target.OffsetByVector(prevLine.Course);
						var currentLineOffset = currentLine.Target.OffsetByVector(currentLine.Course);

						var currentPoint = prevLine.Target.Crossing(currentLine.Target);

						vertices[index] = Moved.Create(
							currentPoint,
							new Vector128(prevLineOffset.Crossing(currentLineOffset).ToVector() - currentPoint.ToVector())
						);
						prevIndex = index;
					}
				}
				return vertices;
			}
		}
		IReadOnlyList<Moved<Vector128, Vector128>> IPolygonHandler.Vertices => Verticies;

		private Rect? bounds;
		public Rect Bounds
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

		public void Offset(double value)
		{
			switch (State)
			{
				case PolygonState.None:
					OffsetEdges(value);
					OffsetVertices(value);
					bounds = null;
					break;
				case PolygonState.Static:
					break;
				case PolygonState.Undeformable:
					OffsetEdges(value);
					OffsetVertices(value);
					if (!(bounds is null))
					{
						var course = new Vector128(Edges[0].Course.ToVector() * value);
						bounds = new Rect(
							bounds.Value.Left + course.X,
							bounds.Value.Right + course.X,
							bounds.Value.Top + course.Y,
							bounds.Value.Bottom + course.Y
						);
					}
					break;
				default: throw new InvalidCollisiopnException();
			}
		}
		private void OffsetEdges(double value)
		{
			if (Edges.IsChanged)
			{
				throw new InvalidCollisiopnException();
			}
			for (int i = 0; i < Edges.Count; i++)
			{
				Edges[i] = Edges[i].Offset(value);
			}
			Edges.ChangesHandled();
		}
		private void OffsetVertices(double value)
		{
			if (!(vertices is null))
			{
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = vertices[i].Offset(value);
				}
			}
		}

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
