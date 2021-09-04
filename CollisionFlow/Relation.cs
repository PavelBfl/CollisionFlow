using System.Collections.Generic;
using System;
using System.Linq;
using CollisionFlow.Polygons;

namespace CollisionFlow
{
	class Relation
	{
		public Relation(Polygon first, Polygon second)
		{
			First = first ?? throw new ArgumentNullException(nameof(first));
			Second = second ?? throw new ArgumentNullException(nameof(second));
			IsCollision = First.IsCollision(Second);
		}

		public bool IsFindPrev { get; set; } = false;

		public Polygon First { get; }
		public Polygon Second { get; }
		private bool IsCollision { get; set; }

		public double? Time => Result?.Offset;

		private bool resultCalculate = false;
		private CollisionResult result;

		public CollisionResult Result
		{
			get
			{
				if (resultCalculate)
				{
					if (result != null && NumberUnitComparer.Instance.IsZero(result.Offset))
					{
						resultCalculate = false;
						IsCollision = !IsCollision;
					}
				}
				if (!resultCalculate)
				{
					result = GetTime();
					if (result != null)
					{
						result.IsCollision = IsCollision;
					}
					resultCalculate = true;
				}
				return result;
			}
		}

		private CollisionResult GetTime()
		{
			var collisions = GetTime(First, Second).Concat(GetTime(Second, First));
			CollisionResult result = null;
			if (IsCollision)
			{
				foreach (var collision in collisions)
				{
					if (result is null || result.Offset < collision.Offset)
					{
						result = collision;
					}
				}
				if (result != null)
				{
					result.Offset += NumberUnitComparer.Instance.Epsilon;
				}
			}
			else
			{
				if (FlatCheck())
				{
					return null;
				}
				else
				{
					result = GetMinResult(collisions); 
				}
			}
			return result;
		}


		private CollisionResult GetMinResult(IEnumerable<CollisionResult> collisions)
		{
			CollisionResult result = null;
			foreach (var collision in collisions)
			{
				if (result is null || result.Offset > collision.Offset)
				{
					result = collision;
				}
				if (NumberUnitComparer.Instance.IsZero(result.Offset - NumberUnitComparer.Instance.Epsilon))
				{
					result.Offset -= NumberUnitComparer.Instance.Epsilon;
					return result;
				}
			}
			if (result != null)
			{
				result.Offset -= NumberUnitComparer.Instance.Epsilon;
			}
			return result;
		}

		private bool FlatCheck()
		{
			var firstBounds = First.Bounds;
			var secondBounds = Second.Bounds;

			var quadrants = GetQuadrant(firstBounds, secondBounds);
			if (quadrants is null)
			{
				return false;
			}
			else
			{
				var firstCourse = First.CourseQuadrant;
				var secondCourse = Second.CourseQuadrant;
				if ((firstCourse & quadrants.Value.second) == 0 && (secondCourse & quadrants.Value.first) == 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private static int Compare(Range first, Range second)
		{
			if (first.Intersect(second))
			{
				return 0;
			}
			else if (first.Max < second.Min)
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}
		private (Quadrant first, Quadrant second)? GetQuadrant(Rect first, Rect second)
		{
			var xCompare = Compare(first.Horisontal, second.Horisontal);

			switch (xCompare)
			{
				case 0:
					var yCompare = Compare(first.Vertical, second.Vertical);
					switch (yCompare)
					{
						case 0: return null;
						case 1: return (Quadrant.Top, Quadrant.Bottom);
						case -1: return (Quadrant.Bottom, Quadrant.Top);
						default: throw new InvalidCollisiopnException();
					}
				case 1:
					return (Quadrant.Right, Quadrant.Left);
				case -1:
					return (Quadrant.Left, Quadrant.Right);
				default: throw new InvalidCollisiopnException();
			}
		}

		private static IEnumerable<CollisionResult> GetTime(Polygon main, Polygon other)
		{
			for (int iEdge = 0; iEdge < main.Edges.Count; iEdge++)
			{
				var mainLine = main.Edges[iEdge];
				for (int iVertex = 0; iVertex < other.Verticies.Length; iVertex++)
				{
					var otherPoint = other.Verticies[iVertex];

					var prevMainLine = main.Edges[iEdge == 0 ? main.Edges.Count - 1 : iEdge - 1];
					var nextMainLine = main.Edges[iEdge == main.Edges.Count - 1 ? 0 : iEdge + 1];
					var time = GetTime(mainLine, prevMainLine, nextMainLine, otherPoint);
					if (time.HasValue)
					{
						yield return new CollisionResult(main, iEdge, other, iVertex, time.Value);
					}
				}
			}
		}

		private static double? GetTime(Moved<LineFunction, Vector128> mainLine, Moved<LineFunction, Vector128> prevMainLine, Moved<LineFunction, Vector128> nextMainLine, Moved<Vector128, Vector128> freePoin)
		{
			var time = GetTime(mainLine, freePoin);
			if (time.HasValue)
			{
				var prevLine = prevMainLine.Offset(time.Value).Target;
				var currentLine = mainLine.Offset(time.Value).Target;
				var nextLine = nextMainLine.Offset(time.Value).Target;

				var point = freePoin.Offset(time.Value).Target;
				var beginPoint = prevLine.Crossing(currentLine);
				var endPoint = nextLine.Crossing(currentLine);

				var inRange = currentLine.GetOptimalProjection() == LineState.Horisontal ?
					Contains(beginPoint.X, endPoint.X, point.X):
					Contains(beginPoint.Y, endPoint.Y, point.Y);

				return inRange ? time : null;
			}
			else
			{
				return null;
			}
		}
		private static bool Contains(double first, double second, double value)
		{
			if (NumberUnitComparer.Instance.InRange(first) && NumberUnitComparer.Instance.InRange(second))
			{
				if (NumberUnitComparer.Instance.Equals(first, second))
				{
					return NumberUnitComparer.Instance.Equals(first, value);
				}
				else
				{
					return Range.Auto(first, second).Contains(value);
				}
			}
			else
			{
				return false;
			}
		}

		private static double? GetTime(Moved<LineFunction, Vector128> line, Moved<Vector128, Vector128> freePoin)
		{
			var projectionLine = line.Target.Perpendicular();

			var currentLineProjection = line.Target.Crossing(projectionLine);
			var nextLineProjection = line.Target.OffsetByVector(new Vector128(line.Course.ToVector())).Crossing(projectionLine);

			var currentPointProjection = line.Target.OffsetToPoint(freePoin.Target).Crossing(projectionLine);
			var nextPointProjection = line.Target.OffsetToPoint(new Vector128(freePoin.Target.ToVector() + freePoin.Course.ToVector())).Crossing(projectionLine);

			if (projectionLine.GetOptimalProjection() == LineState.Horisontal)
			{
				return GetTime(Moved.Create(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X), Moved.Create(currentPointProjection.X, nextPointProjection.X - currentPointProjection.X));
			}
			else
			{
				return GetTime(Moved.Create(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y), Moved.Create(currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y));
			}
		}

		private static double? GetTime(Moved<double, double> point1, Moved<double, double> point2)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return NumberUnitComparer.Instance.InRange(localOffset) ? localOffset : new double?();
			}
			else
			{
				return null;
			}
		}
	}
}
