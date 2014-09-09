namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    //the mvvm object for a codeshare agreement
    public class CodeshareAgreementMVVM
    {
        #region Constructors and Destructors

        public CodeshareAgreementMVVM(CodeshareAgreement agreement)
        {
            this.Agreement = agreement;
        }

        #endregion

        #region Public Properties

        public CodeshareAgreement Agreement { get; set; }

        public Boolean IsHuman
        {
            get
            {
                return this.Agreement.Airline1.IsHuman || this.Agreement.Airline2.IsHuman;
            }
            private set
            {
                ;
            }
        }

        #endregion
    }

    //the mvvm object for an alliance member
    public class AllianceMemberMVVM
    {
        #region Constructors and Destructors

        public AllianceMemberMVVM(AllianceMember member, Boolean isremoveable)
        {
            this.Member = member;
            this.IsRemoveable = isremoveable;
        }

        #endregion

        #region Public Properties

        public Boolean IsRemoveable { get; set; }

        public AllianceMember Member { get; set; }

        #endregion
    }

    //the mvvm object for an alliance
    public class AllianceMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _destinations;

        private int _fleetsize;

        private int _passengers;

        private int _routes;

        private int _servingcountries;

        #endregion

        #region Constructors and Destructors

        public AllianceMVVM(Alliance alliance)
        {
            this.Alliance = alliance;
            this.Members = new ObservableCollection<AllianceMemberMVVM>();
            this.PendingMembers = new ObservableCollection<PendingAllianceMember>();
            this.AllianceRoutes = new List<AllianceRouteMMVM>();

            foreach (AllianceMember member in this.Alliance.Members)
            {
                this.Members.Add(new AllianceMemberMVVM(member, this.Alliance.IsHumanAlliance));
            }

            foreach (PendingAllianceMember member in this.Alliance.PendingMembers)
            {
                this.PendingMembers.Add(member);
            }

            this.setValues();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Alliance Alliance { get; set; }

        public List<AllianceRouteMMVM> AllianceRoutes { get; set; }

        public int Destinations
        {
            get
            {
                return this._destinations;
            }
            set
            {
                this._destinations = value;
                this.NotifyPropertyChanged("Destinations");
            }
        }

        public int FleetSize
        {
            get
            {
                return this._fleetsize;
            }
            set
            {
                this._fleetsize = value;
                this.NotifyPropertyChanged("FleetSize");
            }
        }

        public Boolean IsHumanAlliance
        {
            get
            {
                return this.Alliance.IsHumanAlliance;
            }
            private set
            {
                ;
            }
        }

        public ObservableCollection<AllianceMemberMVVM> Members { get; set; }

        public int Passengers
        {
            get
            {
                return this._passengers;
            }
            set
            {
                this._passengers = value;
                this.NotifyPropertyChanged("Passengers");
            }
        }

        public ObservableCollection<PendingAllianceMember> PendingMembers { get; set; }

        public int Routes
        {
            get
            {
                return this._routes;
            }
            set
            {
                this._routes = value;
                this.NotifyPropertyChanged("Routes");
            }
        }

        public int ServingCountries
        {
            get
            {
                return this._servingcountries;
            }
            set
            {
                this._servingcountries = value;
                this.NotifyPropertyChanged("ServiceCountries");
            }
        }

        #endregion

        //adds a member to the alliance

        #region Public Methods and Operators

        public void addMember(AllianceMember member)
        {
            this.Members.Add(new AllianceMemberMVVM(member, this.Alliance.IsHumanAlliance));
            this.Alliance.AddMember(member);

            this.setValues();
        }

        //removes a member from the alliance
        public void removeMember(AllianceMemberMVVM member)
        {
            this.Members.Remove(member);
            this.Alliance.RemoveMember(member.Member);

            this.setValues();
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
            this.Alliance.RemovePendingMember(pending);
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void setValues()
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            foreach (Route route in this.Members.SelectMany(m => m.Member.Airline.Routes))
            {
                this.AllianceRoutes.Add(new AllianceRouteMMVM(route.Airline, route));
            }

            this.Routes = this.Members.Sum(m => m.Member.Airline.Routes.Count);
            this.Destinations = this.Members.SelectMany(m => m.Member.Airline.Airports).Distinct().Count();
            this.ServingCountries =
                this.Members.SelectMany(m => m.Member.Airline.Airports.Select(a => a.Profile.Country))
                    .Distinct()
                    .Count();
            this.FleetSize = this.Members.Sum(m => m.Member.Airline.Fleet.Count);
            this.Passengers =
                this.Members.Sum(
                    m =>
                        (int)
                            m.Member.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, stat));
        }

        #endregion
    }

    //the mvvm class for an alliance route
    public class AllianceRouteMMVM
    {
        #region Constructors and Destructors

        public AllianceRouteMMVM(Airline airline, Route route)
        {
            this.Route = route;
            this.Airline = airline;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public Route Route { get; set; }

        #endregion
    }
}