using Flowing.Mutate;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CollisionFlow
{
	public enum LineState
	{
		None,
		Vectical,
		Horisontal
	}
	public struct LineFunction
	{
		private static IEqualityComparer<double> DefaultComparer { get; } = new NumberUnitComparer(0.00000000001);
		private static IEqualityComparer<double> GetComparer(IEqualityComparer<double> customComparer)
			=> customComparer ?? DefaultComparer;

		public static LineFunction AsVerticalUp(double offset, IEqualityComparer<double> comparer) => new LineFunction(double.PositiveInfinity, offset, comparer);
		public static LineFunction AsVerticalDown(double offset, IEqualityComparer<double> comparer) => new LineFunction(double.NegativeInfinity, offset, comparer);
		public static LineFunction AsHorisontal(double offset, IEqualityComparer<double> comparer) => new LineFunction(0, offset, comparer);

		private static LineState GetState(double slope, IEqualityComparer<double> comparer)
		{
			if (double.IsInfinity(slope))
			{
				return LineState.Vectical;
			}
			else if (comparer.Equals(slope, 0))
			{
				return LineState.Horisontal;
			}
			else
			{
				return LineState.None;
			}
		}

		public LineFunction(Vector2<double> begin, Vector2<double> end, IEqualityComparer<double> comparer = null)
		{
			comparer = GetComparer(comparer);

			if (comparer.Equals(begin.X, end.X) && comparer.Equals(begin.Y, end.Y))
			{
				throw new InvalidOperationException();
			}
			else if (comparer.Equals(begin.X, end.X))
			{
				Slope = double.PositiveInfinity;
				Offset = begin.X;
			}
			else if (comparer.Equals(begin.Y, end.Y))
			{
				Slope = 0;
				Offset = begin.Y;
			}
			else
			{
				var beginCalc = begin.ToVector();
				var endCalc = end.ToVector();
				var difference = endCalc - beginCalc;
				Slope = difference.GetY() / difference.GetX();
				Offset = begin.Y - (begin.X * Slope); 
			}
			State = GetState(Slope, comparer);
		}
		public LineFunction(double slope, double offset, IEqualityComparer<double> comparer = null)
		{
			Slope = slope;
			Offset = offset;
			State = GetState(Slope, GetComparer(comparer));
		}

		public double Slope { get; }
		public double Offset { get; }
		public LineState State { get; }

		public LineState GetOptimalProjection()
		{
			switch (State)
			{
				case LineState.None: return -1 < Slope && Slope < 1 ? LineState.Horisontal : LineState.Vectical;
				case LineState.Vectical:
				case LineState.Horisontal: return State;
				default: throw new InvalidCollisiopnException();
			}
		}
		public Vector2<double> GetVector()
		{
			switch (State)
			{
				case LineState.None:
					var begin = VectorExtensions.Create2d(0, GetY(0));
					var end = VectorExtensions.Create2d(1, GetY(1));
					return (end - begin).ToVector2().ToNormal();
				case LineState.Vectical: return Vector2Builder.Create(0d, 1d);
				case LineState.Horisontal: return Vector2Builder.Create(1d, 0d);
				default: throw new InvalidCollisiopnException();
			}
		}

		public bool IsVertical() => double.IsInfinity(Slope);
		public bool IsVerticalUp() => double.IsPositiveInfinity(Slope);
		public bool IsVerticalDown() => double.IsNegativeInfinity(Slope);
		public bool IsHorizontal() => Slope == 0;

		public bool IsParalel(LineFunction lineFunction, double epsilon = 0.000001)
		{
			if (epsilon < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(epsilon));
			}

			switch (State)
			{
				case LineState.None:
					switch (lineFunction.State)
					{
						case LineState.Horisontal:
						case LineState.None: return Math.Abs(Slope - lineFunction.Slope) < epsilon;
						case LineState.Vectical: return false;
						default: throw new InvalidCollisiopnException();
					}
				case LineState.Vectical:
					switch (lineFunction.State)
					{
						case LineState.None: return false;
						case LineState.Vectical: return true;
						case LineState.Horisontal: return false;
						default: throw new InvalidCollisiopnException();
					}
				case LineState.Horisontal:
					switch (lineFunction.State)
					{
						case LineState.None: return Math.Abs(Slope - lineFunction.Slope) < epsilon;
						case LineState.Vectical: return false;
						case LineState.Horisontal: return true;
						default: throw new InvalidCollisiopnException();
					}
				default: throw new InvalidCollisiopnException();
			}
		}

		public double GetY(double x)
		{
			if (State == LineState.Vectical)
			{
				throw new InvalidOperationException();
			}
			return Slope * x + Offset;
		}
		public double GetX(double y)
		{
			if (State == LineState.Horisontal)
			{
				throw new InvalidOperationException();
			}
			return (y - Offset) / Slope;
		}

		public LineFunction Perpendicular(IEqualityComparer<double> comparer = null)
		{
			return new LineFunction(-1 / Slope, Offset, comparer);
		}
		public LineFunction OffsetToPoint(Vector2<double> point, IEqualityComparer<double> comparer = null)
		{
			if (IsVerticalUp())
			{
				return AsVerticalUp(point.X, comparer);
			}
			else if (IsVerticalDown())
			{
				return AsVerticalDown(point.X, comparer);
			}
			else if (IsHorizontal())
			{
				return AsHorisontal(point.Y, comparer);
			}
			else
			{
				var newY = GetY(point.X);
				var difference = point.Y - newY;
				return new LineFunction(Slope, Offset + difference, comparer);
			}
		}
		public LineFunction OffsetByVector(Vector2<double> vector, IEqualityComparer<double> comparer = null)
		{
			return new LineFunction(Slope, Offset + GetCourseOffset(vector), comparer);
		}
		public double GetCourseOffset(Vector2<double> vector)
		{
			switch (State)
			{
				case LineState.Vectical: return vector.X;
				case LineState.Horisontal: return vector.Y;
				case LineState.None: return -vector.X * Slope + vector.Y;
				default: throw new InvalidCollisiopnException();
			}
		}

		public Vector2<double> Crossing(LineFunction lineFunction)
		{
			if (IsParalel(lineFunction))
			{
				throw new InvalidOperationException();
			}

			if (State == LineState.Vectical)
			{
				return Vector2Builder.Create(Offset, lineFunction.GetY(Offset));
			}
			else if (lineFunction.State == LineState.Vectical)
			{
				return Vector2Builder.Create(lineFunction.Offset, GetY(lineFunction.Offset));
			}
			else
			{
				var x = (Offset - lineFunction.Offset) / (lineFunction.Slope - Slope);
				return Vector2Builder.Create(x, GetY(x));
			}
		}
	}
}
