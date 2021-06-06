using System.Collections.Generic;
using System;
using System.Linq;

namespace CollisionFlow
{
	public class CollisionPolygon
	{
		private static IEnumerable<Moved<LineFunction, Vector128>> GetLines(Vector128 vector, IEnumerable<Vector128> points)
		{
			if (points is null)
			{
				throw new ArgumentNullException(nameof(points));
			}
			var localPoints = points.ToArray();

			for (int i = 0; i < localPoints.Length; i++)
			{
				var prevPoint = localPoints[i == 0 ? localPoints.Length - 1 : i - 1];
				var currentPoint = localPoints[i];

				yield return new Moved<LineFunction, Vector128>(new LineFunction(prevPoint, currentPoint), vector);
			}
		}

		public CollisionPolygon(Vector128 vector, IEnumerable<Vector128> points)
			: this(GetLines(vector, points))
		{
			
		}
		public CollisionPolygon(IEnumerable<Moved<LineFunction, Vector128>> lines)
		{
			this.lines = lines?.ToArray() ?? throw new ArgumentNullException(nameof(lines));
		}

		public IEnumerable<Moved<LineFunction, Vector128>> Lines => lines;
		private Moved<LineFunction, Vector128>[] lines;

		private Moved<Vector128, Vector128>[] points;
		public Moved<Vector128, Vector128>[] Points => points ?? (points = GetPoints().ToArray());
		private IEnumerable<Moved<Vector128, Vector128>> GetPoints()
		{
			for (int i = 0; i < lines.Length; i++)
			{
				var prevLine = lines[i == 0 ? lines.Length - 1 : i - 1];
				var currentLine = lines[i];

				var prevLineOffset = prevLine.Target.OffsetByVector(prevLine.Course);
				var currentLineOffset = currentLine.Target.OffsetByVector(currentLine.Course);

				var currentPoint = prevLine.Target.Crossing(currentLine.Target);
				yield return new Moved<Vector128, Vector128>(
					currentPoint,
					new Vector128(prevLineOffset.Crossing(currentLineOffset).ToVector() - currentPoint.ToVector())
				);
			}
		}

		private Rect? bounds;
		public Rect Bounds => (bounds ?? (bounds = GetBounds())).GetValueOrDefault();

		private Rect GetBounds()
		{
			return new Rect(Points.Select(x => x.Target));
		}

		public void Offset(double value)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Offset(value);
			}
			points = null;
			bounds = null;
		}

		public CollisionPolygon Clone()
		{
			return new CollisionPolygon(Lines);
		}
	}
}
