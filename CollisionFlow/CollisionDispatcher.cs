using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using CollisionFlow.Polygons;
using Flowing.Mutate;

namespace CollisionFlow
{
	public class CollisionDispatcher
	{
		private readonly List<List<Relation>> relations = new List<List<Relation>>();

		private readonly List<Polygon> polygons = new List<Polygon>();
		public IEnumerable<IPolygonHandler> Polygons => polygons;

		public IPolygonHandler Add(IEnumerable<Mutated<LineFunction, Course>> lines)
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

		protected virtual double Planning(double offset) => offset;

		public GroupCollisionResult Offset(double value)
		{
			var resultMin = new List<(CollisionData Data, Relation Relation)>();
			double? min = null;

			var pResult = relations
				//.AsParallel()
				.SelectMany(x => x)
				.Select(x => (Data: x.GetResult(value), Relation: x))
				.Where(x => x.Data != null && x.Data.Offset < value)
				.OrderBy(x => x.Data.Offset);

			foreach (var item in pResult)
			{
				if (min is null)
				{
					min = item.Data.Offset;
				}
				else if (!UnitComparer.Time.Equals(min.Value, item.Data.Offset))
				{
					break;
				}
				resultMin.Add((item.Data.CollisionData, item.Relation));
			}

			var offset = min ?? value;

			var offsetPlanning = Planning(offset);
			if (offsetPlanning < 0 || offset < offsetPlanning)
			{
				throw new InvalidOperationException();
			}
			
			if (!UnitComparer.Time.IsZero(offsetPlanning))
			{
				foreach (var row in relations)
				{
					foreach (var cell in row)
					{
						cell.Step(offsetPlanning);
					}
				}
				foreach (var polygon in polygons)
				{
					polygon.Offset(offsetPlanning);
				}
			}

			if (min is null)
			{
				return null;
			}
			else if (offsetPlanning < offset)
			{
				return new GroupCollisionResult(Enumerable.Empty<CollisionData>(), offsetPlanning);
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
