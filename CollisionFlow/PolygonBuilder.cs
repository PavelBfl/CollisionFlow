using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow
{
	public class PolygonBuilder
	{
		public static IEnumerable<Vector128> RegularPolygon(double radius, int verticesCount)
		{
			const double FULL_ROUND = Math.PI * 2;

			if (verticesCount < 3)
			{
				throw new InvalidOperationException();
			}

			for (int i = 0; i < verticesCount; i++)
			{
				var angleStep = FULL_ROUND / verticesCount * i;

				var x = Math.Cos(angleStep) * radius;
				var y = Math.Sin(angleStep) * radius;

				yield return new Vector128(x, y);
			}
		}

		public PolygonBuilder()
		{

		}
		public PolygonBuilder(Vector128 defaultCourse)
		{
			DefaultCourse = defaultCourse;
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
