using System.Collections.Generic;
using System.Text;
using System;

namespace CollisionFlow
{
	public class CollisionDispatcher
	{
		private readonly List<List<Relation>> relations = new List<List<Relation>>();

		private readonly List<Polygon> polygons = new List<Polygon>();
		public IEnumerable<IPolygonHandler> Polygons => polygons;

		public IPolygonHandler Add(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			var polygon = Polygon.Create(lines);
			polygon.GlobalIndex = polygons.Count;
			polygons.Add(polygon);

			if (polygons.Count > 1)
			{
				var row = new List<Relation>();
				for (var i = 0; i < polygons.Count - 1; i++)
				{
					row.Add(new Relation(polygon, polygons[i]));
				}
				relations.Add(row);
			}
			return polygon;
		}
		public void Remove(IPolygonHandler handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			var polygon = (Polygon)handler;

			relations.RemoveAt(polygon.GlobalIndex > 0 ? polygon.GlobalIndex - 1 : 0);
			foreach (var row in relations)
			{
				if (polygon.GlobalIndex < row.Count)
				{
					row.RemoveAt(polygon.GlobalIndex);
				}
			}

			polygons.RemoveAt(polygon.GlobalIndex);
			for (var i = polygon.GlobalIndex; i < polygons.Count; i++)
			{
				polygons[i].GlobalIndex = i;
			}
		}

		public CollisionResult Offset(double value)
		{
			CollisionResult resultMin = null;
			foreach (var row in relations)
			{
				foreach (var cell in row)
				{
					var cellResult = cell.Result;
					if (resultMin is null || (cellResult != null && cellResult.Offset < resultMin.Offset))
					{
						resultMin = cellResult;
					}
				}
			}

			if (resultMin != null)
			{
				resultMin = resultMin.Clone();
				if (resultMin.Offset > value)
				{
					resultMin.Offset = value;
				}
			}
			var offset = resultMin?.Offset ?? value;
			if (!NumberUnitComparer.Instance.IsZero(offset))
			{
				foreach (var row in relations)
				{
					foreach (var cell in row)
					{
						cell.Result?.Step(offset);
					}
				}
				foreach (var polygon in polygons)
				{
					polygon.Offset(offset);
				}
			}
			return resultMin;
		}

		public static bool IsCollision(IEnumerable<Vector128> polygon1, IEnumerable<Vector128> polygon2)
		{
			return Polygon.IsCollision(polygon1, polygon2);
		}
	}
}
