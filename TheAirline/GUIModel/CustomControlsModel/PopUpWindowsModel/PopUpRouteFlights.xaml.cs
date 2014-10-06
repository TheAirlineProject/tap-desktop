using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpRouteFlights.xaml
    /// </summary>
    public partial class PopUpRouteFlights : Window
    {

        public ObservableCollection<RouteTimeTableEntry> ViewEntries { get; set; }
        public ObservableCollection<FleetAirliner> Aircrafts { get; set; }
        private Route Route;
     
        public static void ShowPopUp(Route route)
        {
            PopUpRouteFlights window = new PopUpRouteFlights(route);
            window.ShowDialog(); 
        }
        public PopUpRouteFlights(Route route)
        {
            this.Route = route;

            this.Aircrafts = new ObservableCollection<FleetAirliner>();

            var allRoutes = Airlines.GetAllAirlines().SelectMany(a=>a.Routes.Where(r=>(r.Destination1 == route.Destination1 && r.Destination2 == route.Destination2) || (r.Destination2 == route.Destination1 && r.Destination1 == route.Destination2)));

            var airliners = allRoutes.SelectMany(r => r.TimeTable.Entries.Select(e => e.Airliner)).Distinct();

            foreach (FleetAirliner airliner in airliners)
                this.Aircrafts.Add(airliner);

            this.ViewEntries = new ObservableCollection<RouteTimeTableEntry>();
            this.ViewEntries.CollectionChanged += ViewEntries_CollectionChanged;

            InitializeComponent();

            this.Loaded += PopUpRouteFlights_Loaded;
        }

        private void PopUpRouteFlights_Loaded(object sender, RoutedEventArgs e)
        {
            var allRoutes = Airlines.GetAllAirlines().SelectMany(a => a.Routes.Where(r => (r.Destination1 == this.Route.Destination1 && r.Destination2 == this.Route.Destination2) || (r.Destination2 == this.Route.Destination1 && r.Destination1 == this.Route.Destination2)));

            foreach (RouteTimeTableEntry entry in allRoutes.SelectMany(r=>r.TimeTable.Entries))
                this.ViewEntries.Add(entry); 

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(lbAircrafts.ItemsSource);

            if (view != null)
            {
                if (view.GroupDescriptions != null)
                {
                    view.GroupDescriptions.Clear();

                    var groupDescription = new PropertyGroupDescription("Airliner.Airline");
                    view.GroupDescriptions.Add(groupDescription);
                }
            }
        }

        private void ViewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
          
            if (e.NewItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.NewItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(entry.Airliner.Airliner.Type));

                    string id = entry.Airliner.Airliner.ID;

                    var g2 = new Guid(id);
       
                    byte[] bytes = g2.ToByteArray();

                    byte red = bytes[0];
                    byte green = bytes[1];
                    byte blue = bytes[2];
                    
                    var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                    brush.Opacity = 0.60;

                    TimeSpan localTimeDept = MathHelpers.ConvertTimeSpanToLocalTime(
                        sTime,
                        entry.DepartureAirport.Profile.TimeZone);
                    TimeSpan localTimeDest = MathHelpers.ConvertTimeSpanToLocalTime(
                        eTime,
                        entry.Destination.Airport.Profile.TimeZone);

                    //  string text = string.Format("{0}-{1}\n{2}-{3}", new AirportCodeConverter().Convert(entry.DepartureAirport), new AirportCodeConverter().Convert(entry.Destination.Airport),string.Format("{0:hh\\:mm}", entry.Time),string.Format("{0:hh\\:mm}",localTimeDept));
                    string text = entry.Airliner.Name;

                    string tooltip = string.Format(
                        "{0}-{1}\n",
                        new AirportCodeConverter().Convert(entry.DepartureAirport),
                        new AirportCodeConverter().Convert(entry.Destination.Airport)) + string.Format(
                        "{0}-{3}\n({1} {2})-({4} {5})",
                        string.Format("{0:hh\\:mm}", entry.Time),
                        string.Format("{0:hh\\:mm}", localTimeDept),
                        entry.DepartureAirport.Profile.TimeZone.ShortName,
                        string.Format("{0:hh\\:mm}", eTime),
                        string.Format("{0:hh\\:mm}", localTimeDest),
                        entry.Destination.Airport.Profile.TimeZone.ShortName);

                    uctimetable.addTimelineEntry(entry, sTime, eTime, text, brush, tooltip);
        
                }
            }

            if (e.OldItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.OldItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(entry.Airliner.Airliner.Type));

                    string text = entry.Airliner.Name;

                    uctimetable.removeTimelineEntry(sTime, eTime, text);
                }
            }

        }
    }
    public class AirlinerColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FleetAirliner airliner = (FleetAirliner)value;

            string id = airliner.Airliner.ID;

            var g2 = new Guid(id);


            byte[] bytes = g2.ToByteArray();

            byte red = bytes[0];
            byte green = bytes[1];
            byte blue = bytes[2];

            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Opacity = 0.60;

            return brush;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
