using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    //the facilities for a route
    public class MVVMRouteFacility : INotifyPropertyChanged
    {
        public List<RouteFacility> Facilities { get; set; }

        private RouteFacility _selectedFacility;
        public RouteFacility SelectedFacility
        {
            get { return _selectedFacility; }
            set { _selectedFacility = value; NotifyPropertyChanged("SelectedFacility"); }
        }

        public RouteFacility.FacilityType Type { get; set; }
        public MVVMRouteFacility(RouteFacility.FacilityType type)
        {
            this.Facilities = new List<RouteFacility>();

            this.Type = type;

        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the route class
    public class MVVMRouteClass
    {

        public RouteAirlinerClass.SeatingType Seating { get; set; }

        public double FarePrice { get; set; }

        public List<MVVMRouteFacility> Facilities { get; set; }

        public AirlinerClass.ClassType Type { get; set; }
        //public int CabinCrew { get; set; }

        public MVVMRouteClass(AirlinerClass.ClassType type, RouteAirlinerClass.SeatingType seating, double fareprice)
        {
            this.Type = type;
            this.Seating = seating;
            this.FarePrice = 10;

            this.Facilities = new List<MVVMRouteFacility>();

            foreach (RouteFacility.FacilityType facType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)facType)
                {

                    MVVMRouteFacility facility = new MVVMRouteFacility(facType);

                    foreach (RouteFacility fac in RouteFacilities.GetFacilities(facType))
                        facility.Facilities.Add(fac);


                    this.Facilities.Add(facility);
                }
            }
        }

    }
    //the converter for the statistics for a route
    public class RouteStatisticsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Route route = (Route)value;
            List<KeyValuePair<string, object>> stats = new List<KeyValuePair<string, object>>();

            if (route.Type == Route.RouteType.Passenger)
            {
                RouteAirlinerClass raClass = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                double passengers = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));

                stats.Add(new KeyValuePair<string, object>("Total Passengers", passengers));
                stats.Add(new KeyValuePair<string, object>("Average Passengers", avgPassengers));

            }
            if (route.Type == Route.RouteType.Cargo)
            {
                double cargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                stats.Add(new KeyValuePair<string, object>("Total Cargo", cargo));
                stats.Add(new KeyValuePair<string, object>("Average Cargo", avgCargo));
            }

            stats.Add(new KeyValuePair<string, object>("Filling Degree", string.Format("{0:0.##} %", route.FillingDegree * 100)));

            return stats;


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for an unions member
    public class UnionMemberConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseUnit unit = (BaseUnit)value;

            if (unit is Union)
            {
                var countries = ((Union)unit).Members.Where(m => GameObject.GetInstance().GameTime >= m.MemberFromDate && GameObject.GetInstance().GameTime <= m.MemberToDate).Select(m => m.Country);

                return countries;
            }
            else
            {
               
                List<Country> countries = new List<Country>();

                countries.Add((Country)unit);

                return countries;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for possibility of starting a flight
    public class StartFlightBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FleetAirliner airliner = (FleetAirliner)value;

            if (parameter.ToString() == "start")
            {
                if (airliner.HasRoute && airliner.Status == FleetAirliner.AirlinerStatus.Stopped)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            if (parameter.ToString() == "stop")
            {
                if (airliner.HasRoute && airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
