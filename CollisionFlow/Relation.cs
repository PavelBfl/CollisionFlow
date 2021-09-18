﻿using System.Collections.Generic;
using System;
using System.Linq;
using CollisionFlow.Polygons;

namespace CollisionFlow
{
	abstract class RelationResult
	{
		public abstract void Step(double value);
	}
	class OffsetResult : RelationResult
	{
		public OffsetResult(CollisionData collisionData, double offset)
		{
			CollisionData = collisionData ?? throw new ArgumentNullException(nameof(collisionData));
			Offset = offset;
		}

		public CollisionData CollisionData { get; }
		public double Offset { get; set; }

		public override void Step(double value)
		{
			Offset -= value;
		}
	}
	class WaitResult : RelationResult
	{
		public WaitResult(double offset)
		{
			if (offset < 0)
			{
				throw new InvalidCollisiopnException();
			}
			Offset = offset;
		}

		public double Offset { get; private set; }

		public bool IsWait => Offset >= 0.000001;

		public override void Step(double value)
		{
			if (value > Offset)
			{
				throw new InvalidCollisiopnException();
			}
			Offset -= value;
		}
	}
	class InfinitResult : RelationResult
	{
		public static InfinitResult Instance { get; } = new InfinitResult();
		private InfinitResult()
		{

		}
		public override void Step(double value)
		{
			
		}
	}

	class Relation
	{
		private abstract class PreviewChecker
		{
			public OffsetResult Result { get; set; }
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

		public Polygon First { get; }
		public Polygon Second { get; }
		private bool IsCollision { get; set; }

		public void OnHandled()
		{
			result = null;
			IsCollision = !IsCollision;
		}

		private RelationResult result;

		public void Step(double value)
		{
			result?.Step(value);
		}

		public OffsetResult GetResult(double offset)
		{
			if (result is null || (result is WaitResult waitResult && waitResult.Offset < offset))
			{
				result = GetTime(offset);
			}
			return result as OffsetResult;
		}

		private RelationResult GetTime(double offset)
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
				return checker.Result;
			}
			else
			{
				var flatResult = FlatCheck();
				if (flatResult != null)
				{
					return flatResult;
				}
				var undeformableResult = UndeformableResult(offset);
				if (undeformableResult != null)
				{
					return undeformableResult;
				}
				return GetMinResult();
			}
		}
		private static int flatCounter = 0;
		private static int notFlatCounter = 0;

		private WaitResult UndeformableResult(double offset)
		{
			if (First is UndeformablePolygon first && Second is UndeformablePolygon second)
			{
				var distance = GetDistance(first.Bounds, second.Bounds);
				if (distance.Equals(Vector128.Zero))
				{
					return null;
				}
				var firstCourse = first.Course;
				var secondCourse = second.Course;
				double wait;
				if (NumberUnitComparer.Instance.IsZero(distance.Y))
				{
					var xTime = distance.X / Math.Abs(firstCourse.X - secondCourse.X);
					wait = xTime;
				}
				else if (NumberUnitComparer.Instance.IsZero(distance.X))
				{
					var yTime = distance.Y / Math.Abs(firstCourse.Y - secondCourse.Y);
					wait = yTime;
				}
				else
				{
					var xTime = distance.X / Math.Abs(firstCourse.X - secondCourse.X);
					var yTime = distance.Y / Math.Abs(firstCourse.Y - secondCourse.Y);
					wait = Math.Max(xTime, yTime);
				}

				if (wait < offset)
				{
					return null;
				}
				else
				{
					return new WaitResult(wait);
				}
			}
			else
			{
				return null;
			}
		}
		private RelationResult GetMinResult()
		{
			var checker = new MinChecker();
			foreach (var collision in GetTime(First, Second, checker).Concat(GetTime(Second, First, checker)))
			{
				if (checker.Result is null || checker.Result.Offset > collision.Offset)
				{
					checker.Result = collision;
				}
				
				if (NumberUnitComparer.Instance.IsZero(SubtracEpsilon(checker.Result.Offset)))
				{
					checker.Result.Offset = 0;
					return checker.Result;
				}
			}
			if (checker.Result != null)
			{
				checker.Result.Offset = SubtracEpsilon(checker.Result.Offset);
			}
			return (RelationResult)checker.Result ?? InfinitResult.Instance;
		}
		private static double SubtracEpsilon(double value)
		{
			if (value < NumberUnitComparer.Instance.Epsilon)
			{
				return 0;
			}
			else
			{
				return value - NumberUnitComparer.Instance.Epsilon;
			}
		}

		private static InfinitResult GetFlat(double pursuerCourse, double runawayCourse)
			=> pursuerCourse - runawayCourse > 0 ? null : InfinitResult.Instance;
		private InfinitResult GetFlatHorisontal()
		{
			switch (Compare(First.Bounds.Horisontal, Second.Bounds.Horisontal))
			{
				case RangeCompare.Equals: return null;
				case RangeCompare.Less: return GetFlat(First.BoundsCourse.Right, Second.BoundsCourse.Left);
				case RangeCompare.Over: return GetFlat(Second.BoundsCourse.Right, First.BoundsCourse.Left);
				default: throw new InvalidCollisiopnException();
			}
		}
		private InfinitResult GetFlatVertical()
		{
			switch (Compare(First.Bounds.Vertical, Second.Bounds.Vertical))
			{
				case RangeCompare.Equals: return null;
				case RangeCompare.Less: return GetFlat(First.BoundsCourse.Top, Second.BoundsCourse.Bottom);
				case RangeCompare.Over: return GetFlat(Second.BoundsCourse.Top, First.BoundsCourse.Bottom);
				default: throw new InvalidCollisiopnException();
			}
		}
		private RelationResult FlatCheck() => GetFlatHorisontal() ?? GetFlatVertical();

		private enum RangeCompare
		{
			Equals,
			Less,
			Over
		}
		private static RangeCompare Compare(Range first, Range second)
		{
			if (first.Intersect(second))
			{
				return RangeCompare.Equals;
			}
			else if (first.Max < second.Min)
			{
				return RangeCompare.Less;
			}
			else
			{
				return RangeCompare.Over;
			}
		}
		private (Quadrant first, Quadrant second)? GetQuadrant(Rect first, Rect second)
		{
			var xCompare = Compare(first.Horisontal, second.Horisontal);

			switch (xCompare)
			{
				case RangeCompare.Equals:
					var yCompare = Compare(first.Vertical, second.Vertical);
					switch (yCompare)
					{
						case RangeCompare.Equals: return null;
						case RangeCompare.Over: return (Quadrant.Top, Quadrant.Bottom);
						case RangeCompare.Less: return (Quadrant.Bottom, Quadrant.Top);
						default: throw new InvalidCollisiopnException();
					}
				case RangeCompare.Over: return (Quadrant.Right, Quadrant.Left);
				case RangeCompare.Less: return (Quadrant.Left, Quadrant.Right);
				default: throw new InvalidCollisiopnException();
			}
		}
		private Vector128 GetDistance(Rect first, Rect second)
		{
			var xCompare = Compare(first.Horisontal, second.Horisontal);
			var yCompare = Compare(first.Vertical, second.Vertical);

			var x = 0d;
			switch (xCompare)
			{
				case RangeCompare.Over:
					x = first.Left - second.Right;
					break;
				case RangeCompare.Less:
					x = second.Left - first.Right;
					break;
			}
			var y = 0d;
			switch (yCompare)
			{
				case RangeCompare.Over:
					y = first.Bottom - second.Top;
					break;
				case RangeCompare.Less:
					y = second.Bottom - first.Top;
					break;
			}
			return new Vector128(x, y);
		}

		private static IEnumerable<OffsetResult> GetTime(Polygon main, Polygon other, PreviewChecker previewChecker)
		{
			for (int iEdge = 0; iEdge < main.Edges.Length; iEdge++)
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
							yield return new OffsetResult(
								new CollisionData(main, iEdge, other, iVertex),
								time.Value
							);
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
