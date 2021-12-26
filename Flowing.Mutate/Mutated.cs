namespace Flowing.Mutate
{
	public struct Mutated<TTarget, TCourse>
	{
		public Mutated(TTarget target, TCourse course)
		{
			Target = target;
			Course = course;
		}

		public TTarget Target { get; }
		public TCourse Course { get; }

		public Mutated<TNewTarget, TCourse> SetTarget<TNewTarget>(TNewTarget target) => new Mutated<TNewTarget, TCourse>(target, Course);
		public Mutated<TTarget, TNewCourse> SetCourse<TNewCourse>(TNewCourse course) => new Mutated<TTarget, TNewCourse>(Target, course);
	}
}
