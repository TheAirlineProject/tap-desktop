namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Data;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.StatisticsModel;
    using TheAirline.Model.PilotModel;

    public class FleetAirlinerMVVM : INotifyPropertyChanged
    {
        #region Fields
      
        private Airport _homebase;

        private Boolean _isbuyable;

        private Boolean _isconvertable;

        private Boolean _ismissingpilots;

        private Boolean _isoutleasable;

        #endregion

        #region Constructors and Destructors

        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.Homebase = this.Airliner.Homebase;
            this.Classes = new ObservableCollection<AirlinerClassMVVM>();
            this.Owner = (this.Airliner.Airliner.Owner != null && this.Airliner.Airliner.Airline != this.Airliner.Airliner.Owner) ? this.Airliner.Airliner.Owner : null;

            AirlinerClass tClass;

            if (this.Airliner.Airliner.Classes.Count == 0)
            {
                tClass = null;
            }
            else
            {
                tClass = this.Airliner.Airliner.Classes[0];
            }

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
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

                Boolean changeable = this.Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats;

                if (this.Airliner.Airliner.Classes.Count == 3)
                {
                    if (this.Airliner.Airliner.Classes.IndexOf(aClass) == 1)
                    {
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[2].RegularSeatingCapacity;
                    }
                    else
                    {
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[1].RegularSeatingCapacity;
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
                this.Classes.Add(amClass);
            }

            this.Pilots = new ObservableCollection<Pilot>();

            foreach (Pilot pilot in this.Airliner.Pilots)
            {
                this.Pilots.Add(pilot);
            }

            this.IsMissingPilots = this.Airliner.Airliner.Type.CockpitCrew > this.Pilots.Count;

            this.Maintenances = new ObservableCollection<FleetAirlinerMaintenanceMVVM>();

            foreach (AirlinerMaintenanceCheck check in Airliner.Maintenance.Checks)
            {
                Boolean canperformcheck = this.Airliner.GroundedToDate < GameObject.GetInstance().GameTime;
                this.Maintenances.Add(new FleetAirlinerMaintenanceMVVM(check.Type, check.LastCheck, airliner.Maintenance.getNextCheck(check.Type),check.Interval,check.CheckCenter,canperformcheck));
            }

            this.IsBuyable = this.Airliner.Airliner.Airline != null && this.Airliner.Airliner.Airline.IsHuman
                             && this.Airliner.Purchased == FleetAirliner.PurchasedType.Leased && this.Airliner.Airliner.Owner == null;

            this.BuyPrice = this.IsBuyable ? this.Airliner.Airliner.getPrice() : 0;

            this.IsConvertable = this.Airliner.Airliner.Airline.IsHuman
                                 && this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                                 && !this.Airliner.HasRoute
                                 && (this.Airliner.Airliner.Owner!=null || this.Airliner.Airliner.Airline.isHuman())
                                 && this.Airliner.Purchased == FleetAirliner.PurchasedType.Bought
                                 && this.Airliner.Airliner.Type.IsConvertable;

            this.IsOutleasable = this.Airliner.Airliner.Airline.IsHuman
                && this.Airliner.Purchased == FleetAirliner.PurchasedType.Bought
                && this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                && !this.Airliner.HasRoute;

            
    
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties
        public ObservableCollection<FleetAirlinerMaintenanceMVVM> Maintenances{ get; set; }

       
        public FleetAirliner Airliner { get; set; }

        public Airline Owner { get; set; }

        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }

        public Airport Homebase
        {
            get
            {
                return this._homebase;
            }
            set
            {
                this._homebase = value;
                this.NotifyPropertyChanged("Homebase");
            }
        }

        public Boolean IsBuyable
        {
            get
            {
                return this._isbuyable;
            }
            set
            {
                this._isbuyable = value;
                this.NotifyPropertyChanged("IsBuyable");
            }
        }

        public Boolean IsConvertable
        {
            get
            {
                return this._isconvertable;
            }
            set
            {
                this._isconvertable = value;
                this.NotifyPropertyChanged("IsConvertable");
            }
        }
        public Boolean IsOutleasable
        {
            get
            {
                return this._isoutleasable;
            }
            set
            {
                this._isoutleasable = value;
                this.NotifyPropertyChanged("IsOutleasable");
            }
        }
        public Boolean IsMissingPilots
        {
            get
            {
                return this._ismissingpilots;
            }
            set
            {
                this._ismissingpilots = value;
                this.NotifyPropertyChanged("IsMissingPilots");
            }
        }

        public ObservableCollection<Pilot> Pilots { get; set; }

        public double BuyPrice { get; set; }
      
        #endregion

        #region Public Methods and Operators

        public void addPilot(Pilot pilot)
        {
            this.Pilots.Add(pilot);
            this.Airliner.addPilot(pilot);

            this.IsMissingPilots = this.Airliner.Airliner.Type.CockpitCrew > this.Pilots.Count;
        }

        //buys the airliner if leased
        public void buyAirliner()
        {
            this.Airliner.Purchased = FleetAirliner.PurchasedType.Bought;
            this.Airliner.Airliner.Owner = GameObject.GetInstance().HumanAirline;

            this.IsBuyable = false;

            AirlineHelpers.AddAirlineInvoice(
                GameObject.GetInstance().HumanAirline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -this.Airliner.Airliner.getPrice());
        }

        //converts the airliner to a cargo airliner
        public void convertToCargo()
        {
            FleetAirlinerHelpers.ConvertPassengerToCargoAirliner(this.Airliner);

            PageNavigator.NavigateTo(new PageAirline(this.Airliner.Airliner.Airline));
        }

        //adds a pilot to the airliner

        //removes a pilot from the airliner
        public void removePilot(Pilot pilot)
        {
            this.Pilots.Remove(pilot);
            this.Airliner.removePilot(pilot);

            this.IsMissingPilots = true;
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
            this.Facilities = new ObservableCollection<AirlinerFacility>();

            this.AirlinerClass = airlinerClass;
            this.Type = type;
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
                return this._selectedFacility;
            }
            set
            {
                if (value != null)
                {
                    AirlinerFacility oldValue = this._selectedFacility;
                    this._selectedFacility = value;
                    this.NotifyPropertyChanged("SelectedFacility");
                    this.setSeating(oldValue);
                }
            }
        }

        public AirlinerFacility.FacilityType Type { get; set; }

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

        private void setSeating(AirlinerFacility oldValue)
        {
            if (this.Type == AirlinerFacility.FacilityType.Seat && this._selectedFacility != null && oldValue != null)
            {
                this.AirlinerClass.ChangedFacility = true;
                double diff = oldValue.SeatUses / this._selectedFacility.SeatUses;
                this.AirlinerClass.Seating = Convert.ToInt16(Convert.ToDouble(this.AirlinerClass.Seating) * diff);
                this.AirlinerClass.MaxSeats = Convert.ToInt16(Convert.ToDouble(this.AirlinerClass.MaxSeats) * diff);
                this.AirlinerClass.ChangedFacility = false;
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
            this.Type = type.Type;
            this.Seating = seating;
            this.RegularSeatingCapacity = regularSeating;
            this.ChangeableSeats = changeableSeats;
            this.MaxSeats = maxseats;
            this.MaxSeatsCapacity = maxseats;
            this.ChangedFacility = false;

            this.Facilities = new ObservableCollection<AirlinerFacilityMVVM>();

            foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                var facility = new AirlinerFacilityMVVM(facType, this);

                var facilities = AirlinerFacilities.GetFacilities(facType);

                foreach (AirlinerFacility fac in facilities.FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year))
                {
                    facility.Facilities.Add(fac);
                }

                AirlinerFacility selectedFacility = type.getFacility(facType) == null
                    ? AirlinerFacilities.GetBasicFacility(facType)
                    : type.getFacility(facType);
                facility.SelectedFacility = selectedFacility;

                this.Facilities.Add(facility);
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
                return this._maxseats;
            }
            set
            {
                this._maxseats = value;
                this.NotifyPropertyChanged("MaxSeats");
            }
        }

        public int MaxSeatsCapacity { get; set; }

        public int RegularSeatingCapacity { get; set; }

        public int Seating
        {
            get
            {
                return this._seating;
            }
            set
            {
                this._seating = value;
                this.NotifyPropertyChanged("Seating");
            }
        }

        public AirlinerClass.ClassType Type { get; set; }

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

        #endregion
    }

    //the mvvm object for airline statistics
    public class FleetAirlinerStatisticsMVVM
    {
        #region Constructors and Destructors

        public FleetAirlinerStatisticsMVVM(FleetAirliner airliner, StatisticsType type)
        {
            this.Type = type;
            this.Airliner = airliner;
        }

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public double Change
        {
            get
            {
                return this.getChange();
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
                return this.getCurrentYear();
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
                return this.getLastYear();
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
            double lastYear = this.getLastYear();

            if (lastYear == 0)
            {
                return 1;
            }

            double changePercent = Convert.ToDouble(this.getCurrentYear() - lastYear) / lastYear;

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

            return this.Airliner.Statistics.getStatisticsValue(year, this.Type);
        }

        private double getLastYear()
        {
            return this.Airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, this.Type);
        }

        #endregion
    }
    //the mvvm object for an airliner maintenance 
    public class FleetAirlinerMaintenanceMVVM : INotifyPropertyChanged
    {
        public AirlinerMaintenanceType Type { get; set; }
        private DateTime _lastcheck;
        public DateTime LastCheck
        {
            get
            {
                return this._lastcheck;
            }
            set
            {
                this._lastcheck = value;
                this.NotifyPropertyChanged("LastCheck");
            }
        }
        private DateTime _nextcheck;
        public DateTime NextCheck 
        {
            get
            {
                return this._nextcheck;
            }
            set
            {
                this._nextcheck = value;
                this.NotifyPropertyChanged("NextCheck");
            }
        }
        public double Interval { get; set; }
        private MaintenanceCenterMVVM _center;
        public System.Collections.ObjectModel.ObservableCollection<MaintenanceCenterMVVM> Centers { get; set; }
        public MaintenanceCenterMVVM Center
        {
            get
            {
                return this._center;
            }
            set
            {
                this._center = value;
                this.NotifyPropertyChanged("Center");
            }
        }
        private Boolean _canperformcheck;
        public Boolean CanPerformCheck
        {
            get
            {
                return this._canperformcheck;
            }
            set
            {
                this._canperformcheck = value;
                this.NotifyPropertyChanged("CanPerformCheck");
            }
        }
        public FleetAirlinerMaintenanceMVVM(AirlinerMaintenanceType type, DateTime lastcheck,DateTime nextcheck,double interval,AirlinerMaintenanceCenter center,Boolean canperformcheck)
        {
            this.LastCheck = lastcheck;
            this.NextCheck = nextcheck;
            this.Type = type;
            this.Interval = interval;
            this.CanPerformCheck = canperformcheck;

            this.Centers = new ObservableCollection<MaintenanceCenterMVVM>();

            foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel >= this.Type.Requirement.TypeLevel))
                this.Centers.Add(new MaintenanceCenterMVVM(airport));

            foreach (MaintenanceCenter mCenter in GameObject.GetInstance().HumanAirline.MaintenanceCenters)
            {
                this.Centers.Add(new MaintenanceCenterMVVM(mCenter));

            }
         
            if (center != null)
            {
                this.Center = center.Airport != null ? this.Centers.First(c=>c.Airport == center.Airport) : this.Centers.First(c=>c.Center == center.Center); 
            }

        }
        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
    //the mvvm class for a center
    public class MaintenanceCenterMVVM
    {
        public Airport Airport { get; set; }
        public MaintenanceCenter Center { get; set; }
        public string Name { get; set; }
        public MaintenanceCenterMVVM(Airport airport)
        {
            this.Airport = airport;
            this.Name = this.Airport.Profile.Name;
        }
        public MaintenanceCenterMVVM(MaintenanceCenter center)
        {
            this.Center = center;
            this.Name = this.Center.Name;
        }

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