using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using CollisionFlow.Polygons;

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

			if (polygon.GlobalIndex > 0)
			{
				relations.RemoveAt(polygon.GlobalIndex - 1);
			}
			else if (relations.Count > 0)
			{
				// После отчистки эта строка будет хранить пустой список, поэтому удаляем сразу всю строку
				relations.RemoveAt(0);
			}
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

		private static int counter = -1;
		public GroupCollisionResult Offset(double value)
		{
			counter++;
			var resultMin = new List<(CollisionData Data, Relation Relation)>();
			double? min = null;

			//var pResult = relations.AsParallel()
			//	.WithDegreeOfParallelism(1)
			//	.SelectMany(x => x)
			//	.Select(x => (Data: x.GetResult(value), Relation: x))
			//	.Where(x => x.Data != null && x.Data.Offset < value)
			//	.ToLookup(x => x.Data.Offset, NumberUnitComparer.Instance);

			//if (pResult.Any())
			//{
			//	var pMin = pResult.Min(x => x.Key);
			//	min = pMin;
			//	resultMin.AddRange(pResult[pMin].Select(x => (x.Data.CollisionData, x.Relation)));
			//}

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
							resultMin.Add((cellResult.CollisionData, cell));
						}
						else
						{
							var compare = NumberUnitComparer.Instance.Compare(cellResult.Offset, min.Value);
							if (compare < 0)
							{
								min = cellResult.Offset;
								resultMin.Clear();
								resultMin.Add((cellResult.CollisionData, cell));
							}
							else if (compare == 0)
							{
								resultMin.Add((cellResult.CollisionData, cell));
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
				foreach (var item in resultMin)
				{
					item.Relation.OnHandled();
				}
				return new GroupCollisionResult(resultMin.Select(x => x.Data), min.Value);
			}
		}

		public static bool IsCollision(IEnumerable<Vector128> polygon1, IEnumerable<Vector128> polygon2)
		{
			return CommonPolygon.IsCollision(polygon1, polygon2);
		}
	}
}
