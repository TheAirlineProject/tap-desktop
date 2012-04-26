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
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerRoutes.xaml
    /// </summary>
    public partial class PopUpAirlinerRoutes : PopUpWindow
    {
        private FleetAirliner Airliner;
        public static object ShowPopUp(FleetAirliner airliner)
        {
            PopUpWindow window = new PopUpAirlinerRoutes(airliner);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirlinerRoutes(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            InitializeComponent();

            this.Title = this.Airliner.Name ;

            this.Width = 800;

            this.Height = 600;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbRoutes = new ListBox();
            lbRoutes.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            //lbRoutes.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbRoutes.MaxHeight = 500;

            mainPanel.Children.Add(lbRoutes);

            foreach (Route route in this.Airliner.Routes)
            {
                //lbRoutes.Items.Add(new QuickInfoValue(string.Format("{0}-{1}",route.Destination1.Profile.IATACode,route.Destination2.Profile.IATACode),createRoutePanel(route)));
                lbRoutes.Items.Add(createRoutePanel(route));
            }

            this.Content = mainPanel;
        }
        //creates the panel for a route
        private Canvas createRoutePanel(Route route)
        {
             Canvas cnvFlights = new Canvas();
             cnvFlights.Height = 20;
      
            foreach (RouteTimeTableEntry e in route.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner))
            {
                TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                ContentControl ccFlight = new ContentControl();
                ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(flightTime.TotalMinutes, e);
                int minutes = 15 * (e.Time.Minutes / 15);
                int hours = e.Time.Hours>=12 ? e.Time.Hours-12 : e.Time.Hours;
                //Canvas.SetTop(ccFlight, 60 * e.Time.Hours + e.Time.Minutes);
                
                Canvas.SetLeft(ccFlight, 60*hours +e.Time.Minutes);

           
                cnvFlights.Children.Add(ccFlight);
            }

            return cnvFlights;
        }
    }
}
