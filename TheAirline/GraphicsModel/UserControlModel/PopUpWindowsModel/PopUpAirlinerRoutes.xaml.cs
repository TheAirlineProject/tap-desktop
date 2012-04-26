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

            this.Width = 1200;

            this.Height = 250;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbRoutes = new ListBox();
            lbRoutes.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRoutes.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
        
            mainPanel.Children.Add(lbRoutes);

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
               lbRoutes.Items.Add(new QuickInfoValue(day.ToString(),createRoutePanel(day)));
         
            }

            this.Content = mainPanel;
        }
        //creates the panel for a route for a day
        private Border createRoutePanel(DayOfWeek day)
        {
            Border brdDay = new Border();
            brdDay.BorderThickness = new Thickness(1,1,1,1);
            brdDay.BorderBrush = Brushes.Black;

            Canvas cnvFlights = new Canvas();
            cnvFlights.Width = 24 * 60 / 2;
            cnvFlights.Height = 20;
            brdDay.Child = cnvFlights;
        
            List<RouteTimeTableEntry> entries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == day)).ToList(); //&& e.Time.Hours<12)).ToList();
       
            foreach (RouteTimeTableEntry e in entries)
            {
                double maxTime = new TimeSpan(24, 0, 0).Subtract(e.Time).TotalMinutes;
                
                TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                ContentControl ccFlight = new ContentControl();
                ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(Math.Min(flightTime.TotalMinutes/2,maxTime/2), e);
                int minutes = 15 * (e.Time.Minutes / 15);
                int hours = e.Time.Hours;
            
                Canvas.SetLeft(ccFlight, 30*hours +e.Time.Minutes/2);
                           
                cnvFlights.Children.Add(ccFlight);
            }

            List<RouteTimeTableEntry> dayBeforeEntries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == day - 1)).ToList();

            foreach (RouteTimeTableEntry e in dayBeforeEntries)
            {
                TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                TimeSpan endTime = e.Time.Add(flightTime);
                if (endTime.Days == 1)
                {
                    ContentControl ccFlight = new ContentControl();
                    ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                    ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(endTime.Subtract(new TimeSpan(1,0,0,0)).TotalMinutes/2, e);
                 
                    Canvas.SetLeft(ccFlight, 0);

                    cnvFlights.Children.Add(ccFlight);
                }
            }

            return brdDay;
        }
    }
    //the converter for getting the end time for a route entry
    public class RouteEntryEndTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            RouteTimeTableEntry e = (RouteTimeTableEntry)value;
            TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, e.Airliner.Airliner.Type);
            
            return e.Time.Add(flightTime);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
   
}
