using Flowing.Mutate;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CollisionFlow
{
	public struct Course : IEquatable<Course>
	{
		public static Course Zero { get; } = new Course(Vector<double>.Zero, Vector<double>.Zero);

		public Course(Vector<double> v, Vector<double> a)
		{
			V = v;
			A = a;
		}
		public Vector<double> V { get; }
		public Vector<double> A { get; }

		public Course SetV(double x, double y)
		{
			return new Course(Vector128.Create(x, y), A);
		}
		public Course SetA(double x, double y)
		{
			return new Course(V, Vector128.Create(x, y));
		}

		public bool Equals(Course other)
		{
			return UnitComparer.Position.Equals(V.GetX(), other.V.GetX()) &&
				UnitComparer.Position.Equals(V.GetY(), other.V.GetY()) &&
				UnitComparer.Position.Equals(A.GetX(), other.A.GetX()) &&
				UnitComparer.Position.Equals(A.GetY(), other.A.GetY());
		}

		public Course Offset(double time) => new Course(V + A * time, A);

		public Vector<double> Offset(Vector<double> vector, double time) => CourseA.OffsetValue(V, A, vector, time);
		public Rect Offset(Rect rect, double time)
		{
			var leftTop = Offset(Vector128.Create(rect.Left, rect.Top), time);

			return Rect.CreateLeftTop(
				leftTop.GetX(),
				leftTop.GetY(),
				rect.Right - rect.Left,
				rect.Top - rect.Bottom
			);
		}
	}
}
