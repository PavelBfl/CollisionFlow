using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CollisionFlow;
using CollisionFlow.Polygons;
using SolidFlow;
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

			var _bodyDispatcher = new BodyDispatcher();

			const double WEIGHT_MAX = 10;
			const double HEIGHT_MAX = 10;
			const double SPEED_MAX = 0;
			const int ROWS_COUNT = 10;
			const int COLUMNS_COUNT = 10;
			const double GLOBAL_OFFSET = 300;

			var random = new Random(1);
			for (int iRow = 0; iRow < ROWS_COUNT; iRow++)
			{
				for (int iColumn = 0; iColumn < COLUMNS_COUNT; iColumn++)
				{
					var offsetX = GLOBAL_OFFSET + iColumn * WEIGHT_MAX;
					var offsetY = iRow * HEIGHT_MAX;
					var centerX = offsetX + WEIGHT_MAX / 2;
					var centerY = offsetY + HEIGHT_MAX / 2;
					var points = PolygonBuilder.RegularPolygon((Math.Min(WEIGHT_MAX, HEIGHT_MAX) - 1) / 2, random.Next(3, 10))
						.Select(x => new Vector128(x.X + centerX, x.Y + centerY))
						.ToArray();

					var body = new Body(_bodyDispatcher.Dispatcher, points, new Vector128(random.NextDouble() * SPEED_MAX, random.NextDouble() * SPEED_MAX))
					{
						Pull = new Vector128(0, 0.1),
						Bounce = 0.95,
					};
					_bodyDispatcher.Bodies.Add(body);
				}
			}

			var bottom = new Body(_bodyDispatcher.Dispatcher, new Vector128[]
			{
				new Vector128(0, -1000),
				new Vector128(0, 300),
				new Vector128(700, 300),
				new Vector128(700, -1000),
				new Vector128(710, -1000),
				new Vector128(710, 310),
				new Vector128(-10, 310),
				new Vector128(-10, -1000),
			})
			{
				Weight = 1000000000000,
			};
			_bodyDispatcher.Bodies.Add(bottom);

			var bod = new Body(_bodyDispatcher.Dispatcher, new Vector128[]
			{
				new Vector128(350, 200),
				new Vector128(400, 250),
				new Vector128(300, 250),
			})
			{
				Weight = 1000000000000,
			};
			_bodyDispatcher.Bodies.Add(bod);

			for (int i = 0; i < 2000; i++)
			{
				Console.WriteLine(i);
				_bodyDispatcher.Offset(1);
			}
		}
	}

	public class Test
	{
		private static CollisionDispatcher DispatcherLight { get; } = GetDispatcher(2);
		private static CollisionDispatcher DispatcherMedium { get; } = GetDispatcher(100);
		private static CollisionDispatcher DispatcherHeavy { get; } = GetDispatcher(1000);

		private static CollisionDispatcher Dispatcher10 { get; } = GetDispatcher(10);
		private static CollisionDispatcher Dispatcher50 { get; } = GetDispatcher(50);

		public static CollisionDispatcher GetDispatcher(int count)
		{
			if (count < 1)
			{
				throw new InvalidOperationException();
			}

			var result = new CollisionDispatcher();

			var left = PolygonBuilder.CreateRect(new Rect(-1, 0, count, 0));
			var right = PolygonBuilder.CreateRect(new Rect(2, 3, count, 0));

			result.Add(left.GetLines());
			result.Add(right.GetLines());

			for (int i = 0; i < count; i++)
			{
				var polygon = PolygonBuilder.CreateRegular(0.4, 10, 0, new Vector128(1, i + 0.5))
					.SetAllCourse(new Vector128(10, 0));
				result.Add(polygon.GetLines());
			}
			return result;
		}

		[Benchmark]
		public void Test10()
		{
			Offset(Dispatcher10);
		}
		[Benchmark]
		public void Test50()
		{
			Offset(Dispatcher50);
		}
		[Benchmark]
		public void Light()
		{
			Offset(DispatcherLight);
		}
		[Benchmark]
		public void Medium()
		{
			Offset(DispatcherMedium);
		}
		[Benchmark]
		public void Heavy()
		{
			Offset(DispatcherHeavy);
		}

		private static void Offset(CollisionDispatcher collisionDispatcher)
		{
			collisionDispatcher.Offset(1);
			foreach (var polygon in collisionDispatcher.Polygons.ToArray())
			{
				var builder = new PolygonBuilder(
					polygon.Vertices.Select(x => x.Target),
					new Vector128(-polygon.Vertices.First().Target.X, 0)
				);
				collisionDispatcher.Remove(polygon);
				collisionDispatcher.Add(builder.GetLines());
			}
		}
	}
}
