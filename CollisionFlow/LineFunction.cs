using System;
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
		public static LineFunction AsVerticalUp(double offset) => new LineFunction(double.PositiveInfinity, offset);
		public static LineFunction AsVerticalDown(double offset) => new LineFunction(double.NegativeInfinity, offset);
		public static LineFunction AsHorisontal(double offset) => new LineFunction(0, offset);

		private static LineState GetState(double slope)
		{
			if (double.IsInfinity(slope))
			{
				return LineState.Vectical;
			}
			else if (NumberUnitComparer.Instance.Equals(slope, 0))
			{
				return LineState.Horisontal;
			}
			else
			{
				return LineState.None;
			}
		}

		public LineFunction(Vector128 begin, Vector128 end)
		{
			if (NumberUnitComparer.Instance.Equals(begin.X, end.X) && NumberUnitComparer.Instance.Equals(begin.Y, end.Y))
			{
				throw new InvalidOperationException();
			}
			else if (NumberUnitComparer.Instance.Equals(begin.X, end.X))
			{
				Slope = double.PositiveInfinity;
				Offset = begin.X;
			}
			else if (NumberUnitComparer.Instance.Equals(begin.Y, end.Y))
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
			State = GetState(Slope);
		}
		public LineFunction(double slope, double offset)
		{
			Slope = slope;
			Offset = offset;
			State = GetState(Slope);
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
		public Vector128 GetVector()
		{
			switch (State)
			{
				case LineState.None:
					var begin = Vector128.Create(0, GetY(0));
					var end = Vector128.Create(1, GetY(1));
					return new Vector128(end - begin).ToNormal();
				case LineState.Vectical: return new Vector128(0, 1);
				case LineState.Horisontal: return new Vector128(1, 0);
				default: throw new InvalidCollisiopnException();
			}
		}

		public bool IsVertical() => double.IsInfinity(Slope);
		public bool IsVerticalUp() => double.IsPositiveInfinity(Slope);
		public bool IsVerticalDown() => double.IsNegativeInfinity(Slope);
		public bool IsHorizontal() => Slope == 0;

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

		public LineFunction Perpendicular()
		{
			return new LineFunction(-1 / Slope, Offset);
		}
		public LineFunction OffsetToPoint(Vector128 point)
		{
			if (IsVerticalUp())
			{
				return AsVerticalUp(point.X);
			}
			else if (IsVerticalDown())
			{
				return AsVerticalDown(point.X);
			}
			else if (IsHorizontal())
			{
				return AsHorisontal(point.Y);
			}
			else
			{
				var newY = GetY(point.X);
				var difference = point.Y - newY;
				return new LineFunction(Slope, Offset + difference);
			}
		}
		public LineFunction OffsetByVector(Vector128 vector)
		{
			switch (State)
			{
				case LineState.Vectical: return AsVerticalUp(Offset + vector.X);
				case LineState.Horisontal: return AsHorisontal(Offset + vector.Y);
				case LineState.None: return new LineFunction(Slope, Offset - vector.X * Slope + vector.Y);
				default: throw new InvalidCollisiopnException();
			}
		}
		public Vector128 Crossing(LineFunction lineFunction)
		{
			if (State == lineFunction.State && State != LineState.None)
			{
				throw new InvalidOperationException();
			}

			if (State == LineState.Vectical)
			{
				return new Vector128(Offset, lineFunction.GetY(Offset));
			}
			else if (lineFunction.State == LineState.Vectical)
			{
				return new Vector128(lineFunction.Offset, GetY(lineFunction.Offset));
			}
			else
			{
				var x = (Offset - lineFunction.Offset) / (lineFunction.Slope - Slope);
				return new Vector128(x, GetY(x));
			}
		}
	}
}
