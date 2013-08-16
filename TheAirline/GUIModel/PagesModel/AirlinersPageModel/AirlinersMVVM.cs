using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    //the class for the airliner orders
    public class AirlinerOrdersMVVM : INotifyPropertyChanged
    {
        public ObservableCollection<AirlinerOrderMVVM> Orders { get; set; }
        public ObservableCollection<Airport> Homebases { get; set; }
        private long _totalamount;
        public long TotalAmount
        {
            get { return _totalamount; }
            set { _totalamount = value; NotifyPropertyChanged("TotalAmount"); }
        }
        private long _discount;
        public long Discount
        {
            get { return _discount; }
            set { _discount = value; NotifyPropertyChanged("Discount"); }
        }
        public AirlinerOrdersMVVM()
        {
            this.Homebases = new ObservableCollection<Airport>();
            this.Orders = new ObservableCollection<AirlinerOrderMVVM>();
            this.Orders.CollectionChanged += Orders_CollectionChanged;

        }
        private void Orders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            long price = this.Orders.Sum(o => o.Type.Price * o.Amount);

            this.Discount = Convert.ToInt64(price * (GeneralHelpers.GetAirlinerOrderDiscount(this.Orders.Sum(o => o.Amount)) / 100));

            if (GameObject.GetInstance().HumanAirline.Contract != null && GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Orders.First().Type.Manufacturer)
                this.Discount += Convert.ToInt64(price * (GameObject.GetInstance().HumanAirline.Contract.Discount / 100));

            this.TotalAmount = price - this.Discount;

        }
        public void addOrder(AirlinerOrderMVVM order)
        {
            this.Orders.Add(order);
            order.PropertyChanged += order_PropertyChanged;

            long minRequiredRunway = this.Orders.Count == 0 ? 0 : this.Orders.Max(o => o.Type.MinRunwaylength);

            this.Homebases.Clear();

            foreach (var homebase in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= minRequiredRunway))
                this.Homebases.Add(homebase);
           

        }
        private void order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            long price = this.Orders.Sum(o => o.Type.Price * o.Amount);

            this.Discount = Convert.ToInt64(price * (GeneralHelpers.GetAirlinerOrderDiscount(this.Orders.Sum(o => o.Amount)) / 100));

            if (GameObject.GetInstance().HumanAirline.Contract != null && GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Orders.First().Type.Manufacturer)
                this.Discount += Convert.ToInt64(price * (GameObject.GetInstance().HumanAirline.Contract.Discount / 100));

            this.TotalAmount = price - this.Discount;

        }
        //returns the delivery date for the order
        public DateTime getDeliveryDate()
        {
            double monthsToComplete = 0;

            foreach (AirlinerOrderMVVM order in this.Orders)
            {
                double orderToComplete = Math.Ceiling(Convert.ToDouble(order.Amount) / order.Type.ProductionRate);

                if (orderToComplete > monthsToComplete)
                    monthsToComplete = orderToComplete;
            }

            DateTime latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerOrderMVVM order in this.Orders)
            {
                DateTime date = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, GameObject.GetInstance().GameTime.Day);
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
                    latestDate = date;
            }

            return latestDate;
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
    //the class for an airliner order
    public class AirlinerOrderMVVM : INotifyPropertyChanged
    {
        public AirlinerType Type { get; set; }
        public List<AirlinerClass> Classes { get; set; }
    
        private int _amount;
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; NotifyPropertyChanged("Amount"); }
        }

        public AirlinerOrderMVVM(AirlinerType type, int amount = 1)
        {
            this.Type = type;
            this.Amount = amount;
            this.Classes = new List<AirlinerClass>();

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                AirlinerClass eClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, ((AirlinerPassengerType)type).MaxSeatingCapacity);
                eClass.createBasicFacilities(null);
                this.Classes.Add(eClass);
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
}
