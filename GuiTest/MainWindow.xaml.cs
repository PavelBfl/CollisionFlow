using CollisionFlow;
using CollisionFlow.Polygons;
using GuiTest.ViewModel;
using SolidFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuiTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			const double Y = 100;

			var x1 = 50d;
			var v1 = 5d;
			var a1 = -0.1d;

			var x2 = 500d;
			var v2 = -10d;
			var a2 = 0.1d;

			DrawPoint(new Point(x1, Y), Brushes.Red, 7);
			DrawPoint(new Point(x2, Y), Brushes.Red, 7);

			var time = Acceleration.GetTime(x1, v1, a1, x2, v2, a2);

			if (time is not null)
			{
				var collisionX1 = Acceleration.Offset(time.Value, x1, v1, a1);
				var collisionX2 = Acceleration.Offset(time.Value, x2, v2, a2);

				DrawPoint(new Point(collisionX1, Y + 10), Brushes.Green);
				DrawPoint(new Point(collisionX2, Y + 20), Brushes.Green);

				const double TIME_STEP = 1;
				var timeCounter = TIME_STEP;
				while (timeCounter < time.Value)
				{
					var offset1 = Acceleration.Offset(timeCounter, x1, v1, a1);
					var offset2 = Acceleration.Offset(timeCounter, x2, v2, a2);

					DrawPoint(new Point(offset1, Y), Brushes.Pink);
					DrawPoint(new Point(offset2, Y), Brushes.Pink);

					timeCounter += TIME_STEP;
				} 
			}
		}

		private void DrawPoint(Point position, Brush color, double size = 5)
		{
			var ellipse = new Ellipse()
			{
				Width = size,
				Height = size,
				Fill = color,
			};
			Canvas.SetTop(ellipse, position.Y - size / 2);
			Canvas.SetLeft(ellipse, position.X - size / 2);
			CnvRoot.Children.Add(ellipse);
		}
	}
}