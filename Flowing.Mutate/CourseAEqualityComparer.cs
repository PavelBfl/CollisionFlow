using System.Collections.Generic;

namespace Flowing.Mutate
{
	public class CourseAEqualityComparer : IEqualityComparer<CourseA>
	{
		public CourseAEqualityComparer(IEqualityComparer<double> unitComparer = null)
		{
			UnitComparer = unitComparer ?? EqualityComparer<double>.Default;
		}

		public IEqualityComparer<double> UnitComparer { get; }
		public bool Equals(CourseA x, CourseA y) => UnitComparer.Equals(x.V, y.V) && UnitComparer.Equals(x.A, y.A);
		public int GetHashCode(CourseA obj) => UnitComparer.GetHashCode(obj.V) ^ UnitComparer.GetHashCode(obj.A);
	}
}
