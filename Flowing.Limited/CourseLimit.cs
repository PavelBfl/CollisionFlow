using System;
using System.Collections.Generic;
using System.Linq;
using Flowing.Mutate;

namespace Flowing.Limited
{
	public class CourseLimit
	{
		public double V { get; set; }

		public double TotalA => GetEnableLimits().Select(x => x.A).DefaultIfEmpty().Sum();

		public CourseA ToCourseA() => new CourseA(V, TotalA);

		public double GetNextTime()
		{
			return (from limitA in GetEnableLimits()
					let course = new CourseA(V, limitA.A)
					select Math.Min(course.GetTime(limitA.Limit), course.GetTime(-limitA.Limit)))
				   .DefaultIfEmpty(double.PositiveInfinity)
				   .Min();
		}

		public IEnumerable<LimitA> GetEnableLimits()
		{
			var absV = Math.Abs(V);
			return from limitA in LimitsA
				   where limitA.Limit > absV && !double.IsInfinity(limitA.Limit)
				   select limitA;
		}

		private HashSet<LimitA> LimitsAContainer { get; } = new HashSet<LimitA>();

		public ICollection<LimitA> LimitsA => LimitsAContainer;

		public LimitA Add(double a, double limit)
		{
			var item = new LimitA(a, limit);
			LimitsA.Add(item);
			return item;
		}

		public bool Remove(LimitA limit) => LimitsA.Remove(limit);
	}
}
