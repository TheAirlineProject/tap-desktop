using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    //the mvvm object for an alliance member
    public class AllianceMemberMVVM
    {
        public AllianceMember Member { get; set; }
        public Boolean IsRemoveable { get; set; }
        public AllianceMemberMVVM(AllianceMember member, Boolean isremoveable)
        {
            this.Member = member;
            this.IsRemoveable = isremoveable;
        }
    }
    //the mvvm object for an alliance
    public class AllianceMVVM : INotifyPropertyChanged
    {
        public Alliance Alliance { get; set; }
        public List<AllianceRouteMMVM> AllianceRoutes { get; set; }
        public ObservableCollection<AllianceMemberMVVM> Members { get; set; }
        public ObservableCollection<PendingAllianceMember> PendingMembers { get; set; }
        public Boolean IsHumanAlliance { get { return this.Alliance.IsHumanAlliance; } private set { ;} }
        private int _passengers;
        public int Passengers
        {
            get { return _passengers; }
            set { _passengers = value; NotifyPropertyChanged("Passengers"); }
        }
        private int _destinations;
        public int Destinations
        {
            get { return _destinations; }
            set { _destinations = value; NotifyPropertyChanged("Destinations"); }
        }
        private int _routes;
        public int Routes
        {
            get { return _routes; }
            set { _routes = value; NotifyPropertyChanged("Routes"); }
        }
        private int _servingcountries;
        public int ServingCountries
        {
            get { return _servingcountries; }
            set { _servingcountries = value; NotifyPropertyChanged("ServiceCountries"); }
        }
        private int _fleetsize;
        public int FleetSize
        {
            get { return _fleetsize; }
            set { _fleetsize = value; NotifyPropertyChanged("FleetSize"); }
        }
        public AllianceMVVM(Alliance alliance)
        {
            this.Alliance = alliance;
            this.Members = new ObservableCollection<AllianceMemberMVVM>();
            this.PendingMembers = new ObservableCollection<PendingAllianceMember>();
            this.AllianceRoutes = new List<AllianceRouteMMVM>();

            foreach (AllianceMember member in this.Alliance.Members)
                this.Members.Add(new AllianceMemberMVVM(member,this.Alliance.IsHumanAlliance));

            foreach (PendingAllianceMember member in this.Alliance.PendingMembers)
                this.PendingMembers.Add(member);

            setValues();   
        }
        //sets the values
        private void setValues()
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            foreach (Route route in this.Members.SelectMany(m => m.Member.Airline.Routes))
                this.AllianceRoutes.Add(new AllianceRouteMMVM(route.Airline, route));

            this.Routes = this.Members.Sum(m => m.Member.Airline.Routes.Count);
            this.Destinations = this.Members.SelectMany(m => m.Member.Airline.Airports).Distinct().Count();
            this.ServingCountries = this.Members.SelectMany(m => m.Member.Airline.Airports.Select(a => a.Profile.Country)).Distinct().Count();
            this.FleetSize = this.Members.Sum(m => m.Member.Airline.Fleet.Count);
            this.Passengers = this.Members.Sum(m => (int)m.Member.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, stat));
        }
        //adds a member to the alliance
        public void addMember(AllianceMember member)
        {
            this.Members.Add(new AllianceMemberMVVM(member,this.Alliance.IsHumanAlliance));
            this.Alliance.addMember(member);

            setValues();
        }
        //removes a member from the alliance
        public void removeMember(AllianceMemberMVVM member)
        {
            this.Members.Remove(member);
            this.Alliance.removeMember(member.Member);

            setValues();
        }
        public void removeMember(Airline airline)
        {
            AllianceMemberMVVM member = this.Members.FirstOrDefault(m => m.Member.Airline == airline);

            removeMember(member);
        }
        //removes a pending member from the alliance
        public void removePendingMember(PendingAllianceMember pending)
        {
            this.PendingMembers.Remove(pending);
            this.Alliance.removePendingMember(pending);
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
    //the mvvm class for an alliance route
    public class AllianceRouteMMVM
    {
        public Route Route { get; set; }
        public Airline Airline { get; set; }
        public AllianceRouteMMVM(Airline airline, Route route)
        {
            this.Route = route;
            this.Airline = airline;
        }
    }
}
