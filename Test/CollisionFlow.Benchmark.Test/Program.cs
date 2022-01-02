using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CollisionFlow.Benchmark.Test
{
	internal class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<MyClass>();
			//var collision = MyClass.CreateDispatcher(1000, 10);
			//Console.WriteLine("Create");
			//collision.Offset(1);
		}
	}

	public class MyClass
	{
		public static CollisionDispatcher CreateDispatcher(int rowsCount, int verticesCount)
		{
			var result = new CollisionDispatcher();
			for (int i = 0; i < rowsCount; i++)
			{
				var rowPosition = i * 5;

				var left = PolygonBuilder.CreateRegular(
					1,
					verticesCount,
					center: new Course(Vector128.Create(10, 0), Vector128.Create(1, 0))
				).OffsetAll(new Vector128(0, rowPosition));
				var right = PolygonBuilder.CreateRegular(
					1,
					verticesCount,
					center: new Course(Vector128.Create(-10, 0), Vector128.Create(-1, 0))
				).OffsetAll(new Vector128(5, rowPosition));

				result.Add(left.GetLines());
				result.Add(right.GetLines());
			}
			return result;
		}
		public static IEnumerable<CollisionDispatcher> GetDispatchers()
		{
			yield return CreateDispatcher(1, 3);
			yield return CreateDispatcher(10, 3);
			yield return CreateDispatcher(100, 3);
			yield return CreateDispatcher(100, 5);
		}

		[Benchmark]
		[ArgumentsSource(nameof(GetDispatchers))]
		public void Native(CollisionDispatcher dispatcher)
		{
			dispatcher.Offset(1);
		}
	}
}
