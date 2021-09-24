using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public class BodyDispatcher
	{
		private static bool IsDeadSpeed(Vector128 speed, Body body)
		{
			if (0 <= body.Bounce && body.Bounce <= 1)
			{
				if (!body.Pull.Equals(Vector128.Zero))
				{
					var reverseBounce = 1 - body.Bounce;

					var reverseSpeed = speed.ToVector() * reverseBounce;

					var pullLength = Math.Max(Math.Abs(body.Pull.X), Math.Abs(body.Pull.Y));
					return Math.Abs(reverseSpeed.GetX()) < pullLength && Math.Abs(reverseSpeed.GetY()) < pullLength;
				}
				else
				{
					const double MIN_SPEED = 0.01;
					return Math.Abs(speed.X) < MIN_SPEED && Math.Abs(speed.Y) < MIN_SPEED;
				}
			}
			else
			{
				return false;
			}
		}

		public double StepLength { get; } = 1d;
		private double LastStep { get; set; } = 0;

		public CollisionDispatcher Dispatcher { get; } = new CollisionDispatcher();
		public List<Body> Bodies { get; } = new List<Body>();

		public void Offset(double value)
		{
			if (value <= LastStep)
			{
				OffsetStep(value);
				LastStep -= value;
				if (LastStep == 0)
				{
					Refresh();
				}
			}
			else if (value <= StepLength)
			{
				OffsetStep(LastStep);
				Refresh();
				OffsetStep(value - LastStep);
				LastStep = StepLength - (value - LastStep);
			}
			else
			{
				var offset = LastStep > 0 ? LastStep : StepLength;
				while (value > 0)
				{
					OffsetStep(offset);
					Refresh();
					value -= offset;
					LastStep -= offset;
					offset = Math.Min(value, StepLength);
				}
				while (LastStep < 0)
				{
					LastStep += StepLength;
				}
			}
		}

		private void Refresh()
		{
			foreach (var body in Bodies)
			{
				body.Refresh();
			}
		}

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
		private void OffsetStep(double value)
		{
			var result = Dispatcher.Offset(value);
			while (!(result is null) && result.Offset < value)
			{
				foreach (var pairResult in result.Results)
				{
					var edgeBody = (Body)pairResult.EdgePolygon.AttachetData;
					var vertexBody = (Body)pairResult.VertexPolygon.AttachetData;

					var coefficientX = 2 * (edgeBody.Weight * edgeBody.Course.X + vertexBody.Weight * vertexBody.Course.X) / (edgeBody.Weight + vertexBody.Weight);
					var coefficientY = 2 * (edgeBody.Weight * edgeBody.Course.Y + vertexBody.Weight * vertexBody.Course.Y) / (edgeBody.Weight + vertexBody.Weight);

					var edgeV = new Vector128(
						coefficientX - edgeBody.Course.X,
						coefficientY - edgeBody.Course.Y
					);
					var vertexV = new Vector128(
						coefficientX - vertexBody.Course.X,
						coefficientY - vertexBody.Course.Y
					);

					var edge =
						GetLine(pairResult.Vertex.Target, pairResult.EdgePolygon.GetBeginVertex(pairResult.EdgeIndex).Target) ??
						GetLine(pairResult.Vertex.Target, pairResult.EdgePolygon.GetEndVertex(pairResult.EdgeIndex).Target) ??
						pairResult.Edge.Target;
					
					var edgeCourse = (Mirror(edge, pairResult.Vertex.Target, edgeV).ToVector() * edgeBody.Bounce).ToVector128();
					if (IsDeadSpeed(edgeCourse, edgeBody))
					{
						edgeBody.CreateRest(vertexBody);
					}
					else
					{
						edgeBody.Push(edgeCourse);
					}

					var vertexCourse = (Mirror(edge, pairResult.Vertex.Target, vertexV).ToVector() * vertexBody.Bounce).ToVector128();
					if (IsDeadSpeed(vertexCourse, vertexBody))
					{
						vertexBody.CreateRest(edgeBody);
					}
					else
					{
						vertexBody.Push(vertexCourse);
					}
				}

				value -= result.Offset;
				result = Dispatcher.Offset(value);
			}
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
}
