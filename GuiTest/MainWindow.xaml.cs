using CollisionFlow;
using CollisionFlow.Polygons;
using GuiTest.ViewModel;
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

			var random = new Random(0);
			var polygonVm = new PolygonVm(new System.Windows.Rect(10, 10, 100, 100), CollisionDispatcher, random)
			{
				Course = new Course(
					Vector128.Create(0.1, -2),
					Vector128.Create(0, 0.1)
				),
			};
			PolygonsVm.Add(polygonVm);

			var bottom = new RectangleVm(new System.Windows.Rect(-100, 300, 1000, 10), CollisionDispatcher);
			CnvRoot.Children.Add(bottom.Rectangle);

			foreach (var item in PolygonsVm)
			{
				CnvRoot.Children.Add(item.Polygon);
			}

			DispatcherTimer.Tick += UpdateFrame;
			DispatcherTimer.Start();
		}

		private void UpdateFrame(object? sender, EventArgs e)
		{
			var result = CollisionDispatcher.Offset(1);

			if (result is not null)
			{
				foreach (var item in PolygonsVm)
				{
					item.Course = Course.Zero;
				}
			}

			foreach (var polygon in PolygonsVm)
			{
				polygon.Update();
			}
		}

		private CollisionDispatcher CollisionDispatcher { get; } = new();
		private List<PolygonVm> PolygonsVm { get; } = new();
		private DispatcherTimer DispatcherTimer = new()
		{
			Interval = TimeSpan.FromSeconds(1 / 2),
		};

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