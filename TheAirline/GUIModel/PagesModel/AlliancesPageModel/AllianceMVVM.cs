using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    //the mvvm object for a codeshare agreement
    public class CodeshareAgreementMVVM
    {
        #region Constructors and Destructors

        public CodeshareAgreementMVVM(CodeshareAgreement agreement)
        {
            Agreement = agreement;
        }

        #endregion

        #region Public Properties

        public CodeshareAgreement Agreement { get; set; }

        public Boolean IsHuman
        {
            get
            {
                return Agreement.Airline1.IsHuman || Agreement.Airline2.IsHuman;
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
            Member = member;
            IsRemoveable = isremoveable;
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
            Alliance = alliance;
            Members = new ObservableCollection<AllianceMemberMVVM>();
            PendingMembers = new ObservableCollection<PendingAllianceMember>();
            AllianceRoutes = new List<AllianceRouteMMVM>();

            foreach (AllianceMember member in Alliance.Members)
            {
                Members.Add(new AllianceMemberMVVM(member, Alliance.IsHumanAlliance));
            }

            foreach (PendingAllianceMember member in Alliance.PendingMembers)
            {
                PendingMembers.Add(member);
            }

            setValues();
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
                return _destinations;
            }
            set
            {
                _destinations = value;
                NotifyPropertyChanged("Destinations");
            }
        }

        public int FleetSize
        {
            get
            {
                return _fleetsize;
            }
            set
            {
                _fleetsize = value;
                NotifyPropertyChanged("FleetSize");
            }
        }

        public Boolean IsHumanAlliance
        {
            get
            {
                return Alliance.IsHumanAlliance;
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
                return _passengers;
            }
            set
            {
                _passengers = value;
                NotifyPropertyChanged("Passengers");
            }
        }

        public ObservableCollection<PendingAllianceMember> PendingMembers { get; set; }

        public int Routes
        {
            get
            {
                return _routes;
            }
            set
            {
                _routes = value;
                NotifyPropertyChanged("Routes");
            }
        }

        public int ServingCountries
        {
            get
            {
                return _servingcountries;
            }
            set
            {
                _servingcountries = value;
                NotifyPropertyChanged("ServiceCountries");
            }
        }

        #endregion

        //adds a member to the alliance

        #region Public Methods and Operators

        public void addMember(AllianceMember member)
        {
            Members.Add(new AllianceMemberMVVM(member, Alliance.IsHumanAlliance));
            Alliance.AddMember(member);

            setValues();
        }

        //removes a member from the alliance
        public void removeMember(AllianceMemberMVVM member)
        {
            Members.Remove(member);
            Alliance.RemoveMember(member.Member);

            setValues();
        }

        public void removeMember(Airline airline)
        {
            AllianceMemberMVVM member = Members.FirstOrDefault(m => m.Member.Airline == airline);

            removeMember(member);
        }

        //removes a pending member from the alliance
        public void removePendingMember(PendingAllianceMember pending)
        {
            PendingMembers.Remove(pending);
            Alliance.RemovePendingMember(pending);
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void setValues()
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            foreach (Route route in Members.SelectMany(m => m.Member.Airline.Routes))
            {
                AllianceRoutes.Add(new AllianceRouteMMVM(route.Airline, route));
            }

            Routes = Members.Sum(m => m.Member.Airline.Routes.Count);
            Destinations = Members.SelectMany(m => m.Member.Airline.Airports).Distinct().Count();
            ServingCountries =
                Members.SelectMany(m => m.Member.Airline.Airports.Select(a => a.Profile.Country))
                    .Distinct()
                    .Count();
            FleetSize = Members.Sum(m => m.Member.Airline.Fleet.Count);
            Passengers =
                Members.Sum(
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
            Route = route;
            Airline = airline;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public Route Route { get; set; }

        #endregion
    }
}