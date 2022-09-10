using Flowing.Mutate;
using System.Collections.Generic;

namespace CollisionFlow.Polygons
{
	public interface IPolygonHandler
	{
		IReadOnlyList<Mutated<LineFunction, Vector2<CourseA>>> Edges { get; }
		IReadOnlyList<Vector2<Mutated<double, CourseA>>> Vertices { get; }

		object AttachetData { get; set; }

		Vector2<Mutated<double, CourseA>> GetBeginVertex(int edgeIndex);
		Vector2<Mutated<double, CourseA>> GetEndVertex(int edgeIndex);
	}
}
