using System;
using Flowing.Mutate;

namespace Flowing.Limited
{
	public static class LimitedExtensions
	{
		public static double GetNextTime(this Vector2<CourseLimit> course)
			=> Math.Min(course.X.GetNextTime(), course.Y.GetNextTime());

		public static Vector2<CourseA> ToCourseA(this Vector2<CourseLimit> course) => new Vector2<CourseA>(
			course.X.ToCourseA(),
			course.Y.ToCourseA()
		);
	}
}
