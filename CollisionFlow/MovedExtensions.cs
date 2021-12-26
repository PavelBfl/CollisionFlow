using Flowing.Mutate;

namespace CollisionFlow
{
	public static class Moved
	{
		public static Mutated<TTarget, TCourse> Create<TTarget, TCourse>(TTarget target, TCourse course) => new Mutated<TTarget, TCourse>(target, course);

		public static Mutated<LineFunction, Vector128> Offset(this Mutated<LineFunction, Vector128> moved, double value)
			=> moved.SetTarget(moved.Target.OffsetByVector(new Vector128(moved.Course.ToVector() * value)));

		public static Mutated<Vector128, Vector128> Offset(this Mutated<Vector128, Vector128> moved, double value)
			=> moved.SetTarget(new Vector128(moved.Target.ToVector() + moved.Course.ToVector() * value));

		public static Mutated<double, double> Offset(this Mutated<double, double> moved, double value)
			=> moved.SetTarget(moved.Target + moved.Course * value);

		public static Mutated<Vector128, Course> Offset(this Mutated<Vector128, Course> moved, double time)
			=> Moved.Create(
				moved.Course.Offset(moved.Target.ToVector(), time).ToVector128(),
				moved.Course.Offset(time)
			);

		public static Rect Offset(this Rect rect, Vector128 course) => new Rect(
			left: rect.Left + course.X,
			right: rect.Right + course.X,
			top: rect.Top + course.Y,
			bottom: rect.Bottom + course.Y
		);
	}
}
