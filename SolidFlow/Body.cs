using CollisionFlow;
using CollisionFlow.Polygons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFlow
{
	public enum BodyState
	{
		None,
		Rest,
		Excite,
	}
	public class Body
	{
		public Vector128? Center { get; set; }
		public string Name { get; set; }

		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies)
			: this(dispatcher, verticies, Vector128.Zero)
		{

		}
		public Body(CollisionDispatcher dispatcher, IEnumerable<Vector128> verticies, Vector128 course)
		{
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			if (verticies is null)
			{
				throw new ArgumentNullException(nameof(verticies));
			}

			this.course = course;
			var builder = new PolygonBuilder(course);
			foreach (var vertex in verticies)
			{
				builder.Add(vertex);
			}
			Handler = Dispatcher.Add(builder.GetLines());
			Handler.AttachetData = this;
		}

		public CollisionDispatcher Dispatcher { get; }
		public IPolygonHandler Handler { get; private set; }
		public double Weight { get; set; } = 1;
		public double Bounce { get; set; } = 0;

		public Vector128 Pull { get; set; }
		public HashSet<Body> RestOn { get; } = new HashSet<Body>();
		private HashSet<Body> RestFor { get; } = new HashSet<Body>();

		public void Refresh()
		{
			if (!IsRest)
			{
				Course = (Course.ToVector() + Pull.ToVector() * Weight).ToVector128();
				ClearRest();
			}
		}
		public bool IsRest => RestOn.Any();
		public void CreateRest(Body body)
		{
			Course = Vector128.Zero;
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
			}
			RestFor.Clear();
		}
		public void Push(Vector128 course)
		{
			if (!Course.Equals(course))
			{
				ClearRest();
				Course = course; 
			}
		}

		private Vector128 course;
		public Vector128 Course
		{
			get => course;
			private set
			{
				if (!Course.Equals(value))
				{
					course = value;
					var builder = new PolygonBuilder(Course);
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
	}
}
