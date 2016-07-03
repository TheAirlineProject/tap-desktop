using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;

namespace TheAirline.ViewModels.Airline
{
    //the mvvm object for an airline
    public class AirlineMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Alliance _alliance;

        private double _balance;

        private bool _hasalliance;

        private Models.Airlines.Airline.AirlineLicense _license;

        private double _maxLoan;

        private double _maxsubsidiarymoney;

        private double _maxtransferfunds;

        private double _money;

        private int _neededpilots;

        private int _pilotstoretire;

        private int _unassignedpilots;

        #endregion

        #region Constructors and Destructors

        public AirlineMVVM(Models.Airlines.Airline airline)
        {
            Airline = airline;

            Alliance = Airline.Alliances.Count == 0 ? null : Airline.Alliances[0];

            DeliveredFleet = new ObservableCollection<FleetAirliner>();
            foreach (
                FleetAirliner airliner in
                    Airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && a.Airliner.Status == Airliner.StatusTypes.Normal))
            {
                DeliveredFleet.Add(airliner);
            }

            OrderedFleet = Airline.Fleet.FindAll(
                a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);

            OutleasedFleet = new ObservableCollection<FleetAirliner>();

            foreach (
             FleetAirliner airliner in
                 Airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && a.Airliner.Status == Airliner.StatusTypes.Leasing))
            {
                OutleasedFleet.Add(airliner);
            }

            Finances = new ObservableCollection<AirlineFinanceMVVM>();
            LoanRate = GeneralHelpers.GetAirlineLoanRate(Airline);
            FleetStatus = new ObservableCollection<KeyValuePair<string, int>>();
            Loans = new ObservableCollection<LoanMVVM>();
            Pilots = new ObservableCollection<PilotMVVM>();
            Wages = new ObservableCollection<AirlineFeeMVVM>();
            Discounts = new ObservableCollection<AirlineFeeMVVM>();
            Facilities = new ObservableCollection<AirlineFacilityMVVM>();
            TrainingFacilities = new ObservableCollection<AirlineFacilityMVVM>();
            Fees = new ObservableCollection<AirlineFeeMVVM>();
            Chargers = new ObservableCollection<AirlineFeeMVVM>();
            Subsidiaries = new ObservableCollection<SubsidiaryAirline>();
            Insurances = new ObservableCollection<AirlineInsurance>();
            Advertisements = new ObservableCollection<AirlineAdvertisementMVVM>();
            Destinations = new ObservableCollection<AirlineDestinationMVVM>();
            AirlineAirlines = new ObservableCollection<Models.Airlines.Airline>();
            FundsAirlines = new ObservableCollection<Models.Airlines.Airline>();
            Routes = new List<AirlineRouteMVVM>();
            Codeshares = new ObservableCollection<Models.Airlines.Airline>();
            Cooperations = new List<CooperationMVVM>();

            Airline.Routes.ForEach(r => Routes.Add(new AirlineRouteMVVM(r)));
            Airline.Loans.FindAll(l => l.IsActive).ForEach(l => Loans.Add(new LoanMVVM(l, Airline)));
            Airline.Pilots.ForEach(p => Pilots.Add(new PilotMVVM(p)));

            UnassignedPilots = Pilots.Count(p => p.Pilot.Airliner == null);
            PilotsToRetire = Pilots.Count(p => p.Pilot.Profile.Age == Pilot.RetirementAge - 1);

            FeeTypes.GetTypes(FeeType.EFeeType.Wage)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Wages.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));
            FeeTypes.GetTypes(FeeType.EFeeType.Discount)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Discounts.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));

            FeeTypes.GetTypes(FeeType.EFeeType.FoodDrinks)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Chargers.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));

            FeeTypes.GetTypes(FeeType.EFeeType.Fee)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Fees.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));

            Airline.Subsidiaries.ForEach(s => Subsidiaries.Add(s));
            Airline.InsurancePolicies.ForEach(i => Insurances.Add(i));
            Airline.Codeshares.ForEach(
                c => Codeshares.Add(c.Airline1 == Airline ? c.Airline2 : c.Airline1));

            foreach (Airport airport in Airline.Airports)
            {
                foreach (Cooperation cooperation in airport.Cooperations.Where(c => c.Airline == Airline))
                {
                    Cooperations.Add(new CooperationMVVM(airport, cooperation));
                }
            }

            setValues();

            Colors = new List<PropertyInfo>();

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
            {
                Colors.Add(c);
            }

            foreach (
                Airport airport in
                    Airline.Airports.OrderByDescending(a => Airline.Airports[0] == a)
                        .ThenBy(a => a.Profile.Name))
            {
                Destinations.Add(new AirlineDestinationMVVM(airport, airport.HasHub(Airline), Airline.HasRouteTo(airport), Airline.HasAirplaneOnRouteTo(airport)));
            }

            double buyingPrice = Airline.GetValue() * 1000000 * 1.10;
            IsBuyable = !Airline.IsHuman && GameObject.GetInstance().HumanAirline.Money > buyingPrice;

            ActiveQuantity = new List<AirlinerQuantityMVVM>();
            
            var fleet = new List<FleetAirliner>(Airline.Fleet);

            foreach (FleetAirliner airliner in fleet)
            {
                if (airliner.Airliner.BuiltDate > GameObject.GetInstance().GameTime)
                {
                    if (ActiveQuantity.Any(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration))
                    {
                        ActiveQuantity.First(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration).OnOrder++;
                    }
                    else
                    {
                        ActiveQuantity.Add(new AirlinerQuantityMVVM(airliner.Airliner.Type, airliner.Airliner.CabinConfiguration,0, 1));
                    }
                }
                else
                {
                    if (ActiveQuantity.Any(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration))
                    {
                        ActiveQuantity.First(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration).Quantity++;
                    }
                    else
                    {
                        ActiveQuantity.Add(new AirlinerQuantityMVVM(airliner.Airliner.Type, airliner.Airliner.CabinConfiguration, 1,0));
                    }
                }
            }

            ActiveQuantity = ActiveQuantity.OrderBy(a => a.Type.Name).ToList();

            HasAlliance = Alliance != null || Codeshares.Count > 0;

            FillFleetStatusReport();
        }

        private void FillFleetStatusReport()
        {
            FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1038"),
                ActiveQuantity.Sum(aq => aq.OnOrder)));

            FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1042"),
                ActiveQuantity.Sum(aq => aq.Quantity)));

            FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1043"),
                Airline.Fleet.Count(fa => fa.GroundedToDate > GameObject.GetInstance().GameTime)));

            FleetStatus.Add(new KeyValuePair<string, int>(
               Translator.GetInstance().GetString("PageAirlineInfo", "1044"),
               ActiveQuantity.Sum(aq => aq.Quantity) - Airline.Fleet.Count(fa => fa.GroundedToDate > GameObject.GetInstance().GameTime)));
            
            FleetStatus.Add(new KeyValuePair<string, int>(
               Translator.GetInstance().GetString("PageAirlineInfo", "1045"),
               Airline.Fleet.Count(fa => !fa.HasRoute && fa.Airliner.BuiltDate <= GameObject.GetInstance().GameTime)));

        }

        public ObservableCollection<KeyValuePair<string, int>> FleetStatus { get; }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public List<AirlinerQuantityMVVM> ActiveQuantity { get; set; }

        public ObservableCollection<AirlineAdvertisementMVVM> Advertisements { get; set; }

        public Models.Airlines.Airline Airline { get; set; }

        public ObservableCollection<Models.Airlines.Airline> AirlineAirlines { get; set; }

        public Alliance Alliance
        {
            get
            {
                return _alliance;
            }
            set
            {
                _alliance = value;
                NotifyPropertyChanged("Alliance");
            }
        }

        public double Balance
        {
            get
            {
                return _balance;
            }
            set
            {
                _balance = value;
                NotifyPropertyChanged("Balance");
            }
        }

        public int CabinCrew { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Chargers { get; set; }

        public ObservableCollection<Models.Airlines.Airline> Codeshares { get; set; }

        public List<PropertyInfo> Colors { get; set; }

        public List<CooperationMVVM> Cooperations { get; set; }

        public ObservableCollection<FleetAirliner> DeliveredFleet { get; set; }

        public ObservableCollection<AirlineDestinationMVVM> Destinations { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Discounts { get; set; }

        public ObservableCollection<AirlineFacilityMVVM> Facilities { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Fees { get; set; }

        public ObservableCollection<AirlineFinanceMVVM> Finances { get; set; }

        public ObservableCollection<Models.Airlines.Airline> FundsAirlines { get; set; }

        public bool HasAlliance
        {
            get
            {
                return _hasalliance;
            }
            set
            {
                _hasalliance = value;
                NotifyPropertyChanged("HasAlliance");
            }
        }

        public ObservableCollection<AirlineInsurance> Insurances { get; set; }

        public bool IsBuyable { get; set; }

        public Models.Airlines.Airline.AirlineLicense License
        {
            get
            {
                return _license;
            }
            set
            {
                _license = value;
                NotifyPropertyChanged("License");
            }
        }

        public double LoanRate { get; set; }

        public ObservableCollection<LoanMVVM> Loans { get; set; }

        public int MaintenanceCrew { get; set; }

        public double MaxLoan
        {
            get
            {
                return _maxLoan;
            }
            set
            {
                _maxLoan = value;
                NotifyPropertyChanged("MaxLoan");
            }
        }

        public double MaxSubsidiaryMoney
        {
            get
            {
                return _maxsubsidiarymoney;
            }
            set
            {
                _maxsubsidiarymoney = value;
                NotifyPropertyChanged("MaxSubsidiaryMoney");
            }
        }

        public double MaxTransferFunds
        {
            get
            {
                return _maxtransferfunds;
            }
            set
            {
                _maxtransferfunds = value;
                NotifyPropertyChanged("MaxTransferFunds");
            }
        }

        public double Money
        {
            get
            {
                return _money;
            }
            set
            {
                _money = value;
                NotifyPropertyChanged("Money");
            }
        }

        public int NeededPilots
        {
            get
            {
                return _neededpilots;
            }
            set
            {
                _neededpilots = value;
                NotifyPropertyChanged("NeededPilots");
            }
        }
        public ObservableCollection<FleetAirliner> OutleasedFleet { get; set; }

        public List<FleetAirliner> OrderedFleet { get; set; }

        public ObservableCollection<PilotMVVM> Pilots { get; set; }

        public int PilotsToRetire
        {
            get
            {
                return _pilotstoretire;
            }
            set
            {
                _pilotstoretire = value;
                NotifyPropertyChanged("PilotsToRetire");
            }
        }

        public List<AirlineRouteMVVM> Routes { get; set; }

        public ObservableCollection<SubsidiaryAirline> Subsidiaries { get; set; }

        public int SupportCrew { get; set; }

        public ObservableCollection<AirlineFacilityMVVM> TrainingFacilities { get; set; }

        public int UnassignedPilots
        {
            get
            {
                return _unassignedpilots;
            }
            set
            {
                _unassignedpilots = value;
                NotifyPropertyChanged("UnassignedPilots");
            }
        }

        public ObservableCollection<AirlineFeeMVVM> Wages { get; set; }

        #endregion

        //adds an airline insurance

        #region Public Methods and Operators

        public void addAirlineInsurance(AirlineInsurance insurance)
        {
            Insurances.Add(insurance);
            Airline.AddInsurance(insurance);
        }

        public void addCodeshareAgreement(CodeshareAgreement share)
        {
            Codeshares.Add(share.Airline1 == Airline ? share.Airline2 : share.Airline1);
            Airline.AddCodeshareAgreement(share);

            HasAlliance = Alliance != null || Codeshares.Count > 0;
        }

        public void addFacility(AirlineFacilityMVVM facility)
        {
            facility.Type = AirlineFacilityMVVM.MVVMType.Purchased;

            Airline.AddFacility(facility.Facility);

            AirlineHelpers.AddAirlineInvoice(
                Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -facility.Facility.Price);
        }

        public void addLoan(Loan loan)
        {
            Loans.Add(new LoanMVVM(loan, Airline));

            Airline.AddLoan(loan);

            setValues();
        }
        //calls back an airliner for leasing
        public void CallbackAirliner(FleetAirliner airliner)
        {
            airliner.Airliner.Status = Airliner.StatusTypes.Normal;

            DeliveredFleet.Add(airliner);
            OutleasedFleet.Remove(airliner);
        }
        //adds a subsidiary airline
        public void addSubsidiaryAirline(SubsidiaryAirline airline)
        {
            Subsidiaries.Add(airline);

            AirlineHelpers.AddSubsidiaryAirline(
                GameObject.GetInstance().MainAirline,
                airline,
                airline.Money,
                airline.Airports[0]);

            MaxSubsidiaryMoney = Airline.Money / 2;

            AirlineAirlines.Add(airline);

            FundsAirlines.Add(airline);
        }

        //removes a subsidiary airline

        //adds a training facility 
        public void addTrainingFacility(AirlineFacilityMVVM facility)
        {
            facility.Type = AirlineFacilityMVVM.MVVMType.Purchased;

            TrainingFacilities.Remove(facility);

            Facilities.Add(facility);

            Airline.AddFacility(facility.Facility);

            AirlineHelpers.AddAirlineInvoice(
                Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -facility.Facility.Price);
        }

        //removes a training facility

        //removes a fleet airliner
        public void removeAirliner(FleetAirliner airliner)
        {
            DeliveredFleet.Remove(airliner);
            Airline.RemoveAirliner(airliner);
        }

        public void removeFacility(AirlineFacilityMVVM facility)
        {
            Airline.RemoveFacility(facility.Facility);

            facility.Type = AirlineFacilityMVVM.MVVMType.Available;
        }

        //removes a pilot
        public void removePilot(PilotMVVM pilot)
        {
            Pilots.Remove(pilot);
            Airline.RemovePilot(pilot.Pilot);

            UnassignedPilots = Pilots.Count(p => p.Pilot.Airliner == null);
            PilotsToRetire = Pilots.Count(p => p.Pilot.Profile.Age == Pilot.RetirementAge - 1);
            NeededPilots = DeliveredFleet.Sum(f => f.Airliner.Type.CockpitCrew - f.Pilots.Count);
        }

        public void removeSubsidiaryAirline(SubsidiaryAirline airline)
        {
            Subsidiaries.Remove(airline);
            AirlineAirlines.Remove(airline);

            FundsAirlines.Remove(airline);
        }

        public void removeTrainingFacility(AirlineFacilityMVVM facility)
        {
            Airline.RemoveFacility(facility.Facility);

            Facilities.Remove(facility);

            TrainingFacilities.Add(facility);
        }

        public void resetFees()
        {
            Wages.Clear();
            Chargers.Clear();
            Fees.Clear();
            Discounts.Clear();

            FeeTypes.GetTypes(FeeType.EFeeType.Wage)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Wages.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));
            FeeTypes.GetTypes(FeeType.EFeeType.Discount)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Discounts.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));
            FeeTypes.GetTypes(FeeType.EFeeType.Fee)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Fees.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));
            FeeTypes.GetTypes(FeeType.EFeeType.FoodDrinks)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => Chargers.Add(new AirlineFeeMVVM(f, Airline.Fees.GetValue(f))));
        }

        //adds a loan

        //saves the advertisements
        public void saveAdvertisements()
        {
            foreach (AirlineAdvertisementMVVM advertisement in Advertisements)
            {
                AdvertisementType type = advertisement.SelectedType;
                Airline.SetAirlineAdvertisement(type);
            }
        }

        public void saveFees()
        {
            foreach (AirlineFeeMVVM fee in Wages)
            {
                Airline.Fees.SetValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in Fees)
            {
                Airline.Fees.SetValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in Discounts)
            {
                Airline.Fees.SetValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in Chargers)
            {
                Airline.Fees.SetValue(fee.FeeType, fee.Value);
            }
        }

        //sets the values

        //sets the max transfer funds
        public void setMaxTransferFunds(Models.Airlines.Airline airline)
        {
            MaxTransferFunds = airline.Money / 2;
        }

        #endregion

        //adds a codeshare agreement

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //moves an airliner to other airline
        public void moveAirliner(FleetAirliner airliner, Models.Airlines.Airline airline)
        {
            DeliveredFleet.Remove(airliner);

            Airline.RemoveAirliner(airliner);

            airline.AddAirliner(airliner);

            var pilots = new List<Pilot>(airliner.Pilots);

            foreach (Pilot pilot in pilots)
                pilot.Airline = airline;

        }
        private void setValues()
        {
            Finances.Clear();

            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
            {
                Finances.Add(new AirlineFinanceMVVM(Airline, type));
            }

            Money = Airline.Money;
            Balance = Airline.Money - Airline.StartMoney;
            double tMoney = GameObject.GetInstance().HumanMoney;

            CabinCrew =
                Airline.Routes.Where(r => r.Type == Route.RouteType.Passenger)
                    .Sum(r => ((PassengerRoute)r).GetTotalCabinCrew());
            SupportCrew =
                Airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(Airline))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support)
                    .Sum(a => a.NumberOfEmployees);
            MaintenanceCrew =
                Airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(Airline))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                    .Sum(a => a.NumberOfEmployees);

            MaintenanceCrew +=
                Airline.Airports.Where(
                    a => a.GetCurrentAirportFacility(Airline, AirportFacility.FacilityType.Service).TypeLevel > 0)
                    .Sum(a => Airline.Fleet.Count(f => f.Homebase == a));
            NeededPilots = DeliveredFleet.Sum(f => f.Airliner.Type.CockpitCrew - f.Pilots.Count);

            foreach (
                AirlineFacility facility in
                    AirlineFacilities.GetFacilities(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                        .OrderBy(f => f.Name))
            {
                if (Airline.Facilities.Exists(f => f.Uid == facility.Uid) || Airline.IsHuman)
                {
                    if (facility is PilotTrainingFacility && !Airline.Facilities.Exists(f => f.Uid == facility.Uid))
                    {
                        TrainingFacilities.Add(
                            new AirlineFacilityMVVM(
                                Airline,
                                facility,
                                Airline.Facilities.Exists(f => f.Uid == facility.Uid)
                                    ? AirlineFacilityMVVM.MVVMType.Purchased
                                    : AirlineFacilityMVVM.MVVMType.Available));
                    }
                    else
                    {
                        Facilities.Add(
                            new AirlineFacilityMVVM(
                                Airline,
                                facility,
                                Airline.Facilities.Exists(f => f.Uid == facility.Uid)
                                    ? AirlineFacilityMVVM.MVVMType.Purchased
                                    : AirlineFacilityMVVM.MVVMType.Available));
                    }
                }
            }
            foreach (
                AdvertisementType.AirlineAdvertisementType type in
                    Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                {
                    var advertisement = new AirlineAdvertisementMVVM(type) {Types = AdvertisementTypes.GetTypes(type)};


                    Advertisements.Add(advertisement);
                }
            }

            MaxSubsidiaryMoney = Airline.Money / 2;
            MaxTransferFunds = Airline.Money / 2;

            License = Airline.License;

            if (Airline.IsSubsidiary)
            {
                AirlineAirlines.Add(((SubsidiaryAirline)Airline).Airline);

                foreach (SubsidiaryAirline airline in ((SubsidiaryAirline)Airline).Airline.Subsidiaries)
                {
                    AirlineAirlines.Add(airline);
                }
            }
            else
            {
                foreach (SubsidiaryAirline airline in Subsidiaries)
                {
                    AirlineAirlines.Add(airline);
                }

                AirlineAirlines.Add(Airline);
            }

            foreach (Models.Airlines.Airline airline in AirlineAirlines)
            {
                if (airline != GameObject.GetInstance().HumanAirline)
                {
                    FundsAirlines.Add(airline);
                }
            }

            MaxLoan = AirlineHelpers.GetMaxLoanAmount(Airline);
        }

        #endregion
    }

    //the mvvm object for a pilot
    public class PilotMVVM : INotifyPropertyChanged
    {
        #region Fields

        private bool _ontraining;

        #endregion

        #region Constructors and Destructors

        public PilotMVVM(Pilot pilot)
        {
            Pilot = pilot;
            OnTraining = Pilot.OnTraining;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public bool OnTraining
        {
            get
            {
                return _ontraining;
            }
            set
            {
                _ontraining = value;
                NotifyPropertyChanged("OnTraining");
            }
        }

        public Pilot Pilot { get; set; }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the mvvm object for a pilot training facility
    public class PilotTrainingMVVM
    {
        #region Constructors and Destructors

        public PilotTrainingMVVM(string family, int traningdays, double price)
        {
            Family = family;
            TrainingDays = traningdays;
            Price = price;
        }

        #endregion

        #region Public Properties

        public string Family { get; set; }

        public double Price { get; set; }

        public int TrainingDays { get; set; }

        #endregion
    }

    //the mvvm object for airline facilities
    public class AirlineFacilityMVVM : INotifyPropertyChanged
    {
        #region Fields

        private MVVMType _type;

        #endregion

        #region Constructors and Destructors

        public AirlineFacilityMVVM(Models.Airlines.Airline airline, AirlineFacility facility, MVVMType type)
        {
            Type = type;
            Airline = airline;
            Facility = facility;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Enums

        public enum MVVMType
        {
            Purchased,

            Available
        }

        #endregion

        #region Public Properties

        public Models.Airlines.Airline Airline { get; set; }

        public AirlineFacility Facility { get; set; }

        public MVVMType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                NotifyPropertyChanged("Type");
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the mvvm object for arline fees
    public class AirlineFeeMVVM
    {
        #region Constructors and Destructors

        public AirlineFeeMVVM(FeeType feeType, double value)
        {
            FeeType = feeType;
            Value = value;

            Frequency = FeeType.MaxValue - FeeType.MinValue < 4 ? 0.05 : 0.25;
        }

        #endregion

        #region Public Properties

        public FeeType FeeType { get; set; }

        public double Frequency { get; set; }

        public double Value { get; set; }

        #endregion
    }

    //the mvvm object for airline statistics
    public class AirlineStatisticsMVVM
    {
        #region Constructors and Destructors

        public AirlineStatisticsMVVM(Models.Airlines.Airline airline, StatisticsType type)
        {
            Type = type;
            Airline = airline;
        }

        #endregion

        #region Public Properties

        public Models.Airlines.Airline Airline { get; set; }

        public double Change => getChange();

        public double CurrentYear => getCurrentYear();

        public double LastYear => getLastYear();

        public StatisticsType Type { get; set; }

        #endregion

        //returns the value for the last year

        //returns the change in %

        #region Methods

        private double getChange()
        {
            double currentYear = getCurrentYear();
            double lastYear = getLastYear();

            if (lastYear == 0)
            {
                return 1;
            }

            double changePercent = Convert.ToDouble(currentYear - lastYear) / lastYear;

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

            return Airline.Statistics.GetStatisticsValue(year, Type);
        }

        private double getLastYear()
        {
            int year = GameObject.GetInstance().GameTime.Year - 1;

            return Airline.Statistics.GetStatisticsValue(year, Type);
        }

        #endregion
    }

    //the mvvm object for airline finances
    public class AirlineFinanceMVVM
    {
        #region Constructors and Destructors

        public AirlineFinanceMVVM(Models.Airlines.Airline airline, Invoice.InvoiceType type)
        {
            InvoiceType = type;
            Airline = airline;
        }

        #endregion

        #region Public Properties

        public Models.Airlines.Airline Airline { get; set; }

        public double CurrentMonth => getCurrentMonthTotal();

        public Invoice.InvoiceType InvoiceType { get; set; }

        public double LastMonth => getLastMonthTotal();

        public double YearToDate => getYearToDateTotal();

        #endregion

        //returns the total amount for the current month

        #region Public Methods and Operators

        public double getCurrentMonthTotal()
        {
            var startDate = new DateTime(
                GameObject.GetInstance().GameTime.Year,
                GameObject.GetInstance().GameTime.Month,
                1);
            return Airline.GetInvoicesAmount(startDate, GameObject.GetInstance().GameTime, InvoiceType);
        }

        //returns the total amount for the last month
        public double getLastMonthTotal()
        {
            DateTime tDate = GameObject.GetInstance().GameTime.AddMonths(-1);
            return Airline.GetInvoicesAmountMonth(tDate.Year, tDate.Month, InvoiceType);
        }

        //returns the total amount for the year to date
        public double getYearToDateTotal()
        {
            return Airline.GetInvoicesAmountYear(GameObject.GetInstance().GameTime.Year, InvoiceType);
        }

        #endregion
    }

    //the facilities for a airline
    public class AirlineClassFacilityMVVM : INotifyPropertyChanged
    {
        #region Fields

        private RouteFacility _selectedFacility;

        #endregion

        #region Constructors and Destructors

        public AirlineClassFacilityMVVM(RouteFacility.FacilityType type)
        {
            Facilities = new ObservableCollection<RouteFacility>();

            Type = type;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<RouteFacility> Facilities { get; set; }

        public RouteFacility SelectedFacility
        {
            get
            {
                return _selectedFacility;
            }
            set
            {
                _selectedFacility = value;
                NotifyPropertyChanged("SelectedFacility");
            }
        }

        public RouteFacility.FacilityType Type { get; set; }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the facility class
    public class AirlineClassMVVM
    {
        #region Constructors and Destructors

        public AirlineClassMVVM(AirlinerClass.ClassType type)
        {
            Type = type;

            Facilities = new List<AirlineClassFacilityMVVM>();
        }

        #endregion

        #region Public Properties

        public List<AirlineClassFacilityMVVM> Facilities { get; set; }

        public AirlinerClass.ClassType Type { get; set; }

        #endregion
    }

    //the class for an advertisement object
    public class AirlineAdvertisementMVVM : INotifyPropertyChanged
    {
        #region Fields

        private AdvertisementType _selectedType;

        #endregion

        #region Constructors and Destructors

        public AirlineAdvertisementMVVM(AdvertisementType.AirlineAdvertisementType type)
        {
            Type = type;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public AdvertisementType SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                _selectedType = value;
                NotifyPropertyChanged("SelectedType");
            }
        }

        public AdvertisementType.AirlineAdvertisementType Type { get; set; }

        public List<AdvertisementType> Types { get; set; }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the mvvm class for a rating/score
    public class AirlineScoreMVVM
    {
        #region Constructors and Destructors

        public AirlineScoreMVVM(string name, int score)
        {
            Name = name;
            Score = score;
        }

        #endregion

        #region Public Properties

        public string Name { get; set; }

        public int Score { get; set; }

        #endregion
    }

    //the mvvm class for a loan
    public class LoanMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _monthsLeft;

        private double _paymentLeft;

        #endregion

        #region Constructors and Destructors

        public LoanMVVM(Loan loan, Models.Airlines.Airline airline)
        {
            Loan = loan;
            PaymentLeft = loan.PaymentLeft;
            MonthsLeft = loan.MonthsLeft;
            Airline = airline;
        }

        #endregion

        //pay some of the loan

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Models.Airlines.Airline Airline { get; set; }

        public Loan Loan { get; set; }

        public int MonthsLeft
        {
            get
            {
                return _monthsLeft;
            }
            set
            {
                _monthsLeft = value;
                NotifyPropertyChanged("MonthsLeft");
            }
        }

        public double PaymentLeft
        {
            get
            {
                return _paymentLeft;
            }
            set
            {
                _paymentLeft = value;
                NotifyPropertyChanged("PaymentLeft");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void payOnLoan(double amount)
        {
            Loan.PaymentLeft -= amount;
            PaymentLeft = Loan.PaymentLeft;
            MonthsLeft = Loan.MonthsLeft;
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the mvvm class for a cooperation
    public class CooperationMVVM
    {
        #region Constructors and Destructors

        public CooperationMVVM(Airport airport, Cooperation cooperation)
        {
            Airport = airport;
            Cooperation = cooperation;
        }

        #endregion

        #region Public Properties

        public Airport Airport { get; set; }

        public Cooperation Cooperation { get; set; }

        #endregion
    }

    //the mvvm class for an airline route
    public class AirlineRouteMVVM
    {
        #region Constructors and Destructors

        public AirlineRouteMVVM(Route route)
        {
            Route = route;

            if (Route.Type == Route.RouteType.Passenger)
            {
                PriceIndex =
                    ((PassengerRoute)Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass).FarePrice;
            }
            else if (Route.Type == Route.RouteType.Cargo)
            {
                PriceIndex = ((CargoRoute)Route).PricePerUnit;
            }
            else if (Route.Type == Route.RouteType.Mixed)
            {
                PriceIndex =
                    ((CombiRoute)Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass).FarePrice
                    + ((CombiRoute)Route).PricePerUnit;
            }
            else if (Route.Type == Route.RouteType.Helicopter)
            {
                PriceIndex =
                ((HelicopterRoute)Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass).FarePrice;

            }
        }

        #endregion

        #region Public Properties

        public double PriceIndex { get; set; }

        public Route Route { get; set; }

        #endregion
    }

    //the mvvm class for a destination
    public class AirlineDestinationMVVM : INotifyPropertyChanged
    {
        #region Fields

        private bool _isHub;

        #endregion

        #region Constructors and Destructors

        public AirlineDestinationMVVM(Airport airport, bool isHub, bool hasRoutes, bool hasAirplaneOnRouteTo)
        {
            IsHub = isHub;
            HasRoutes = hasRoutes;
            HasAirplaneOnRouteTo = hasAirplaneOnRouteTo;
            Airport = airport;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airport Airport { get; set; }

        public bool IsHub
        {
            get
            {
                return _isHub;
            }
            set
            {
                _isHub = value;
                NotifyPropertyChanged("IsHub");
            }
        }

        public bool HasRoutes { get; set; }

        public bool HasAirplaneOnRouteTo { get; set; }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //the mvvm object for the airliner quantity
    public class AirlinerQuantityMVVM
    {
        #region Constructors and Destructors

        public AirlinerQuantityMVVM(AirlinerType type, string cabinConfig, int quantity, int onOrder)
        {
            Quantity = quantity;
            OnOrder = onOrder;
            Type = type;
            CabinConfiguration = cabinConfig;
        }

        #endregion

        #region Public Properties

        public int OnOrder { get; set; }

        public int Quantity { get; set; }

        public AirlinerType Type { get; set; }

        public string CabinConfiguration { get; set; }

        #endregion
    }
    //the converter if an airliner can be moved to a subsidiary
    public class AirlinerToSubsidiaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FleetAirliner airliner = (FleetAirliner)value;

                bool hasSubsidiary = airliner.Airliner.Airline.Subsidiaries.Count > 0 || airliner.Airliner.Airline.IsSubsidiary;

                return airliner.Status == FleetAirliner.AirlinerStatus.Stopped && airliner.Airliner.Airline == GameObject.GetInstance().HumanAirline && hasSubsidiary;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter if an airliner can be called back from outleasing
    public class AirlinerCallBackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FleetAirliner airliner = (FleetAirliner)value;

                return airliner.Airliner.Airline == GameObject.GetInstance().HumanAirline && airliner.Airliner.Airline == airliner.Airliner.Owner;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the montly payment of a loan
    public class MonthlyPaymentConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double amount = System.Convert.ToDouble(values[0]);
            int lenght = System.Convert.ToInt16(values[1]) * 12;

            return
                new ValueCurrencyConverter().Convert(
                    MathHelpers.GetMonthlyPayment(
                        amount,
                        GeneralHelpers.GetAirlineLoanRate(GameObject.GetInstance().HumanAirline),
                        lenght) * GameObject.GetInstance().Difficulty.LoanLevel);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter if an airline is the human airline in use
    public class AirlineInuseConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var airline = (Models.Airlines.Airline)value;

            if (GameObject.GetInstance().HumanAirline == airline)
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