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
			for (int i = 0; i < 100; i++)
			{
				var result = CollisionDispatcher.Offset(Test.Polygons, 1);
				if (result is { Offset: < 1 })
				{
					Console.WriteLine($"{i, 2}: Херня");
				}
				else
				{
					Console.WriteLine($"{i, 2}: Нормально");
				}
			}
		}
	}

	public class Test
	{
		public static CollisionPolygon[] Polygons { get; } = GetPolygons().ToArray();

		public static IEnumerable<CollisionPolygon> GetPolygons()
		{
			const int ROWS_COUNT = 10;
			const int COLUMNS_COUNT = 10;

			for (int iRow = 0; iRow < ROWS_COUNT; iRow++)
			{
				for (int iColumn = 0; iColumn < COLUMNS_COUNT; iColumn++)
				{
					yield return new PolygonBuilder(new Vector128(1, 0))
						.Add(new Vector128(iColumn, iRow))
						.OffsetX(0.9)
						.OffsetY(0.9)
						.OffsetX(-0.9)
						.Build();
				}
			}
		}

		[Benchmark]
		public void Common()
		{
			CollisionDispatcher.Offset(Polygons, 1);
		}
	}
}
