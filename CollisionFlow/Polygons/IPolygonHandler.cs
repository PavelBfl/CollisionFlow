using System.Collections.Generic;

namespace CollisionFlow
{
	public interface IPolygonHandler
	{
		IList<Moved<LineFunction, Vector128>> Edges { get; }
		IReadOnlyList<Moved<Vector128, Vector128>> Vertices { get; }
	}
}
