using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airports;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using TheAirline.Model.GeneralModel;

    public enum IntervalType
    {
        Day,

        Week
    }

    public enum OpsType
    {
        Regular,

        Business,

        Whole_Day
    }

    //the class for the route planner mvvm item
    public class RoutePlannerItemMVVM
    {
        #region Constructors and Destructors

        public RoutePlannerItemMVVM(Route route, AirlinerType type)
        {
            this.Route = route;

            var g2 = new Guid(this.Route.Id);

            byte[] bytes = g2.ToByteArray();

            byte red = bytes[0];
            byte green = bytes[1];
            byte blue = bytes[2];

            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Opacity = 0.4;

            this.Brush = brush;
            this.FlightTime = MathHelpers.GetFlightTime(this.Route.Destination1, this.Route.Destination2, type);

            this.Airliners = this.Route.GetAirliners().Count;
        }

        #endregion

        #region Public Properties

        public int Airliners { get; set; }

        public Brush Brush { get; set; }

        public TimeSpan FlightTime { get; set; }

        public Route Route { get; set; }

        #endregion
    }

    //the mvvm class for an hour item
    public class HourItemMVVM
    {
        #region Constructors and Destructors

        public HourItemMVVM(DateTime start, DateTime end)
        {
            this.Start = start;
            this.End = end;
        }

        #endregion

        #region Public Properties

        public DateTime End { get; set; }

        public DateTime Start { get; set; }

        #endregion
    }

    //the mvvm class for the route planner entry
    public class RouteEntryVVM
    {
        #region Constructors and Destructors

        public RouteEntryVVM(Route route, DayOfWeek day, TimeSpan departure)
        {
            this.Route = route;
            this.Day = day;
            this.DepartureTime = departure;
        }

        #endregion

        #region Public Properties

        public DayOfWeek Day { get; set; }

        public TimeSpan DepartureTime { get; set; }

        public Route Route { get; set; }

        #endregion
    }

    //the converter for the homebound airport
    public class RouteHomeboundConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var airport = (Airport)values[0];
                var route = (Route)values[1];

                return route.Destination1 == airport ? route.Destination2 : route.Destination1;
            }
            catch
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}