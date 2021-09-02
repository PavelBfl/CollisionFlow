using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow
{
	abstract class Polygon : IPolygonHandler
	{
		public static Polygon Create(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			return new CommonPolygon(lines);
		}

		public int GlobalIndex { get; set; }
		public abstract Moved<LineFunction, Vector128>[] Edges { get; }
		IReadOnlyList<Moved<LineFunction, Vector128>> IPolygonHandler.Edges => Edges;
		public abstract Moved<Vector128, Vector128>[] Verticies { get; }
		IReadOnlyList<Moved<Vector128, Vector128>> IPolygonHandler.Vertices => Verticies;
		public abstract Rect Bounds { get; }

		public abstract void Offset(double value);

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
