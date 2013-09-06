using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    //the mvvm object for an airline
    public class AirlineMVVM : INotifyPropertyChanged
    {
        public List<PropertyInfo> Colors { get; set; }
        public Airline Airline { get; set; }
        public List<FleetAirliner> DeliveredFleet { get; set; }
        public List<FleetAirliner> OrderedFleet { get; set; }
        public ObservableCollection<AirlineFacilityMVVM> Facilities { get; set; }
        public ObservableCollection<AirlineFinanceMVVM> Finances { get; set; }
        public ObservableCollection<LoanMVVM> Loans { get; set; }
        public ObservableCollection<Pilot> Pilots { get; set; }
        public ObservableCollection<AirlineFeeMVVM> Wages { get; set; }
        public ObservableCollection<AirlineFeeMVVM> Discounts { get; set; }
        public ObservableCollection<AirlineFeeMVVM> Chargers { get; set; }
        public ObservableCollection<AirlineFeeMVVM> Fees { get; set; }
        public ObservableCollection<SubsidiaryAirline> Subsidiaries { get; set; }
        public ObservableCollection<AirlineInsurance> Insurances { get; set; }
        public ObservableCollection<AirlineAdvertisementMVVM> Advertisements { get; set; }
        public ObservableCollection<AirlineDestinationMVVM> Destinations { get; set; }
        public ObservableCollection<Airline> AirlineAirlines { get; set; }
        public Boolean IsBuyable { get; set; }
        public double LoanRate { get; set; }

        public int CabinCrew { get; set; }
        public int SupportCrew { get; set; }
        public int MaintenanceCrew { get; set; }
      
        private double _maxsubsidiarymoney;
        public double MaxSubsidiaryMoney
        {
            get { return _maxsubsidiarymoney; }
            set { _maxsubsidiarymoney = value; NotifyPropertyChanged("MaxSubsidiaryMoney"); }
        }
        private int _cockpitCrew;
        public int CockpitCrew
        {
            get { return _cockpitCrew; }
            set { _cockpitCrew = value; NotifyPropertyChanged("CockpitCrew"); }
        }
        private double _money;
        public double Money
        {
            get { return _money; }
            set { _money = value; NotifyPropertyChanged("Money"); }
        }
        private double _balance;
        public double Balance
        {
            get { return _balance; }
            set { _balance = value; NotifyPropertyChanged("Balance"); }
        }
        private Airline.AirlineLicense _license;
        public Airline.AirlineLicense License
        {
            get { return _license; }
            set { _license = value; NotifyPropertyChanged("License"); }
        }
        public AirlineMVVM(Airline airline)
        {
            this.Airline = airline;
            this.DeliveredFleet = this.Airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime);
            this.OrderedFleet = this.Airline.Fleet.FindAll(a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);
            this.Finances = new ObservableCollection<AirlineFinanceMVVM>();
            this.LoanRate = GeneralHelpers.GetAirlineLoanRate(this.Airline);

            this.Loans = new ObservableCollection<LoanMVVM>();
            this.Pilots = new ObservableCollection<Pilot>();
            this.Wages = new ObservableCollection<AirlineFeeMVVM>();
            this.Discounts = new ObservableCollection<AirlineFeeMVVM>();
            this.Facilities = new ObservableCollection<AirlineFacilityMVVM>();
            this.Fees = new ObservableCollection<AirlineFeeMVVM>();
            this.Chargers = new ObservableCollection<AirlineFeeMVVM>();
            this.Subsidiaries = new ObservableCollection<SubsidiaryAirline>();
            this.Insurances = new ObservableCollection<AirlineInsurance>();
            this.Advertisements = new ObservableCollection<AirlineAdvertisementMVVM>();
            this.Destinations = new ObservableCollection<AirlineDestinationMVVM>();
            this.AirlineAirlines = new ObservableCollection<Airline>();

            this.Airline.Loans.FindAll(l => l.IsActive).ForEach(l => this.Loans.Add(new LoanMVVM(l)));
            this.Airline.Pilots.ForEach(p => this.Pilots.Add(p));

            FeeTypes.GetTypes(FeeType.eFeeType.Wage).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Wages.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.Discount).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Discounts.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));

            FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Chargers.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            
            FeeTypes.GetTypes(FeeType.eFeeType.Fee).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Fees.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
          
            this.Airline.Subsidiaries.ForEach(s => this.Subsidiaries.Add(s));
            this.Airline.InsurancePolicies.ForEach(i => this.Insurances.Add(i));

            setValues();

            this.Colors = new List<PropertyInfo>();
      
            foreach (PropertyInfo c in typeof(Colors).GetProperties())
                this.Colors.Add(c);

            foreach (Airport airport in this.Airline.Airports)
                this.Destinations.Add(new AirlineDestinationMVVM(airport, airport.hasHub(this.Airline)));

            double buyingPrice = this.Airline.getValue() * 1000000 * 1.10;
            this.IsBuyable = !this.Airline.IsHuman && GameObject.GetInstance().HumanAirline.Money > buyingPrice;
     
        }
        //saves all the fees
        public void saveFees()
        {
            foreach (AirlineFeeMVVM fee in this.Wages)
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);

            foreach (AirlineFeeMVVM fee in this.Fees)
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);

            foreach (AirlineFeeMVVM fee in this.Discounts)
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);

            foreach (AirlineFeeMVVM fee in this.Chargers)
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);

        }
        //resets the fees
        public void resetFees()
        {
            this.Wages.Clear();
            this.Chargers.Clear();
            this.Fees.Clear();
            this.Discounts.Clear();

            FeeTypes.GetTypes(FeeType.eFeeType.Wage).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Wages.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.Discount).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Discounts.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.Fee).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Fees.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year).ForEach(f => this.Chargers.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));

        }
        //adds an airline insurance
        public void addAirlineInsurance(AirlineInsurance insurance)
        {
            this.Insurances.Add(insurance);
            this.Airline.addInsurance(insurance);
        }
        //adds a subsidiary airline
        public void addSubsidiaryAirline(SubsidiaryAirline airline)
        {
            this.Subsidiaries.Add(airline);

            AirlineHelpers.AddSubsidiaryAirline(GameObject.GetInstance().MainAirline, airline, airline.Money, airline.Airports[0]);
            airline.Airports.RemoveAt(0);

            this.MaxSubsidiaryMoney = this.Airline.Money / 2;

            this.AirlineAirlines.Add(airline);
        }
        //removes a subsidiary airline
        public void removeSubsidiaryAirline(SubsidiaryAirline airline)
        {
            this.Subsidiaries.Remove(airline);
            this.AirlineAirlines.Remove(airline);
        }
       //adds a facility
        public void addFacility(AirlineFacilityMVVM facility)
        {
            facility.Type = AirlineFacilityMVVM.MVVMType.Purchased;

            this.Airline.addFacility(facility.Facility);

            AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.Facility.Price);
            
        }
        //removes a facility 
        public void removeFacility(AirlineFacilityMVVM facility)
        {
            this.Airline.removeFacility(facility.Facility);

            facility.Type = AirlineFacilityMVVM.MVVMType.Available;

        }
        //removes a pilot
        public void removePilot(Pilot pilot)
        {
            this.Pilots.Remove(pilot);
            this.Airline.removePilot(pilot);
        }
        //adds a loan
        public void addLoan(Loan loan)
        {
            this.Loans.Add(new LoanMVVM(loan));

            this.Airline.addLoan(loan);

            setValues();

        }
        //saves the advertisements
        public void saveAdvertisements()
        {
            foreach (AirlineAdvertisementMVVM advertisement in this.Advertisements)
            {
                AdvertisementType type = advertisement.SelectedType;
                this.Airline.setAirlineAdvertisement(type);
            }
        }
        //sets the values
        private void setValues()
        {
            this.Finances.Clear();

            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
                this.Finances.Add(new AirlineFinanceMVVM(this.Airline, type));

            this.Money = this.Airline.Money;
            this.Balance = this.Airline.Money - this.Airline.StartMoney;

            this.CockpitCrew = this.Airline.Pilots.Count;
            this.CabinCrew = this.Airline.Routes.Where(r => r.Type == Route.RouteType.Passenger).Sum(r => ((PassengerRoute)r).getTotalCabinCrew());
            this.SupportCrew = this.Airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(this.Airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support).Sum(a => a.NumberOfEmployees);
            this.MaintenanceCrew = this.Airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(this.Airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees);

            foreach (AirlineFacility facility in AirlineFacilities.GetFacilities(f=>f.FromYear<=GameObject.GetInstance().GameTime.Year).OrderBy(f=>f.Name))
                this.Facilities.Add(new AirlineFacilityMVVM(this.Airline,facility,this.Airline.Facilities.Exists(f=>f.Uid == facility.Uid) ? AirlineFacilityMVVM.MVVMType.Purchased : AirlineFacilityMVVM.MVVMType.Available));

            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                {
                    AirlineAdvertisementMVVM advertisement = new AirlineAdvertisementMVVM(type);

                    advertisement.Types = AdvertisementTypes.GetTypes(type);

                    this.Advertisements.Add(advertisement);
             
                }
            }

            this.MaxSubsidiaryMoney = this.Airline.Money / 2;
            this.License = this.Airline.License;
           
            if (this.Airline.IsSubsidiary)
            {
                this.AirlineAirlines.Add(((SubsidiaryAirline)this.Airline).Airline);

                foreach (SubsidiaryAirline airline in ((SubsidiaryAirline)this.Airline).Airline.Subsidiaries)
                    this.AirlineAirlines.Add(airline);
            }
            else
            {
                foreach (SubsidiaryAirline airline in this.Subsidiaries)
                    this.AirlineAirlines.Add(airline);

                this.AirlineAirlines.Add(this.Airline);

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
    //the mvvm object for airline facilities
    public class AirlineFacilityMVVM : INotifyPropertyChanged
    {
        public Airline Airline { get; set; }
        public enum MVVMType { Purchased, Available }
        private MVVMType _type;
        public MVVMType Type
        {
            get { return _type; }
            set { _type = value; NotifyPropertyChanged("Type"); }
        }
        public AirlineFacility Facility { get; set; }
        public AirlineFacilityMVVM(Airline airline, AirlineFacility facility, MVVMType type)
        {
            this.Type = type;
            this.Airline = airline;
            this.Facility = facility;
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
    //the mvvm object for arline fees
    public class AirlineFeeMVVM
    {
        public FeeType FeeType { get; set; }
        public double Value { get; set; }
        public AirlineFeeMVVM(FeeType feeType, double value)
        {
            this.FeeType = feeType;
            this.Value = value;
        }
    }
    //the mvvm object for airline statistics
    public class AirlineStatisticsMVVM
    {
        public StatisticsType Type{ get; set; }
        public double LastYear { get { return getLastYear(); } private set { ;} }
        public double CurrentYear { get { return getCurrentYear(); } private set { ;} }
        public double Change { get { return getChange(); } private set { ;} }
        public Airline Airline { get; set; }
        public AirlineStatisticsMVVM(Airline airline, StatisticsType type)
        {
            this.Type = type;
            this.Airline = airline;
        }
        //returns the value for the last year
        private double getLastYear()
        {
            int year = GameObject.GetInstance().GameTime.Year-1;

            return this.Airline.Statistics.getStatisticsValue(year,this.Type);
        }
        //returns the value for the current year
        private double getCurrentYear()
        {
            int year = GameObject.GetInstance().GameTime.Year;

            return this.Airline.Statistics.getStatisticsValue(year, this.Type);
 
        }
        //returns the change in %
        private double getChange()
        {
            double currentYear = getCurrentYear();
            double lastYear = getLastYear();

            if (lastYear == 0)
                return 1;
            
            double changePercent = System.Convert.ToDouble(currentYear - lastYear) / lastYear;

            if (double.IsInfinity(changePercent))
                return 1;
            if (double.IsNaN(changePercent))
                return 0;

            return changePercent;
            

        }
        
    }
    //the mvvm object for airline finances
    public class AirlineFinanceMVVM
    {
        public Invoice.InvoiceType InvoiceType { get; set; }
        public Airline Airline { get; set; }
        public double CurrentMonth { get { return getCurrentMonthTotal(); } set { ;} }
        public double LastMonth { get { return getLastMonthTotal(); } set { ;} }
        public double YearToDate { get { return getYearToDateTotal(); } set { ;} }
        public AirlineFinanceMVVM(Airline airline, Invoice.InvoiceType type)
        {
            this.InvoiceType = type;
            this.Airline = airline;
        }

        //returns the total amount for the current month
        public double getCurrentMonthTotal()
        {
            DateTime startDate = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, 1);
            return this.Airline.getInvoicesAmount(startDate, GameObject.GetInstance().GameTime, this.InvoiceType);
        }

        //returns the total amount for the last month
        public double getLastMonthTotal()
        {
            DateTime tDate = GameObject.GetInstance().GameTime.AddMonths(-1);
            return this.Airline.getInvoicesAmountMonth(tDate.Year, tDate.Month, this.InvoiceType);
        }

        //returns the total amount for the year to date
        public double getYearToDateTotal()
        {
            return this.Airline.getInvoicesAmountYear(GameObject.GetInstance().GameTime.Year, this.InvoiceType);
        }

    }
    //the facilities for a airline
    public class AirlineClassFacilityMVVM : INotifyPropertyChanged
    {
        public List<RouteFacility> Facilities { get; set; }

        private RouteFacility _selectedFacility;
        public RouteFacility SelectedFacility
        {
            get { return _selectedFacility; }
            set { _selectedFacility = value; NotifyPropertyChanged("SelectedFacility"); }
        }

        public RouteFacility.FacilityType Type { get; set; }
        public AirlineClassFacilityMVVM(RouteFacility.FacilityType type)
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
    //the facility class
    public class AirlineClassMVVM
    {

        public List<AirlineClassFacilityMVVM> Facilities { get; set; }

        public AirlinerClass.ClassType Type { get; set; }
      

        public AirlineClassMVVM(AirlinerClass.ClassType type)
        {
            this.Type = type;

            this.Facilities = new List<AirlineClassFacilityMVVM>();


        }

    }
    //the class for an advertisement object
    public class AirlineAdvertisementMVVM : INotifyPropertyChanged
    {
        public AdvertisementType.AirlineAdvertisementType Type { get; set; }
        public List<AdvertisementType> Types { get; set; }
        private AdvertisementType _selectedType;
        public AdvertisementType SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; NotifyPropertyChanged("SelectedType"); }
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
        public AirlineAdvertisementMVVM(AdvertisementType.AirlineAdvertisementType type)
        {
            this.Type = type;
        }
    }
    //the mvvm class for a rating/score
    public class AirlineScoreMVVM
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public AirlineScoreMVVM(string name, int score)
        {
            this.Name = name;
            this.Score = score;
        }
    }
    //the mvvm class for a loan
    public class LoanMVVM : INotifyPropertyChanged
    {
        public Loan Loan { get; set; }
        private int _monthsLeft;
        public int MonthsLeft
        {
            get { return _monthsLeft; }
            set { _monthsLeft = value; NotifyPropertyChanged("MonthsLeft"); }
        }
        private double _paymentLeft;
        public double PaymentLeft
        {
            get { return _paymentLeft; }
            set { _paymentLeft = value; NotifyPropertyChanged("PaymentLeft"); }
        }
        public LoanMVVM(Loan loan)
        {
            this.Loan = loan;
            this.PaymentLeft = loan.PaymentLeft;
            this.MonthsLeft = loan.MonthsLeft;
        }
        //pay some of the loan
        public void payOnLoan(double amount)
        {
            this.Loan.PaymentLeft -= amount;
            this.PaymentLeft = this.Loan.PaymentLeft;
            this.MonthsLeft = this.Loan.MonthsLeft;
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
    //the mvvm class for a destination
    public class AirlineDestinationMVVM : INotifyPropertyChanged
    {
        public Airport Airport { get; set; }
        private Boolean _isHub;
        public Boolean IsHub
        {
            get { return _isHub; }
            set { _isHub = value; NotifyPropertyChanged("IsHub"); }
           
        }
        public AirlineDestinationMVVM(Airport airport, Boolean isHub)
        {
            this.IsHub = isHub;
            this.Airport = airport;
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
    //the converter for the montly payment of a loan
    public class MonthlyPaymentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double amount = System.Convert.ToDouble(values[0]);
            int lenght = System.Convert.ToInt16(values[1]) * 12;

            return new ValueCurrencyConverter().Convert(MathHelpers.GetMonthlyPayment(amount, GeneralHelpers.GetAirlineLoanRate(GameObject.GetInstance().HumanAirline), lenght) * GameObject.GetInstance().Difficulty.LoanLevel);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter if an airline is the human airline in use
    public class AirlineInuseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Airline airline = (Airline)value;

            if (GameObject.GetInstance().HumanAirline == airline)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
