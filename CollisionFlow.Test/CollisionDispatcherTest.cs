using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using Xunit;

namespace CollisionFlow.Test
{
	public class CollisionDispatcherTest
	{
		public static IEnumerable<object[]> GetPolygonsWithTarget()
		{
			const int POLYGONS_COUNT = 5;
			var polygons = new PolygonBuilder[POLYGONS_COUNT];
			for (int i = 0; i < polygons.Length; i++)
			{
				polygons[i] = PolygonBuilder.CreateRegular(1, 10);
			}
			for (int i = 0; i < polygons.Length; i++)
			{
				yield return new object[] { polygons, i };
			}
		}

		[Fact]
		public void Dispatcher_AddNull_ArgumentNullException()
		{
			var dispatcher = new CollisionDispatcher();
			Assert.Throws<ArgumentNullException>(() => dispatcher.Add(null));
		}

		[Fact]
		public void Dispatcher_Add_PolygonsContains()
		{
			var builder = PolygonBuilder.CreateRegular(1, 10);
			var dispatcher = new CollisionDispatcher();
			var handler = dispatcher.Add(builder.GetLines());

			Assert.Contains(handler, dispatcher.Polygons);
		}

		[Fact]
		public void Dispatcher_Remove_PolygonsContains()
		{
			var builder = PolygonBuilder.CreateRegular(1, 10);
			var dispatcher = new CollisionDispatcher();
			var handler = dispatcher.Add(builder.GetLines());
			dispatcher.Remove(handler);

			Assert.DoesNotContain(handler, dispatcher.Polygons);
		}

		[Theory]
		[MemberData(nameof(GetPolygonsWithTarget))]
		public void Add_AddCollection_ContainsTarget(PolygonBuilder[] polygons, int index)
		{
			var dispatcher = new CollisionDispatcher();
			IPolygonHandler handler = null;
			for (int i = 0; i < polygons.Length; i++)
			{
				if (i == index)
				{
					handler = dispatcher.Add(polygons[i].GetLines());
				}
				else
				{
					dispatcher.Add(polygons[i].GetLines());
				}
			}
			Assert.Contains(handler, dispatcher.Polygons);
		}

		[Theory]
		[MemberData(nameof(GetPolygonsWithTarget))]
		public void Remove_AddCollection_DoesNotContainTarget(PolygonBuilder[] polygons, int index)
		{
			var dispatcher = new CollisionDispatcher();
			IPolygonHandler handler = null;
			for (int i = 0; i < polygons.Length; i++)
			{
				if (i == index)
				{
					handler = dispatcher.Add(polygons[i].GetLines());
				}
				else
				{
					dispatcher.Add(polygons[i].GetLines());
				}
			}
			dispatcher.Remove(handler);

			Assert.DoesNotContain(handler, dispatcher.Polygons);
		}
	}
}
