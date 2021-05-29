using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CollisionFlow.Test
{
	public class PolygonBuilderTest
	{
		public static IEnumerable<object[]> GetDefaultCourses()
		{
			yield return new object[] { new Vector128(Vector<double>.Zero) };
			yield return new object[] { new Vector128(Vector<double>.One) };
			yield return new object[] { new Vector128(1, 0) };
			yield return new object[] { new Vector128(0, 1) };
		}

		[Fact]
		public void Constructor_DefaultCourse_Zero()
		{
			var builder = new PolygonBuilder();
			Assert.Equal(Vector128.Zero, builder.DefaultCourse);
		}

		[Theory]
		[MemberData(nameof(GetDefaultCourses))]
		public void Constructor_SetDefaultCourse_InitCourse(Vector128 course)
		{
			var builder = new PolygonBuilder(course);
			Assert.Equal(course, builder.DefaultCourse);
		}

		[Theory]
		[MemberData(nameof(GetDefaultCourses))]
		public void SetDefaultCourse_SetValue_ExpectedCourse(Vector128 course)
		{
			var builder = new PolygonBuilder()
				.SetDefault(course);
			Assert.Equal(course, builder.DefaultCourse);
		}

		[Theory]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(5)]
		[InlineData(100)]
		public void RegularPolygon_VerticesCount_InitCount(int verticesCount)
		{
			var vertices = PolygonBuilder.RegularPolygon(1, verticesCount);

			Assert.Equal(verticesCount, vertices.Count());
		}

		[Theory]
		[InlineData(2d, 3)]
		[InlineData(2.5d, 4)]
		[InlineData(4.73d, 5)]
		[InlineData(100.001d, 100)]
		public void RegularPolygon_Distance_InitRadius(double radius, int verticesCount)
		{
			var vertices = PolygonBuilder.RegularPolygon(radius, verticesCount);

			foreach (var vertex in vertices)
			{
				var distance = Math.Sqrt(vertex.X * vertex.X + vertex.Y * vertex.Y);
				
				Assert.Equal(radius, distance, NumberUnitComparer.Instance);
			}
		}
	}
}
