using System.Collections.Generic;
using System;
using System.Linq;
using CollisionFlow.Polygons;

namespace CollisionFlow
{

	class Relation
	{
		private enum ResultState
		{
			None,
			Wait,
			Success
		}
		private abstract class PreviewChecker
		{
			public CollisionResult Result { get; set; }
			public bool Check(double result)
			{
				if (Result is null)
				{
					return true;
				}
				else
				{
					return ResultCheck(result);
				}
			}

			protected abstract bool ResultCheck(double result);
		}
		private class MinChecker : PreviewChecker
		{
			protected override bool ResultCheck(double result) => result < Result.Offset;
		}
		private class MaxChecker : PreviewChecker
		{
			protected override bool ResultCheck(double result) => result > Result.Offset;
		}

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

		private ResultState resultState = ResultState.None;
		private double wait = 0;
		//private bool resultCalculate = false;
		private CollisionResult result;

		public void Step(double value)
		{
			if (resultState == ResultState.Wait)
			{
				wait -= value;
			}
			else
			{
				result?.Step(value);
			}
		}

		public CollisionResult Result
		{
			get
			{
				if (resultState == ResultState.Success)
				{
					if (result != null && NumberUnitComparer.Instance.IsZero(result.Offset))
					{
						resultState = ResultState.None;
						IsCollision = !IsCollision;
					}
				}
				if (resultState == ResultState.None || (resultState == ResultState.Wait && wait < 0.000001))
				{
					result = GetTime();
					if (result != null)
					{
						result.IsCollision = IsCollision;
					}
				}
				return result;
			}
		}

		private CollisionResult GetTime()
		{
			if (IsCollision)
			{
				var checker = new MaxChecker();
				foreach (var collision in GetTime(First, Second, checker).Concat(GetTime(Second, First, checker)))
				{
					if (checker.Result is null || checker.Result.Offset < collision.Offset)
					{
						checker.Result = collision;
					}
				}
				if (checker.Result != null)
				{
					checker.Result.Offset += NumberUnitComparer.Instance.Epsilon;
				}
				resultState = ResultState.Success;
				return checker.Result;
			}
			else
			{
				if (FlatCheck())
				{
					resultState = ResultState.Success;
					return null;
				}
				else
				{
					if (First.State == PolygonState.Undeformable && Second.State == PolygonState.Undeformable)
					{
						const double WAIT_TIME = 10;
						var firstBounds = GetFullRect(First.Bounds, (First.Edges[0].Course.ToVector() * WAIT_TIME).ToVector128());
						var secondBounds = GetFullRect(Second.Bounds, (Second.Edges[0].Course.ToVector() * WAIT_TIME).ToVector128());
						if (firstBounds.Intersect(secondBounds))
						{
							resultState = ResultState.Success;
							return GetMinResult();
						}
						else
						{
							wait = WAIT_TIME;
							resultState = ResultState.Wait;
							return null;
						}
					}
					else
					{
						resultState = ResultState.Success;
						return GetMinResult(); 
					}
				}
			}
		}
		private static Rect GetFullRect(Rect rect, Vector128 course)
		{
			var offsetRect = new Rect(
				left: rect.Left + course.X,
				top: rect.Top + course.Y,
				right: rect.Right + course.X,
				bottom: rect.Bottom + course.Y
			);
			return rect.Union(offsetRect);
		}
		private CollisionResult GetMinResult()	
		{
			var checker = new MinChecker();
			foreach (var collision in GetTime(First, Second, checker).Concat(GetTime(Second, First, checker)))
			{
				if (checker.Result is null || checker.Result.Offset > collision.Offset)
				{
					checker.Result = collision;
				}
				if (NumberUnitComparer.Instance.IsZero(checker.Result.Offset - NumberUnitComparer.Instance.Epsilon))
				{
					checker.Result.Offset -= NumberUnitComparer.Instance.Epsilon;
					return checker.Result;
				}
			}
			if (checker.Result != null)
			{
				checker.Result.Offset -= NumberUnitComparer.Instance.Epsilon;
			}
			return checker.Result;
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

		private static IEnumerable<CollisionResult> GetTime(Polygon main, Polygon other, PreviewChecker previewChecker)
		{
			for (int iEdge = 0; iEdge < main.Edges.Count; iEdge++)
			{
				var mainLine = main.Edges[iEdge];
				for (int iVertex = 0; iVertex < other.Verticies.Length; iVertex++)
				{
					var otherPoint = other.Verticies[iVertex];

					var time = GetTime(mainLine, otherPoint);
					if (time.HasValue && previewChecker.Check(time.Value))
					{
						if (InRange(time.Value, mainLine.Target.GetOptimalProjection(), main.GetBeginVertex(iEdge), main.GetEndVertex(iEdge), otherPoint))
						{
							yield return new CollisionResult(main, iEdge, other, iVertex, time.Value);
						}
					}
				}
			}
		}

		private static bool InRange(double offset, LineState state, Moved<Vector128, Vector128> begin, Moved<Vector128, Vector128> end, Moved<Vector128, Vector128> freePoin)
		{
			var beginOffset = begin.Offset(offset).Target;
			var endOffset = end.Offset(offset).Target;
			var freePointOffset = freePoin.Offset(offset).Target;
			return state == LineState.Horisontal ?
				Contains(beginOffset.X, endOffset.X, freePointOffset.X) :
				Contains(beginOffset.Y, endOffset.Y, freePointOffset.Y);
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
			var freeLine = line.Target.OffsetToPoint(freePoin.Target);
			return GetTime(
				Moved.Create(line.Target.Offset, line.Target.GetCourseOffset(line.Course)),
				Moved.Create(freeLine.Offset, freeLine.GetCourseOffset(freePoin.Course))
			);
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
