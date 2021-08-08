using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GuiTest.ViewModel
{
	class PolygonVm
	{
		public PolygonVm(System.Windows.Rect rect, CollisionDispatcher collisionDispatcher, Random random, bool isAdd)
		{
			IsAdd = isAdd;
			CollisionDispatcher = collisionDispatcher;

			var size = Math.Min(rect.Height, rect.Width);

			var radius = size / 4 + (size / 4 * random.NextDouble());
			var points = PolygonBuilder.RegularPolygon(radius, random.Next(3, 10))
				.Select(x => new Vector128(x.X + rect.X + rect.Width / 2, x.Y + rect.Y + rect.Height / 2)).ToArray();

			course = new Vector128(random.NextDouble(), random.NextDouble());
			var builder = new PolygonBuilder(course);
			foreach (var point in points)
			{
				builder.Add(point);
			}

			if (IsAdd)
			{
				PolygonHandler = collisionDispatcher.Add(builder.GetLines());
				Polygon = new Polygon()
				{
					Points = new PointCollection(PolygonHandler.Vertices.Select(x => new Point(x.Target.X, x.Target.Y))),
					Stroke = Brushes.Red,
				}; 
			}
		}

		public bool IsAdd { get; }
		public CollisionDispatcher CollisionDispatcher { get; }

		private Vector128 course;
		public Vector128 Course
		{
			get => course;
			set
			{
				if (Polygon is not null)
				{
					var oldCourse = course;
					course = value;
					var polygonBuilder = new PolygonBuilder(Course);
					for (int i = 0; i < PolygonHandler.Vertices.Count; i++)
					{
						polygonBuilder.Add(PolygonHandler.Vertices[i].Target);
					}
					CollisionDispatcher.Remove(PolygonHandler);
					PolygonHandler = CollisionDispatcher.Add(polygonBuilder.GetLines()); 
				}
			}
		}
		public IPolygonHandler PolygonHandler { get; private set; }
		public System.Windows.Shapes.Polygon Polygon { get; }

		public void Update()
		{
			for (var i = 0; i < PolygonHandler.Vertices.Count; i++)
			{
				var vertex = PolygonHandler.Vertices[i].Target;
				Polygon.Points[i] = new Point(vertex.X, vertex.Y);
			}
		}
	}
}
