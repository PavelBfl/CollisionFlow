using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CollisionFlow.Test
{
	public class CollisionDispatcherTest
	{
		public static IEnumerable<object[]> GetPolygons()
		{
			var polygon1 = new[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1) };
			var polygon2 = new[] { new Vector2(2, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(2, 1) };

			for (float i = 0; i < 180; i += 5.123f)
			{
				var transformMatrix = Matrix3x2.CreateRotation(i);
				var p1 = polygon1.Select(x => Vector2.Transform(x, transformMatrix)).Select(x => new Vector128(x.X, x.Y));
				var p2 = polygon2.Select(x => Vector2.Transform(x, transformMatrix)).Select(x => new Vector128(x.X, x.Y));
				var pv1 = Vector2.Transform(new Vector2(2, 0), transformMatrix);

				yield return new object[]
				{
					new []
					{
						new CollisionPolygon(new Vector128(pv1.X, pv1.Y), p1),
						new CollisionPolygon(new Vector128(0, 0), p2),
					},
					1,
					0.5
				};
			}
		}

		[Theory]
		[MemberData(nameof(GetPolygons))]
		public void Offset_ResultOffset_Expected(IEnumerable<CollisionPolygon> polygons, double offset, double expectedOffset)
		{
			var result = CollisionDispatcher.Offset(polygons, offset);

			Assert.Equal(expectedOffset, result.Offset, 5);
		}
	}
}
