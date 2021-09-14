using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow
{
	public class GroupCollisionResult
	{
		public GroupCollisionResult(IEnumerable<CollisionData> results, double offset)
		{
			Results = results?.ToArray() ?? throw new ArgumentNullException(nameof(results));
			Offset = offset;
		}

		public CollisionData[] Results { get; }
		public double Offset { get; }
	}
}
