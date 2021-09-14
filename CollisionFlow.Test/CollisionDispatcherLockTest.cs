using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CollisionFlow.Test
{
	public class CollisionDispatcherLockTest
	{
		private const double LEFT_WALL = 1;
		public static IEnumerable<object[]> GetPolygons()
		{
			var closeWall = LEFT_WALL - NumberUnitComparer.Instance.Epsilon;
			yield return new[] { PolygonBuilder.CreateRect(new Rect(0, closeWall, 1, 0), new Vector128(1, 0)) };
			yield return new[]
			{
				new PolygonBuilder(
					new Vector128[]
					{
						new(0, -1),
						new(0, 1),
						new(closeWall, 0),
					},
					new Vector128(1, 0)
				)
			};
		}
		private PolygonBuilder Wall { get; } = PolygonBuilder.CreateRect(new Rect(LEFT_WALL, 2, 100, -100));
		

		[Theory]
		[MemberData(nameof(GetPolygons))]
		public void Dispatcher_Offset_WithoutOffset(PolygonBuilder polygon)
		{
			var dispatcher = new CollisionDispatcher();
			dispatcher.Add(polygon.GetLines());
			dispatcher.Add(Wall.GetLines());

			var result = dispatcher.Offset(1);
			Assert.Equal(0, result.Offset, 4);
		}

		[Theory]
		[MemberData(nameof(GetPolygons))]
		public void Dispatcher_Offset_ResultsCount(PolygonBuilder polygon)
		{
			var dispatcher = new CollisionDispatcher();
			dispatcher.Add(polygon.GetLines());
			dispatcher.Add(Wall.GetLines());

			var result = dispatcher.Offset(1);
			Assert.Single(result.Results);
		}
	}
}
