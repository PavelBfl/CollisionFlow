using CollisionFlow;
using System;
using System.Collections.Generic;

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
		private static Vector128 TrembleSpeed(Vector128 vector)
		{
			return new Vector128(
				TrembleSpeed(vector.X),
				TrembleSpeed(vector.Y)
			);
		}
		private static double TrembleSpeed(double value)
		{
			if (Math.Abs(value) < MIN_SPEED)
			{
				return value < 0 ? -MIN_SPEED : MIN_SPEED;
			}
			else
			{
				return value;
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

		private static double GetSpeedWithoutTremble(double origin)
			=> Math.Abs(origin) <= MIN_SPEED ? 0 : origin;
		private static Vector128 GetCourseWithoutTremble(Body body)
		{
			return new Vector128(
				GetSpeedWithoutTremble(body.Course.V.GetX()),
				GetSpeedWithoutTremble(body.Course.V.GetY())
			);
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

					var edgeCourseWithoutTremble = GetCourseWithoutTremble(edgeBody);
					var vertecCourseWithoutTremble = GetCourseWithoutTremble(vertexBody);
					var coefficientX = 2 * (edgeBody.Weight * edgeCourseWithoutTremble.X + vertexBody.Weight * vertecCourseWithoutTremble.X) / (edgeBody.Weight + vertexBody.Weight);
					var coefficientY = 2 * (edgeBody.Weight * edgeCourseWithoutTremble.Y + vertexBody.Weight * vertecCourseWithoutTremble.Y) / (edgeBody.Weight + vertexBody.Weight);

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
					edgeCourse = TrembleSpeed(edgeCourse);
					edgeBody.Speed = new Vector128(edgeCourse.X, edgeCourse.Y);

					var vertexCourse = (Mirror(edge, pairResult.Vertex.Target, vertexV).ToVector() * vertexBody.Bounce).ToVector128();
					vertexCourse = TrembleSpeed(vertexCourse);
					vertexBody.Speed = new Vector128(vertexCourse.X, vertexCourse.Y);
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
}
