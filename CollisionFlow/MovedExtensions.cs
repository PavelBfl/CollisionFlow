namespace CollisionFlow
{
	public static class Moved
	{
		public static Moved<TTarget, TCourse> Create<TTarget, TCourse>(TTarget target, TCourse course) => new Moved<TTarget, TCourse>(target, course);

		public static Moved<LineFunction, Vector128> Offset(this Moved<LineFunction, Vector128> moved, double value)
			=> moved.SetTarget(moved.Target.OffsetByVector(new Vector128(moved.Course.ToVector() * value)));
	}
}
