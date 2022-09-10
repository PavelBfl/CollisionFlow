using CollisionFlow;
using CollisionFlow.Polygons;
using Flowing.Mutate;
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
			Acceleration.X.Changed += AccelerationVectorChanged;
			Acceleration.Y.Changed += AccelerationVectorChanged;
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

		public Vector2<CourseLimit> Acceleration { get; } = new Vector2<CourseLimit>();

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
			const double LOCAL_TIME_EPSILON = 0.000000001;
			foreach (var xLimit in xLimits)
			{
				var localTime = new CourseA(currentSpeed.X, x).GetTime(xLimit);
				if (localTime > LOCAL_TIME_EPSILON)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
				localTime = new CourseA(currentSpeed.X, x).GetTime(-xLimit);
				if (localTime > LOCAL_TIME_EPSILON)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
			}
			foreach (var yLimit in yLimits)
			{
				var localTime = new CourseA(currentSpeed.Y, y).GetTime(yLimit);
				if (localTime > LOCAL_TIME_EPSILON)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
				localTime = new CourseA(currentSpeed.Y, y).GetTime(-yLimit);
				if (localTime > LOCAL_TIME_EPSILON)
				{
					time = time is null ? localTime : Math.Min(time.Value, localTime);
				}
			}

			if (!(LimitEvent is null))
			{
				Dispatcher.Remove(LimitEvent);
				LimitEvent = null;
			}
			var time = Acceleration.GetNextTime();
			if (!double.IsInfinity(time))
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
