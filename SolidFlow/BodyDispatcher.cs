using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public class BodyDispatcher
	{
		public const double GRAVITY = 0.03;

		private static bool IsDeadSpeed(Vector128 speed, Body body)
		{
			const double MIN_SPEED = 0.001;

			return Math.Abs(speed.X) < MIN_SPEED && Math.Abs(speed.Y) < MIN_SPEED;
		}

		public CollisionDispatcher Dispatcher { get; } = new CollisionDispatcher();
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
					if (IsDeadSpeed(edgeCourse, edgeBody))
					{
						edgeBody.CreateRest(vertexBody);
					}
					else
					{
						edgeBody.Course = new Course(
							Vector128.Create(edgeCourse.X, edgeCourse.Y),
							Vector128.Create(0, GRAVITY)
						);
					}

					var vertexCourse = (Mirror(edge, pairResult.Vertex.Target, vertexV).ToVector() * vertexBody.Bounce).ToVector128();
					if (IsDeadSpeed(vertexCourse, vertexBody))
					{
						vertexBody.CreateRest(edgeBody);
					}
					else
					{
						vertexBody.Course = new Course(
							Vector128.Create(vertexCourse.X, vertexCourse.Y),
							Vector128.Create(0, GRAVITY)
						);
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
}
