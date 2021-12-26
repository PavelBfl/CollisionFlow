using System.Collections.Generic;

namespace Flowing.Mutate
{
	public class MutatedEqualitiComparer<TTarget, TCourse> : IEqualityComparer<Mutated<TTarget, TCourse>>
	{
		public MutatedEqualitiComparer(IEqualityComparer<TTarget> targetComparer, IEqualityComparer<TCourse> courseComparer)
		{
			TargetComparer = targetComparer ?? EqualityComparer<TTarget>.Default;
			CourseComparer = courseComparer ?? EqualityComparer<TCourse>.Default;
		}

		public IEqualityComparer<TTarget> TargetComparer { get; }
		public IEqualityComparer<TCourse> CourseComparer { get; }

		public bool Equals(Mutated<TTarget, TCourse> x, Mutated<TTarget, TCourse> y)
			=> TargetComparer.Equals(x.Target, y.Target) && CourseComparer.Equals(x.Course, y.Course);

		public int GetHashCode(Mutated<TTarget, TCourse> obj)
			=> TargetComparer.GetHashCode(obj.Target) ^ CourseComparer.GetHashCode(obj.Course);
	}
}
