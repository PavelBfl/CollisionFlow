using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BenchmarkTest
{

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Test>();
			//for (int i = 0; i < 200; i++)
			//{
			//	var result = CollisionDispatcher.Offset(Test.Polygons100, 1);

			//	if (result is { Offset: < 1 })
			//	{
			//		Console.WriteLine($"{i,-2} Error: ({result.Offset})");
			//	}
			//	else
			//	{
			//		Console.WriteLine($"{i,-2} Success");
			//	}
			//}
		}
	}

	public class Test
	{
		public static CollisionPolygon[] Polygons2 { get; } = GetPolygons(2, 1).ToArray();
		public static CollisionPolygon[] Polygons100 { get; } = GetPolygons(10, 10).ToArray();
		public static CollisionPolygon[] Polygons1000 { get; } = GetPolygons(10, 100).ToArray();
		public static Rect[] ControlObjects { get; } = Enumerable.Range(0, 1000)
			.Select(x => new Rect(0, 10, 10, 0))
			.ToArray();

		public static IEnumerable<CollisionPolygon> GetPolygons(int rowsCount, int columnsCount)
		{
			if (rowsCount < 1)
			{
				throw new InvalidOperationException();
			}
			if (columnsCount < 1)
			{
				throw new InvalidOperationException();
			}

			for (int iRow = 0; iRow < rowsCount; iRow++)
			{
				for (int iColumn = 0; iColumn < columnsCount; iColumn++)
				{
					yield return new CollisionPolygon(new PolygonBuilder(new Vector128(1, 0))
						.Add(new Vector128(iColumn, iRow))
						.Add(new Vector128(iColumn + 0.5, iRow + 0.5))
						.Add(new Vector128(iColumn, iRow + 0.9))
						.Add(new Vector128(iColumn + 1.1, iRow + 0.5))
						.GetLines());
				}
			}
		}

		[Benchmark]
		public bool Control()
		{
			var result = true;

			for (int i = 0; i < ControlObjects.Length; i++)
			{
				var rect1 = ControlObjects[i];
				for (int j = i + 1; j < ControlObjects.Length; j++)
				{
					var rect2 = ControlObjects[j];
					result = rect1.Intersect(rect2);
				}
			}

			return result;
		}

		[Benchmark]
		public void Common2()
		{
			CollisionDispatcher.Offset(Polygons2, 1);
		}
		[Benchmark]
		public void Common100()
		{
			CollisionDispatcher.Offset(Polygons100, 1);
		}
		[Benchmark]
		public void Common1000()
		{
			CollisionDispatcher.Offset(Polygons1000, 1);
		}
	}
}
