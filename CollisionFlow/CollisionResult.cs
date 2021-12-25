using CollisionFlow.Polygons;
using System;

namespace CollisionFlow
{
	public class CollisionData
	{
		public CollisionData(IPolygonHandler edgePolygon, int edgeIndex, IPolygonHandler vertexPolygon, int vertexIndex)
		{
			EdgePolygon = edgePolygon ?? throw new ArgumentNullException(nameof(edgePolygon));
			EdgeIndex = edgeIndex;
			VertexPolygon = vertexPolygon ?? throw new ArgumentNullException(nameof(vertexPolygon));
			VertexIndex = vertexIndex;
		}

		public IPolygonHandler EdgePolygon { get; }
		public int EdgeIndex { get; }
		public Moved<LineFunction, Course> Edge => EdgePolygon.Edges[EdgeIndex];
		public IPolygonHandler VertexPolygon { get; }
		public int VertexIndex { get; }
		public Moved<Vector128, Course> Vertex => VertexPolygon.Vertices[VertexIndex];
	}
	public class CollisionResult
	{
		public CollisionResult(IPolygonHandler edgePolygon, int edgeIndex, IPolygonHandler vertexPolygon, int vertexIndex, double offset)
		{
			EdgePolygon = edgePolygon;
			EdgeIndex = edgeIndex;
			VertexPolygon = vertexPolygon;
			VertexIndex = vertexIndex;
			Offset = offset;
		}

		public IPolygonHandler EdgePolygon { get; }
		public int EdgeIndex { get; }
		public Moved<LineFunction, Course> Edge => EdgePolygon.Edges[EdgeIndex];
		public IPolygonHandler VertexPolygon { get; }
		public int VertexIndex { get; }
		public Moved<Vector128, Course> Vertex => VertexPolygon.Vertices[VertexIndex];
		public double Offset { get; set; }
		public bool? IsCollision { get; set; }

		public void Step(double value)
		{
			var compare = NumberUnitComparer.Instance.Compare(Offset, value);
			if (compare < 0)
			{
				throw new InvalidCollisiopnException();
			}
			else if (compare > 0)
			{
				Offset -= value;
			}
			else
			{
				Offset = 0;
			}
		}

		public CollisionResult Clone() => new CollisionResult(EdgePolygon, EdgeIndex, VertexPolygon, VertexIndex, Offset)
		{
			IsCollision = IsCollision,
		};
	}
}
