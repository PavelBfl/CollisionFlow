using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CollisionFlow;
using CollisionFlow.Polygons;
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
