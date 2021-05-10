using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

namespace CollisionFlow
{
	public class CollisionDispatcher
	{
		public static CollisionResult Offset(IEnumerable<CollisionPolygon> polygons, double offset)
		{
			CollisionResult result = null;
			var localPolygons = polygons.ToArray();
			foreach (var main in localPolygons)
			{
				foreach (var other in localPolygons)
				{
					if (!ReferenceEquals(main, other))
					{
						if (IsCollision(main, other))
						{
							return new CollisionResult(main, new Moved<LineFunction, Vector128>(), other, new Moved<Vector128, Vector128>(), 0);
						}
						result = Offset(main, other, result, offset);
						result = Offset(other, main, result, offset);

						if (!(result is null) && NumberUnitComparer.Instance.Equals(result.Offset, 0))
						{
							return result;
						}
					}
				}
			}

			var currentOffset = result?.Offset ?? offset;
			foreach (var polygon in localPolygons)
			{
				polygon.Offset(currentOffset);
			}

			return result;
		}
		private static CollisionResult Offset(CollisionPolygon main, CollisionPolygon other, CollisionResult prevResult, double offset)
		{
			if (prevResult is null)
			{
				return Offset(main, other, offset);
			}
			else
			{
				var currentResult = Offset(main, other, prevResult.Offset);
				if (currentResult is null)
				{
					return prevResult;
				}
				else
				{
					return prevResult.Offset < currentResult.Offset ? prevResult : currentResult;
				}
			}
		}

		private static CollisionResult Offset(CollisionPolygon main, CollisionPolygon other, double offset)
		{
			CollisionResult result = null;
			var mainLines = main.Lines.ToArray();
			for (int i = 0; i < mainLines.Length; i++)
			{
				var mainLine = mainLines[i];
				foreach (var otherPoint in other.GetPoints())
				{
					var currentOffset = result?.Offset ?? offset;
					var localOffset = Offset(mainLine, otherPoint, currentOffset);
					if (localOffset < currentOffset)
					{
						var prevMainLine = mainLines[i == 0 ? mainLines.Length - 1 : i - 1];
						var nextMainLine = mainLines[i == mainLines.Length - 1 ? 0 : i + 1];

						var prevLine = prevMainLine.Target.OffsetByVector(new Vector128(prevMainLine.Course.ToVector() * localOffset));
						var currentLine = mainLine.Target.OffsetByVector(new Vector128(mainLine.Course.ToVector() * localOffset));
						var nextLine = nextMainLine.Target.OffsetByVector(new Vector128(nextMainLine.Course.ToVector() * localOffset));

						var point = new Vector128(otherPoint.Target.ToVector() + otherPoint.Course.ToVector() * localOffset);
						var beginPoint = prevLine.Crossing(currentLine);
						var endPoint = nextLine.Crossing(currentLine);

						var inRange = -1 < currentLine.Slope && currentLine.Slope < 1 ?
							InRange(point.X, beginPoint.X, endPoint.X) :
							InRange(point.Y, beginPoint.Y, endPoint.Y);
						if (inRange)
						{
							if (!NumberUnitComparer.Instance.Equals(localOffset, 0))
							{
								result = new CollisionResult(main, mainLine, other, otherPoint, localOffset);
							}
							else
							{
								return new CollisionResult(main, mainLine, other, otherPoint, 0);
							}
						}
					}
				}
			}
			return result;
		}
		private static double Offset(Moved<LineFunction, Vector128> line, Moved<Vector128, Vector128> freePoin, double offset)
		{
			var projectionLine = line.Target.Perpendicular();

			var currentLineProjection = line.Target.Crossing(projectionLine);
			var nextLineProjection = line.Target.OffsetByVector(new Vector128(line.Course.ToVector() * offset)).Crossing(projectionLine);

			var currentPointProjection = line.Target.OffsetToPoint(freePoin.Target).Crossing(projectionLine);
			var nextPointProjection = line.Target.OffsetToPoint(new Vector128(freePoin.Target.ToVector() + freePoin.Course.ToVector() * offset)).Crossing(projectionLine);

			if (-1 < projectionLine.Slope && projectionLine.Slope < 1)
			{
				return Offset(Moved.Create(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X), Moved.Create(currentPointProjection.X, nextPointProjection.X - currentPointProjection.X), offset);
			}
			else
			{
				return Offset(Moved.Create(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y), Moved.Create(currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y), offset);
			}
		}
		private static double Offset(Moved<double, double> value1, Moved<double, double> value2, double offset)
		{
			var (min, max) = value1.Target < value2.Target ? (value1, value2) : (value2, value1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return localOffset < 1 ? localOffset * offset : offset;
			}
			else
			{
				return offset;
			}
		}
		private static bool InRange(double value, double begin, double end)
		{
			var (min, max) = begin < end ? (begin, end) : (end, begin);
			return min <= value && value <= max;
		}

		private static bool IsCollision(CollisionPolygon polygon1, CollisionPolygon polygon2)
		{
			var points1 = polygon1.GetPoints().Select(x => x.Target).ToArray();
			var points2 = polygon2.GetPoints().Select(x => x.Target).ToArray();
			var result = points1.Any(x => IsContainsPoint(points2, x)) ||
				points2.Any(x => IsContainsPoint(points1, x));

			if (result)
			{
				return result;
			}

			var prev1 = points1.Length - 1;
			for (int i = 0; i < points1.Length; i++)
			{
				var prev2 = points2.Length - 1;
				for (int j = 0; j < points2.Length; j++)
				{
					if (IsCrossing(points1[i], points1[prev1], points2[j], points2[prev2]))
					{
						return true;
					}
					prev2 = j;
				}
				prev1 = i;
			}
			return false;
		}
		private static bool IsCrossing(Vector128 begin1, Vector128 end1, Vector128 begin2, Vector128 end2)
		{
			var line = new LineFunction(begin1, end1);
			var projectLine = line.Perpendicular();

			var projectPoint = line.Crossing(projectLine);

			var projectBegin = line.OffsetToPoint(begin2).Crossing(projectLine);
			var projectEnd = line.OffsetToPoint(end2).Crossing(projectLine);

			return -1 < projectLine.Slope && projectLine.Slope < 1 ?
				InRange(projectPoint.X, projectBegin.X, projectEnd.X) :
				InRange(projectPoint.Y, projectBegin.Y, projectEnd.Y);
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
				if (currentPoint.Y < point.Y && prevPoint.Y >= point.Y || prevPoint.Y < point.Y && currentPoint.Y > point.Y)
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
	}
}
