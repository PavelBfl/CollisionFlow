using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CollisionFlow.Test
{
	public class CollisionDispatcherFreeTest
	{
		public static IEnumerable<object[]> GetOffsets() => new[]
		{
			new object[] { 0d },
			new object[] { 1d },
			new object[] { 1000000d },
		};

		[Theory]
		[MemberData(nameof(GetOffsets))]
		public void Offset_Empty_WithoutResult(double offset)
		{
			var dispatcher = new CollisionDispatcher();
			var result = dispatcher.Offset(offset);
			Assert.Null(result);
		}

		[Theory]
		[MemberData(nameof(GetOffsets))]
		public void Offset_Single_WithoutResult(double offset)
		{
			var builder = PolygonBuilder.CreateRect(new Rect(0, 1, 1, 0), new Vector128(1, 1));
			var dispatcher = new CollisionDispatcher();
			dispatcher.Add(builder.GetLines());

			var result = dispatcher.Offset(offset);
			Assert.Null(result);
		}

		[Theory]
		[MemberData(nameof(GetOffsets))]
		public void Offset_AllStatic_WithoutResult(double offset)
		{
			const double POLYGON_SIZE = 10;
			const double POLYGON_MARGIN = 1;
			const int TABLE_SIZE = 10;

			var dispatcher = new CollisionDispatcher();
			for (int iRow = 0; iRow < TABLE_SIZE; iRow++)
			{
				for (int iColumn = 0; iColumn < TABLE_SIZE; iColumn++)
				{
					var builder = PolygonBuilder.CreateRect(Rect.CreateLeftTop(
						iColumn * POLYGON_SIZE, iRow * POLYGON_SIZE,
						POLYGON_SIZE - POLYGON_MARGIN, POLYGON_SIZE - POLYGON_MARGIN
					));
					dispatcher.Add(builder.GetLines());
				}
			}

			var result = dispatcher.Offset(offset);
			Assert.Null(result);
		}

		[Theory]
		[MemberData(nameof(GetOffsets))]
		public void Offset_SameCourse_WithoutResult(double offset)
		{
			const double POLYGON_SIZE = 10;
			const double POLYGON_MARGIN = 1;
			const int TABLE_SIZE = 10;

			var dispatcher = new CollisionDispatcher();
			for (int iRow = 0; iRow < TABLE_SIZE; iRow++)
			{
				for (int iColumn = 0; iColumn < TABLE_SIZE; iColumn++)
				{
					var builder = PolygonBuilder.CreateRect(Rect.CreateLeftTop(
						iColumn * POLYGON_SIZE, iRow * POLYGON_SIZE,
						POLYGON_SIZE - POLYGON_MARGIN, POLYGON_SIZE - POLYGON_MARGIN
					), new Vector128(1, 1));
					dispatcher.Add(builder.GetLines());
				}
			}

			var result = dispatcher.Offset(offset);
			Assert.Null(result);
		}
	}
}
