using CollisionFlow;
using System;
using System.Collections.Generic;

namespace SolidFlow
{
	[Obsolete]
	public class SpeedAccumulator
	{
		public HashSet<ISpeedHandler> Speeds { get; } = new HashSet<ISpeedHandler>();

		public ISpeedHandler Add(double x, double y) => Add(x, y, double.PositiveInfinity, double.PositiveInfinity);
		public ISpeedHandler Add(double x, double y, double xLimit, double yLimit)
		{
			var pull = new Pull(this, new Vector128(x, y), new Vector128(xLimit, yLimit));
			Speeds.Add(pull);
			RefreshVector();
			return pull;
		}
		public void Remove(ISpeedHandler pull)
		{
			Speeds.Remove(pull);
			RefreshVector();
		}

		public Vector128 GetVector(Vector128 currentSpeed)
		{
			var x = 0d;
			var y = 0d;
			foreach (var speed in Speeds)
			{
				if (speed.Limit.X > Math.Abs(currentSpeed.X))
				{
					x += speed.Vector.X;
				}
				if (speed.Limit.Y > Math.Abs(currentSpeed.Y))
				{
					y += speed.Vector.Y;
				}
			}
			return new Vector128(x, y);
		}

		public event EventHandler VectorChanged;

		private void RefreshVector()
		{
			VectorChanged?.Invoke(this, EventArgs.Empty);
		}

		private class Pull : ISpeedHandler
		{
			public Pull(SpeedAccumulator owner, Vector128 vector, Vector128 limit)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
				Vector = vector;
				Limit = limit;
			}

			public SpeedAccumulator Owner { get; }
			public Vector128 Vector
			{
				get => vector;
				set
				{
					if (!Vector.Equals(value))
					{
						vector = value;
						Owner.RefreshVector();
					}
				}
			}
			private Vector128 vector;

			public Vector128 Limit
			{
				get => limit;
				set
				{
					if (!limit.Equals(value))
					{
						limit = value;
						Owner.RefreshVector();
					}
				}
			}
			private Vector128 limit;
		}
	}
}
