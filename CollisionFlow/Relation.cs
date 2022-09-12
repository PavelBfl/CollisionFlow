using System.Collections.Generic;
using System;
using System.Linq;
using CollisionFlow.Polygons;
using Flowing.Mutate;

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
			if (value > Offset)
			{
				throw new InvalidCollisiopnException();
			}
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
					checker.Result.Offset = AddSteps(checker.Result, 1);
				}
				return checker.Result;
			}
			else
			{
				if (First is UndeformablePolygon undeformableFirst && Second is UndeformablePolygon undeformableSecond)
				{
					var result = GetWaitResult(undeformableFirst, undeformableSecond, offset);
					return result ?? GetMinResult();
				}
				return GetMinResult();
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

				var prevTime = UnitComparer.Time.Decrement(checker.Result.Offset);
				if (UnitComparer.Time.Compare(prevTime, 0) <= 0)
				{
					checker.Result.Offset = 0;
					return checker.Result;
				}
			}
			if (checker.Result != null)
			{
				var prevTime = UnitComparer.Time.Decrement(checker.Result.Offset);
				if (UnitComparer.Time.Compare(prevTime, 0) <= 0)
				{
					prevTime = 0;
				}

				checker.Result.Offset = prevTime;
			}
			return (RelationResult)checker.Result ?? InfinitResult.Instance;
		}
		private static double AddSteps(OffsetResult result, int steps)
		{
			var vector = (result.CollisionData.Vertex.GetCource().GetV().ToVector() - result.CollisionData.Edge.Course.GetV().ToVector()).ToVector2();

			var length = Math.Abs(vector.X) > Math.Abs(vector.Y) ? vector.X : vector.Y;

			var timeStep = length >= UnitComparer.Position.Epsilon ?
				UnitComparer.Position.Epsilon / length :
				UnitComparer.Position.Epsilon;

			return result.Offset + steps * timeStep;
		}

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
		private static RelationResult GetWaitResult(UndeformablePolygon mainPolygon, UndeformablePolygon otherPolygon, double time)
		{
			switch (Compare(mainPolygon.Bounds.Horisontal, otherPolygon.Bounds.Horisontal))
			{
				case RangeCompare.Equals:
					switch (Compare(mainPolygon.Bounds.Vertical, otherPolygon.Bounds.Vertical))
					{
						case RangeCompare.Equals: return null;
						case RangeCompare.Less:
							return GetFlatResult(
								new Mutated<double, CourseA>(mainPolygon.Bounds.Top, mainPolygon.Course.Y),
								new Mutated<double, CourseA>(otherPolygon.Bounds.Bottom, otherPolygon.Course.Y),
								time
							);
						case RangeCompare.Over:
							return GetFlatResult(
								new Mutated<double, CourseA>(otherPolygon.Bounds.Top, otherPolygon.Course.Y),
								new Mutated<double, CourseA>(mainPolygon.Bounds.Bottom, mainPolygon.Course.Y),
								time
							);
						default: throw new InvalidOperationException();
					}
				case RangeCompare.Less:
					return GetFlatResult(
						new Mutated<double, CourseA>(mainPolygon.Bounds.Right, mainPolygon.Course.X),
						new Mutated<double, CourseA>(otherPolygon.Bounds.Left, otherPolygon.Course.X),
						time
					);
				case RangeCompare.Over:
					return GetFlatResult(
						new Mutated<double, CourseA>(otherPolygon.Bounds.Right, otherPolygon.Course.X),
						new Mutated<double, CourseA>(mainPolygon.Bounds.Left, mainPolygon.Course.X),
						time
					);
				default: throw new InvalidOperationException();
			}
		}
		private static RelationResult GetFlatResult(Mutated<double, CourseA> main, Mutated<double, CourseA> other, double time)
		{
			if (main.GetTimeCollision(other) is TimeA collisionTime)
			{
				double? waitTime = null;
				if (collisionTime.Result1 > 0)
				{
					waitTime = collisionTime.Result1;
				}
				if (collisionTime.Result2 > 0 && collisionTime.Result2 > collisionTime.Result1)
				{
					waitTime = collisionTime.Result2;
				}

				if (waitTime.HasValue && waitTime.Value > time)
				{
					if (waitTime.Value > 100000000)
					{
						return InfinitResult.Instance;
					}
					else
					{
						return new WaitResult(waitTime.Value); 
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				return InfinitResult.Instance;
			}
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
					if (time.HasValue)
					{
						var timeInstance = time.Value;
						if (timeInstance.Result1 < 0 && timeInstance.Result2 < 0)
						{
							throw new InvalidOperationException();
						}
						if (timeInstance.Result1 >= 0 &&
							previewChecker.Check(timeInstance.Result1) &&
							InRange(timeInstance.Result1, mainLine.Target.GetOptimalProjection(), main.GetBeginVertex(iEdge), main.GetEndVertex(iEdge), otherPoint)
						)
						{
							yield return new OffsetResult(
								new CollisionData(main, iEdge, other, iVertex),
								timeInstance.Result1
							);
						}

						if (timeInstance.Result2 >= 0 &&
							previewChecker.Check(timeInstance.Result2) &&
							InRange(timeInstance.Result2, mainLine.Target.GetOptimalProjection(), main.GetBeginVertex(iEdge), main.GetEndVertex(iEdge), otherPoint)
						)
						{
							yield return new OffsetResult(
								new CollisionData(main, iEdge, other, iVertex),
								timeInstance.Result2
							);
						}
					}
				}
			}
		}

		private static bool InRange(double time, LineState state, Vector2<Mutated<double, CourseA>> begin, Vector2<Mutated<double, CourseA>> end, Vector2<Mutated<double, CourseA>> freePoin)
		{
			var beginOffset = begin.Offset(time).GetTarget();
			var endOffset = end.Offset(time).GetTarget();
			var freePointOffset = freePoin.Offset(time).GetTarget();
			return state == LineState.Horisontal ?
				Contains(beginOffset.X, endOffset.X, freePointOffset.X) :
				Contains(beginOffset.Y, endOffset.Y, freePointOffset.Y);
		}
		private static bool Contains(double first, double second, double value)
		{
			if (UnitComparer.Position.InRange(first) && UnitComparer.Position.InRange(second))
			{
				if (UnitComparer.Position.Equals(first, second))
				{
					return UnitComparer.Position.Equals(first, value);
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
		private static TimeA? GetTime(Mutated<LineFunction, Vector2<CourseA>> line, Vector2<Mutated<double, CourseA>> freePoin)
		{
			var freeLine = line.Target.OffsetToPoint(freePoin.GetTarget());
			var freeV = freeLine.GetCourseOffset(freePoin.GetCource().GetV());
			var freeA = freeLine.GetCourseOffset(freePoin.GetCource().GetA());

			var lineV = line.Target.GetCourseOffset(line.Course.GetV());
			var lineA = line.Target.GetCourseOffset(line.Course.GetA());

			var freeMutated = Moved.Create(freeLine.Offset, new CourseA(freeV, freeA));
			var lineMutated = Moved.Create(line.Target.Offset, new CourseA(lineV, lineA));
			return freeMutated.GetTimeCollision(lineMutated);
		}
		[Obsolete]
		public static double? GetTime(Mutated<double, double> point1, Mutated<double, double> point2)
		{
			var (min, max) = point1.Target < point2.Target ? (point1, point2) : (point2, point1);

			var speed = min.Course - max.Course;
			if (speed > 0)
			{
				var distance = max.Target - min.Target;
				var localOffset = distance / speed;
				return UnitComparer.Time.InRange(localOffset) ? localOffset : new double?();
			}
			else
			{
				return null;
			}
		}
	}
}
