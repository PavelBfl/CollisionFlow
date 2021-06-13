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
			for (var iMain = 0; iMain < localPolygons.Length; iMain++)
			{
				var main = localPolygons[iMain];
				for (var iOther = iMain + 1; iOther < localPolygons.Length; iOther++)
				{
					var other = localPolygons[iOther];
					if (!ReferenceEquals(main, other))
					{
						if (IsCollision(main, other))
						{
							return new CollisionResult(main, new Moved<LineFunction, Vector128>(), other, new Moved<Vector128, Vector128>(), 0);
						}

						if (!Flat.TryOffset(main.Points, other.Points, offset))
						{
							result = Offset(main, other, result, offset);
							result = Offset(other, main, result, offset);

							if (!(result is null) && NumberUnitComparer.Instance.Equals(result.Offset, 0))
							{
								return result;
							}
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
				foreach (var otherPoint in other.Points)
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
			var nextLineProjection = line.Target.OffsetByVector(new Vector128(line.Course.ToVector())).Crossing(projectionLine);

			var currentPointProjection = line.Target.OffsetToPoint(freePoin.Target).Crossing(projectionLine);
			var nextPointProjection = line.Target.OffsetToPoint(new Vector128(freePoin.Target.ToVector() + freePoin.Course.ToVector())).Crossing(projectionLine);

			if (-1 < projectionLine.Slope && projectionLine.Slope < 1)
			{
				return Flat.Offset(Moved.Create(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X), Moved.Create(currentPointProjection.X, nextPointProjection.X - currentPointProjection.X), offset);
			}
			else
			{
				return Flat.Offset(Moved.Create(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y), Moved.Create(currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y), offset);
			}
		}

		private static (double min, double max) BinarySort(double value1, double value2)
		{
			return NumberUnitComparer.Instance.Compare(value1, value2) < 0 ? (value1, value2) : (value2, value1);
		}
		private static bool InRange(double value, double begin, double end)
		{
			var (min, max) = BinarySort(begin, end);
			return min <= value && value <= max;
		}
		private static bool IsCrossing(double begin1, double end1, double begin2, double end2)
		{
			var (min1, max1) = BinarySort(begin1, end1);
			var (min2, max2) = BinarySort(begin2, end2);
			return NumberUnitComparer.Instance.Compare(min2, max1) <= 0 && NumberUnitComparer.Instance.Compare(min1, max2) <= 0;
		}

		private static bool IsCollision(CollisionPolygon polygon1, CollisionPolygon polygon2)
		{
			if (!polygon1.Bounds.Intersect(polygon2.Bounds))
			{
				return false;
			}

			return IsCollision(
				polygon1.Points.Select(x => x.Target),
				polygon2.Points.Select(x => x.Target)
			);
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
