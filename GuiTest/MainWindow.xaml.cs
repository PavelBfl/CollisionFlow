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

			for (int iRow = 0; iRow < 5; iRow++)
			{
				for (int iColumn = 0; iColumn < 5; iColumn++)
				{
					var polygonVm = new PolygonVm(
						new System.Windows.Rect(iColumn * 100, iRow * 100, 100, 100),
						CollisionDispatcher
					);
					Polygons.Add(polygonVm);
					CnvRoot.Children.Add(polygonVm.Polygon);
				}
			}
		}

		private List<PolygonVm> Polygons { get; } = new();
		private CollisionDispatcher CollisionDispatcher { get; } = new();
	}
}
