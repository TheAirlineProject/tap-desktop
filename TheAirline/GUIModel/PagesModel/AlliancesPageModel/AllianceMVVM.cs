using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    //the mvvm object for an alliance
    public class AllianceMVVM : INotifyPropertyChanged
    {
        public Alliance Alliance { get; set; }
        public ObservableCollection<AllianceMember> Members { get; set; }
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
            this.Members = new ObservableCollection<AllianceMember>();
            this.PendingMembers = new ObservableCollection<PendingAllianceMember>();

            foreach (AllianceMember member in this.Alliance.Members)
                this.Members.Add(member);

            foreach (PendingAllianceMember member in this.Alliance.PendingMembers)
                this.PendingMembers.Add(member);

            setValues();   
        }
        //sets the values
        private void setValues()
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            this.Routes = this.Members.Sum(m => m.Airline.Routes.Count);
            this.Destinations = this.Members.SelectMany(m => m.Airline.Airports).Distinct().Count();
            this.ServingCountries = this.Members.SelectMany(m => m.Airline.Airports.Select(a => a.Profile.Country)).Distinct().Count();
            this.FleetSize = this.Members.Sum(m => m.Airline.Fleet.Count);
            this.Passengers = this.Members.Sum(m => (int)m.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, stat));
        }
        //adds a member to the alliance
        public void addMember(AllianceMember member)
        {
            this.Members.Add(member);
            this.Alliance.addMember(member);

            setValues();
        }
        //removes a member from the alliance
        public void removeMember(AllianceMember member)
        {
            this.Members.Remove(member);
            this.Alliance.removeMember(member);

            setValues();
        }
        public void removeMember(Airline airline)
        {
            AllianceMember member = this.Members.FirstOrDefault(m => m.Airline == airline);

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
}
