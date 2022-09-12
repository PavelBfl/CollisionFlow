using Flowing.Mutate;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow.Polygons
{
	public class PolygonBuilder
	{
		public static IEnumerable<Vector2<double>> RegularPolygon(double radius, int verticesCount, double offset = 0)
		{
			const double FULL_ROUND = Math.PI * 2;

			if (verticesCount < 3)
			{
				throw new InvalidOperationException();
			}

			for (int i = 0; i < verticesCount; i++)
			{
				var angleStep = FULL_ROUND / verticesCount * i + offset;

				var x = Math.Cos(angleStep) * radius;
				var y = Math.Sin(angleStep) * radius;

				yield return Vector2Builder.Create(x, y);
			}
		}

		public static PolygonBuilder CreateRegular(double radius, int verticesCount, double offset = 0, Vector2<CourseA>? center = null)
			=> new PolygonBuilder(RegularPolygon(radius, verticesCount, offset), center ?? Vector2Extensions.CourseZero);
		public static PolygonBuilder CreateRect(Rect rect, Vector2<CourseA>? course = null)
			=> new PolygonBuilder(course ?? Vector2Extensions.CourseZero)
				.Add(Vector2Builder.Create(rect.Left, rect.Top))
				.Add(Vector2Builder.Create(rect.Right, rect.Top))
				.Add(Vector2Builder.Create(rect.Right, rect.Bottom))
				.Add(Vector2Builder.Create(rect.Left, rect.Bottom));

		public PolygonBuilder()
		{

		}
		public PolygonBuilder(Vector2<CourseA> defaultCourse)
		{
			DefaultCourse = defaultCourse;
		}
		public PolygonBuilder(IEnumerable<Vector2<double>> vertices, Vector2<CourseA> defaultCourse)
			: this(defaultCourse)
		{
			if (vertices is null)
			{
				throw new ArgumentNullException(nameof(vertices));
			}
			foreach (var vertex in vertices)
			{
				Add(vertex);
			}
		}

		private List<Vector2<Mutated<double, CourseA>>> Points { get; } = new List<Vector2<Mutated<double, CourseA>>>();
		private Vector2<Mutated<double, CourseA>> GetLast()
		{
			return Points.Count > 0 ? Points[Points.Count - 1] : throw new InvalidOperationException();
		}

		public Vector2<CourseA> DefaultCourse { get; set; } = Vector2Extensions.CourseZero;
		public PolygonBuilder SetDefault(Vector2<CourseA> course)
		{
			DefaultCourse = course;
			return this;
		}

		public PolygonBuilder Add(Vector2<double> point, Vector2<CourseA> course)
		{
			Points.Add(Vector2Builder.Create(
				Moved.Create(point.X, course.X),
				Moved.Create(point.Y, course.Y)
			));
			return this;
		}
		public PolygonBuilder Add(Vector2<double> point) => Add(point, DefaultCourse);

		public PolygonBuilder Offset(Vector2<double> point, Vector2<CourseA> course)
		{
			return Add((point.ToVector() + GetLast().GetTarget().ToVector()).ToVector2(), course);
		}
		public PolygonBuilder Offset(Vector2<double> point) => Offset(point, DefaultCourse);
		public PolygonBuilder OffsetX(double x, Vector2<CourseA> course) => Offset(Vector2Builder.Create(x, 0), course);
		public PolygonBuilder OffsetX(double x) => OffsetX(x, DefaultCourse);
		public PolygonBuilder OffsetY(double y, Vector2<CourseA> course) => Offset(Vector2Builder.Create(0, y), course);
		public PolygonBuilder OffsetY(double y) => OffsetY(y, DefaultCourse);
		public PolygonBuilder OffsetAll(Vector2<double> vector)
		{
			var result = new PolygonBuilder(DefaultCourse);
			foreach (var point in Points)
			{
				result.Add((point.GetTarget().ToVector() + vector.ToVector()).ToVector2(), point.GetCource());
			}
			return result;
		}
		
		public PolygonBuilder SetAllCourse(Vector2<CourseA> course)
		{
			var result = new PolygonBuilder(DefaultCourse);
			foreach (var point in Points)
			{
				result.Add(point.GetTarget(), course);
			}
			return result;
		}

		public IEnumerable<Mutated<LineFunction, Vector2<CourseA>>> GetLines()
		{
			if (Points.Count < 3)
			{
				throw new InvalidOperationException();
			}

			var prevIndex = Points.Count - 1;
			for (int i = 0; i < Points.Count; i++)
			{
				var prevPoint = Points[prevIndex];
				var currentIndex = Points[i];
				yield return Moved.Create(new LineFunction(prevPoint.GetTarget(), currentIndex.GetTarget()), prevPoint.GetCource());
				prevIndex = i;
			}
		}
	}
}
