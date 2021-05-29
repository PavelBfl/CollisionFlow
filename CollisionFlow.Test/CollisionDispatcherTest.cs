using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CollisionFlow.Test
{
	public class CollisionDispatcherTest
	{
		struct PolygonData
		{
			public PolygonData(IEnumerable<Vector2> points, Vector2 vector)
			{
				Points = points?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(points));
				Vectors = Enumerable.Repeat(vector, Points.Length).ToImmutableArray();
			}
			public PolygonData(IEnumerable<Vector2> points, IEnumerable<Vector2> vectors)
			{
				Points = points?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(points));
				Vectors = vectors?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(vectors));
				if (Points.Length != Vectors.Length)
				{
					throw new InvalidOperationException();
				}
			}

			public ImmutableArray<Vector2> Points { get; }
			public ImmutableArray<Vector2> Vectors { get; }

			public PolygonData Transform(Matrix3x2 transformMatrix)
			{
				return new PolygonData(
					points: Points.Select(x => Vector2.Transform(x, transformMatrix)),
					vectors: Vectors.Select(x => Vector2.Transform(x, transformMatrix))
				);
			}

			private static Vector128 ToVector128(Vector2 vector) => new Vector128(vector.X, vector.Y);
			public CollisionPolygon ToCollisionPolygon()
			{
				var lines = new List<Moved<LineFunction, Vector128>>();
				for (int i = 0; i < Points.Length; i++)
				{
					var currentPoint = Points[i];
					var nextPoint = Points[(i + 1) < Points.Length ? i + 1 : 0];
					var vector = Vectors[i];

					lines.Add(Moved.Create(
						new LineFunction(ToVector128(currentPoint), ToVector128(nextPoint)),
						ToVector128(vector)
					));
				}
				return new CollisionPolygon(lines);
			}
		}
		struct CollisionData
		{
			public CollisionData(IEnumerable<PolygonData> polygons, double offset, double expectedOffset)
			{
				Polygons = polygons?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(polygons));
				Offset = offset;
				ExpectedOffset = expectedOffset;
			}

			public IEnumerable<PolygonData> Polygons { get; }
			public double Offset { get; }
			public double ExpectedOffset { get; }

			public CollisionData Transform(Matrix3x2 transformMatrix)
			{
				return new CollisionData(
					polygons: Polygons.Select(x => x.Transform(transformMatrix)),
					offset: Offset,
					expectedOffset: ExpectedOffset
				);
			}
		}
		private static IEnumerable<CollisionData> GetCollisionsData()
		{
			yield return new CollisionData(
				polygons: new[]
				{
					new PolygonData(
						new Vector2[] { new(0, 0), new(-1, 0), new(-1, 1), new(0, 1) },
						new Vector2(2, 0)
					),
					new PolygonData(
						new Vector2[] { new(2, 0), new(1, 0), new(1, 1), new(2, 1) },
						Vector2.Zero
					),
				},
				offset: 1,
				expectedOffset: 0.5
			);
			yield return new CollisionData(
				polygons: new[]
				{
					new PolygonData(
						new Vector2[] { new(0, 0), new(0, 2), new(1, 1) },
						new Vector2(4, 0)
					),
					new PolygonData(
						new Vector2[] { new(2, 0), new(2, 2), new(3, 2), new(3, 0) },
						Vector2.Zero
					),
				},
				offset: 1,
				expectedOffset: 0.25
			);
			yield return new CollisionData(
				polygons: new[]
				{
					new PolygonData(
						new Vector2[] { new(0, 0), new(0, 2), new(1, 1) },
						new Vector2(4, 0)
					),
					new PolygonData(
						new Vector2[] { new(2, 0), new(2, 2), new(3, 2), new(3, 0) },
						new Vector2[] { new (4, 0), Vector2.Zero, Vector2.Zero, Vector2.Zero }
					),
				},
				offset: 1,
				expectedOffset: 0.5
			);
			yield return new CollisionData(
				polygons: new[]
				{
					new PolygonData(
						new Vector2[] { new(0, 0), new(-1, 0), new(-1, 1), new(0, 1) },
						new Vector2(1, 0)
					),
					new PolygonData(
						new Vector2[] { new(1, 2), new(1, -1), new(0, -1), new(0, -2), new(2, -2), new(2, 3), new(0, 3), new(0, 2) },
						new Vector2[] { new(-1, 0), Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero }
					),
				},
				offset: 1,
				expectedOffset: 0.5
			);
		}

		public static IEnumerable<object[]> GetPolygons()
		{
			foreach (var collisionData in GetCollisionsData())
			{
				foreach (var matrix in GetTransforms())
				{
					var transformData = collisionData.Transform(matrix);

					yield return new object[]
					{
						transformData.Polygons.Select(x => x.ToCollisionPolygon()),
						transformData.Offset,
						transformData.ExpectedOffset,
					};
				}
			}
		}

		private struct CollisionStaticData
		{
			public CollisionStaticData(IEnumerable<Vector2> points1, IEnumerable<Vector2> points2, bool isCollision)
			{
				Points1 = points1 ?? throw new ArgumentNullException(nameof(points1));
				Points2 = points2 ?? throw new ArgumentNullException(nameof(points2));
				IsCollision = isCollision;
			}

			public IEnumerable<Vector2> Points1 { get; }
			public IEnumerable<Vector2> Points2 { get; }
			public bool IsCollision { get; }

			public CollisionStaticData Transform(Matrix3x2 matrix)
			{
				return new CollisionStaticData(
					points1: Points1.Select(x => Vector2.Transform(x, matrix)),
					points2: Points2.Select(x => Vector2.Transform(x, matrix)),
					IsCollision
				);
			}
		}
		private static IEnumerable<CollisionStaticData> GetStaticData()
		{
			yield return new CollisionStaticData
			(
				points1: new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) },
				points2: new Vector2[] { new(2, 0), new(2, 1), new(3, 1), new(3, 0) },
				isCollision: false
			);
			yield return new CollisionStaticData
			(
				points1: new Vector2[] { new(0, 0), new(0, 1), new(2, 1), new(2, 0) },
				points2: new Vector2[] { new(1, 0), new(1, 1), new(3, 1), new(3, 0) },
				isCollision: true
			);
		}

		public static IEnumerable<object[]> GetStaticPolygons()
		{
			foreach (var data in GetStaticData())
			{
				foreach (var matrix in GetTransforms())
				{
					var result = data.Transform(matrix);
					yield return new object[] { result.Points1.Select(x => new Vector128(x.X, x.Y)), result.Points2.Select(x => new Vector128(x.X, x.Y)), result.IsCollision };
				}
			}
		}

		private static IEnumerable<Matrix3x2> GetTransforms()
		{
			const float SMALL_VALUE = 0.1f;
			const float LARGE_VALUE = 100f;
			var rotations = new[]
			{
				Matrix3x2.CreateRotation(0),
				Matrix3x2.CreateRotation(SMALL_VALUE),
				Matrix3x2.CreateRotation(SMALL_VALUE, new Vector2(1)),
				Matrix3x2.CreateRotation(SMALL_VALUE, new Vector2(5, 10)),
				Matrix3x2.CreateRotation(MathF.PI / 2),
				Matrix3x2.CreateRotation(MathF.PI / 4),
			};
			var scales = new[]
			{
				Matrix3x2.CreateScale(1),
				Matrix3x2.CreateScale(SMALL_VALUE),
				Matrix3x2.CreateScale(LARGE_VALUE),
				Matrix3x2.CreateScale(1, 2),
				Matrix3x2.CreateScale(2, 1),
				Matrix3x2.CreateScale(2),
				Matrix3x2.CreateScale(2, 3, new Vector2(4, 5)),
			};
			var translates = new[]
			{
				Matrix3x2.CreateTranslation(0, 0),
				Matrix3x2.CreateTranslation(SMALL_VALUE, SMALL_VALUE),
				Matrix3x2.CreateTranslation(1, 1),
				Matrix3x2.CreateTranslation(1, 2),
				Matrix3x2.CreateTranslation(2, 1),
				Matrix3x2.CreateTranslation(LARGE_VALUE, 0),
			};

			return from rotate in rotations
				   from scale in scales
				   from translate in translates
				   select rotate * scale * translate;
		}

		[Theory]
		[MemberData(nameof(GetPolygons))]
		public void Offset_ResultOffset_Expected(IEnumerable<CollisionPolygon> polygons, double offset, double expectedOffset)
		{
			var result = CollisionDispatcher.Offset(polygons, offset);

			Assert.Equal(expectedOffset, result.Offset, NumberUnitComparer.Instance);
		}

		[Theory]
		[MemberData(nameof(GetStaticPolygons))]
		public void IsCollision_Collision_Expected(IEnumerable<Vector128> points1, IEnumerable<Vector128> points2, bool expected)
		{
			var result = CollisionDispatcher.IsCollision(points1, points2);

			Assert.Equal(expected, result);
		}
	}
}
