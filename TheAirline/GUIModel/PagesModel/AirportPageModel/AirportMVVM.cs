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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

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
            this.Airport = airport;

            this.TerminalGatePrice = this.Airport.GetTerminalGatePrice();
            this.TerminalPrice = this.Airport.GetTerminalPrice();
            this.LandingFee = GeneralHelpers.GetInflationPrice(this.Airport.LandingFee);
            this.FuelPrice = AirportHelpers.GetFuelPrice(this.Airport);

            this.TotalPaxGates = this.Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Passenger).Sum(t=>t.Gates.NumberOfDeliveredGates);
            this.TotalCargoGates = this.Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Cargo).Sum(t=>t.Gates.NumberOfDeliveredGates);
        
            this.Cooperations = new ObservableCollection<Cooperation>();
            this.Terminals = new ObservableCollection<AirportTerminalMVVM>();
            this.BuildingTerminals = new ObservableCollection<AirportTerminalMVVM>();

            foreach (Terminal terminal in this.Airport.Terminals.GetTerminals())
            {
                Boolean isSellable = terminal.Airline != null
                                     && terminal.Airline == GameObject.GetInstance().HumanAirline;

                if (terminal.IsBuilt)
                {
                    this.Terminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
                }
                else
                {
                    this.BuildingTerminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
                }
            }
            this.Contracts = new ObservableCollection<ContractMVVM>();

            foreach (AirportContract contract in this.Airport.AirlineContracts)
            {
                this.Contracts.Add(new ContractMVVM(contract));
            }

            foreach (Cooperation cooperation in this.Airport.Cooperations)
            {
                this.Cooperations.Add(cooperation);
            }

            AirportHelpers.CreateAirportWeather(this.Airport);

            this.Weather = this.Airport.Weather.ToList();

            if (!GameObject.GetInstance().DayRoundEnabled)
            {
                this.CurrentWeather = this.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour];
            }

            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;

            this.FreeCargoGates = this.Airport.Terminals.AirportTerminals.Where(t=>t.Type == Terminal.TerminalType.Cargo).Sum(t=>t.GetFreeGates());

            this.FreePaxGates = this.Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Passenger).Sum(t => t.GetFreeGates());

            this.DomesticDemands = new List<DemandMVVM>();
            this.IntlDemands = new List<DemandMVVM>();

            IOrderedEnumerable<Airport> demands =
                this.Airport.GetDestinationDemands()
                    .Where(a => a != null && GeneralHelpers.IsAirportActive(a))
                    .OrderByDescending(
                        a => this.Airport.GetDestinationPassengersRate(a, AirlinerClass.ClassType.EconomyClass));

            IEnumerable<Airport> internationalDemand =
                demands.Where(
                    a =>
                        new CountryCurrentCountryConverter().Convert(a.Profile.Country)
                        != new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country));
            IEnumerable<Airport> domesticDemand =
                demands.Where(
                    a =>
                        new CountryCurrentCountryConverter().Convert(a.Profile.Country)
                        == new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country));

            foreach (Airport destination in internationalDemand)
            {
                this.IntlDemands.Add(
                    new DemandMVVM(
                        destination,
                        this.Airport.GetDestinationPassengersRate(destination, AirlinerClass.ClassType.EconomyClass),
                        (int)this.Airport.Profile.Pax,
                        this.Airport.GetDestinationCargoRate(destination),MathHelpers.GetDistance(destination,this.Airport)));
            }

            foreach (Airport destination in domesticDemand)
            {
                this.DomesticDemands.Add(
                    new DemandMVVM(
                        destination,
                        this.Airport.GetDestinationPassengersRate(destination, AirlinerClass.ClassType.EconomyClass),
                        (int)this.Airport.Profile.Pax,
                        this.Airport.GetDestinationCargoRate(destination), MathHelpers.GetDistance(destination,this.Airport)));
            }

            this.AirportFacilities =
                this.Airport.GetAirportFacilities()
                    .FindAll(f => f.Airline == null && f.Facility.TypeLevel != 0)
                    .Select(f => f.Facility)
                    .Distinct()
                    .ToList();

            this.AirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();
            this.BuildingAirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();

            foreach (
                AirlineAirportFacility facility in this.Airport.GetAirportFacilities().FindAll(f => f.Airline != null))
            {
                if (facility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = facility.Airline.Alliances.Count == 0 ? null : facility.Airline.Alliances[0];

                    var airlineFacility = new AirlineAirportFacilityMVVM(facility, alliance);

                    if (airlineFacility.IsDelivered)
                    {
                        if (facility == this.Airport.GetAirlineAirportFacility(facility.Airline, facility.Facility.Type))
                        {
                            this.AirlineFacilities.Add(airlineFacility);
                        }
                    }
                    else
                    {
                        this.BuildingAirlineFacilities.Add(airlineFacility);
                    }
                }
            }

            this.AirlineStatistics = new List<AirportStatisticsMVMM>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
                StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
                StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");

                double passengers = this.Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    passengersType);
                double passengersAvg = this.Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    passengersAvgType);
                double arrivals = this.Airport.Statistics.GetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    airline,
                    arrivalsType);

                int routes = airline.Routes.Count(r => r.Destination1 == this.Airport || r.Destination2 == this.Airport);

                this.AirlineStatistics.Add(
                    new AirportStatisticsMVMM(airline, passengers, passengersAvg, arrivals, routes));
            }

            this.Traffic = new List<AirportTrafficMVVM>();

            IOrderedEnumerable<Airport> passengerDestinations = from a in Airports.GetAllActiveAirports()
                orderby this.Airport.GetDestinationPassengerStatistics(a) descending
                select a;
            IOrderedEnumerable<Airport> cargoDestinations = from a in Airports.GetAllActiveAirports()
                orderby this.Airport.GetDestinationCargoStatistics(a) descending
                select a;

            foreach (Airport a in passengerDestinations.Take(20))
            {
                this.Traffic.Add(
                    new AirportTrafficMVVM(
                        a,
                        this.Airport.GetDestinationPassengerStatistics(a),
                        AirportTrafficMVVM.TrafficType.Passengers));
            }

            foreach (Airport a in cargoDestinations.Take(20))
            {
                this.Traffic.Add(
                    new AirportTrafficMVVM(
                        a,
                        Convert.ToInt64(this.Airport.GetDestinationCargoStatistics(a)),
                        AirportTrafficMVVM.TrafficType.Cargo));
            }

            this.Flights = new List<DestinationFlightsMVVM>();

            IEnumerable<Route> airportRoutes =
                AirportHelpers.GetAirportRoutes(this.Airport).Where(r => r.GetAirliners().Count > 0);

            foreach (Route airportRoute in airportRoutes)
            {
                double distance = MathHelpers.GetDistance(airportRoute.Destination1, airportRoute.Destination2);

                Airport destination = airportRoute.Destination1 == this.Airport
                    ? airportRoute.Destination2
                    : airportRoute.Destination1;
                if (this.Flights.Exists(f => f.Airline == airportRoute.Airline && f.Airport == destination))
                {
                    DestinationFlightsMVVM flight =
                        this.Flights.First(f => f.Airline == airportRoute.Airline && f.Airport == destination);

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
                    this.Flights.Add(
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

            this.Hubs = new ObservableCollection<Hub>();

            foreach (Hub hub in this.Airport.GetHubs())
            {
                this.Hubs.Add(hub);
            }

            this.CanBuildHub = canBuildHub();
            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == this.Airport);

            this.LocalTime = MathHelpers.ConvertDateTimeToLoalTime(
                GameObject.GetInstance().GameTime,
                this.Airport.Profile.TimeZone);

            this.ShowLocalTime = !GameObject.GetInstance().DayRoundEnabled;

            this.AirlineReputations = new List<AirlineReputationMVVM>();

            IDictionary<Airline, double> airlineScores = new Dictionary<Airline, double>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                airlineScores.Add(airline, this.Airport.GetAirlineReputation(airline));
            }

            foreach (var score in StatisticsHelpers.GetRatingScale(airlineScores))
            {
                this.AirlineReputations.Add(new AirlineReputationMVVM(score.Key, (int)score.Value));
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
                return !this.Airport.Runways.Exists(r=>r.Type == Runway.RunwayType.Regular);
            }
            private set { ;} 
        }

        public Boolean CanBuildHub
        {
            get
            {
                return this._canBuildHub;
            }
            set
            {
                this._canBuildHub = value;
                this.NotifyPropertyChanged("CanBuildHub");
            }
        }

        public Boolean CanMakeCooperation
        {
            get
            {
                return this._canMakeCooperation;
            }
            set
            {
                this._canMakeCooperation = value;
                this.NotifyPropertyChanged("CanMakeCooperation");
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
               return this._freeCargoGates ;
            }
            set
            {
                this._freeCargoGates = value;
                this.NotifyPropertyChanged("FreeCargoGates");
            } 
        }

        public int FreePaxGates 
        {
            get
            {
             return this._freePaxGates   ;
            }
            set
            {
                this._freePaxGates = value;
                this.NotifyPropertyChanged("FreePaxGates");
            }
        }

        public int FreeGates
        {
            get
            {
                return this._freeGates;
            }
            set
            {
                this._freeGates = value;
                this.NotifyPropertyChanged("FreeGates");
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

            this.Contracts.Add(new ContractMVVM(contract));

            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;

            this.CanBuildHub = canBuildHub();

            foreach (AirportTerminalMVVM terminal in this.Terminals)
            {
                terminal.FreeGates = terminal.Terminal.GetFreeGates();
            }

            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == this.Airport);

            this.FreeCargoGates = this.Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Cargo).Sum(t => t.GetFreeGates());

            this.FreePaxGates = this.Airport.Terminals.AirportTerminals.Where(t => t.Type == Terminal.TerminalType.Passenger).Sum(t => t.GetFreeGates());

        }

        public void addAirlineFacility(AirportFacility facility)
        {
            var nextFacility = new AirlineAirportFacility(
                GameObject.GetInstance().HumanAirline,
                this.Airport,
                facility,
                GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));
            this.Airport.AddAirportFacility(nextFacility);

            /*
            AirlineAirportFacilityMVVM currentFacility = this.AirlineFacilities.Where(f => f.Facility.Facility.Type == facility.Type).FirstOrDefault();

            if (currentFacility != null)
                removeAirlineFacility(currentFacility);
            
            Alliance alliance = nextFacility.Airline.Alliances.Count == 0 ? null : nextFacility.Airline.Alliances[0];
            
            this.AirlineFacilities.Add(new AirlineAirportFacilityMVVM(nextFacility,alliance));
             * */
            this.AirlineFacilities.Clear();
            this.BuildingAirlineFacilities.Clear();

            foreach (
                AirlineAirportFacility tFacility in this.Airport.GetAirportFacilities().FindAll(f => f.Airline != null))
            {
                if (tFacility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = tFacility.Airline.Alliances.Count == 0 ? null : tFacility.Airline.Alliances[0];

                    var airlineFacility = new AirlineAirportFacilityMVVM(tFacility, alliance);

                    if (airlineFacility.IsDelivered)
                    {
                        this.AirlineFacilities.Add(airlineFacility);
                    }
                    else
                    {
                        this.BuildingAirlineFacilities.Add(airlineFacility);
                    }
                }
            }
        }

        //removes an airline contract from the airport

        //adds a cooperation to the airport
        public void addCooperation(Cooperation cooperation)
        {
            this.Cooperations.Add(cooperation);
            this.Airport.AddCooperation(cooperation);

            AirlineHelpers.AddAirlineInvoice(
                cooperation.Airline,
                cooperation.BuiltDate,
                Invoice.InvoiceType.Purchases,
                -cooperation.Type.Price);
        }

        public void addHub(Hub hub)
        {
            this.Hubs.Add(hub);
            this.Airport.AddHub(hub);

            this.CanBuildHub = this.canBuildHub();
        }

        public void addTerminal(Terminal terminal)
        {
            this.Airport.AddTerminal(terminal);

            this.BuildingTerminals.Add(
                new AirportTerminalMVVM(
                    terminal,
                    false,
                    terminal.Airline != null && terminal.Airline == GameObject.GetInstance().HumanAirline));
        }

        public void removeAirlineContract(ContractMVVM contract)
        {
            this.Airport.RemoveAirlineContract(contract.Contract);

            this.Contracts.Remove(contract);

            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;

            foreach (AirportTerminalMVVM terminal in this.Terminals)
            {
                terminal.FreeGates = terminal.Terminal.GetFreeGates();
            }

            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == this.Airport);
        }

        //removes an airline facility from the airport
        public void removeAirlineFacility(AirlineAirportFacilityMVVM facility)
        {
            this.Airport.RemoveFacility(facility.Facility.Airline, facility.Facility.Facility);

            this.AirlineFacilities.Remove(facility);

            if (
                this.Airport.GetAirlineAirportFacility(facility.Facility.Airline, facility.Facility.Facility.Type)
                    .Facility.TypeLevel > 0)
            {
                Alliance alliance = facility.Facility.Airline.Alliances.Count == 0
                    ? null
                    : facility.Facility.Airline.Alliances[0];

                var airlineFacility =
                    new AirlineAirportFacilityMVVM(
                        this.Airport.GetAirlineAirportFacility(
                            facility.Facility.Airline,
                            facility.Facility.Facility.Type),
                        alliance);

                if (airlineFacility.IsDelivered)
                {
                    this.AirlineFacilities.Add(airlineFacility);
                }
                else
                {
                    this.BuildingAirlineFacilities.Add(airlineFacility);
                }
            }
        }

        public void removeCooperation(Cooperation cooperation)
        {
            this.Cooperations.Remove(cooperation);
            this.Airport.RemoveCooperation(cooperation);
        }

        public void removeHub(Hub hub)
        {
            this.Hubs.Remove(hub);
            this.Airport.RemoveHub(hub);

            this.CanBuildHub = this.canBuildHub();
        }
        public void purchaseTerminal(AirportTerminalMVVM terminal, Airline airline)
        {

            terminal.purchaseTerminal(airline);

            foreach (AirportContract contract in this.Airport.AirlineContracts)
            {
                if (this.Contracts.FirstOrDefault(c=>c.Contract == contract) == null)
                    this.Contracts.Add(new ContractMVVM(contract));
     
            }
        
            
        }
        public void removeTerminal(AirportTerminalMVVM terminal)
        {
            this.Airport.RemoveTerminal(terminal.Terminal);

            this.Terminals.Remove(terminal);

            var contracts = new List<ContractMVVM>(this.Contracts);

            foreach (ContractMVVM contract in contracts)
            {
                if (!this.Airport.AirlineContracts.Exists(c=>contract.Contract == c))
                    this.Contracts.Remove(contract);
            }
            
        }

        #endregion

        //adds an airline facility to the airport

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Boolean canBuildHub()
        {
            Boolean hasServiceCenter =
                this.Airport.GetAirlineAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    AirportFacility.FacilityType.Service).Facility.TypeLevel > 0;

            double gatesPercent =
                Convert.ToDouble(this.Contracts.Where(c => c.Airline == GameObject.GetInstance().HumanAirline).Sum(c=>c.NumberOfGates))
                / Convert.ToDouble(this.Airport.Terminals.NumberOfGates);

           return gatesPercent > 0.2 && this.Hubs.Count == 0 && hasServiceCenter;
        }

        #endregion
    }

    //the mvvm class for airport traffic
    public class AirportTrafficMVVM
    {
        #region Constructors and Destructors

        public AirportTrafficMVVM(Airport destination, long value, TrafficType type)
        {
            this.Destination = destination;
            this.Value = value;
            this.Type = type;
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
            this.Cargo = cargo;
            this.Passengers = passengers;
            this.TotalPax = totalpax;
            this.Destination = destination;
           this.Contracted =
                this.Destination.AirlineContracts.Exists(c => c.Airline == GameObject.GetInstance().HumanAirline);
            this.HasFreeGates = this.Destination.Terminals.GetFreeGates(GameObject.GetInstance().HumanAirline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger) > 0;
            this.Distance = distance;

            this.GatesPercent = new List<KeyValuePair<string, int>>();

            var airlines = this.Destination.AirlineContracts.Select(a=>a.Airline).Distinct();

            foreach (Airline airline in airlines)
            {
                int airlineGates = this.Destination.AirlineContracts.Where(a=>a.Airline == airline).Sum(c=>c.NumberOfGates);
            
                this.GatesPercent.Add(new KeyValuePair<string,int>(airline.Profile.Name, airlineGates));
            }

            this.GatesPercent.Add(new KeyValuePair<string,int>("Free", this.Destination.Terminals.GetFreeGates()));

            this.Type = "";
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
                return this._contracted;
            }
            set
            {
                this._contracted = value;
                this.NotifyPropertyChanged("Contracted");
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
            PropertyChangedEventHandler handler = this.PropertyChanged;
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
            this.Contract = contract;
            this.Airline = this.Contract.Airline;
            this.NumberOfGates = this.Contract.NumberOfGates;
            this.MonthsLeft = this.Contract.MonthsLeft;
            this.IsHuman = this.Airline == GameObject.GetInstance().HumanAirline;
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
                return this._monthsleft;
            }
            set
            {
                this._monthsleft = value;
                this.NotifyPropertyChanged("MonthsLeft");
            }
        }

        public int NumberOfGates
        {
            get
            {
                return this._numberofgates;
            }
            set
            {
                this._numberofgates = value;
                this.NotifyPropertyChanged("NumberOfGates");
            }
        }

        #endregion

        //extends the contract with a number of year

        #region Public Methods and Operators

        public void extendContract(int years)
        {
            this.Contract.Length += years;
            this.Contract.ExpireDate = this.Contract.ExpireDate.AddYears(years);
            this.MonthsLeft = this.Contract.MonthsLeft;
        }

        //sets the number of gates

        //sets the expire date
        public void setExpireDate(DateTime expireDate)
        {
            int years = MathHelpers.CalculateAge(this.Contract.ExpireDate, expireDate);
            this.extendContract(years);
        }

        public void setNumberOfGates(int gates)
        {
            this.NumberOfGates = gates;
            this.Contract.NumberOfGates = gates;
            this.Contract.YearlyPayment = AirportHelpers.GetYearlyContractPayment(
                this.Contract.Airport,
                this.Contract.Type,
                gates,
                this.Contract.Length);
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

    //the mvvm class for an airport gate
    public class AirportGateMVVM : INotifyPropertyChanged
    {
        #region Constructors and Destructors

        public AirportGateMVVM(int gatenumber, Airline airline)
        {
            this.Airline = airline;
            this.GateNumber = gatenumber;
            this.IsFree = this.Airline == null;
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
            PropertyChangedEventHandler handler = this.PropertyChanged;
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
            this.Terminal = terminal;

            this.Name = this.Terminal.Name;
            this.Airline = this.Terminal.Airline;
            this.Gates = this.Terminal.Gates.NumberOfGates;
            this.FreeGates = this.Terminal.GetFreeGates();
            this.IsBuyable = isBuyable;
            this.DeliveryDate = this.Terminal.DeliveryDate;
            this.IsSellable = isSellable;

            this.AllGates = new ObservableCollection<AirportGateMVVM>();

            int gatenumber = 1;

            foreach (Gate gate in this.Terminal.Gates.GetGates())
            {
                this.AllGates.Add(new AirportGateMVVM(gatenumber, gate.Airline));

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
                return this._airline;
            }
            set
            {
                this._airline = value;
                this.NotifyPropertyChanged("Airline");
            }
        }

        public ObservableCollection<AirportGateMVVM> AllGates { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int FreeGates
        {
            get
            {
                return this._freegates;
            }
            set
            {
                this._freegates = value;
                this.NotifyPropertyChanged("FreeGates");
            }
        }

        public int Gates { get; set; }

        public Boolean IsBuyable
        {
            get
            {
                return this._isBuyable;
            }
            set
            {
                this._isBuyable = value;
                this.NotifyPropertyChanged("IsBuyable");
            }
        }

        public Boolean IsSellable
        {
            get
            {
                return this._isSellable;
            }
            set
            {
                this._isSellable = value;
                this.NotifyPropertyChanged("IsSellable");
            }
        }

        public string Name { get; set; }

        public Terminal Terminal { get; set; }

        #endregion

        //purchase a terminal

        #region Public Methods and Operators

        public void purchaseTerminal(Airline airline)
        {
            this.Terminal.PurchaseTerminal(airline);
            this.Airline = airline;

            this.IsBuyable = false;

            this.FreeGates = 0;

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

    //the mvvm object for airport distance
    public class AirportDistanceMVVM
    {
        #region Constructors and Destructors

        public AirportDistanceMVVM(Airport destination, double distance)
        {
            this.Destination = destination;
            this.Distance = distance;
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
            this.Flights = flights;
            this.Airport = airport;
            this.Distance = distance;
            this.Airline = airline;
            this.Aircrafts = aircrafts;
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
            this.Passengers = passengers;
            this.Airline = airline;
            this.PassengersPerFlight = passengersPerFlight;
            this.Flights = flights;
            this.Routes = routes;
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
            this.Facility = facility;
            this.Alliance = alliance;
            this.IsHuman = GameObject.GetInstance().HumanAirline == facility.Airline;
            this.IsDelivered = facility.FinishedDate < GameObject.GetInstance().GameTime;
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
            this.Airline = airline;
            this.Reputation = reputation;
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
            return this.Convert(value, null, null, null);
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