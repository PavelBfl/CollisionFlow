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

		public Body(CollisionSpace dispatcher, IEnumerable<Vector128> verticies)
			: this(dispatcher, verticies, Course.Zero)
		{

		}
		public Body(CollisionSpace dispatcher, IEnumerable<Vector128> verticies, Course course)
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

		public CollisionSpace Dispatcher { get; }
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
			var currentSpeed = speed ?? Speed;

			var x = 0d;
			var y = 0d;
			var xLimits = new List<double>();
			var yLimits = new List<double>();
			foreach (var aSpeed in Acceleration.Speeds)
			{
				if (aSpeed.Limit.X > Math.Abs(currentSpeed.X))
				{
					if (!double.IsInfinity(aSpeed.Limit.X))
					{
						xLimits.Add(aSpeed.Limit.X);
					}
					x += aSpeed.Vector.X;
				}
				if (aSpeed.Limit.Y > Math.Abs(currentSpeed.Y))
				{
					if (!double.IsInfinity(aSpeed.Limit.Y))
					{
						yLimits.Add(aSpeed.Limit.Y);
					}
					y += aSpeed.Vector.Y;
				}
			}

			double? time = null;
			foreach (var xLimit in xLimits)
			{
				var localTime = CollisionFlow.Acceleration.GetTime(currentSpeed.X, x, xLimit);
				if (localTime > 0)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
				localTime = CollisionFlow.Acceleration.GetTime(currentSpeed.X, x, -xLimit);
				if (localTime > 0)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
			}
			foreach (var yLimit in yLimits)
			{
				var localTime = CollisionFlow.Acceleration.GetTime(currentSpeed.Y, y, yLimit);
				if (localTime > 0)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
				localTime = CollisionFlow.Acceleration.GetTime(currentSpeed.Y, y, -yLimit);
				if (localTime > 0)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
			}

			if (!(LimitEvent is null))
			{
				Dispatcher.Remove(LimitEvent);
				LimitEvent = null;
			}
			if (!(time is null))
			{
				LimitEvent = new FlowEvent(this);
				Dispatcher.AddExpectationOffset(LimitEvent, time.Value);
			}

			Course = new Course(
				currentSpeed.ToVector(),
				Vector128.Create(x, y)
			);
		}
		private FlowEvent LimitEvent { get; set; }

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

		private class FlowEvent : IFlowEvent
		{
			public FlowEvent(Body owner)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public Body Owner { get; }

			public void Handle()
			{
				Owner.RefreshCourse();
			}
		}
	}
}
