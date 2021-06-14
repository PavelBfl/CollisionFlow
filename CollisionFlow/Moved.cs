namespace CollisionFlow
{
	public struct Moved<TTarget, TCourse>
	{
		public Moved(TTarget target, TCourse course)
		{
			Target = target;
			Course = course;
		}

		public TTarget Target { get; }
		public TCourse Course { get; }

		public Moved<TTarget, TCourse> SetTarget(TTarget target) => new Moved<TTarget, TCourse>(target, Course);
		public Moved<TTarget, TCourse> SetCourse(TCourse course) => new Moved<TTarget, TCourse>(Target, course);
	}
}
