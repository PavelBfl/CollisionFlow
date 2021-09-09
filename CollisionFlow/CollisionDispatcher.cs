using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using CollisionFlow.Polygons;

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
	public class CollisionDispatcher
	{
		private readonly List<List<Relation>> relations = new List<List<Relation>>();

		private readonly List<Polygon> polygons = new List<Polygon>();
		public IEnumerable<IPolygonHandler> Polygons => polygons;

		public IPolygonHandler Add(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			var polygon = new Polygon(lines);
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

		public GroupCollisionResult Offset(double value)
		{
			var resultMin = new List<CollisionData>();
			double? min = null;
			foreach (var row in relations)
			{
				foreach (var cell in row)
				{
					var cellResult = cell.GetResult(value);
					if (cellResult != null && cellResult.Offset < value)
					{
						if (min is null)
						{
							min = cellResult.Offset;
							resultMin.Add(cellResult.CollisionData);
						}
						else
						{
							var compare = NumberUnitComparer.Instance.Compare(cellResult.Offset, min.Value);
							if (compare < 0)
							{
								min = cellResult.Offset;
								resultMin.Clear();
								resultMin.Add(cellResult.CollisionData);
							}
							else if (compare == 0)
							{
								resultMin.Add(cellResult.CollisionData);
							}
						}
					}
				}
			}

			var offset = min ?? value;
			if (!NumberUnitComparer.Instance.IsZero(offset))
			{
				foreach (var row in relations)
				{
					foreach (var cell in row)
					{
						cell.Step(offset);
					}
				}
				foreach (var polygon in polygons)
				{
					polygon.Offset(offset);
				}
			}

			if (min is null)
			{
				return null;
			}
			else
			{
				return new GroupCollisionResult(resultMin, min.Value);
			}
		}

		public static bool IsCollision(IEnumerable<Vector128> polygon1, IEnumerable<Vector128> polygon2)
		{
			return Polygon.IsCollision(polygon1, polygon2);
		}
	}
}
