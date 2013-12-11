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
    //the mvvm object for a route
    public class RouteMVVM
    {
        public Route Route { get; set; }
        public double FillingDegree { get; set; }
        public double Total { get; set; }
        public double Average { get; set; }
        public double Balance { get; set; }
        public RouteMVVM(Route route)
        {
            this.Route = route;
            this.FillingDegree = this.Route.FillingDegree;
            this.Balance = this.Route.Balance;

            if (route.Type == Route.RouteType.Passenger)
            {
                RouteAirlinerClass raClass = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                this.Total = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                this.Average = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));

                
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                this.Total = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                this.Average = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

               
            }

            if (this.Average < 0)
                this.Average = 0;
        }
    }
    //the mvvm object for an airliner
    public class FleetAirlinerMVVM : INotifyPropertyChanged
    {
        public enum StatusMVVM { Stopped, Started }
        
        private StatusMVVM _status;
        public StatusMVVM Status
        {
            get { return _status; }
            set { _status= value; NotifyPropertyChanged("Status"); }
        }
        public Boolean HasRoute { get; set; }
        public FleetAirliner Airliner { get; set; }
        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.HasRoute = this.Airliner.HasRoute;
            this.Status = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped ? StatusMVVM.Stopped : StatusMVVM.Started;
        }
        //sets the status of the airliner
        public void setStatus(FleetAirliner.AirlinerStatus status)
        {
            this.Airliner.Status = status;

            this.Status = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped ? StatusMVVM.Stopped : StatusMVVM.Started;
       
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
    //the profit for a route
    public class RouteProfitMVVM
    {
        public Route Route { get; set; }
        public double Percent { get; set; }
        public RouteProfitMVVM(Route route, double total)
        {
            this.Route = route;

            double balance = this.Route.Balance;

            this.Percent = balance / total * 100;
        }
    }
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
    public class MVVMRouteClass : INotifyPropertyChanged
    {

        public RouteAirlinerClass.SeatingType Seating { get; set; }

        private double _fareprice;
        public double FarePrice
        {
            get { return _fareprice; }
            set { _fareprice = value; NotifyPropertyChanged("FarePrice"); }
        }
        public List<MVVMRouteFacility> Facilities { get; set; }

        public AirlinerClass.ClassType Type { get; set; }
        //public int CabinCrew { get; set; }

        public MVVMRouteClass(AirlinerClass.ClassType type, RouteAirlinerClass.SeatingType seating, double fareprice)
        {
            this.Type = type;
            this.Seating = seating;
            this.FarePrice = fareprice;

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
                stats.Add(new KeyValuePair<string, object>("Average Passengers", Math.Max(0,avgPassengers)));

            }
            if (route.Type == Route.RouteType.Cargo)
            {
                double cargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                stats.Add(new KeyValuePair<string, object>("Total Cargo", cargo));
                stats.Add(new KeyValuePair<string, object>("Average Cargo", Math.Max(0,avgCargo)));
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
    
}
