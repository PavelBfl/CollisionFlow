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
		public static LineFunction AsVertical(double offset) => new LineFunction(double.PositiveInfinity, offset);
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

		public bool IsVertical() => double.IsInfinity(Slope);
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
			if (IsHorizontal())
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
			if (IsVertical())
			{
				return AsVertical(point.X);
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
			if (IsVertical())
			{
				return AsVertical(Offset + vector.X);
			}
			else if (IsHorizontal())
			{
				return AsHorisontal(Offset + vector.Y);
			}
			else
			{
				return OffsetToPoint(new Vector128(Vector128.Create(0, GetY(0)) + vector.ToVector())); 
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
