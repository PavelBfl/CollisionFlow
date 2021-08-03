using CollisionFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GuiTest.ViewModel
{
	class RectangleVm
	{
		public RectangleVm(System.Windows.Rect rect, CollisionDispatcher collisionDispatcher)
		{
			Rectangle = new System.Windows.Shapes.Rectangle()
			{
				Width = rect.Width,
				Height = rect.Height,
				Stroke = Brushes.Green,
			};
			Canvas.SetLeft(Rectangle, rect.X);
			Canvas.SetTop(Rectangle, rect.Y);

			var polygonBuilder = new PolygonBuilder()
				.Add(new Vector128(rect.TopLeft.X, rect.TopLeft.Y))
				.Add(new Vector128(rect.TopRight.X, rect.TopRight.Y))
				.Add(new Vector128(rect.BottomRight.X, rect.BottomRight.Y))
				.Add(new Vector128(rect.BottomLeft.X, rect.BottomLeft.Y));

			PolygonHandler = collisionDispatcher.Add(polygonBuilder.GetLines());
		}

		public IPolygonHandler PolygonHandler { get; }
		public System.Windows.Shapes.Rectangle Rectangle { get; }
	}
}
