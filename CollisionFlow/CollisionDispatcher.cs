using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CollisionFlow
{
	class Relation
	{
		public Relation(Polygon first, Polygon second)
		{
			First = first ?? throw new ArgumentNullException(nameof(first));
			Second = second ?? throw new ArgumentNullException(nameof(second));
		}

		public bool IsFindPrev { get; set; } = false;

		public Polygon First { get; }
		public Polygon Second { get; }

		public double? Time => Result?.Offset;

		private bool resultCalculate = false;
		private CollisionResult result;

		public CollisionResult Result
		{
			get
			{
				if (!resultCalculate)
				{
					result = GetTime();
					resultCalculate = true;
				}
				return result;
			}
		}

		private CollisionResult GetTime()
		{
			var collisions = GetTime(First, Second).Concat(GetTime(Second, First));
			CollisionResult result = null;
			if (First.IsCollision(Second))
			{
				foreach (var collision in collisions)
				{
					if (result is null || result.Offset < collision.Offset)
					{
						result = collision;
					}
				}
			}
			else
			{
				foreach (var collision in collisions)
				{
					if (result is null || result.Offset > collision.Offset)
					{
						result = collision;
					}
					if (NumberUnitComparer.Instance.IsZero(result.Offset))
					{
						return result;
					}
				}
			}
			return result;
		}

		private static IEnumerable<CollisionResult> GetTime(Polygon main, Polygon other)
		{
			var mainLines = main.Lines.ToArray();
			for (int i = 0; i < mainLines.Length; i++)
			{
				var mainLine = mainLines[i];
				foreach (var otherPoint in other.Points)
				{
					var prevMainLine = mainLines[i == 0 ? mainLines.Length - 1 : i - 1];
					var nextMainLine = mainLines[i == mainLines.Length - 1 ? 0 : i + 1];
					var time = GetTime(mainLine, prevMainLine, nextMainLine, otherPoint);
					if (time.HasValue)
					{
						yield return new CollisionResult(main, mainLine, other, otherPoint, time.Value);
					}
				}
			}
		}

		private static double? GetTime(Segment segment, Moved<Vector128, Vector128> freePoin)
		{
			var time = GetTime(segment.Line, freePoin);
			if (time.HasValue)
			{
				var stepSegment = segment.Offset(time.Value);

				var point = freePoin.Offset(time.Value).Target;
				var beginPoint = stepSegment.GetBeginPoint();
				var endPoint = stepSegment.GetEndPoint();

				var inRange = stepSegment.Line.Target.GetOptimalProjection() == LineState.Horisontal ?
					Range.Auto(beginPoint.X, endPoint.X).Contains(point.X) :
					Range.Auto(beginPoint.Y, endPoint.Y).Contains(point.Y);

				return inRange ? time : null;
			}
			else
			{
				return null;
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
					Range.Auto(beginPoint.X, endPoint.X).Contains(point.X):
					Range.Auto(beginPoint.Y, endPoint.Y).Contains(point.Y);

				return inRange ? time : null;
			}
			else
			{
				return null;
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
				return Flat.GetTime(Moved.Create(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X), Moved.Create(currentPointProjection.X, nextPointProjection.X - currentPointProjection.X));
			}
			else
			{
				return Flat.GetTime(Moved.Create(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y), Moved.Create(currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y));
			}
		}
	}

	public class CollisionDispatcher
	{
		private class Link
		{
			public AllowedOffset? AllowedOffset { get; set; } = null;
			public bool IsFindPrev { get; set; } = false;
		}
		private readonly List<List<Link>> links = new List<List<Link>>();
		private readonly List<List<Relation>> relations = new List<List<Relation>>();

		private readonly List<Polygon> polygons = new List<Polygon>();
		public IEnumerable<IPolygonHandler> Polygons => polygons;

		public IPolygonHandler Add(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			var polygon = Polygon.Create(lines);
			polygon.GlobalIndex = polygons.Count;
			polygons.Add(polygon);

			// старая система связей
			if (polygons.Count > 1)
			{
				links.Add(new List<Link>());
				foreach (var row in links)
				{
					row.Add(new Link());
				} 
			}

			// новая система связей
			if (polygons.Count > 1)
			{
				var row = new List<Relation>();
				for (var i = 0; i < polygons.Count - 1; i++)
				{
					row.Add(new Relation(polygon, polygons[i]));
				}
				relations.Add(row);
			}
			return polygon;
		}
		public void Remove(IPolygonHandler handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			var polygon = (Polygon)handler;

			relations.RemoveAt(polygon.GlobalIndex - 1);
			foreach (var row in relations)
			{
				if (polygon.GlobalIndex < row.Count)
				{
					row.RemoveAt(polygon.GlobalIndex);
				}
			}

			polygons.RemoveAt(polygon.GlobalIndex);
			for (var i = polygon.GlobalIndex; i < polygons.Count; i++)
			{
				polygons[i].GlobalIndex = i;
			}
		}

		public CollisionResult OffsetNew(double value)
		{
			CollisionResult resultMin = null;
			foreach (var row in relations)
			{
				foreach (var cell in row)
				{
					var cellResult = cell.Result;
					if (resultMin is null || (cellResult != null && cellResult.Offset < resultMin.Offset))
					{
						resultMin = cellResult;
					}
				}
			}

			var offset = resultMin?.Offset ?? value;
			if (!NumberUnitComparer.Instance.IsZero(offset))
			{
				foreach (var row in relations)
				{
					foreach (var cell in row)
					{
						cell.Result?.Step(offset);
					}
				}
				foreach (var polygon in polygons)
				{
					polygon.Offset(value);
				}
			}

			return resultMin;
		}

		public CollisionResult Offset(double value)
		{
			CollisionResult result = null;
			Link resultLink = null;
			var localPolygons = polygons.Select(x => new PolygonDelta(x, value)).ToArray();
			for (var iMain = 0; iMain < localPolygons.Length; iMain++)
			{
				var main = localPolygons[iMain];
				for (var iOther = iMain + 1; iOther < localPolygons.Length; iOther++)
				{
					var other = localPolygons[iOther];

					var row = links[main.Current.GlobalIndex];
					var rowIndex = other.Current.GlobalIndex - (links.Count - row.Count + 1);
					var aa = row[rowIndex];
					if (aa.AllowedOffset != AllowedOffset.Never && !aa.IsFindPrev)
					{
						if ((main.GroupX & other.GroupX) != 0 && (main.GroupY & other.GroupY) != 0)
						{
							if (IsCollision(main.Current, other.Current))
							{
								result = Offset(main.Current, other.Current, result, value, CourseResult.Max, ref resultLink, aa);
								result = Offset(other.Current, main.Current, result, value, CourseResult.Max, ref resultLink, aa);
								if (!(result is null) && NumberUnitComparer.Instance.Equals(result.Offset, 0))
								{
									aa.IsFindPrev = true;
									return result;
								}
							}
							else
							{

								var allowX = main.Current.GetProjectionX().IsAllowedOffset(other.Current.GetProjectionX(), value);
								if (allowX == AllowedOffset.Collision)
								{
									result = Offset(main.Current, other.Current, result, value, CourseResult.Min, ref resultLink, aa);
									result = Offset(other.Current, main.Current, result, value, CourseResult.Min, ref resultLink, aa);

									if (!(result is null) && NumberUnitComparer.Instance.Equals(result.Offset, 0))
									{
										aa.IsFindPrev = true;
										return result;
									}
								}
								else
								{
									var allowY = main.Current.GetProjectionY().IsAllowedOffset(other.Current.GetProjectionY(), value);
									if (allowY == AllowedOffset.Collision)
									{
										result = Offset(main.Current, other.Current, result, value, CourseResult.Min, ref resultLink, aa);
										result = Offset(other.Current, main.Current, result, value, CourseResult.Min, ref resultLink, aa);

										if (!(result is null) && NumberUnitComparer.Instance.Equals(result.Offset, 0))
										{
											aa.IsFindPrev = true;
											return result;
										}
									}
									else if (allowX == AllowedOffset.Never && allowY == AllowedOffset.Never)
									{
										row[rowIndex].AllowedOffset = AllowedOffset.Never;
									}
								}
							}
						}
					}
				}
			}

			foreach (var row in links)
			{
				foreach (var cell in row)
				{
					cell.IsFindPrev = false;
				}
			}
			if (!(resultLink is null))
			{
				resultLink.IsFindPrev = true;
			}
			var currentOffset = result?.Offset ?? value;
			foreach (var polygon in localPolygons)
			{
				polygon.Current.Offset(currentOffset);
			}

			return result;
		}
		private static CollisionResult Offset(Polygon main, Polygon other, CollisionResult prevResult, double offset, CourseResult courseResult, ref Link resultLink, Link currentLink)
		{
			if (prevResult is null)
			{
				resultLink = currentLink;
				return Offset(main, other, offset, courseResult);
			}
			else
			{
				var currentResult = Offset(main, other, prevResult.Offset, courseResult);
				if (currentResult is null)
				{
					return prevResult;
				}
				else
				{
					if (prevResult.Offset < currentResult.Offset)
					{
						return prevResult;
					}
					else
					{
						resultLink = currentLink;
						return currentResult;
					}
				}
			}
		}

		private enum CourseResult
		{
			Min,
			Max
		}
		private static CollisionResult Offset(Polygon main, Polygon other, double offset, CourseResult courseResult)
		{
			CollisionResult result = null;
			var mainLines = main.Lines.ToArray();
			for (int i = 0; i < mainLines.Length; i++)
			{
				var mainLine = mainLines[i];
				foreach (var otherPoint in other.Points)
				{
					var currentOffset = result?.Offset ?? offset;
					var prevMainLine = mainLines[i == 0 ? mainLines.Length - 1 : i - 1];
					var nextMainLine = mainLines[i == mainLines.Length - 1 ? 0 : i + 1];
					var time = GetTime(mainLine, prevMainLine, nextMainLine, otherPoint, currentOffset);
					if (time.HasValue)
					{
						if (result is null)
						{
							result = new CollisionResult(main, mainLine, other, otherPoint, time.Value);
						}
						else
						{
							switch (courseResult)
							{
								case CourseResult.Min:
									if (!NumberUnitComparer.Instance.Equals(time.Value, 0))
									{
										result = new CollisionResult(main, mainLine, other, otherPoint, time.Value);
									}
									else
									{
										return new CollisionResult(main, mainLine, other, otherPoint, 0);
									}
									break;
								case CourseResult.Max:
									if (result.Offset < time)
									{
										result = new CollisionResult(main, mainLine, other, otherPoint, time.Value);
									}
									break;
								default: throw new InvalidCollisiopnException();
							}
						}
					}
				}
			}
			return result;
		}

		private static double? GetTime(Moved<LineFunction, Vector128> mainLine, Moved<LineFunction, Vector128> prevMainLine, Moved<LineFunction, Vector128> nextMainLine, Moved<Vector128, Vector128> freePoin, double max = double.PositiveInfinity)
		{
			var time = GetTime(mainLine, freePoin);
			if (time.HasValue && NumberUnitComparer.Instance.Compare(time.Value, max) < 0)
			{
				var prevLine = prevMainLine.Offset(time.Value).Target;
				var currentLine = mainLine.Offset(time.Value).Target;
				var nextLine = nextMainLine.Offset(time.Value).Target;

				var point = freePoin.Offset(time.Value).Target;
				var beginPoint = prevLine.Crossing(currentLine);
				var endPoint = nextLine.Crossing(currentLine);

				var inRange = -1 < currentLine.Slope && currentLine.Slope < 1 ?
							InRange(point.X, beginPoint.X, endPoint.X) :
							InRange(point.Y, beginPoint.Y, endPoint.Y);

				return inRange ? time : null;
			}
			else
			{
				return null;
			}
		}
		private static double? GetTime(Moved<LineFunction, Vector128> line, Moved<Vector128, Vector128> freePoin)
		{
			var projectionLine = line.Target.Perpendicular();

			var currentLineProjection = line.Target.Crossing(projectionLine);
			var nextLineProjection = line.Target.OffsetByVector(new Vector128(line.Course.ToVector())).Crossing(projectionLine);

			var currentPointProjection = line.Target.OffsetToPoint(freePoin.Target).Crossing(projectionLine);
			var nextPointProjection = line.Target.OffsetToPoint(new Vector128(freePoin.Target.ToVector() + freePoin.Course.ToVector())).Crossing(projectionLine);

			if (-1 < projectionLine.Slope && projectionLine.Slope < 1)
			{
				return Flat.GetTime(Moved.Create(currentLineProjection.X, nextLineProjection.X - currentLineProjection.X), Moved.Create(currentPointProjection.X, nextPointProjection.X - currentPointProjection.X));
			}
			else
			{
				return Flat.GetTime(Moved.Create(currentLineProjection.Y, nextLineProjection.Y - currentLineProjection.Y), Moved.Create(currentPointProjection.Y, nextPointProjection.Y - currentPointProjection.Y));
			}
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

		private static bool IsCollision(Polygon polygon1, Polygon polygon2)
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

	public class InvalidCollisiopnException : InvalidOperationException
	{
		public InvalidCollisiopnException()
		{
		}

		public InvalidCollisiopnException(string message) : base(message)
		{
		}

		public InvalidCollisiopnException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidCollisiopnException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	class PolygonDelta
	{
		public PolygonDelta(Polygon current, double offset)
		{
			Offset = offset;
			Current = current ?? throw new ArgumentNullException(nameof(current));
		}

		public double Offset { get; }
		public Polygon Current { get; }

		private Polygon next;
		public Polygon Next
		{
			get
			{
				if (next is null)
				{
					next = Current.Step(Offset);
				}
				return next;
			}
		}

		private ulong? groupX;
		public ulong GroupX
		{
			get
			{
				if (groupX is null)
				{
					groupX = Flat.UnionGroup(Current.GetProjectionX().Group, Next.GetProjectionX().Group);
				}
				return groupX.GetValueOrDefault();
			}
		}

		private ulong? groupY;
		public ulong GroupY
		{
			get
			{
				if (groupY is null)
				{
					groupY = Flat.UnionGroup(Current.GetProjectionY().Group, Next.GetProjectionY().Group);
				}
				return groupY.GetValueOrDefault();
			}
		}
	}
	enum PolygonType
	{
		None,
		Static,
	}
	abstract class Polygon : IPolygonHandler
	{
		public static Polygon Create(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			return new CommonPolygon(lines);
		}

		public int GlobalIndex { get; set; }
		public PolygonType Type { get; } = PolygonType.None;
		public abstract IEnumerable<Moved<LineFunction, Vector128>> Lines { get; }
		public abstract Moved<Vector128, Vector128>[] Points { get; }
		public abstract Rect Bounds { get; }
		public abstract void Offset(double value);
		public abstract Flat GetProjectionX();
		public abstract Flat GetProjectionY();

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

			return CollisionDispatcher.IsCollision(
				Points.Select(x => x.Target),
				other.Points.Select(x => x.Target)
			);
		}

		public IEnumerable<Moved<Vector128, Vector128>> GetPoints() => Points;
		public abstract Polygon Step(double offset);
	}
	class CommonPolygon : Polygon
	{
		private const int POLYGOM_MIN_VERTICIES = 3;

		public CommonPolygon(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			if (lines is null)
			{
				throw new ArgumentNullException(nameof(lines));
			}

			var linesInstance = lines.ToArray();
			if (linesInstance.Length < POLYGOM_MIN_VERTICIES)
			{
				throw new InvalidCollisiopnException();
			}

			this.lines = linesInstance;
		}

		private readonly Moved<LineFunction, Vector128>[] lines;
		public override IEnumerable<Moved<LineFunction, Vector128>> Lines => lines;

		private Moved<Vector128, Vector128>[] points;
		public override Moved<Vector128, Vector128>[] Points
		{
			get
			{
				if (points is null)
				{
					points = new Moved<Vector128, Vector128>[lines.Length];
					var prevIndex = lines.Length - 1;
					for (var index = 0; index < lines.Length; index++)
					{
						var prevLine = lines[prevIndex];
						var currentLine = lines[index];

						var prevLineOffset = prevLine.Target.OffsetByVector(prevLine.Course);
						var currentLineOffset = currentLine.Target.OffsetByVector(currentLine.Course);

						var currentPoint = prevLine.Target.Crossing(currentLine.Target);

						points[index] = Moved.Create(
							currentPoint,
							new Vector128(prevLineOffset.Crossing(currentLineOffset).ToVector() - currentPoint.ToVector())
						);
						prevIndex = index;
					}
				}
				return points;
			}
		}

		private Rect? bounds;
		public override Rect Bounds
		{
			get
			{
				if (bounds is null)
				{
					bounds = new Rect(Points.Select(x => x.Target));
				}
				return bounds.GetValueOrDefault();
			}
		}

		private Flat projectionX;
		public override Flat GetProjectionX()
		{
			if (projectionX is null)
			{
				projectionX = new Flat(Points.Select(x => Moved.Create(x.Target.X, x.Course.X)));
			}
			return projectionX;
		}

		private Flat projectionY;
		public override Flat GetProjectionY()
		{
			if (projectionY is null)
			{
				projectionY = new Flat(Points.Select(x => Moved.Create(x.Target.Y, x.Course.Y)));
			}
			return projectionY;
		}

		public override Polygon Step(double offset)
		{
			var linesOffset = new Moved<LineFunction, Vector128>[lines.Length];
			for (int i = 0; i < lines.Length; i++)
			{
				linesOffset[i] = lines[i].Offset(offset);
			}
			return new CommonPolygon(linesOffset);
		}

		public override void Offset(double value)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Offset(value);
			}
			points = null;
			bounds = null;
			projectionX = null;
			projectionY = null;
		}
	}

	public interface IPolygonHandler
	{
		IEnumerable<Moved<Vector128, Vector128>> GetPoints();
	}
}
