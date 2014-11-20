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
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel.PopUpMapModel
{
    /// <summary>
    /// Interaction logic for PopUpMapControl.xaml
    /// </summary>
    public partial class PopUpMapControl : Window
    {
        //shows the pop up for an airport
        public static void ShowPopUp(List<Airport> airports = null,List<Route> routes = null)
        {
            PopUpMapControl window = new PopUpMapControl(airports,routes);
            window.ShowDialog();
        }
        public PopUpMapControl(List<Airport> airports = null, List<Route> routes = null)
        {
            MapViewModel model = new MapViewModel(airports,routes);

            this.DataContext = model;

            InitializeComponent();

        }
        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1));
                //map.TargetCenter = map.ViewportPointToLocation(e.GetPosition(map));
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1));
            }
        }
    }
}
