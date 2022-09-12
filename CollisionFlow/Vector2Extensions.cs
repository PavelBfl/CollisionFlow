using System;
using Flowing.Mutate;

namespace CollisionFlow
{
	public static class Vector2Extensions
	{
		public static Vector2<double> Zero { get; } = Vector2Builder.Create(0d);

		public static Vector2<CourseA> CourseZero { get; } = Vector2Builder.Create(CourseA.Zero);

		public static Rect Offset(this Vector2<CourseA> course, Rect rect, double time)
		{
			return new Rect(
				left: course.X.OffsetValue(rect.Left, time),
				right: course.X.OffsetValue(rect.Right, time),
				top: course.X.OffsetValue(rect.Top, time),
				bottom: course.X.OffsetValue(rect.Bottom, time)
			);
		}

		public static Vector2<CourseA> Offset(this Vector2<CourseA> course, double time)
		{
			return Vector2Builder.Create(course.X.Offset(time), course.Y.Offset(time));
		}

		public static Vector2<Mutated<double, CourseA>> Offset(this Vector2<Mutated<double, CourseA>> vector, double time)
			=> Vector2Builder.Create(
				vector.X.Offset(time),
				vector.Y.Offset(time)
			);

		public static Vector2<double> GetV(this Vector2<CourseA> vector)
			=> Vector2Builder.Create(vector.X.V, vector.Y.V);

		public static Vector2<double> GetA(this Vector2<CourseA> vector)
			=> Vector2Builder.Create(vector.X.A, vector.Y.A);

		public static Vector2<TTarget> GetTarget<TTarget, TCourse>(this Vector2<Mutated<TTarget, TCourse>> mutated)
			=> Vector2Builder.Create(mutated.X.Target, mutated.Y.Target);

		public static Vector2<TCourse> GetCource<TTarget, TCourse>(this Vector2<Mutated<TTarget, TCourse>> mutated)
			=> Vector2Builder.Create(mutated.X.Course, mutated.Y.Course);
	}
}
