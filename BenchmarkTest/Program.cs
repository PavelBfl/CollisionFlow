using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkTest
{

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Test>();
			//for (int i = 0; i < 100; i++)
			//{
			//	CollisionDispatcher.Offset(Test.Polygons100, 1);
			//	Console.WriteLine($"{i, -2} Success");
			//}
		}
	}

	public class Test
	{
		public static CollisionPolygon[] Polygons2 { get; } = GetPolygons(2, 1).ToArray();
		public static CollisionPolygon[] Polygons100 { get; } = GetPolygons(10, 10).ToArray();
		public static CollisionPolygon[] Polygons1000 { get; } = GetPolygons(10, 100).ToArray();

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
						.OffsetX(0.9)
						.OffsetY(0.9)
						.OffsetX(-0.9)
						.GetLines());
				}
			}
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
