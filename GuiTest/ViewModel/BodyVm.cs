using CollisionFlow;
using SolidFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GuiTest.ViewModel
{
	public class BodyVm
	{
		public BodyVm(Body target)
		{
			Target = target ?? throw new ArgumentNullException(nameof(target));
			Polygon = new Polygon()
			{
				Stroke = Brushes.Red,
				Points = new PointCollection(Target.Handler.Vertices.Select(x => new System.Windows.Point(x.Target.X, x.Target.Y))),
			};
		}

		private Body Target { get; }
		public Polygon Polygon { get; }

		public Vector128 Course
		{
			get => Target.Course;
			set
			{
				Target.Course = value;
				PolygonRefresh();
			}
		}

		public void Refresh()
		{
			PolygonRefresh();
		}
		private void PolygonRefresh()
		{
			for (int i = 0; i < Target.Handler.Vertices.Count; i++)
			{
				var vertex = Target.Handler.Vertices[i].Target;
				Polygon.Points[i] = new System.Windows.Point(vertex.X, vertex.Y);
			}
		}
	}
}
