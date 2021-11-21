using CollisionFlow;
using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public class Body
	{
		public Vector128? Center { get; set; }
		public string Name { get; set; }

		private Body()
		{
			Acceleration.VectorChanged += AccelerationVectorChanged;
		}

		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies)
			: this(dispatcher, verticies, Course.Zero)
		{

		}
		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies, Course course)
			: this()
		{
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			if (verticies is null)
			{
				throw new ArgumentNullException(nameof(verticies));
			}

			var builder = new PolygonBuilder(course);
			foreach (var vertex in verticies)
			{
				builder.Add(vertex);
			}
			Handler = Dispatcher.Add(builder.GetLines());
			Handler.AttachetData = this;
		}

		private void AccelerationVectorChanged(object sender, EventArgs e)
		{
			RefreshCourse();
		}

		public CollisionDispatcher Dispatcher { get; }
		public IPolygonHandler Handler { get; private set; }
		public double Weight { get; set; } = 1;
		public double Bounce { get; set; } = 0;
		public bool IsTremble { get; set; } = false;
		public SpeedAccumulator Acceleration { get; } = new SpeedAccumulator();

		public HashSet<Body> RestOn { get; } = new HashSet<Body>();
		private HashSet<Body> RestFor { get; } = new HashSet<Body>();

		public bool IsRest => RestOn.Any();
		public void CreateRest(Body body)
		{
			Course = Course.Zero;
			RestOn.Add(body);
			body.RestFor.Add(this);
		}

		private void ClearRest()
		{
			foreach (var item in RestOn)
			{
				item.RestFor.Remove(this);
			}
			RestOn.Clear();
			foreach (var item in RestFor)
			{
				item.RestOn.Remove(this);
				if (!item.IsRest && item.Name != "Bod" && item.Name != "Bottom")
				{
					item.RefreshCourse();
				}
			}
			RestFor.Clear();
		}

		public Vector128 Speed
		{
			get => Handler.Edges[0].Course.V.ToVector128();
			set => RefreshCourse(value);
		}
		private void RefreshCourse(Vector128? speed = null)
		{
			Course = new Course(
				(speed ?? Speed).ToVector(),
				Acceleration.GetVector(speed ?? Speed).ToVector()
			);
		}
		public Course Course
		{
			get => Handler.Edges[0].Course;
			private set
			{
				if (!Course.Equals(value))
				{
					var builder = new PolygonBuilder(value);
					foreach (var vertex in Handler.Vertices)
					{
						builder.Add(vertex.Target);
					}
					Dispatcher.Remove(Handler);
					Handler = Dispatcher.Add(builder.GetLines());
					Handler.AttachetData = this;

					if (!Course.Equals(Course.Zero))
					{
						ClearRest();
					}
				}
			}
		}
		public void Refresh()
		{
			var builder = new PolygonBuilder(Course);
			foreach (var vertex in Handler.Vertices)
			{
				builder.Add(vertex.Target);
			}
			Dispatcher.Remove(Handler);
			Handler = Dispatcher.Add(builder.GetLines());
			Handler.AttachetData = this;

			if (!Course.Equals(Course.Zero))
			{
				ClearRest();
			}
		}

		public void SetPolygon(IEnumerable<Vector128> vertices, Course course)
		{
			var builder = new PolygonBuilder(course);
			foreach (var vertex in vertices)
			{
				builder.Add(vertex);
			}
			Dispatcher.Remove(Handler);
			Handler = Dispatcher.Add(builder.GetLines());
			Handler.AttachetData = this;

			ClearRest();
		}
	}

	public class SpeedAccumulator
	{
		private HashSet<ISpeedHandler> Speeds { get; } = new HashSet<ISpeedHandler>();

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

	public interface ISpeedHandler
	{
		Vector128 Vector { get; set; }
		Vector128 Limit { get; set; }
	}
}
