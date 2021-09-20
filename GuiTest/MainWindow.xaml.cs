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

			const double WEIGHT_MAX = 10;
			const double HEIGHT_MAX = 10;
			const double SPEED_MAX = 0;
			const int ROWS_COUNT = 10;
			const int COLUMNS_COUNT = 10;
			const double GLOBAL_OFFSET = 300;

			var random = new Random(1);
			for (int iRow = 0; iRow < ROWS_COUNT; iRow++)
			{
				for (int iColumn = 0; iColumn < COLUMNS_COUNT; iColumn++)
				{
					var offsetX = GLOBAL_OFFSET + iColumn * WEIGHT_MAX;
					var offsetY = iRow * HEIGHT_MAX;
					var centerX = offsetX + WEIGHT_MAX / 2;
					var centerY = offsetY + HEIGHT_MAX / 2;
					var points = PolygonBuilder.RegularPolygon((Math.Min(WEIGHT_MAX, HEIGHT_MAX) - 1) / 2, random.Next(3, 10))
						.Select(x => new Vector128(x.X + centerX, x.Y + centerY))
						.ToArray();

					var body = new Body(BodyDispatcher.Dispatcher, points, new Vector128(random.NextDouble() * SPEED_MAX, random.NextDouble() * SPEED_MAX))
					{
						Pull = new Vector128(0, 0.1),
						Bounce = 0.95,
					};
					BodyDispatcher.Bodies.Add(body);
				}
			}

			var bottom = new Body(BodyDispatcher.Dispatcher, new Vector128[]
			{
				new(0, -1000),
				new(0, 300),
				new(700, 300),
				new(700, -1000),
				new(710, -1000),
				new(710, 310),
				new(-10, 310),
				new(-10, -1000),
			})
			{
				Weight = 1000000000000,
			};
			BodyDispatcher.Bodies.Add(bottom);

			var bod = new Body(BodyDispatcher.Dispatcher, new Vector128[]
			{
				new(350, 200),
				new(400, 250),
				new(300, 250),
			})
			{
				Weight = 1000000000000,
			};
			BodyDispatcher.Bodies.Add(bod);

			foreach (var body in BodyDispatcher.Bodies)
			{
				var bodyVm = new BodyVm(body);
				Bodyes.Add(bodyVm);
				CnvRoot.Children.Add(bodyVm.Polygon);
			}

			DispatcherTimer = new DispatcherTimer();
			DispatcherTimer.Tick += FrameUpdate;
			DispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
			DispatcherTimer.Start();
		}

		private void FrameUpdate(object? sender, EventArgs e)
		{
			BodyDispatcher.Offset(1);
			foreach (var bodyVm in Bodyes)
			{
				bodyVm.Refresh();
			}
		}

		private List<BodyVm> Bodyes { get; } = new();
		private BodyDispatcher BodyDispatcher { get; } = new();
		public DispatcherTimer DispatcherTimer { get; }
	}
}