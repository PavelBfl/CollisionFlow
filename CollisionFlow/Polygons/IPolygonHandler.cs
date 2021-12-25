using System.Collections.Generic;

namespace CollisionFlow.Polygons
{
	public interface IPolygonHandler
	{
		IReadOnlyList<Moved<LineFunction, Course>> Edges { get; }
		IReadOnlyList<Moved<Vector128, Course>> Vertices { get; }

		object AttachetData { get; set; }

		Moved<Vector128, Course> GetBeginVertex(int edgeIndex);
		Moved<Vector128, Course> GetEndVertex(int edgeIndex);
	}
}
