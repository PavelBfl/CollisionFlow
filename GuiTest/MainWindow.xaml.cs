using CollisionFlow;
using GuiTest.ViewModel;
using System;
using System.Collections.Generic;
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
			var random = new Random(6);
			for (int iRow = 0; iRow < 5; iRow++)
			{
				for (int iColumn = 0; iColumn < 5; iColumn++)
				{
					var polygonVm = new PolygonVm(
						new System.Windows.Rect(iColumn * 100, iRow * 100, 100, 100),
						CollisionDispatcher,
						random,
						true //iColumn == 0
					);
					if (polygonVm.Polygon is not null)
					{
						Polygons.Add(polygonVm);
						CnvRoot.Children.Add(polygonVm.Polygon);
					}
				}
			}

			//var left = new RectangleVm(new System.Windows.Rect(-10, 0, 9, 350), CollisionDispatcher);
			//var top = new RectangleVm(new System.Windows.Rect(0, -10, 350, 9), CollisionDispatcher);
			//var right = new RectangleVm(new System.Windows.Rect(360, 0, 10, 350), CollisionDispatcher);
			//var bottom = new RectangleVm(new System.Windows.Rect(0, 351, 350, 10), CollisionDispatcher);
			//CnvRoot.Children.Add(left.Rectangle);
			//CnvRoot.Children.Add(right.Rectangle);
			//CnvRoot.Children.Add(top.Rectangle);
			//CnvRoot.Children.Add(bottom.Rectangle);

			DispatcherTimer = new DispatcherTimer();
			DispatcherTimer.Tick += FrameUpdate;
			DispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
			DispatcherTimer.Start();
		}

		private void FrameUpdate(object? sender, EventArgs e)
		{
			var offset = 1d;
			CollisionResult? result = CollisionDispatcher.Offset(offset);
			while (result is not null && result.Offset < offset)
			{
				var edgePolygon = Polygons.Select((x, i) => new { x, i }).FirstOrDefault(x => ReferenceEquals(x.x.PolygonHandler, result.EdgePolygon));
				var vertexPolygon = Polygons.Select((x, i) => new { x, i }).FirstOrDefault(x => ReferenceEquals(x.x.PolygonHandler, result.VertexPolygon));

				if (edgePolygon is not null)
				{
					edgePolygon.x.Course = new Vector128(-edgePolygon.x.Course.ToVector());
				}
				if (vertexPolygon is not null)
				{
					vertexPolygon.x.Course = new Vector128(-vertexPolygon.x.Course.ToVector());
				}

				offset -= result.Offset;
				result = CollisionDispatcher.Offset(offset);
			}
			foreach (var polygon in Polygons)
			{
				polygon.Update();
			}
		}

		private List<PolygonVm> Polygons { get; } = new();
		private CollisionDispatcher CollisionDispatcher { get; } = new();
		public DispatcherTimer DispatcherTimer { get; }

		private void UpdateFrame_Click(object sender, RoutedEventArgs e)
		{
			FrameUpdate(null, EventArgs.Empty);
		}
	}
}