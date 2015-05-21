using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Pilots;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    public class FleetAirlinerMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _AMaintenanceInterval;

        private int _BMaintenanceInterval;

        private int _CMaintenanceInterval;

        private int _DMaintenanceInterval;

        private DateTime _SchedCMaintenance;

        private DateTime _SchedDMaintenance;

        private Airport _homebase;

        private Boolean _isbuyable;

        private Boolean _isconvertable;

        private Boolean _ismissingpilots;

        private Boolean _isoutleasable;

        #endregion

        #region Constructors and Destructors

        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            Airliner = airliner;
            Homebase = Airliner.Homebase;
            Classes = new ObservableCollection<AirlinerClassMVVM>();
            Owner = (Airliner.Airliner.Owner != null && Airliner.Airliner.Airline != Airliner.Airliner.Owner) ? Airliner.Airliner.Owner : null;

            AirlinerClass tClass;

            if (Airliner.Airliner.Classes.Count == 0)
            {
                tClass = null;
            }
            else
            {
                tClass = Airliner.Airliner.Classes[0];
            }

            foreach (AirlinerClass aClass in Airliner.Airliner.Classes)
            {
                int maxCapacity;

                if (airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                {
                    maxCapacity = ((AirlinerPassengerType)airliner.Airliner.Type).MaxSeatingCapacity;
                }
                else
                {
                    maxCapacity = tClass.RegularSeatingCapacity;
                }

                Boolean changeable = Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats;

                if (Airliner.Airliner.Classes.Count == 3)
                {
                    if (Airliner.Airliner.Classes.IndexOf(aClass) == 1)
                    {
                        maxSeats = maxCapacity - 1 - Airliner.Airliner.Classes[2].RegularSeatingCapacity;
                    }
                    else
                    {
                        maxSeats = maxCapacity - 1 - Airliner.Airliner.Classes[1].RegularSeatingCapacity;
                    }
                }
                else
                {
                    maxSeats = maxCapacity - 1;
                }

                var amClass = new AirlinerClassMVVM(
                    aClass,
                    aClass.SeatingCapacity,
                    aClass.RegularSeatingCapacity,
                    maxSeats,
                    changeable);
                Classes.Add(amClass);
            }

            Pilots = new ObservableCollection<Pilot>();

            foreach (Pilot pilot in Airliner.Pilots)
            {
                Pilots.Add(pilot);
            }

            IsMissingPilots = Airliner.Airliner.Type.CockpitCrew > Pilots.Count;

            AMaintenanceInterval = Airliner.AMaintenanceInterval;
            BMaintenanceInterval = Airliner.BMaintenanceInterval;
            CMaintenanceInterval = Airliner.CMaintenanceInterval;
            DMaintenanceInterval = Airliner.DMaintenanceInterval;

            SchedCMaintenance = Airliner.SchedCMaintenance;
            SchedDMaintenance = Airliner.SchedDMaintenance;

            IsBuyable = Airliner.Airliner.Airline.IsHuman
                             && Airliner.Purchased == FleetAirliner.PurchasedType.Leased && Airliner.Airliner.Owner == null;
            IsConvertable = Airliner.Airliner.Airline.IsHuman
                                 && Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                                 && !Airliner.HasRoute
                                 && (Airliner.Airliner.Owner!=null || Airliner.Airliner.Airline.isHuman())
                                 && Airliner.Purchased == FleetAirliner.PurchasedType.Bought
                                 && Airliner.Airliner.Type.IsConvertable;

            IsOutleasable = Airliner.Airliner.Airline.IsHuman
                && Airliner.Purchased == FleetAirliner.PurchasedType.Bought
                && Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                && !Airliner.HasRoute;

            
    
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public int AMaintenanceInterval
        {
            get
            {
                return _AMaintenanceInterval;
            }
            set
            {
                _AMaintenanceInterval = value;
                NotifyPropertyChanged("AMaintenanceInterval");
            }
        }

        public FleetAirliner Airliner { get; set; }

        public Airline Owner { get; set; }

        public int BMaintenanceInterval
        {
            get
            {
                return _BMaintenanceInterval;
            }
            set
            {
                _BMaintenanceInterval = value;
                NotifyPropertyChanged("BMaintenanceInterval");
            }
        }

        public int CMaintenanceInterval
        {
            get
            {
                return _CMaintenanceInterval;
            }
            set
            {
                _CMaintenanceInterval = value;
                NotifyPropertyChanged("CMaintenanceInterval");
            }
        }

        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }

        public int DMaintenanceInterval
        {
            get
            {
                return _DMaintenanceInterval;
            }
            set
            {
                _DMaintenanceInterval = value;
                NotifyPropertyChanged("DMaintenanceInterval");
            }
        }

        public Airport Homebase
        {
            get
            {
                return _homebase;
            }
            set
            {
                _homebase = value;
                NotifyPropertyChanged("Homebase");
            }
        }

        public Boolean IsBuyable
        {
            get
            {
                return _isbuyable;
            }
            set
            {
                _isbuyable = value;
                NotifyPropertyChanged("IsBuyable");
            }
        }

        public Boolean IsConvertable
        {
            get
            {
                return _isconvertable;
            }
            set
            {
                _isconvertable = value;
                NotifyPropertyChanged("IsConvertable");
            }
        }
        public Boolean IsOutleasable
        {
            get
            {
                return _isoutleasable;
            }
            set
            {
                _isoutleasable = value;
                NotifyPropertyChanged("IsOutleasable");
            }
        }
        public Boolean IsMissingPilots
        {
            get
            {
                return _ismissingpilots;
            }
            set
            {
                _ismissingpilots = value;
                NotifyPropertyChanged("IsMissingPilots");
            }
        }

        public ObservableCollection<Pilot> Pilots { get; set; }

        public DateTime SchedCMaintenance
        {
            get
            {
                return _SchedCMaintenance;
            }
            set
            {
                _SchedCMaintenance = value;
                NotifyPropertyChanged("SchedCMaintenance");
            }
        }

        public DateTime SchedDMaintenance
        {
            get
            {
                return _SchedDMaintenance;
            }
            set
            {
                _SchedDMaintenance = value;
                NotifyPropertyChanged("SchedDMaintenance");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void addPilot(Pilot pilot)
        {
            Pilots.Add(pilot);
            Airliner.AddPilot(pilot);

            IsMissingPilots = Airliner.Airliner.Type.CockpitCrew > Pilots.Count;
        }

        //buys the airliner if leased
        public void buyAirliner()
        {
            Airliner.Purchased = FleetAirliner.PurchasedType.Bought;
            IsBuyable = false;

            AirlineHelpers.AddAirlineInvoice(
                GameObject.GetInstance().HumanAirline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -Airliner.Airliner.GetPrice());
        }

        //converts the airliner to a cargo airliner
        public void convertToCargo()
        {
            FleetAirlinerHelpers.ConvertPassengerToCargoAirliner(Airliner);

            PageNavigator.NavigateTo(new PageAirline(Airliner.Airliner.Airline));
        }

        //adds a pilot to the airliner

        //removes a pilot from the airliner
        public void removePilot(Pilot pilot)
        {
            Pilots.Remove(pilot);
            Airliner.RemovePilot(pilot);

            IsMissingPilots = true;
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

        #endregion
    }

    //the mvvm class for an airliner facility class
    public class AirlinerFacilityMVVM : INotifyPropertyChanged
    {
        #region Fields

        private readonly AirlinerClassMVVM AirlinerClass;

        private AirlinerFacility _selectedFacility;

        #endregion

        #region Constructors and Destructors

        public AirlinerFacilityMVVM(AirlinerFacility.FacilityType type, AirlinerClassMVVM airlinerClass)
        {
            Facilities = new ObservableCollection<AirlinerFacility>();

            AirlinerClass = airlinerClass;
            Type = type;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerFacility> Facilities { get; set; }

        public AirlinerFacility SelectedFacility
        {
            get
            {
                return _selectedFacility;
            }
            set
            {
                if (value != null)
                {
                    AirlinerFacility oldValue = _selectedFacility;
                    _selectedFacility = value;
                    NotifyPropertyChanged("SelectedFacility");
                    setSeating(oldValue);
                }
            }
        }

        public AirlinerFacility.FacilityType Type { get; set; }

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

        private void setSeating(AirlinerFacility oldValue)
        {
            if (Type == AirlinerFacility.FacilityType.Seat && _selectedFacility != null && oldValue != null)
            {
                AirlinerClass.ChangedFacility = true;
                double diff = oldValue.SeatUses / _selectedFacility.SeatUses;
                AirlinerClass.Seating = Convert.ToInt16(Convert.ToDouble(AirlinerClass.Seating) * diff);
                AirlinerClass.MaxSeats = Convert.ToInt16(Convert.ToDouble(AirlinerClass.MaxSeats) * diff);
                AirlinerClass.ChangedFacility = false;
            }
        }

        #endregion
    }

    //the mvvm class for an airliner class
    public class AirlinerClassMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _maxseats;

        private int _seating;

        #endregion

        #region Constructors and Destructors

        public AirlinerClassMVVM(
            AirlinerClass type,
            int seating,
            int regularSeating,
            int maxseats,
            Boolean changeableSeats = false)
        {
            Type = type.Type;
            Seating = seating;
            RegularSeatingCapacity = regularSeating;
            ChangeableSeats = changeableSeats;
            MaxSeats = maxseats;
            MaxSeatsCapacity = maxseats;
            ChangedFacility = false;

            Facilities = new ObservableCollection<AirlinerFacilityMVVM>();

            foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                var facility = new AirlinerFacilityMVVM(facType, this);

                foreach (AirlinerFacility fac in AirlinerFacilities.GetFacilities(facType))
                {
                    facility.Facilities.Add(fac);
                }

                AirlinerFacility selectedFacility = type.GetFacility(facType) == null
                    ? AirlinerFacilities.GetBasicFacility(facType)
                    : type.GetFacility(facType);
                facility.SelectedFacility = selectedFacility;

                Facilities.Add(facility);
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean ChangeableSeats { get; set; }

        public Boolean ChangedFacility { get; set; }

        public ObservableCollection<AirlinerFacilityMVVM> Facilities { get; set; }

        public int MaxSeats
        {
            get
            {
                return _maxseats;
            }
            set
            {
                _maxseats = value;
                NotifyPropertyChanged("MaxSeats");
            }
        }

        public int MaxSeatsCapacity { get; set; }

        public int RegularSeatingCapacity { get; set; }

        public int Seating
        {
            get
            {
                return _seating;
            }
            set
            {
                _seating = value;
                NotifyPropertyChanged("Seating");
            }
        }

        public AirlinerClass.ClassType Type { get; set; }

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

        #endregion
    }

    //the mvvm object for airline statistics
    public class FleetAirlinerStatisticsMVVM
    {
        #region Constructors and Destructors

        public FleetAirlinerStatisticsMVVM(FleetAirliner airliner, StatisticsType type)
        {
            Type = type;
            Airliner = airliner;
        }

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public double Change
        {
            get
            {
                return getChange();
            }
            private set
            {
                ;
            }
        }

        public double CurrentYear
        {
            get
            {
                return getCurrentYear();
            }
            private set
            {
                ;
            }
        }

        public double LastYear
        {
            get
            {
                return getLastYear();
            }
            private set
            {
                ;
            }
        }

        public StatisticsType Type { get; set; }

        #endregion

        //returns the value for the last year

        //returns the change in %

        #region Methods

        private double getChange()
        {
            double lastYear = getLastYear();

            if (lastYear == 0)
            {
                return 1;
            }

            double changePercent = Convert.ToDouble(getCurrentYear() - lastYear) / lastYear;

            if (double.IsInfinity(changePercent))
            {
                return 1;
            }
            if (double.IsNaN(changePercent))
            {
                return 0;
            }

            return changePercent;
        }

        private double getCurrentYear()
        {
            int year = GameObject.GetInstance().GameTime.Year;

            return Airliner.Statistics.GetStatisticsValue(year, Type);
        }

        private double getLastYear()
        {
            return Airliner.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, Type);
        }

        #endregion
    }

    public class ValueIsMaxAirlinerClasses : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (System.Convert.ToInt16(values[0]) == System.Convert.ToInt16(values[1]));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}