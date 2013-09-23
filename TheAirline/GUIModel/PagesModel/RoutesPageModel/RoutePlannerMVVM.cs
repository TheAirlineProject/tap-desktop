using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    public enum IntervalType {Day, Week}
    public enum OpsType { Regular, Business, Whole_Day }
    //the class for the route planner mvvm item
    public class RoutePlannerItemMVVM
    {
        public Route Route { get; set; }
        public Brush Brush { get; set; }
        public TimeSpan FlightTime { get; set; }
        public RoutePlannerItemMVVM(Route route, AirlinerType type)
        {
            this.Route = route;

            Guid g2 = new Guid(this.Route.Id);

            byte[] bytes = g2.ToByteArray();

            byte red = bytes[0];
            byte green = bytes[1];
            byte blue = bytes[2];

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Opacity = 0.4;

            this.Brush = brush;
            this.FlightTime = MathHelpers.GetFlightTime(this.Route.Destination1, this.Route.Destination2, type);
        }
    }
    //the mvvm class for an hour item
    public class HourItemMVVM
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public HourItemMVVM(DateTime start, DateTime end)
        {
            this.Start = start;
            this.End = end;
        }
    }
    //the mvvm class for the route planner entry
    public class RouteEntryVVM
    {
        public Route Route { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public RouteEntryVVM(Route route, DayOfWeek day, TimeSpan departure)
        {
            this.Route = route;
            this.Day = day;
            this.DepartureTime = departure;
        }

    }
    //the converter for the homebound airport
    public class RouteHomeboundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Airport airport = (Airport)values[0];
                Route route = (Route)values[1];

                return route.Destination1 == airport ? route.Destination2 : route.Destination1;
            }
            catch
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
  
}
