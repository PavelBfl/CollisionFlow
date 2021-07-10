﻿namespace CollisionFlow
{
	public static class Moved
	{
		public static Moved<TTarget, TCourse> Create<TTarget, TCourse>(TTarget target, TCourse course) => new Moved<TTarget, TCourse>(target, course);

		public static Moved<LineFunction, Vector128> Offset(this Moved<LineFunction, Vector128> moved, double value)
			=> moved.SetTarget(moved.Target.OffsetByVector(new Vector128(moved.Course.ToVector() * value)));

		public static Moved<Vector128, Vector128> Offset(this Moved<Vector128, Vector128> moved, double value)
			=> moved.SetTarget(new Vector128(moved.Target.ToVector() + moved.Course.ToVector() * value));

		public static Moved<double, double> Offset(this Moved<double, double> moved, double value)
			=> moved.SetTarget(moved.Target + moved.Course * value);
	}
}
