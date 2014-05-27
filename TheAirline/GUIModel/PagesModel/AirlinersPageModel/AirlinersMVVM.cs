namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    //the mvvm class for an airliner type
    public class AirlinerTypeMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isselected;

        #endregion

        #region Constructors and Destructors

        public AirlinerTypeMVVM(AirlinerType type)
        {
            this.Type = type;
            this.IsSelected = false;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean IsSelected
        {
            get
            {
                return this._isselected;
            }
            set
            {
                this._isselected = value;
                this.NotifyPropertyChanged("IsSelected");
            }
        }

        public AirlinerType Type { get; set; }

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

    //the mvvm class for an airliner
    public class AirlinerMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isselected;

        #endregion

        #region Constructors and Destructors

        public AirlinerMVVM(Airliner airliner)
        {
            this.Airliner = airliner;
            this.IsSelected = false;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airliner Airliner { get; set; }

        public Boolean IsSelected
        {
            get
            {
                return this._isselected;
            }
            set
            {
                this._isselected = value;
                this.NotifyPropertyChanged("IsSelected");
            }
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

    //the class for the fleet sizes (most used aircrafts)
    public class AirlineFleetSizeMVVM
    {
        #region Constructors and Destructors

        public AirlineFleetSizeMVVM(AirlinerType type, int count)
        {
            this.Type = type;
            this.Count = count;
        }

        #endregion

        #region Public Properties

        public int Count { get; set; }

        public AirlinerType Type { get; set; }

        #endregion
    }

    //the class for the airliner orders
    public class AirlinerOrdersMVVM : INotifyPropertyChanged
    {
        #region Fields

        private DateTime _deliverydate;

        private long _discount;

        private long _totalamount;

        #endregion

        #region Constructors and Destructors

        public AirlinerOrdersMVVM()
        {
            this.Orders = new ObservableCollection<AirlinerOrderMVVM>();
            this.Orders.CollectionChanged += this.Orders_CollectionChanged;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public DateTime DeliveryDate
        {
            get
            {
                return this._deliverydate;
            }
            set
            {
                this._deliverydate = value;
                this.NotifyPropertyChanged("DeliveryDate");
            }
        }

        public long Discount
        {
            get
            {
                return this._discount;
            }
            set
            {
                this._discount = value;
                this.NotifyPropertyChanged("Discount");
            }
        }

        public ObservableCollection<AirlinerOrderMVVM> Orders { get; set; }

        public long TotalAmount
        {
            get
            {
                return this._totalamount;
            }
            set
            {
                this._totalamount = value;
                this.NotifyPropertyChanged("TotalAmount");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void addOrder(AirlinerOrderMVVM order)
        {
            this.Orders.Add(order);
            order.PropertyChanged += this.order_PropertyChanged;
        }

        //the update for the order
        public void orderUpdated()
        {
            long price = this.Orders.Sum(o => o.getOrderPrice());

            this.Discount =
                Convert.ToInt64(price * (GeneralHelpers.GetAirlinerOrderDiscount(this.Orders.Sum(o => o.Amount)) / 100));

            if (GameObject.GetInstance().HumanAirline.Contract != null && this.Orders.Count > 0
                && GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Orders.First().Type.Manufacturer)
            {
                this.Discount += Convert.ToInt64(
                    price * (GameObject.GetInstance().HumanAirline.Contract.Discount / 100));
            }

            this.TotalAmount = price - this.Discount;

            this.DeliveryDate = this.getDeliveryDate();
        }

        #endregion

        //returns a date for delivery based on the aircraft production rate

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Orders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.orderUpdated();
        }

        private DateTime getDeliveryDate()
        {
            double monthsToComplete = 0;

            foreach (AirlinerOrderMVVM order in this.Orders)
            {
                double orderToComplete = Math.Ceiling(Convert.ToDouble(order.Amount) / order.Type.ProductionRate);

                if (orderToComplete > monthsToComplete)
                {
                    monthsToComplete = orderToComplete;
                }
            }

            var latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerOrderMVVM order in this.Orders)
            {
                var date = new DateTime(
                    GameObject.GetInstance().GameTime.Year,
                    GameObject.GetInstance().GameTime.Month,
                    GameObject.GetInstance().GameTime.Day);
                int rate = order.Type.ProductionRate;
                if (order.Amount <= (rate / 4))
                {
                    date = date.AddMonths(3);
                }
                else
                {
                    for (int i = (rate / 4) + 1; i <= order.Amount; i++)
                    {
                        double iRate = 365 / rate;
                        date = date.AddDays(Math.Round(iRate, 0, MidpointRounding.AwayFromZero));
                    }
                }

                if (date > latestDate)
                {
                    latestDate = date;
                }
            }

            return latestDate;
        }

        private void order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.orderUpdated();
        }

        #endregion
    }

    //the class for an airliner order
    public class AirlinerOrderMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _amount;

        private List<AirlinerClass> _classes;

        private Airport _homebase;

        #endregion

        #region Constructors and Destructors

        public AirlinerOrderMVVM(AirlinerType type, AirlinerOrdersMVVM order, int amount = 1)
        {
            this.Type = type;
            this.Order = order;
            this.Amount = amount;
            this._classes = new List<AirlinerClass>();
            this.Homebases = new List<Airport>();

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                var eClass = new AirlinerClass(
                    AirlinerClass.ClassType.Economy_Class,
                    ((AirlinerPassengerType)type).MaxSeatingCapacity);
                eClass.createBasicFacilities(null);
                this.Classes.Add(eClass);
            }

            long minRunway = this.Type.MinRunwaylength;

            //var homebases = GameObject.GetInstance().HumanAirline.Airports.FindAll(a => (a.hasContractType(GameObject.GetInstance().HumanAirline, AirportContract.ContractType.Full_Service) || a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0) && a.getMaxRunwayLength() >= minRunway);

            List<Airport> homebases = AirlineHelpers.GetHomebases(GameObject.GetInstance().HumanAirline, minRunway);

            foreach (Airport homebase in homebases)
            {
                this.Homebases.Add(homebase);
            }
        }

        #endregion

        //returns the price for the order

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public int Amount
        {
            get
            {
                return this._amount;
            }
            set
            {
                this._amount = value;
                this.NotifyPropertyChanged("Amount");
            }
        }

        public List<AirlinerClass> Classes
        {
            get
            {
                return this._classes;
            }
            set
            {
                this._classes = value;
                this.Order.orderUpdated();
            }
        }

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

        public List<Airport> Homebases { get; set; }

        public AirlinerOrdersMVVM Order { get; set; }

        public AirlinerType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public long getOrderPrice()
        {
            double classesPrice = 0;

            foreach (AirlinerClass aClass in this.Classes)
            {
                foreach (AirlinerFacility facility in aClass.AllFacilities)
                {
                    classesPrice += facility.PricePerSeat * (facility.PercentOfSeats / 100) * aClass.SeatingCapacity;
                }
            }

            return Convert.ToInt64(this.Type.Price * this.Amount + classesPrice);
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

    //the mvvm class for the manufacturer contract
    public class ManufacturerContractMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Manufacturer _contracted;

        private Boolean _hascontract;

        #endregion

        #region Constructors and Destructors

        public ManufacturerContractMVVM(Manufacturer manufacturer, Manufacturer contracted)
        {
            this.Contracted = contracted;
            this.Manufacturer = manufacturer;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Manufacturer Contracted
        {
            get
            {
                return this._contracted;
            }
            set
            {
                this._contracted = value;
                this.HasContract = (value != null && this.Manufacturer == value);
                this.NotifyPropertyChanged("Contracted");
            }
        }

        public Boolean HasContract
        {
            get
            {
                return this._hascontract;
            }
            set
            {
                this._hascontract = value;
                this.NotifyPropertyChanged("HasContract");
            }
        }

        public Manufacturer Manufacturer { get; set; }

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

    //the converter if the human has contract with the manufacturer
    public class HumanManufacturerContractConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var manufacturer = (Manufacturer)value;

            if (GameObject.GetInstance().HumanAirline.Contract != null
                && GameObject.GetInstance().HumanAirline.Contract.Manufacturer == manufacturer)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}