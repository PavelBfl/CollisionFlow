using System.Collections.Generic;

namespace CollisionFlow
{
	public interface IPolygonHandler
	{
		IReadOnlyList<Moved<LineFunction, Vector128>> Edges { get; }
		IReadOnlyList<Moved<Vector128, Vector128>> Vertices { get; }

		object AttachetData { get; set; }

		Moved<Vector128, Vector128> GetBeginVertex(int edgeIndex);
		Moved<Vector128, Vector128> GetEndVertex(int edgeIndex);
	}
}
