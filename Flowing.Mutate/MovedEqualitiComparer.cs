using System.Collections.Generic;

namespace Flowing.Mutate
{
	public class MovedEqualitiComparer<TTarget, TCourse> : IEqualityComparer<Moved<TTarget, TCourse>>
	{
		public MovedEqualitiComparer(IEqualityComparer<TTarget> targetComparer, IEqualityComparer<TCourse> courseComparer)
		{
			TargetComparer = targetComparer ?? EqualityComparer<TTarget>.Default;
			CourseComparer = courseComparer ?? EqualityComparer<TCourse>.Default;
		}

		public IEqualityComparer<TTarget> TargetComparer { get; }
		public IEqualityComparer<TCourse> CourseComparer { get; }

		public bool Equals(Moved<TTarget, TCourse> x, Moved<TTarget, TCourse> y)
			=> TargetComparer.Equals(x.Target, y.Target) && CourseComparer.Equals(x.Course, y.Course);

		public int GetHashCode(Moved<TTarget, TCourse> obj)
			=> TargetComparer.GetHashCode(obj.Target) ^ CourseComparer.GetHashCode(obj.Course);
	}
}
