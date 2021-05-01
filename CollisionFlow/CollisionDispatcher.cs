using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

namespace CollisionFlow
{
	public class CollisionDispatcher
	{
		private static bool IsZero(double value) => Math.Abs(value) < 0.000001;
		public static CollisionResult Offset(IEnumerable<CollisionPolygon> polygons, double offset)
		{
			var result = new CollisionResult(offset);
			var localPolygons = polygons.ToArray();
			foreach (var main in localPolygons)
			{
				foreach (var other in localPolygons)
				{
					if (!ReferenceEquals(main, other))
					{
						var mainToOther = Offset(main, other, result.Offset);
						result = mainToOther.Offset < result.Offset ? mainToOther : result;
						var otherToMain = Offset(other, main, result.Offset);
						result = otherToMain.Offset < result.Offset ? otherToMain : result;

						if (IsZero(result.Offset))
						{
							return result;
						}
					}
				}
			}

			foreach (var polygon in localPolygons)
			{
				polygon.Offset(result.Offset);
			}

			return result;
		}
		private static CollisionResult Offset(CollisionPolygon main, CollisionPolygon other, double offset)
		{
			var result = new CollisionResult(offset);
			var mainLines = main.Lines.ToArray();
			for (int i = 0; i < mainLines.Length; i++)
			{
				var mainLine = mainLines[i];
				foreach (var otherPoint in other.GetPoints())
				{
					var localOffset = Offset(mainLine, otherPoint, result.Offset);
					if (localOffset < result.Offset)
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
							if (!IsZero(localOffset))
							{
								result = new CollisionResult(main, other, localOffset);
							}
							else
							{
								return new CollisionResult(main, other, 0);
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
			var nextLineProjection = line.Target.OffsetByVector(new Vector128(freePoin.Course.ToVector() * offset)).Crossing(projectionLine);

			var currentPointProjection = line.Target.OffsetToPoint(freePoin.Target).Crossing(projectionLine);
			var nextPointProjection = line.Target.OffsetToPoint(new Vector128(freePoin.Target.ToVector() + freePoin.Course.ToVector() * offset)).Crossing(projectionLine);

			if (-1 < projectionLine.Slope && projectionLine.Slope < 1)
			{
				return Offset(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X, currentPointProjection.X, nextPointProjection.X - currentPointProjection.X, offset);
			}
			else
			{
				return Offset(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y, currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y, offset);
			}
		}
		private static double Offset(double value1, double vector1, double value2, double vector2, double offset)
		{
			double min;
			double minVector;
			double max;
			double maxVector;
			if (value1 < value2)
			{
				min = value1;
				minVector = vector1;
				max = value2;
				maxVector = vector2;
			}
			else
			{
				min = value2;
				minVector = vector2;
				max = value1;
				maxVector = vector1;
			}

			var speed = minVector - maxVector;
			if (speed > 0)
			{
				var distance = max - min;
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
	}
}
