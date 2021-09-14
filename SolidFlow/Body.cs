﻿using CollisionFlow;
using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;

namespace SolidFlow
{
	public class BodyDispatcher
	{
		public double StepLength { get; } = 1d;
		public CollisionDispatcher Dispatcher { get; } = new CollisionDispatcher();
		public List<Body> Bodies { get; } = new List<Body>();

		public void Offset(double value)
		{
			do
			{
				var offset = value > StepLength ? StepLength : value;
				OffsetStep(offset);
				value -= offset;
			} while (value > 0);
		}
		private void OffsetStep(double value)
		{
			foreach (var body in Bodies)
			{
				body.Course = (body.Course.ToVector() + (Vector128.Create(0, 0.1) * body.Weight)).ToVector128();
			}
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
					edgeBody.Course = edgeV;
					vertexBody.Course = vertexV;
				}

				value -= result.Offset;
				result = Dispatcher.Offset(value);
			}
		}
	}
	public class Body
	{
		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies)
			: this(dispatcher, verticies, Vector128.Zero)
		{

		}
		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies, Vector128 course)
		{
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			if (verticies is null)
			{
				throw new ArgumentNullException(nameof(verticies));
			}

			this.course = course;
			var builder = new PolygonBuilder(course);
			foreach (var vertex in verticies)
			{
				builder.Add(vertex);
			}
			Handler = Dispatcher.Add(builder.GetLines());
			Handler.AttachetData = this;
		}

		public CollisionDispatcher Dispatcher { get; }
		public IPolygonHandler Handler { get; private set; }
		public double Weight { get; set; } = 1;

		private Vector128 course;
		public Vector128 Course
		{
			get => course;
			set
			{
				if (!Course.Equals(value))
				{
					course = value;
					var builder = new PolygonBuilder(Course);
					foreach (var vertex in Handler.Vertices)
					{
						builder.Add(vertex.Target);
					}
					Dispatcher.Remove(Handler);
					Handler = Dispatcher.Add(builder.GetLines());
					Handler.AttachetData = this;
				}
			}
		}
	}
}