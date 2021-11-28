using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public class BodyDispatcher
	{
		public const double DEFAULT_GRAVITY = 0.03;
		private const double MIN_SPEED = 0.001;

		private static bool IsDeadSpeed(Vector128 speed)
		{
			return Math.Abs(speed.X) < MIN_SPEED && Math.Abs(speed.Y) < MIN_SPEED;
		}

		private static ulong trembleCounter = 0;
		private static Vector128 TrembleSpeed()
		{
			trembleCounter++;
			switch (trembleCounter % 4)
			{
				case 0: return new Vector128(MIN_SPEED, MIN_SPEED);
				case 1: return new Vector128(-MIN_SPEED, -MIN_SPEED);
				case 2: return new Vector128(MIN_SPEED, -MIN_SPEED);
				case 3: return new Vector128(-MIN_SPEED, MIN_SPEED);
				default: throw new InvalidOperationException();
			}
		}

		public CollisionSpace Dispatcher { get; } = new CollisionSpace();
		public List<Body> Bodies { get; } = new List<Body>();

		private static bool IsNear(Vector128 main, Vector128 other)
		{
			return Math.Abs(main.X - other.X) <= 0.001 &&
				Math.Abs(main.Y - other.Y) <= 0.001;
		}
		private static LineFunction? GetLine(Vector128 vertex, Vector128 edgePoint)
		{
			if (IsNear(vertex, edgePoint))
			{
				return new LineFunction(
					(vertex.ToVector() * 10).ToVector128(),
					(edgePoint.ToVector() * 10).ToVector128()
				).Perpendicular().OffsetToPoint(edgePoint);
			}
			return null;
		}
		public double? Offset(double value)
		{
			var result = Dispatcher.Offset(value);

			if (!(result is null))
			{
				foreach (var pairResult in result.Results)
				{
					var edgeBody = (Body)pairResult.EdgePolygon.AttachetData;
					var vertexBody = (Body)pairResult.VertexPolygon.AttachetData;

					var coefficientX = 2 * (edgeBody.Weight * edgeBody.Course.V.GetX() + vertexBody.Weight * vertexBody.Course.V.GetX()) / (edgeBody.Weight + vertexBody.Weight);
					var coefficientY = 2 * (edgeBody.Weight * edgeBody.Course.V.GetY() + vertexBody.Weight * vertexBody.Course.V.GetY()) / (edgeBody.Weight + vertexBody.Weight);

					var edgeV = new Vector128(
						coefficientX - edgeBody.Course.V.GetX(),
						coefficientY - edgeBody.Course.V.GetY()
					);
					var vertexV = new Vector128(
						coefficientX - vertexBody.Course.V.GetX(),
						coefficientY - vertexBody.Course.V.GetY()
					);

					var edge =
						GetLine(pairResult.Vertex.Target, pairResult.EdgePolygon.GetBeginVertex(pairResult.EdgeIndex).Target) ??
						GetLine(pairResult.Vertex.Target, pairResult.EdgePolygon.GetEndVertex(pairResult.EdgeIndex).Target) ??
						pairResult.Edge.Target;

					var edgeCourse = (Mirror(edge, pairResult.Vertex.Target, edgeV).ToVector() * edgeBody.Bounce).ToVector128();
					if (IsDeadSpeed(edgeCourse))
					{
						if (edgeBody.IsTremble)
						{
							edgeBody.Speed = TrembleSpeed();
							edgeBody.Refresh();
						}
						else
						{
							edgeBody.CreateRest(vertexBody); 
						}
					}
					else
					{
						edgeBody.Speed = new Vector128(edgeCourse.X, edgeCourse.Y);
					}

					var vertexCourse = (Mirror(edge, pairResult.Vertex.Target, vertexV).ToVector() * vertexBody.Bounce).ToVector128();
					if (IsDeadSpeed(vertexCourse))
					{
						if (vertexBody.IsTremble)
						{
							vertexBody.Speed = TrembleSpeed();
							vertexBody.Refresh();
						}
						else
						{
							vertexBody.CreateRest(edgeBody); 
						}
					}
					else
					{
						vertexBody.Speed = new Vector128(vertexCourse.X, vertexCourse.Y);
					}
				}
			}
			return result?.Offset;
		}
		private static Vector128 Mirror(LineFunction line, Vector128 vertex, Vector128 course)
		{
			var nextLine = line.OffsetByVector(course);
			var overrideLine = line
						.Perpendicular()
						.OffsetToPoint(vertex)
						.OffsetByVector((-course.ToVector()).ToVector128());
			var newPoint = nextLine.Crossing(overrideLine);
			return new Vector128(
				newPoint.X - vertex.X,
				newPoint.Y - vertex.Y
			);
		}
	}

	public class CollisionSpace
	{
		public CollisionSpace()
		{
			Expectations = new SortedList<double, List<IFlowEvent>>(Comparer);
		}

		public IComparer<double> Comparer { get; } = Comparer<double>.Default;
		public double Time { get; private set; } = 0;
		private SortedList<double, List<IFlowEvent>> Expectations { get; }

		public void AddExpectationOffset(IFlowEvent flowEvent, double offset)
		{
			var time = Time + offset;
			if (!Expectations.TryGetValue(time, out var events))
			{
				events = new List<IFlowEvent>();
				Expectations.Add(time, events);
			}

			events.Add(flowEvent);
		}
		public void Remove(IFlowEvent flowEvent)
		{
			if (flowEvent is null)
			{
				throw new ArgumentNullException(nameof(flowEvent));
			}

			foreach (var expection in Expectations.ToArray())
			{
				expection.Value.Remove(flowEvent);
				if (!expection.Value.Any())
				{
					Expectations.Remove(expection.Key);
				}
			}
		}

		public CollisionDispatcher CollisionDispatcher { get; } = new CollisionDispatcher();

		private void ExpectationsHandle(double time)
		{
			while (Expectations.Any())
			{
				var key = Expectations.Keys[0];
				if (Comparer.Compare(key, time) < 0)
				{
					var expectation = Expectations.Values[0];
					Expectations.RemoveAt(0);

					foreach (var flowEvent in expectation)
					{
						flowEvent.Handle();
					}
				}
				else
				{
					return;
				}
			}
		}

		public GroupCollisionResult Offset(double value)
		{
			Time += value;
			ExpectationsHandle(Time);

			return CollisionDispatcher.Offset(value);
		}
	}

	public interface IFlowEvent
	{
		void Handle();
	}
}
