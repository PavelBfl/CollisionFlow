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
	}
}
