using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.CalendarModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpRouteAirliners.xaml
    /// </summary>
    public partial class PopUpRouteAirliners : PopUpWindow
    {
        private Route Route;
        public static object ShowPopUp(Route route)
        {
            PopUpWindow window = new PopUpRouteAirliners(route);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpRouteAirliners(Route route)
        {
            this.Route = route;
            
            InitializeComponent();

            this.Title = string.Format("{0} - {1}", this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name);

            this.Width = 800;

            this.Height = 600;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ucCalendar ucCalendar = new ucCalendar();
            mainPanel.Children.Add(ucCalendar);

            this.Content = mainPanel;
        }
    }
}
