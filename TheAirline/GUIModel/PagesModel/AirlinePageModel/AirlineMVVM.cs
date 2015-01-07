namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlineModel.AirlineCooperationModel;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.InvoicesModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;
    using TheAirline.Model.PilotModel;

    //the mvvm object for an airline
    public class AirlineMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Alliance _alliance;

        private double _balance;

        private Boolean _hasalliance;

        private Airline.AirlineLicense _license;

        private double _maxLoan;

        private double _maxsubsidiarymoney;

        private double _maxtransferfunds;

        private double _money;

        private int _neededpilots;

        private int _pilotstoretire;

        private int _unassignedpilots;

        #endregion

        #region Constructors and Destructors

        public AirlineMVVM(Airline airline)
        {
            this.Airline = airline;

            this.Alliance = this.Airline.Alliances.Count == 0 ? null : this.Airline.Alliances[0];

            this.DeliveredFleet = new ObservableCollection<FleetAirliner>();
            foreach (
                FleetAirliner airliner in
                    this.Airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && a.Airliner.Airline == this.Airline && a.Airliner.Status == Airliner.StatusTypes.Normal))
            {
                this.DeliveredFleet.Add(airliner);
            }

            this.OrderedFleet = new ObservableCollection<FleetAirliner>();
            this.Airline.Fleet.FindAll(
                a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime).ForEach(f=>this.OrderedFleet.Add(f));

            this.OutleasedFleet = new ObservableCollection<FleetAirliner>();

            foreach (
             FleetAirliner airliner in
                 this.Airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && (a.Airliner.Status == Airliner.StatusTypes.Leasing || (a.Airliner.Owner != a.Airliner.Airline && a.Airliner.Owner == this.Airline))))
            {
                this.OutleasedFleet.Add(airliner);
            }

            this.Finances = new ObservableCollection<AirlineFinanceMVVM>();
            this.LoanRate = GeneralHelpers.GetAirlineLoanRate(this.Airline);
            this.FleetStatus = new ObservableCollection<KeyValuePair<string, int>>();
            this.Loans = new ObservableCollection<LoanMVVM>();
            this.Pilots = new ObservableCollection<PilotMVVM>();
            this.Wages = new ObservableCollection<AirlineFeeMVVM>();
            this.Discounts = new ObservableCollection<AirlineFeeMVVM>();
            this.Facilities = new ObservableCollection<AirlineFacilityMVVM>();
            this.TrainingFacilities = new ObservableCollection<AirlineFacilityMVVM>();
            this.Fees = new ObservableCollection<AirlineFeeMVVM>();
            this.Chargers = new ObservableCollection<AirlineFeeMVVM>();
            this.Subsidiaries = new ObservableCollection<SubsidiaryAirline>();
            this.Insurances = new ObservableCollection<AirlineInsurance>();
            this.Advertisements = new ObservableCollection<AirlineAdvertisementMVVM>();
            this.Destinations = new ObservableCollection<AirlineDestinationMVVM>();
            this.AirlineAirlines = new ObservableCollection<Airline>();
            this.FundsAirlines = new ObservableCollection<Airline>();
            this.Routes = new ObservableCollection<AirlineRouteMVVM>();
            this.Codeshares = new ObservableCollection<Airline>();
            this.Cooperations = new ObservableCollection<CooperationMVVM>();
            this.MaintenanceCenters = new ObservableCollection<MaintenanceCenter>();
            this.Maintenances = new List<AirlineMaintenanceMVVM>();

            this.Airline.Routes.ForEach(r => this.Routes.Add(new AirlineRouteMVVM(r)));
            this.Airline.Loans.FindAll(l => l.IsActive).ForEach(l => this.Loans.Add(new LoanMVVM(l, this.Airline)));
            this.Airline.Pilots.ForEach(p => this.Pilots.Add(new PilotMVVM(p)));

            this.UnassignedPilots = this.Pilots.Count(p => p.Pilot.Airliner == null);
            this.PilotsToRetire = this.Pilots.Count(p => p.Pilot.Profile.Age == Pilot.RetirementAge - 1);
            
            FeeTypes.GetTypes(FeeType.eFeeType.Wage)
                .FindAll(f => f!= null && f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Wages.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f)))); 

            FeeTypes.GetTypes(FeeType.eFeeType.Discount)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Discounts.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));

            FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Chargers.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));

            FeeTypes.GetTypes(FeeType.eFeeType.Fee)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Fees.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));

            this.Airline.Subsidiaries.ForEach(s => this.Subsidiaries.Add(s));
            this.Airline.InsurancePolicies.ForEach(i => this.Insurances.Add(i));
            this.Airline.Codeshares.ForEach(
                c => this.Codeshares.Add(c.Airline1 == this.Airline ? c.Airline2 : c.Airline1));

            foreach (Airport airport in this.Airline.Airports)
            {
                foreach (Cooperation cooperation in airport.Cooperations.Where(c => c.Airline == this.Airline))
                {
                    this.Cooperations.Add(new CooperationMVVM(airport, cooperation));
                }
            }

            if (airline.MaintenanceCenters != null)
            {

                foreach (MaintenanceCenter center in airline.MaintenanceCenters)
                    this.MaintenanceCenters.Add(center);
            }

            foreach (AirlinerMaintenanceType maintenanceType in AirlinerMaintenanceTypes.GetMaintenanceTypes())
            {
                MaintenanceCenterMVVM selected;

                List<MaintenanceCenterMVVM> centers = new List<MaintenanceCenterMVVM>();

                foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel >= maintenanceType.Requirement.TypeLevel))
                    centers.Add(new MaintenanceCenterMVVM(airport));

                foreach (MaintenanceCenter mCenter in GameObject.GetInstance().HumanAirline.MaintenanceCenters)
                {
                    centers.Add(new MaintenanceCenterMVVM(mCenter));
                }
                
                if (this.Airline.Maintenances.ContainsKey(maintenanceType))
                {
                    if (this.Airline.Maintenances[maintenanceType].Airport == null)
                        selected = centers.Find(c=>c.Center == this.Airline.Maintenances[maintenanceType].Center);
                    else
                        selected = centers.Find(c => c.Airport == this.Airline.Maintenances[maintenanceType].Airport);
                }
                else
                    selected = null;

                this.Maintenances.Add(new AirlineMaintenanceMVVM(maintenanceType, selected, centers));
            }

            this.setValues();

            this.Colors = new List<PropertyInfo>();

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
            {
                this.Colors.Add(c);
            }

            foreach (
                Airport airport in
                    this.Airline.Airports.OrderByDescending(a => this.Airline.Airports[0] == a)
                        .ThenBy(a => a.Profile.Name))
            {
                this.Destinations.Add(new AirlineDestinationMVVM(airport, airport.hasHub(this.Airline), this.Airline.hasRouteTo(airport), this.Airline.hasAirplaneOnRouteTo(airport)));
            }

            double buyingPrice = this.Airline.getValue() * 1000000 * 1.10;
            this.IsBuyable = !this.Airline.IsHuman && GameObject.GetInstance().HumanAirline.Money > buyingPrice;

            this.ActiveQuantity = new ObservableCollection<AirlinerQuantityMVVM>();
        
            var fleet = new List<FleetAirliner>(this.Airline.Fleet);

            foreach (FleetAirliner airliner in fleet)
            {
                if (airliner.Airliner.BuiltDate > GameObject.GetInstance().GameTime)
                {
                    if (this.ActiveQuantity.Any(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration))
                    {
                        this.ActiveQuantity.First(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration).OnOrder++;
                    }
                    else
                    {
                        this.ActiveQuantity.Add(new AirlinerQuantityMVVM(airliner.Airliner.Type, airliner.Airliner.CabinConfiguration, 0, 1));
                    }
                }
                else
                {
                    if (this.ActiveQuantity.Any(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration))
                    {
                        this.ActiveQuantity.First(o => o.Type.Name == airliner.Airliner.Type.Name && o.CabinConfiguration == airliner.Airliner.CabinConfiguration).Quantity++;
                    }
                    else
                    {
                        this.ActiveQuantity.Add(new AirlinerQuantityMVVM(airliner.Airliner.Type, airliner.Airliner.CabinConfiguration, 1, 0));
                    }
                }
            }

            this.ActiveQuantity = new ObservableCollection<AirlinerQuantityMVVM>(this.ActiveQuantity.OrderBy(a=>a.Type.Name).ToList());
          
            this.HasAlliance = this.Alliance != null || this.Codeshares.Count > 0;

            FillFleetStatusReport();
        }

        private void FillFleetStatusReport()
        {
            this.FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1038"),
                ActiveQuantity.Sum(aq => aq.OnOrder)));

            this.FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1042"),
                ActiveQuantity.Sum(aq => aq.Quantity)));

            this.FleetStatus.Add(new KeyValuePair<string, int>(
                Translator.GetInstance().GetString("PageAirlineInfo", "1043"),
                this.Airline.Fleet.Count(fa => fa.GroundedToDate > GameObject.GetInstance().GameTime)));

            this.FleetStatus.Add(new KeyValuePair<string, int>(
               Translator.GetInstance().GetString("PageAirlineInfo", "1044"),
               ActiveQuantity.Sum(aq => aq.Quantity) - this.Airline.Fleet.Count(fa => fa.GroundedToDate > GameObject.GetInstance().GameTime)));

            this.FleetStatus.Add(new KeyValuePair<string, int>(
               Translator.GetInstance().GetString("PageAirlineInfo", "1045"),
               this.Airline.Fleet.Count(fa => !fa.HasRoute && fa.Airliner.BuiltDate <= GameObject.GetInstance().GameTime)));

        }

        public ObservableCollection<KeyValuePair<string, int>> FleetStatus { get; private set; }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerQuantityMVVM> ActiveQuantity { get; set; }

        public ObservableCollection<AirlineAdvertisementMVVM> Advertisements { get; set; }

        public Airline Airline { get; set; }

        public ObservableCollection<Airline> AirlineAirlines { get; set; }

        public Alliance Alliance
        {
            get
            {
                return this._alliance;
            }
            set
            {
                this._alliance = value;
                this.NotifyPropertyChanged("Alliance");
            }
        }

        public double Balance
        {
            get
            {
                return this._balance;
            }
            set
            {
                this._balance = value;
                this.NotifyPropertyChanged("Balance");
            }
        }

        public int CabinCrew { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Chargers { get; set; }

        public ObservableCollection<Airline> Codeshares { get; set; }

        public List<PropertyInfo> Colors { get; set; }

        public ObservableCollection<CooperationMVVM> Cooperations { get; set; }

        public ObservableCollection<FleetAirliner> DeliveredFleet { get; set; }

        public ObservableCollection<AirlineDestinationMVVM> Destinations { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Discounts { get; set; }

        public ObservableCollection<AirlineFacilityMVVM> Facilities { get; set; }

        public ObservableCollection<AirlineFeeMVVM> Fees { get; set; }

        public ObservableCollection<AirlineFinanceMVVM> Finances { get; set; }

        public ObservableCollection<Airline> FundsAirlines { get; set; }

        public Boolean HasAlliance
        {
            get
            {
                return this._hasalliance;
            }
            set
            {
                this._hasalliance = value;
                this.NotifyPropertyChanged("HasAlliance");
            }
        }

        public ObservableCollection<AirlineInsurance> Insurances { get; set; }

        public Boolean IsBuyable { get; set; }

        public Airline.AirlineLicense License
        {
            get
            {
                return this._license;
            }
            set
            {
                this._license = value;
                this.NotifyPropertyChanged("License");
            }
        }

        public double LoanRate { get; set; }

        public ObservableCollection<LoanMVVM> Loans { get; set; }

        public int MaintenanceCrew { get; set; }

        public double MaxLoan
        {
            get
            {
                return this._maxLoan;
            }
            set
            {
                this._maxLoan = value;
                this.NotifyPropertyChanged("MaxLoan");
            }
        }

        public double MaxSubsidiaryMoney
        {
            get
            {
                return this._maxsubsidiarymoney;
            }
            set
            {
                this._maxsubsidiarymoney = value;
                this.NotifyPropertyChanged("MaxSubsidiaryMoney");
            }
        }

        public double MaxTransferFunds
        {
            get
            {
                return this._maxtransferfunds;
            }
            set
            {
                this._maxtransferfunds = value;
                this.NotifyPropertyChanged("MaxTransferFunds");
            }
        }

        public double Money
        {
            get
            {
                return this._money;
            }
            set
            {
                this._money = value;
                this.NotifyPropertyChanged("Money");
            }
        }

        public int NeededPilots
        {
            get
            {
                return this._neededpilots;
            }
            set
            {
                this._neededpilots = value;
                this.NotifyPropertyChanged("NeededPilots");
            }
        }
        public ObservableCollection<FleetAirliner> OutleasedFleet { get; set; }

        public ObservableCollection<FleetAirliner> OrderedFleet { get; set; }

        public ObservableCollection<PilotMVVM> Pilots { get; set; }

        public List<AirlineMaintenanceMVVM> Maintenances { get; set; }

        public ObservableCollection<MaintenanceCenter> MaintenanceCenters { get; set; }

        public int PilotsToRetire
        {
            get
            {
                return this._pilotstoretire;
            }
            set
            {
                this._pilotstoretire = value;
                this.NotifyPropertyChanged("PilotsToRetire");
            }
        }

        public ObservableCollection<AirlineRouteMVVM> Routes { get; set; }

        public ObservableCollection<SubsidiaryAirline> Subsidiaries { get; set; }

        public int SupportCrew { get; set; }

        public ObservableCollection<AirlineFacilityMVVM> TrainingFacilities { get; set; }

        public int UnassignedPilots
        {
            get
            {
                return this._unassignedpilots;
            }
            set
            {
                this._unassignedpilots = value;
                this.NotifyPropertyChanged("UnassignedPilots");
            }
        }

        public ObservableCollection<AirlineFeeMVVM> Wages { get; set; }

        #endregion

        //adds an airline insurance

        #region Public Methods and Operators

        public void addAirlineInsurance(AirlineInsurance insurance)
        {
            this.Insurances.Add(insurance);
            this.Airline.addInsurance(insurance);
        }

        public void addCodeshareAgreement(CodeshareAgreement share)
        {
            this.Codeshares.Add(share.Airline1 == this.Airline ? share.Airline2 : share.Airline1);
            this.Airline.addCodeshareAgreement(share);

            this.HasAlliance = this.Alliance != null || this.Codeshares.Count > 0;
        }
        public void addMaintenanceCenter(MaintenanceCenter center)
        {
            this.MaintenanceCenters.Add(center);
            this.Airline.MaintenanceCenters.Add(center);

            foreach (AirlineMaintenanceMVVM maintenance in this.Maintenances)
            {
                maintenance.Centers.Add(new MaintenanceCenterMVVM(center));
            }



        }
        public void removeMaintenanceCenter(MaintenanceCenter center)
        {
            this.MaintenanceCenters.Remove(center);
            this.Airline.MaintenanceCenters.Remove(center);

            foreach (AirlineMaintenanceMVVM maintenance in this.Maintenances)
            {
                MaintenanceCenterMVVM mCenter = maintenance.Centers.First(m=>m.Center == center);

                maintenance.Centers.Remove(mCenter);
            }
           
        }
        public void addFacility(AirlineFacilityMVVM facility)
        {
            facility.Type = AirlineFacilityMVVM.MVVMType.Purchased;

            this.Airline.addFacility(facility.Facility);

            AirlineHelpers.AddAirlineInvoice(
                this.Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -facility.Facility.Price);
        }

        public void addLoan(Loan loan)
        {
            this.Loans.Add(new LoanMVVM(loan, this.Airline));

            this.Airline.addLoan(loan);

            this.setValues();
        }
        //calls back an airliner for leasing
        public void CallbackAirliner(FleetAirliner airliner)
        {
            airliner.Airliner.Status = Airliner.StatusTypes.Normal;

            this.DeliveredFleet.Add(airliner);
            this.OutleasedFleet.Remove(airliner);

        }
        //adds a subsidiary airline
        public void addSubsidiaryAirline(SubsidiaryAirline airline)
        {
            this.Subsidiaries.Add(airline);

            AirlineHelpers.AddSubsidiaryAirline(
                GameObject.GetInstance().MainAirline,
                airline,
                airline.Money,
                airline.Airports[0]);

            this.MaxSubsidiaryMoney = this.Airline.Money / 2;

            this.AirlineAirlines.Add(airline);

            this.FundsAirlines.Add(airline);
        }

        //removes a subsidiary airline

        //adds a training facility 
        public void addTrainingFacility(AirlineFacilityMVVM facility)
        {
            facility.Type = AirlineFacilityMVVM.MVVMType.Purchased;

            this.TrainingFacilities.Remove(facility);

            this.Facilities.Add(facility);

            this.Airline.addFacility(facility.Facility);

            AirlineHelpers.AddAirlineInvoice(
                this.Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -facility.Facility.Price);
        }

        //removes a training facility

        //removes a fleet airliner
        public void removeAirliner(FleetAirliner airliner)
        {
            this.DeliveredFleet.Remove(airliner);
            this.Airline.removeAirliner(airliner);
        }

        public void removeFacility(AirlineFacilityMVVM facility)
        {
            this.Airline.removeFacility(facility.Facility);

            facility.Type = AirlineFacilityMVVM.MVVMType.Available;
        }

        //removes a pilot
        public void removePilot(PilotMVVM pilot)
        {
            this.Pilots.Remove(pilot);
            this.Airline.removePilot(pilot.Pilot);

            this.UnassignedPilots = this.Pilots.Count(p => p.Pilot.Airliner == null);
            this.PilotsToRetire = this.Pilots.Count(p => p.Pilot.Profile.Age == Pilot.RetirementAge - 1);
            this.NeededPilots = this.DeliveredFleet.Sum(f => f.Airliner.Type.CockpitCrew - f.Pilots.Count);
        }

        public void removeSubsidiaryAirline(SubsidiaryAirline airline)
        {
            this.Subsidiaries.Remove(airline);
            this.AirlineAirlines.Remove(airline);

            this.FundsAirlines.Remove(airline);
        }

        public void removeTrainingFacility(AirlineFacilityMVVM facility)
        {
            this.Airline.removeFacility(facility.Facility);

            this.Facilities.Remove(facility);

            this.TrainingFacilities.Add(facility);
        }

        public void resetFees()
        {
            this.Wages.Clear();
            this.Chargers.Clear();
            this.Fees.Clear();
            this.Discounts.Clear();

            FeeTypes.GetTypes(FeeType.eFeeType.Wage)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Wages.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.Discount)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Discounts.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.Fee)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Fees.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
            FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks)
                .FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                .ForEach(f => this.Chargers.Add(new AirlineFeeMVVM(f, this.Airline.Fees.getValue(f))));
        }

        //adds a loan

        //saves the advertisements
        public void saveAdvertisements()
        {
            foreach (AirlineAdvertisementMVVM advertisement in this.Advertisements)
            {
                AdvertisementType type = advertisement.SelectedType;
                this.Airline.setAirlineAdvertisement(type);
            }
        }

        public void saveFees()
        {
            foreach (AirlineFeeMVVM fee in this.Wages)
            {
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in this.Fees)
            {
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in this.Discounts)
            {
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);
            }

            foreach (AirlineFeeMVVM fee in this.Chargers)
            {
                this.Airline.Fees.setValue(fee.FeeType, fee.Value);
            }
        }

        //sets the values

        //sets the max transfer funds
        public void setMaxTransferFunds(Airline airline)
        {
            this.MaxTransferFunds = airline.Money / 2;
        }
        //sets the maintenance
        public void setMaintenance()
        {
            foreach (AirlineMaintenanceMVVM maintenance in this.Maintenances)
            {
                if (maintenance.SelectedType != null)
                {
                    AirlinerMaintenanceCenter center = new AirlinerMaintenanceCenter(maintenance.Type);

                    if (maintenance.SelectedType.Airport != null)
                        center.Airport = maintenance.SelectedType.Airport;
                    else
                        center.Center = maintenance.SelectedType.Center;

                    if (this.Airline.Maintenances.ContainsKey(maintenance.Type))
                    {
                        if (maintenance.SelectedType.Airport == null)
                        {
                            this.Airline.Maintenances[maintenance.Type].Center = maintenance.SelectedType.Center;
                            this.Airline.Maintenances[maintenance.Type].Airport = null;
                        }
                        else
                        {
                            this.Airline.Maintenances[maintenance.Type].Center = null;
                            this.Airline.Maintenances[maintenance.Type].Airport = maintenance.SelectedType.Airport;
                        }
                    }
                    else
                        this.Airline.Maintenances.Add(maintenance.Type, center);

                }
            }
        }
        #endregion

        //adds a codeshare agreement

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //moves an airliner to other airline
        public void moveAirliner(FleetAirliner airliner, Airline airline)
        {
            this.DeliveredFleet.Remove(airliner);

            this.Airline.removeAirliner(airliner);

            airline.addAirliner(airliner);

            airliner.Airliner.Airline = airline;

            var pilots = new List<Pilot>(airliner.Pilots);

            foreach (Pilot pilot in pilots)
                pilot.Airline = airline;

        }
        private void setValues()
        {
            this.Finances.Clear();

            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
            {
                this.Finances.Add(new AirlineFinanceMVVM(this.Airline, type));
            }
    
            this.Money = this.Airline.Money;
            this.Balance = this.Airline.Money - this.Airline.StartMoney;
            double tMoney = GameObject.GetInstance().HumanMoney;

            this.CabinCrew =
                this.Airline.Routes.Where(r => r is PassengerRoute)
                    .Sum(r => ((PassengerRoute)r).getTotalCabinCrew());
            this.SupportCrew =
                this.Airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(this.Airline))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support)
                    .Sum(a => a.NumberOfEmployees);
            this.MaintenanceCrew =
                this.Airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(this.Airline))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                    .Sum(a => a.NumberOfEmployees);

            this.MaintenanceCrew +=
                this.Airline.Airports.Where(
                    a => a.getCurrentAirportFacility(this.Airline, AirportFacility.FacilityType.Service).TypeLevel > 0)
                    .Sum(a => this.Airline.Fleet.Count(f => f.Homebase == a));
            this.NeededPilots = this.DeliveredFleet.Sum(f => f.Airliner.Type.CockpitCrew - f.Pilots.Count);

            foreach (
                AirlineFacility facility in
                    AirlineFacilities.GetFacilities(f => f.FromYear <= GameObject.GetInstance().GameTime.Year)
                        .OrderBy(f => f.Name))
            {
                if (this.Airline.Facilities.Exists(f => f.Uid == facility.Uid) || this.Airline.IsHuman)
                {
                    if (facility is PilotTrainingFacility && !this.Airline.Facilities.Exists(f => f.Uid == facility.Uid))
                    {
                        this.TrainingFacilities.Add(
                            new AirlineFacilityMVVM(
                                this.Airline,
                                facility,
                                this.Airline.Facilities.Exists(f => f.Uid == facility.Uid)
                                    ? AirlineFacilityMVVM.MVVMType.Purchased
                                    : AirlineFacilityMVVM.MVVMType.Available));
                    }
                    else
                    {
                        this.Facilities.Add(
                            new AirlineFacilityMVVM(
                                this.Airline,
                                facility,
                                this.Airline.Facilities.Exists(f => f.Uid == facility.Uid)
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
                    var advertisement = new AirlineAdvertisementMVVM(type);

                    advertisement.Types = new ObservableCollection<AdvertisementType>();
                    AdvertisementTypes.GetTypes(type).ForEach(t=>advertisement.Types.Add(t));

                    this.Advertisements.Add(advertisement);
                }
            }

            this.MaxSubsidiaryMoney = this.Airline.Money / 2;
            this.MaxTransferFunds = this.Airline.Money / 2;

            this.License = this.Airline.License;

            if (this.Airline.IsSubsidiary)
            {
                this.AirlineAirlines.Add(((SubsidiaryAirline)this.Airline).Airline);

                foreach (SubsidiaryAirline airline in ((SubsidiaryAirline)this.Airline).Airline.Subsidiaries)
                {
                    this.AirlineAirlines.Add(airline);
                }
            }
            else
            {
                foreach (SubsidiaryAirline airline in this.Subsidiaries)
                {
                    this.AirlineAirlines.Add(airline);
                }

                this.AirlineAirlines.Add(this.Airline);
            }

            foreach (Airline airline in this.AirlineAirlines)
            {
                if (airline != GameObject.GetInstance().HumanAirline)
                {
                    this.FundsAirlines.Add(airline);
                }
            }

            this.MaxLoan = AirlineHelpers.GetMaxLoanAmount(this.Airline);
        }

        #endregion
    }

    //the mvvm object for a pilot
    public class PilotMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _ontraining;

        #endregion

        #region Constructors and Destructors

        public PilotMVVM(Pilot pilot)
        {
            this.Pilot = pilot;
            this.OnTraining = this.Pilot.OnTraining;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean OnTraining
        {
            get
            {
                return this._ontraining;
            }
            set
            {
                this._ontraining = value;
                this.NotifyPropertyChanged("OnTraining");
            }
        }

        public Pilot Pilot { get; set; }

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

    //the mvvm object for a pilot training facility
    public class PilotTrainingMVVM
    {
        #region Constructors and Destructors

        public PilotTrainingMVVM(string family, int traningdays, double price)
        {
            this.Family = family;
            this.TrainingDays = traningdays;
            this.Price = price;
        }

        #endregion

        #region Public Properties

        public string Family { get; set; }

        public double Price { get; set; }

        public int TrainingDays { get; set; }

        #endregion
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
    //the mvvm object for an airline maintenance
    public class AirlineMaintenanceMVVM
    {
        private MaintenanceCenterMVVM _selectedtype;
        public AirlinerMaintenanceType Type { get; set; }

        public MaintenanceCenterMVVM SelectedType
        {
            get
            {
                return this._selectedtype;
            }
            set
            {
                this._selectedtype = value;
                this.NotifyPropertyChanged("SelectedType");
            }
        }

        public ObservableCollection<MaintenanceCenterMVVM> Centers { get; set; }
        public AirlineMaintenanceMVVM(AirlinerMaintenanceType type, MaintenanceCenterMVVM selected, List<MaintenanceCenterMVVM> centers)
        {
            this.Type = type;
            this.Centers = new ObservableCollection<MaintenanceCenterMVVM>(centers);

            this.SelectedType = selected;


        }
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        #region Methods
        public MaintenanceCenterMVVM getCenterFromList(AirlinerMaintenanceCenter center)
        {
            if (center.Airport == null)
                return this.Centers.FirstOrDefault(c => c.Center == center.Center);
            else
                return this.Centers.FirstOrDefault(c => c.Airport == center.Airport);
        }
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

    //the mvvm object for airline facilities
    public class AirlineFacilityMVVM : INotifyPropertyChanged
    {
        #region Fields

        private MVVMType _type;

        #endregion

        #region Constructors and Destructors

        public AirlineFacilityMVVM(Airline airline, AirlineFacility facility, MVVMType type)
        {
            this.Type = type;
            this.Airline = airline;
            this.Facility = facility;
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

        public Airline Airline { get; set; }

        public AirlineFacility Facility { get; set; }

        public MVVMType Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
                this.NotifyPropertyChanged("Type");
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

    //the mvvm object for arline fees
    public class AirlineFeeMVVM
    {
        #region Constructors and Destructors

        public AirlineFeeMVVM(FeeType feeType, double value)
        {
            this.FeeType = feeType;
            this.Value = value;

            if (this.FeeType.MaxValue - this.FeeType.MinValue < 4)
            {
                this.Frequency = 0.05;
            }
            else
            {
                this.Frequency = 0.25;
            }
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

        public AirlineStatisticsMVVM(Airline airline, StatisticsType type)
        {
            this.Type = type;
            this.Airline = airline;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

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
            double currentYear = this.getCurrentYear();
            double lastYear = this.getLastYear();

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

            return this.Airline.Statistics.getStatisticsValue(year, this.Type);
        }

        private double getLastYear()
        {
            int year = GameObject.GetInstance().GameTime.Year - 1;

            return this.Airline.Statistics.getStatisticsValue(year, this.Type);
        }

        #endregion
    }

    //the mvvm object for airline finances
    public class AirlineFinanceMVVM
    {
        #region Constructors and Destructors

        public AirlineFinanceMVVM(Airline airline, Invoice.InvoiceType type)
        {
            this.InvoiceType = type;
            this.Airline = airline;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public double CurrentMonth
        {
            get
            {
                return this.getCurrentMonthTotal();
            }
            set
            {
                ;
            }
        }

        public Invoice.InvoiceType InvoiceType { get; set; }

        public double LastMonth
        {
            get
            {
                return this.getLastMonthTotal();
            }
            set
            {
                ;
            }
        }

        public double YearToDate
        {
            get
            {
                return this.getYearToDateTotal();
            }
            set
            {
                ;
            }
        }

        #endregion

        //returns the total amount for the current month

        #region Public Methods and Operators

        public double getCurrentMonthTotal()
        {
            var startDate = new DateTime(
                GameObject.GetInstance().GameTime.Year,
                GameObject.GetInstance().GameTime.Month,
                1);
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
            this.Facilities = new ObservableCollection<RouteFacility>();

            this.Type = type;
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
                return this._selectedFacility;
            }
            set
            {
                this._selectedFacility = value;
                this.NotifyPropertyChanged("SelectedFacility");
            }
        }

        public RouteFacility.FacilityType Type { get; set; }

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

    //the facility class
    public class AirlineClassMVVM
    {
        #region Constructors and Destructors

        public AirlineClassMVVM(AirlinerClass.ClassType type)
        {
            this.Type = type;

            this.Facilities = new ObservableCollection<AirlineClassFacilityMVVM>();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlineClassFacilityMVVM> Facilities { get; set; }

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
            this.Type = type;
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
                return this._selectedType;
            }
            set
            {
                this._selectedType = value;
                this.NotifyPropertyChanged("SelectedType");
            }
        }

        public AdvertisementType.AirlineAdvertisementType Type { get; set; }

        public ObservableCollection<AdvertisementType> Types { get; set; }

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

    //the mvvm class for a rating/score
    public class AirlineScoreMVVM : INotifyPropertyChanged
    {
        #region Private properties
        private string _name;
        private int _score;
        #endregion
        
        #region Constructors and Destructors

        public AirlineScoreMVVM(string name, int score)
        {
            this.Name = name;
            this.Score = score;
        }

        #endregion

        #region Public Properties

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

       public int Score
        {
            get
            {
                return this._score;
            }
            set
            {
                this._score = value;
                this.NotifyPropertyChanged("Score");
            }
        }

        #endregion
    
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

}
  
    //the mvvm class for a loan
    public class LoanMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _monthsLeft;

        private double _paymentLeft;

        #endregion

        #region Constructors and Destructors

        public LoanMVVM(Loan loan, Airline airline)
        {
            this.Loan = loan;
            this.PaymentLeft = loan.PaymentLeft;
            this.MonthsLeft = loan.MonthsLeft;
            this.Airline = airline;
        }

        #endregion

        //pay some of the loan

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public Loan Loan { get; set; }

        public int MonthsLeft
        {
            get
            {
                return this._monthsLeft;
            }
            set
            {
                this._monthsLeft = value;
                this.NotifyPropertyChanged("MonthsLeft");
            }
        }

        public double PaymentLeft
        {
            get
            {
                return this._paymentLeft;
            }
            set
            {
                this._paymentLeft = value;
                this.NotifyPropertyChanged("PaymentLeft");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void payOnLoan(double amount)
        {
            this.Loan.PaymentLeft -= amount;
            this.PaymentLeft = this.Loan.PaymentLeft;
            this.MonthsLeft = this.Loan.MonthsLeft;
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

    //the mvvm class for a cooperation
    public class CooperationMVVM
    {
        #region Constructors and Destructors

        public CooperationMVVM(Airport airport, Cooperation cooperation)
        {
            this.Airport = airport;
            this.Cooperation = cooperation;
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
            
            this.Route = route;

            this.Score = -1;
            
            if (this.Route.Type == Route.RouteType.Passenger)
            {
                this.PriceIndex =
                    ((PassengerRoute)this.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).FarePrice;

                if (this.Route.HasAirliner)
                    this.Score = RouteHelpers.GetRouteTotalScore(this.Route);

            }
            else if (this.Route.Type == Route.RouteType.Cargo)
            {
                this.PriceIndex = ((CargoRoute)this.Route).PricePerUnit;
            }
            else if (this.Route.Type == Route.RouteType.Mixed)
            {
                this.PriceIndex =
                    ((CombiRoute)this.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).FarePrice
                    + ((CombiRoute)this.Route).PricePerUnit;
            }
            else if (this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Helicopter)
            {
                this.PriceIndex =
                ((HelicopterRoute)this.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).FarePrice;

            }
        }

        #endregion

        #region Public Properties

        public double PriceIndex { get; set; }

        public double Score{ get; set; }
        public Route Route { get; set; }

        #endregion
    }

    //the mvvm class for a destination
    public class AirlineDestinationMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isHub;

        #endregion

        #region Constructors and Destructors

        public AirlineDestinationMVVM(Airport airport, bool isHub, bool hasRoutes, bool hasAirplaneOnRouteTo)
        {
            this.IsHub = isHub;
            this.HasRoutes = hasRoutes;
            this.HasAirplaneOnRouteTo = hasAirplaneOnRouteTo;
            this.Airport = airport;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airport Airport { get; set; }

        public Boolean IsHub
        {
            get
            {
                return this._isHub;
            }
            set
            {
                this._isHub = value;
                this.NotifyPropertyChanged("IsHub");
            }
        }

        public bool HasRoutes { get; set; }

        public bool HasAirplaneOnRouteTo { get; set; }

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

    //the mvvm object for the airliner quantity
    public class AirlinerQuantityMVVM
    {
        #region Constructors and Destructors

        public AirlinerQuantityMVVM(AirlinerType type, string cabinConfig, int quantity, int onOrder)
        {
            this.Quantity = quantity;
            this.OnOrder = onOrder;
            this.Type = type;
            this.CabinConfiguration = cabinConfig;
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

                Boolean hasSubsidiary = airliner.Airliner.Airline.Subsidiaries.Count > 0 || airliner.Airliner.Airline.IsSubsidiary;

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
            var airline = (Airline)value;

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