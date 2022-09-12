using Flowing.Mutate;

namespace CollisionFlow
{
	public static class Moved
	{
		public static Mutated<TTarget, TCourse> Create<TTarget, TCourse>(TTarget target, TCourse course)
			=> new Mutated<TTarget, TCourse>(target, course);

		public static Mutated<double, CourseA> Offset(this Mutated<double, CourseA> mutated, double time)
			=> Create(
				mutated.Course.OffsetValue(mutated.Target, time),
				mutated.Course.Offset(time)
			);
	}
}
