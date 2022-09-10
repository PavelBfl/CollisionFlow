using System;
using Flowing.Mutate;

namespace CollisionFlow
{
	public static class Vector2Extensions
	{
		public static Vector2<double> GetV(this Vector2<CourseA> vector)
			=> Vector2Builder.Create(vector.X.V, vector.Y.V);

		public static Vector2<double> GetA(this Vector2<CourseA> vector)
			=> Vector2Builder.Create(vector.X.A, vector.Y.A);
	}
}
