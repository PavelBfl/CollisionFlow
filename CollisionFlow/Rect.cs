using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollisionFlow
{
	public struct Rect
	{
		public Rect(double left, double right, double top, double bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}
		public Rect(IEnumerable<Vector128> vectors)
		{
			if (vectors is null)
			{
				throw new ArgumentNullException(nameof(vectors));
			}

			Left = vectors.Min(x => x.X);
			Right = vectors.Max(x => x.X);
			Top = vectors.Max(x => x.Y);
			Bottom = vectors.Min(x => x.Y);
		}

		public double Left { get; }
		public double Right { get; }
		public double Top { get; }
		public double Bottom { get; }

		public Rect Union(Rect rect) => new Rect(
			left: Math.Min(Left, rect.Left),
			right: Math.Max(Right, rect.Right),
			top: Math.Max(Top, rect.Top),
			bottom: Math.Min(Bottom, rect.Bottom)
		);
		public Rect Union(Vector128 vector) => new Rect(
			left: Math.Min(Left, vector.X),
			right: Math.Max(Right, vector.X),
			top: Math.Max(Top, vector.Y),
			bottom: Math.Min(Bottom, vector.Y)
		);
		public Range Horisontal => new Range(Left, Right);
		public Range Vertical => new Range(Bottom, Top);

		public bool Contains(Vector128 vector)
		{
			return Horisontal.Contains(vector.X) && Vertical.Contains(vector.Y);
		}
		public bool Contains(Rect rect)
		{
			return Horisontal.Contains(rect.Horisontal) && Vertical.Contains(rect.Vertical);
		}
		public bool Intersect(Rect rect)
		{
			return Horisontal.Intersect(rect.Horisontal) && Vertical.Intersect(rect.Vertical);
		}
	}
}
