using Flowing.Mutate;
using System.Collections.Generic;

namespace CollisionFlow.Polygons
{
	public interface IPolygonHandler
	{
		IReadOnlyList<Mutated<LineFunction, Course>> Edges { get; }
		IReadOnlyList<Mutated<Vector128, Course>> Vertices { get; }

		object AttachetData { get; set; }

		Mutated<Vector128, Course> GetBeginVertex(int edgeIndex);
		Mutated<Vector128, Course> GetEndVertex(int edgeIndex);
	}
}
