using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow.Polygons
{
	public class PolygonBuilder
	{
		public static IEnumerable<Vector128> RegularPolygon(double radius, int verticesCount, double offset = 0)
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

				yield return new Vector128(x, y);
			}
		}

		public static PolygonBuilder CreateRegular(double radius, int verticesCount, double offset = 0, Vector128? center = null)
			=> new PolygonBuilder(RegularPolygon(radius, verticesCount, offset), center ?? Vector128.Zero);
		public static PolygonBuilder CreateRect(Rect rect, Vector128? course = null)
			=> new PolygonBuilder(course ?? Vector128.Zero)
				.Add(new Vector128(rect.Left, rect.Top))
				.Add(new Vector128(rect.Right, rect.Top))
				.Add(new Vector128(rect.Right, rect.Bottom))
				.Add(new Vector128(rect.Left, rect.Bottom));

		public PolygonBuilder()
		{

		}
		public PolygonBuilder(Vector128 defaultCourse)
		{
			DefaultCourse = defaultCourse;
		}
		public PolygonBuilder(IEnumerable<Vector128> vertices, Vector128 defaultCourse)
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

		private List<Moved<Vector128, Vector128>> Points { get; } = new List<Moved<Vector128, Vector128>>();
		private Moved<Vector128, Vector128> GetLast()
		{
			return Points.Count > 0 ? Points[Points.Count - 1] : throw new InvalidOperationException();
		}

		public Vector128 DefaultCourse { get; set; } = Vector128.Zero;
		public PolygonBuilder SetDefault(Vector128 course)
		{
			DefaultCourse = course;
			return this;
		}

		public PolygonBuilder Add(Vector128 point, Vector128 course)
		{
			Points.Add(Moved.Create(point, course));
			return this;
		}
		public PolygonBuilder Add(Vector128 point) => Add(point, DefaultCourse);

		public PolygonBuilder Offset(Vector128 point, Vector128 course)
		{
			return Add(new Vector128(point.ToVector() + GetLast().Target.ToVector()), course);
		}
		public PolygonBuilder Offset(Vector128 point) => Offset(point, DefaultCourse);
		public PolygonBuilder OffsetX(double x, Vector128 course) => Offset(new Vector128(x, 0), course);
		public PolygonBuilder OffsetX(double x) => OffsetX(x, DefaultCourse);
		public PolygonBuilder OffsetY(double y, Vector128 course) => Offset(new Vector128(0, y), course);
		public PolygonBuilder OffsetY(double y) => OffsetY(y, DefaultCourse);
		public PolygonBuilder OffsetAll(Vector128 vector)
		{
			var result = new PolygonBuilder(DefaultCourse);
			foreach (var point in Points)
			{
				result.Add((point.Target.ToVector() + vector.ToVector()).ToVector128(), point.Course);
			}
			return result;
		}
		public PolygonBuilder OffsetCourse(Vector128 vector)
		{
			var result = new PolygonBuilder(DefaultCourse);
			foreach (var point in Points)
			{
				result.Add(point.Target, (point.Course.ToVector() + vector.ToVector()).ToVector128());
			}
			return result;
		}
		public PolygonBuilder SetAllCourse(Vector128 vector)
		{
			var result = new PolygonBuilder(DefaultCourse);
			foreach (var point in Points)
			{
				result.Add(point.Target, vector);
			}
			return result;
		}

		public IEnumerable<Moved<LineFunction, Vector128>> GetLines()
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
				yield return Moved.Create(new LineFunction(prevPoint.Target, currentIndex.Target), prevPoint.Course);
				prevIndex = i;
			}
		}
	}
}
