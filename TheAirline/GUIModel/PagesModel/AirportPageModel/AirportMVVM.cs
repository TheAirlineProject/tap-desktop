using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Environment;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    public class AirportMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _canBuildHub;

        private int _freeCargoGates;

        private int _freePaxGates;

        private Boolean _canMakeCooperation;

        private int _freeGates;

        #endregion

        #region Constructors and Destructors

        public AirportMVVM(Airport airport)
        {
            Airport = airport;

            TerminalGatePrice = Airport.GetTerminalGatePrice();
            TerminalPrice = Airport.GetTerminalPrice();
            LandingFee = GeneralHelpers.GetInflationPrice(Airport.LandingFee);
            FuelPrice = AirportHelpers.GetFuelPrice(Airport);

            TotalPaxGates = Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Passenger).Sum(t=>t.Gates.NumberOfDeliveredGates);
            TotalCargoGates = Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Cargo).Sum(t=>t.Gates.NumberOfDeliveredGates);
        
            Cooperations = new ObservableCollection<Cooperation>();
            Terminals = new ObservableCollection<AirportTerminalMVVM>();
            BuildingTerminals = new ObservableCollection<AirportTerminalMVVM>();

            foreach (Terminal terminal in Airport.Terminals.GetTerminals())
            {
                Boolean isSellable = terminal.Airline != null
                                     && terminal.Airline == GameObject.GetInstance().HumanAirline;

                if (terminal.IsBuilt)
                {
                    Terminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
                }
                else
                {
                    BuildingTerminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
                }
            }
            Contracts = new ObservableCollection<ContractMVVM>();

            foreach (AirportContract contract in Airport.AirlineContracts)
            {
                Contracts.Add(new ContractMVVM(contract));
            }

            foreach (Cooperation cooperation in Airport.Cooperations)
            {
                Cooperations.Add(cooperation);
            }

            AirportHelpers.CreateAirportWeather(Airport);

            Weather = Airport.Weather.ToList();

            if (!GameObject.GetInstance().DayRoundEnabled)
            {
                CurrentWeather = Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour];
            }

            FreeGates = Airport.Terminals.NumberOfFreeGates;

            FreeCargoGates = Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Cargo).Sum(t=>t.GetFreeGates());

            FreePaxGates = Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Passenger).Sum(t => t.GetFreeGates());

            DomesticDemands = new List<DemandMVVM>();
            IntlDemands = new List<DemandMVVM>();

            IOrderedEnumerable<Airport> demands =
                Airport.GetDestinationDemands()
                    .Where(a => a != null && GeneralHelpers.IsAirportActive(a))
                    .OrderByDescending(
                        a => Airport.GetDestinationPassengersRate(a, AirlinerClass.ClassType.EconomyClass));

            IEnumerable<Airport> internationalDemand =
                demands.Where(
                    a =>
                        new CountryCurrentCountryConverter().Convert(a.Profile.Country)
                        != new CountryCurrentCountryConverter().Convert(Airport.Profile.Country));
            IEnumerable<Airport> domesticDemand =
                demands.Where(
                    a =>
                        new CountryCurrentCountryConverter().Convert(a.Profile.Country)
                        == new CountryCurrentCountryConverter().Convert(Airport.Profile.Country));

            foreach (Airport destination in internationalDemand)
            {
                IntlDemands.Add(
                    new DemandMVVM(
                        destination,
                        Airport.GetDestinationPassengersRate(destination, AirlinerClass.ClassType.EconomyClass),
                        (int)Airport.Profile.Pax,
                        Airport.GetDestinationCargoRate(destination),MathHelpers.GetDistance(destination,Airport)));
            }

            foreach (Airport destination in domesticDemand)
            {
                DomesticDemands.Add(
                    new DemandMVVM(
                        destination,
                        Airport.GetDestinationPassengersRate(destination, AirlinerClass.ClassType.EconomyClass),
                        (int)Airport.Profile.Pax,
                        Airport.GetDestinationCargoRate(destination), MathHelpers.GetDistance(destination,Airport)));
            }

            AirportFacilities =
                Airport.GetAirportFacilities()
                    .FindAll(f => f.Airline == null && f.Facility.TypeLevel != 0)
                    .Select(f => f.Facility)
                    .Distinct()
                    .ToList();

            AirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();
            BuildingAirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();

            foreach (
                AirlineAirportFacility facility in Airport.GetAirportFacilities().FindAll(f => f.Airline != null))
            {
                if (facility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = facility.Airline.Alliances.Count == 0 ? null : facility.Airline.Alliances[0];

                    var airlineFacility = new AirlineAirportFacilityMVVM(facility, alliance);

                    if (airlineFacility.IsDelivered)
                    {
                        if (facility == Airport.GetAirlineAirportFacility(facility.Airline, facility.Facility.Type))
                        {
                            AirlineFacilities.Add(airlineFacility);
                        }
                    }
                    else
                    {
                        BuildingAirlineFacilities.Add(airlineFacility);
                    }
                }
            }

            AirlineStatistics = new List<AirportStatisticsMVMM>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
                StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
                StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");

                double passengers = Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    passengersType);
                double passengersAvg = Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    passengersAvgType);
                double arrivals = Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    arrivalsType);

                int routes = airline.Routes.Count(r => r.Destination1 == Airport || r.Destination2 == Airport);

                AirlineStatistics.Add(
                    new AirportStatisticsMVMM(airline, passengers, passengersAvg, arrivals, routes));
            }

            Traffic = new List<AirportTrafficMVVM>();

            IOrderedEnumerable<Airport> passengerDestinations = from a in Airports.GetAllActiveAirports()
                orderby Airport.GetDestinationPassengerStatistics(a) descending
                select a;
            IOrderedEnumerable<Airport> cargoDestinations = from a in Airports.GetAllActiveAirports()
                orderby Airport.GetDestinationCargoStatistics(a) descending
                select a;

            foreach (Airport a in passengerDestinations.Take(20))
            {
                Traffic.Add(
                    new AirportTrafficMVVM(
                        a,
                        Airport.GetDestinationPassengerStatistics(a),
                        AirportTrafficMVVM.TrafficType.Passengers));
            }

            foreach (Airport a in cargoDestinations.Take(20))
            {
                Traffic.Add(
                    new AirportTrafficMVVM(
                        a,
                        Convert.ToInt64(Airport.GetDestinationCargoStatistics(a)),
                        AirportTrafficMVVM.TrafficType.Cargo));
            }

            Flights = new List<DestinationFlightsMVVM>();

            IEnumerable<Route> airportRoutes =
                AirportHelpers.GetAirportRoutes(Airport).Where(r => r.GetAirliners().Count > 0);

            foreach (Route airportRoute in airportRoutes)
            {
                double distance = MathHelpers.GetDistance(airportRoute.Destination1, airportRoute.Destination2);

                Airport destination = airportRoute.Destination1 == Airport
                    ? airportRoute.Destination2
                    : airportRoute.Destination1;
                if (Flights.Exists(f => f.Airline == airportRoute.Airline && f.Airport == destination))
                {
                    DestinationFlightsMVVM flight =
                        Flights.First(f => f.Airline == airportRoute.Airline && f.Airport == destination);

                    flight.Flights += airportRoute.TimeTable.GetEntries(destination).Count;

                    foreach (AirlinerType aircraft in airportRoute.GetAirliners().Select(a => a.Airliner.Type))
                    {
                        if (!flight.Aircrafts.Contains(aircraft))
                        {
                            flight.Aircrafts.Add(aircraft);
                        }
                    }
                }
                else
                {
                    Flights.Add(
                        new DestinationFlightsMVVM(
                            destination,
                            airportRoute.Airline,
                            distance,
                            airportRoute.GetAirliners().Select(a => a.Airliner.Type).ToList(),
                            airportRoute.TimeTable.GetEntries(destination).Count));
                }
            }
            /*
            Dictionary<Airport, int> destinations = new Dictionary<Airport, int>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(this.Airport).FindAll(r => r.getAirliners().Count > 0))
            {
                if (route.Destination1 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination1))
                        destinations.Add(route.Destination1, 0);
                    destinations[route.Destination1] += route.TimeTable.getEntries(route.Destination1).Count;


                }
                if (route.Destination2 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination2))
                        destinations.Add(route.Destination2, 0);
                    destinations[route.Destination2] += route.TimeTable.getEntries(route.Destination2).Count;
                }
            }

            foreach (Airport a in destinations.Keys)
                this.Flights.Add(new DestinationFlightsMVVM(a, destinations[a]));
            */

            Hubs = new ObservableCollection<Hub>();

            foreach (Hub hub in Airport.GetHubs())
            {
                Hubs.Add(hub);
            }

            CanBuildHub = canBuildHub();
            CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == Airport);

            LocalTime = MathHelpers.ConvertDateTimeToLoalTime(
                GameObject.GetInstance().GameTime,
                Airport.Profile.TimeZone);

            ShowLocalTime = !GameObject.GetInstance().DayRoundEnabled;

            AirlineReputations = new List<AirlineReputationMVVM>();

            IDictionary<Airline, double> airlineScores = new Dictionary<Airline, double>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                airlineScores.Add(airline, Airport.GetAirlineReputation(airline));
            }

            foreach (var score in StatisticsHelpers.GetRatingScale(airlineScores))
            {
                AirlineReputations.Add(new AirlineReputationMVVM(score.Key, (int)score.Value));
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<AirlineAirportFacilityMVVM> AirlineFacilities { get; set; }

        public List<AirlineReputationMVVM> AirlineReputations { get; set; }

        public List<AirportStatisticsMVMM> AirlineStatistics { get; set; }

        public Airport Airport { get; set; }

        public List<AirportFacility> AirportFacilities { get; set; }

        public ObservableCollection<AirlineAirportFacilityMVVM> BuildingAirlineFacilities { get; set; }

        public ObservableCollection<AirportTerminalMVVM> BuildingTerminals { get; set; }

        public Boolean IsHeliport 
        {
            get
            {
                return !Airport.Runways.Exists(r=>r.Type == Runway.RunwayType.Regular);
            }
            private set { ;} 
        }

        public Boolean CanBuildHub
        {
            get
            {
                return _canBuildHub;
            }
            set
            {
                _canBuildHub = value;
                NotifyPropertyChanged("CanBuildHub");
            }
        }

        public Boolean CanMakeCooperation
        {
            get
            {
                return _canMakeCooperation;
            }
            set
            {
                _canMakeCooperation = value;
                NotifyPropertyChanged("CanMakeCooperation");
            }
        }

        public ObservableCollection<ContractMVVM> Contracts { get; set; }

        public ObservableCollection<Cooperation> Cooperations { get; set; }

        public HourlyWeather CurrentWeather { get; set; }

        public List<DemandMVVM> DomesticDemands { get; set; }

        public List<DemandMVVM> IntlDemands { get; set; }

        public List<DestinationFlightsMVVM> Flights { get; set; }


        public int FreeCargoGates 
        {
            get
            {
               return _freeCargoGates ;
            }
            set
            {
                _freeCargoGates = value;
                NotifyPropertyChanged("FreeCargoGates");
            } 
        }

        public int FreePaxGates 
        {
            get
            {
             return _freePaxGates   ;
            }
            set
            {
                _freePaxGates = value;
                NotifyPropertyChanged("FreePaxGates");
            }
        }

        public int FreeGates
        {
            get
            {
                return _freeGates;
            }
            set
            {
                _freeGates = value;
                NotifyPropertyChanged("FreeGates");
            }
        }

        public ObservableCollection<Hub> Hubs { get; set; }

        public DateTime LocalTime { get; set; }

        public Boolean ShowLocalTime { get; set; }

        public double TerminalGatePrice { get; set; }

        public double LandingFee { get; set; }

        public double FuelPrice { get; set; }

        public double TerminalPrice { get; set; }

        public ObservableCollection<AirportTerminalMVVM> Terminals { get; set; }

        public List<AirportTrafficMVVM> Traffic { get; set; }

        public List<Weather> Weather { get; set; }

        public double TotalPaxGates { get; set; }

        public double TotalCargoGates { get; set; }

        #endregion

        //adds an airline contract to the airport

        #region Public Methods and Operators

        public void addAirlineContract(AirportContract contract)
        {
            AirportHelpers.AddAirlineContract(contract);

            Contracts.Add(new ContractMVVM(contract));

            FreeGates = Airport.Terminals.NumberOfFreeGates;

            CanBuildHub = canBuildHub();

            foreach (AirportTerminalMVVM terminal in Terminals)
            {
                terminal.FreeGates = terminal.Terminal.GetFreeGates();
            }

            CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == Airport);

            FreeCargoGates = Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Cargo).Sum(t => t.GetFreeGates());

            FreePaxGates = Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Passenger).Sum(t => t.GetFreeGates());

        }

        public void addAirlineFacility(AirportFacility facility)
        {
            var nextFacility = new AirlineAirportFacility(
                GameObject.GetInstance().HumanAirline,
                Airport,
                facility,
                GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));
            Airport.AddAirportFacility(nextFacility);

            /*
            AirlineAirportFacilityMVVM currentFacility = this.AirlineFacilities.Where(f => f.Facility.Facility.Type == facility.Type).FirstOrDefault();

            if (currentFacility != null)
                removeAirlineFacility(currentFacility);
            
            Alliance alliance = nextFacility.Airline.Alliances.Count == 0 ? null : nextFacility.Airline.Alliances[0];
            
            this.AirlineFacilities.Add(new AirlineAirportFacilityMVVM(nextFacility,alliance));
             * */
            AirlineFacilities.Clear();
            BuildingAirlineFacilities.Clear();

            foreach (
                AirlineAirportFacility tFacility in Airport.GetAirportFacilities().FindAll(f => f.Airline != null))
            {
                if (tFacility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = tFacility.Airline.Alliances.Count == 0 ? null : tFacility.Airline.Alliances[0];

                    var airlineFacility = new AirlineAirportFacilityMVVM(tFacility, alliance);

                    if (airlineFacility.IsDelivered)
                    {
                        AirlineFacilities.Add(airlineFacility);
                    }
                    else
                    {
                        BuildingAirlineFacilities.Add(airlineFacility);
                    }
                }
            }
        }

        //removes an airline contract from the airport

        //adds a cooperation to the airport
        public void addCooperation(Cooperation cooperation)
        {
            Cooperations.Add(cooperation);
            Airport.AddCooperation(cooperation);

            AirlineHelpers.AddAirlineInvoice(
                cooperation.Airline,
                cooperation.BuiltDate,
                Invoice.InvoiceType.Purchases,
                -cooperation.Type.Price);
        }

        public void addHub(Hub hub)
        {
            Hubs.Add(hub);
            Airport.AddHub(hub);

            CanBuildHub = canBuildHub();
        }

        public void addTerminal(Terminal terminal)
        {
            Airport.AddTerminal(terminal);

            BuildingTerminals.Add(
                new AirportTerminalMVVM(
                    terminal,
                    false,
                    terminal.Airline != null && terminal.Airline == GameObject.GetInstance().HumanAirline));
        }

        public void removeAirlineContract(ContractMVVM contract)
        {
            Airport.RemoveAirlineContract(contract.Contract);

            Contracts.Remove(contract);

            FreeGates = Airport.Terminals.NumberOfFreeGates;

            foreach (AirportTerminalMVVM terminal in Terminals)
            {
                terminal.FreeGates = terminal.Terminal.GetFreeGates();
            }

            CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == Airport);
        }

        //removes an airline facility from the airport
        public void removeAirlineFacility(AirlineAirportFacilityMVVM facility)
        {
            Airport.RemoveFacility(facility.Facility.Airline, facility.Facility.Facility);

            AirlineFacilities.Remove(facility);

            if (
                Airport.GetAirlineAirportFacility(facility.Facility.Airline, facility.Facility.Facility.Type)
                    .Facility.TypeLevel > 0)
            {
                Alliance alliance = facility.Facility.Airline.Alliances.Count == 0
                    ? null
                    : facility.Facility.Airline.Alliances[0];

                var airlineFacility =
                    new AirlineAirportFacilityMVVM(
                        Airport.GetAirlineAirportFacility(
                            facility.Facility.Airline,
                            facility.Facility.Facility.Type),
                        alliance);

                if (airlineFacility.IsDelivered)
                {
                    AirlineFacilities.Add(airlineFacility);
                }
                else
                {
                    BuildingAirlineFacilities.Add(airlineFacility);
                }
            }
        }

        public void removeCooperation(Cooperation cooperation)
        {
            Cooperations.Remove(cooperation);
            Airport.RemoveCooperation(cooperation);
        }

        public void removeHub(Hub hub)
        {
            Hubs.Remove(hub);
            Airport.RemoveHub(hub);

            CanBuildHub = canBuildHub();
        }
        public void purchaseTerminal(AirportTerminalMVVM terminal, Airline airline)
        {

            terminal.purchaseTerminal(airline);

            foreach (AirportContract contract in Airport.AirlineContracts)
            {
                if (Contracts.FirstOrDefault(c=>c.Contract == contract) == null)
                    Contracts.Add(new ContractMVVM(contract));
     
            }
        
            
        }
        public void removeTerminal(AirportTerminalMVVM terminal)
        {
            Airport.RemoveTerminal(terminal.Terminal);

            Terminals.Remove(terminal);

            var contracts = new List<ContractMVVM>(Contracts);

            foreach (ContractMVVM contract in contracts)
            {
                if (!Airport.AirlineContracts.Exists(c=>contract.Contract == c))
                    Contracts.Remove(contract);
            }
            
        }

        #endregion

        //adds an airline facility to the airport

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Boolean canBuildHub()
        {
            Boolean hasServiceCenter =
                Airport.GetAirlineAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    AirportFacility.FacilityType.Service).Facility.TypeLevel > 0;

            double gatesPercent =
                Convert.ToDouble(Contracts.Where(c => c.Airline == GameObject.GetInstance().HumanAirline).Sum(c=>c.NumberOfGates))
                / Convert.ToDouble(Airport.Terminals.NumberOfGates);

           return gatesPercent > 0.2 && Hubs.Count == 0 && hasServiceCenter;
        }

        #endregion
    }

    //the mvvm class for airport traffic
    public class AirportTrafficMVVM
    {
        #region Constructors and Destructors

        public AirportTrafficMVVM(Airport destination, long value, TrafficType type)
        {
            Destination = destination;
            Value = value;
            Type = type;
        }

        #endregion

        #region Enums

        public enum TrafficType
        {
            Passengers,

            Cargo
        }

        #endregion

        #region Public Properties

        public Airport Destination { get; set; }

        public TrafficType Type { get; set; }

        public long Value { get; set; }

        #endregion
    }

    //the mvvm class for passenger demand
    public class DemandMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _contracted;

        #endregion

        #region Constructors and Destructors

        public DemandMVVM(Airport destination, int passengers, int totalpax, int cargo, double distance)
        {
            Cargo = cargo;
            Passengers = passengers;
            TotalPax = totalpax;
            Destination = destination;
           Contracted =
                Destination.AirlineContracts.Exists(c => c.Airline == GameObject.GetInstance().HumanAirline);
            HasFreeGates = Destination.Terminals.GetFreeGates(GameObject.GetInstance().HumanAirline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger) > 0;
            Distance = distance;

            GatesPercent = new List<KeyValuePair<string, int>>();

            var airlines = Destination.AirlineContracts.Select(a=>a.Airline).Distinct();

            foreach (Airline airline in airlines)
            {
                int airlineGates = Destination.AirlineContracts.Where(a=>a.Airline == airline).Sum(c=>c.NumberOfGates);
            
                GatesPercent.Add(new KeyValuePair<string,int>(airline.Profile.Name, airlineGates));
            }

            GatesPercent.Add(new KeyValuePair<string,int>("Free", Destination.Terminals.GetFreeGates()));

            Type = "";
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

       
        #region Public Properties
        public List<KeyValuePair<string,int>> GatesPercent { get; set; }

        public double Distance { get; set; }

        public int Cargo { get; set; }

        public object Type { get; set; }

        public Boolean Contracted
        {
            get
            {
                return _contracted;
            }
            set
            {
                _contracted = value;
                NotifyPropertyChanged("Contracted");
            }
        }

        public Boolean HasFreeGates { get; set; }

        public Airport Destination { get; set; }

        public int Passengers { get; set; }

        public int TotalPax { get; set; }

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

    //the mvvm class for an airline contract
    public class ContractMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _monthsleft;

        private int _numberofgates;

        #endregion

        #region Constructors and Destructors

        public ContractMVVM(AirportContract contract)
        {
            Contract = contract;
            Airline = Contract.Airline;
            NumberOfGates = Contract.NumberOfGates;
            MonthsLeft = Contract.MonthsLeft;
            IsHuman = Airline == GameObject.GetInstance().HumanAirline;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public AirportContract Contract { get; set; }

        public Boolean IsHuman { get; set; }

        public int MonthsLeft
        {
            get
            {
                return _monthsleft;
            }
            set
            {
                _monthsleft = value;
                NotifyPropertyChanged("MonthsLeft");
            }
        }

        public int NumberOfGates
        {
            get
            {
                return _numberofgates;
            }
            set
            {
                _numberofgates = value;
                NotifyPropertyChanged("NumberOfGates");
            }
        }

        #endregion

        //extends the contract with a number of year

        #region Public Methods and Operators

        public void extendContract(int years)
        {
            Contract.Length += years;
            Contract.ExpireDate = Contract.ExpireDate.AddYears(years);
            MonthsLeft = Contract.MonthsLeft;
        }

        //sets the number of gates

        //sets the expire date
        public void setExpireDate(DateTime expireDate)
        {
            int years = MathHelpers.CalculateAge(Contract.ExpireDate, expireDate);
            extendContract(years);
        }

        public void setNumberOfGates(int gates)
        {
            NumberOfGates = gates;
            Contract.NumberOfGates = gates;
            Contract.YearlyPayment = AirportHelpers.GetYearlyContractPayment(
                Contract.Airport,
                Contract.Type,
                gates,
                Contract.Length);
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

    //the mvvm class for an airport gate
    public class AirportGateMVVM : INotifyPropertyChanged
    {
        #region Constructors and Destructors

        public AirportGateMVVM(int gatenumber, Airline airline)
        {
            Airline = airline;
            GateNumber = gatenumber;
            IsFree = Airline == null;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public int GateNumber { get; set; }

        public Boolean IsFree { get; set; }

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

    //the mvvm class for an airport terminal
    public class AirportTerminalMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Airline _airline;

        private int _freegates;

        private Boolean _isBuyable;

        private Boolean _isSellable;

        #endregion

        #region Constructors and Destructors

        public AirportTerminalMVVM(Terminal terminal, Boolean isBuyable, Boolean isSellable)
        {
            Terminal = terminal;

            Name = Terminal.Name;
            Airline = Terminal.Airline;
            Gates = Terminal.Gates.NumberOfGates;
            FreeGates = Terminal.GetFreeGates();
            IsBuyable = isBuyable;
            DeliveryDate = Terminal.DeliveryDate;
            IsSellable = isSellable;

            AllGates = new ObservableCollection<AirportGateMVVM>();

            int gatenumber = 1;

            foreach (Gate gate in Terminal.Gates.GetGates())
            {
                AllGates.Add(new AirportGateMVVM(gatenumber, gate.Airline));

                gatenumber++;
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline
        {
            get
            {
                return _airline;
            }
            set
            {
                _airline = value;
                NotifyPropertyChanged("Airline");
            }
        }

        public ObservableCollection<AirportGateMVVM> AllGates { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int FreeGates
        {
            get
            {
                return _freegates;
            }
            set
            {
                _freegates = value;
                NotifyPropertyChanged("FreeGates");
            }
        }

        public int Gates { get; set; }

        public Boolean IsBuyable
        {
            get
            {
                return _isBuyable;
            }
            set
            {
                _isBuyable = value;
                NotifyPropertyChanged("IsBuyable");
            }
        }

        public Boolean IsSellable
        {
            get
            {
                return _isSellable;
            }
            set
            {
                _isSellable = value;
                NotifyPropertyChanged("IsSellable");
            }
        }

        public string Name { get; set; }

        public Terminal Terminal { get; set; }

        #endregion

        //purchase a terminal

        #region Public Methods and Operators

        public void purchaseTerminal(Airline airline)
        {
            Terminal.PurchaseTerminal(airline);
            Airline = airline;

            IsBuyable = false;

            FreeGates = 0;

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

    //the mvvm object for airport distance
    public class AirportDistanceMVVM
    {
        #region Constructors and Destructors

        public AirportDistanceMVVM(Airport destination, double distance)
        {
            Destination = destination;
            Distance = distance;
        }

        #endregion

        #region Public Properties

        public Airport Destination { get; set; }

        public double Distance { get; set; }

        #endregion
    }

    //the mvvm object for airport flights
    public class DestinationFlightsMVVM
    {
        #region Constructors and Destructors

        public DestinationFlightsMVVM(
            Airport airport,
            Airline airline,
            double distance,
            List<AirlinerType> aircrafts,
            int flights)
        {
            Flights = flights;
            Airport = airport;
            Distance = distance;
            Airline = airline;
            Aircrafts = aircrafts;
        }

        #endregion

        #region Public Properties

        public List<AirlinerType> Aircrafts { get; set; }

        public Airline Airline { get; set; }

        public Airport Airport { get; set; }

        public double Distance { get; set; }

        public int Flights { get; set; }

        #endregion
    }

    //the mvvm object for airport statistics
    public class AirportStatisticsMVMM
    {
        #region Constructors and Destructors

        public AirportStatisticsMVMM(
            Airline airline,
            double passengers,
            double passengersPerFlight,
            double flights,
            int routes)
        {
            Passengers = passengers;
            Airline = airline;
            PassengersPerFlight = passengersPerFlight;
            Flights = flights;
            Routes = routes;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public double Flights { get; set; }

        public double Passengers { get; set; }

        public double PassengersPerFlight { get; set; }

        public int Routes { get; set; }

        #endregion
    }

    //the mvvm object for airline airport facility
    public class AirlineAirportFacilityMVVM
    {
        #region Constructors and Destructors

        public AirlineAirportFacilityMVVM(AirlineAirportFacility facility, Alliance alliance)
        {
            Facility = facility;
            Alliance = alliance;
            IsHuman = GameObject.GetInstance().HumanAirline == facility.Airline;
            IsDelivered = facility.FinishedDate < GameObject.GetInstance().GameTime;
        }

        #endregion

        #region Public Properties

        public Alliance Alliance { get; set; }

        public AirlineAirportFacility Facility { get; set; }

        public Boolean IsDelivered { get; set; }

        public Boolean IsHuman { get; set; }

        #endregion
    }

    //the mvvm object for airline reputation score
    public class AirlineReputationMVVM
    {
        #region Constructors and Destructors

        public AirlineReputationMVVM(Airline airline, int reputation)
        {
            Airline = airline;
            Reputation = reputation;
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public int Reputation { get; set; }

        #endregion
    }

    //the converter for the price of a terminal
    public class TerminalPriceConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int gates = System.Convert.ToInt16(values[0]);
            var airport = (Airport)values[1];

            double price = gates * airport.GetTerminalGatePrice() + airport.GetTerminalPrice();

            return new ValueCurrencyConverter().Convert(price);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for the facilities for a specific type
    public class TypeFacilitiesConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (AirportFacility.FacilityType)values[0];
            var airport = (Airport)values[1];

            AirportFacility currentFacility = airport.GetCurrentAirportFacility(
                GameObject.GetInstance().HumanAirline,
                type);
            AirportFacility buildingFacility = airport.GetAirlineBuildingFacility(
                GameObject.GetInstance().HumanAirline,
                type);

            var facilities = new List<AirportFacility>();
            foreach (
                AirportFacility facility in
                    AirportFacilities.GetFacilities(type).Where(f => f.TypeLevel > currentFacility.TypeLevel))
            {
                if (buildingFacility == null || facility.TypeLevel > buildingFacility.TypeLevel)
                {
                    facilities.Add(facility);
                }
            }

            return facilities;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for the temperature (in celsius) to text
    public class TemperatureToTextConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temperature = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().GetLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:0.0}°C", temperature);
            }
            return string.Format("{0:0}°F", MathHelpers.CelsiusToFahrenheit(temperature));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class WindSpeedToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var windspeed = (Weather.eWindSpeed)value;

            var v = (double)windspeed;

            if (AppSettings.GetInstance().GetLanguage().Unit == Language.UnitSystem.Imperial)
            {
                v = MathHelpers.KMToMiles(v);
            }

            return string.Format("{0:0} {1}", v, new StringToLanguageConverter().Convert("km/t"));
        }

        public object Convert(object value)
        {
            return Convert(value, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for the weather
    public class WeatherImageConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Weather)
            {
                var weather = (Weather)value;

                string weatherCondition = "clear";

                if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                {
                    weatherCondition = weather.Precip.ToString();
                }
                else
                {
                    weatherCondition = weather.Cover.ToString();
                }

                return AppSettings.GetDataPath() + "\\graphics\\weather\\" + weatherCondition + ".png";
            }
            if (value is HourlyWeather)
            {
                var weather = (HourlyWeather)value;

                string weatherCondition = "clear";

                if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                {
                    weatherCondition = weather.Precip.ToString();
                }
                else
                {
                    weatherCondition = weather.Cover.ToString();
                }

                if (GameObject.GetInstance().GameTime.Hour < Weather.Sunrise
                    || GameObject.GetInstance().GameTime.Hour > Weather.Sunset)
                {
                    weatherCondition += "-night";
                }

                return AppSettings.GetDataPath() + "\\graphics\\weather\\" + weatherCondition + ".png";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for the yearly payment of a contract
    public class ContractYearlyPaymentConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int gates = System.Convert.ToInt16(values[0]);
            int lenght = System.Convert.ToInt16(values[1]);
            var contractType = (AirportContract.ContractType)values[2];
            var airport = (Airport)values[3];

            return
                new ValueCurrencyConverter().Convert(
                    AirportHelpers.GetYearlyContractPayment(airport, contractType, gates, lenght));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}