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
			//BenchmarkRunner.Run<Test>();
			var disparcher = Test.Dispatcher100;
			for (int i = 0; i < 200; i++)
			{
				var result = disparcher.Offset(1);

				if (result is { Offset: < 1 })
				{
					Console.WriteLine($"{i,-2} Error: ({result.Offset})");
				}
				else
				{
					Console.WriteLine($"{i,-2} Success");
				}
			}
		}
	}

	public class Test
	{
		public static CollisionDispatcher Dispatcher2 { get; } = GetDispatcher(2, 1);
		public static CollisionDispatcher Dispatcher100 { get; } = GetDispatcher(10, 10);
		public static CollisionDispatcher Dispatcher1000 { get; } = GetDispatcher(10, 100);
		public static Rect[] ControlObjects { get; } = Enumerable.Range(0, 1000)
			.Select(x => new Rect(0, 10, 10, 0))
			.ToArray();

		public static CollisionDispatcher GetDispatcher(int rowsCount, int columnsCount)
		{
			if (rowsCount < 1)
			{
				throw new InvalidOperationException();
			}
			if (columnsCount < 1)
			{
				throw new InvalidOperationException();
			}

			var result = new CollisionDispatcher();
			for (int iRow = 0; iRow < rowsCount; iRow++)
			{
				for (int iColumn = 0; iColumn < columnsCount; iColumn++)
				{
					result.Add(new PolygonBuilder(new Vector128(1, 0))
						.Add(new Vector128(iColumn, iRow))
						.Add(new Vector128(iColumn + 0.5, iRow + 0.5))
						.Add(new Vector128(iColumn, iRow + 0.9))
						.Add(new Vector128(iColumn + 1.1, iRow + 0.5))
						.GetLines());
				}
			}
			return result;
		}

		[Benchmark]
		public int ControlFor()
		{
			var result = 0;

			for (int i = 0; i < ControlObjects.Length; i++)
			{
				for (int j = i + 1; j < ControlObjects.Length; j++)
				{
					result++;
				}
			}

			return result;
		}
		[Benchmark]
		public long ControlBinaryAnd()
		{
			var result = ~0L;

			for (long i = 0; i < ControlObjects.Length; i++)
			{
				for (long j = i + 1; j < ControlObjects.Length; j++)
				{
					result &= i & j;
				}
			}

			return result;
		}
		[Benchmark]
		public bool ControlRectCollision()
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
			Dispatcher2.Offset(1);
		}
		[Benchmark]
		public void Common100()
		{
			Dispatcher100.Offset(1);
		}
		[Benchmark]
		public void Common1000()
		{
			Dispatcher1000.Offset(1);
		}
	}
}
