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
		public PolygonVm(System.Windows.Rect rect, CollisionDispatcher collisionDispatcher)
		{
			var size = Math.Min(rect.Height, rect.Width);

			var random = new Random();
			var radius = size / 4 + (size / 4 * random.NextDouble());
			var points = PolygonBuilder.RegularPolygon(radius, random.Next(3, 10))
				.Select(x => new Vector128(x.X + rect.X + rect.Width / 2, x.Y + rect.Y + rect.Height / 2)).ToArray();

			var builder = new PolygonBuilder(new Vector128(random.NextDouble(), random.NextDouble()));
			foreach (var point in points)
			{
				builder.Add(point);
			}

			PolygonHandler = collisionDispatcher.Add(builder.GetLines());
			Polygon = new Polygon()
			{
				Points = new PointCollection(PolygonHandler.Vertices.Select(x => new Point(x.Target.X, x.Target.Y))),
				Stroke = Brushes.Red,
			};
		}
		private IPolygonHandler PolygonHandler { get; }
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
