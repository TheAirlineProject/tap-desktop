using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.General.Models.Countries;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Environment;
using TheAirline.Models.Passengers;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airports
{
    [Serializable]
    //the class for an airport
    public class Airport : BaseModel
    {
        #region Fields

        [Versioning("contracts")] private List<AirportContract> _contracts;
        [Versioning("facilities")] private List<AirlineAirportFacility> _facilities;

        private List<Hub> _hubs;

        #endregion

        #region Constructors and Destructors

        public Airport(AirportProfile profile)
        {
            Profile = profile;
            Income = 0;
            DestinationPassengers = new List<DestinationDemand>();
            DestinationCargo = new List<DestinationDemand>();
            _facilities = new List<AirlineAirportFacility>();
            Cooperations = new List<Cooperation>();
            Statistics = new AirportStatistics();
            Weather = new Weather[5];
            Terminals = new Terminals(this);
            Runways = new List<Runway>();
            _hubs = new List<Hub>();
            DestinationPassengerStatistics = new Dictionary<Airport, long>();
            DestinationCargoStatistics = new Dictionary<Airport, double>();
            LastExpansionDate = new DateTime(1900, 1, 1);
            Statics = new AirportStatics(this);
            AirlineContracts = new List<AirportContract>();
        }

        private Airport(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version < 3)
                LandingFee = AirportHelpers.GetStandardLandingFee(this);

            if (Weather.Contains(null))
            {
                AirportHelpers.CreateAirportWeather(this);
            }
        }

        #endregion

        #region Public Properties

        public List<AirportContract> AirlineContracts
        {
            get { return GetAirlineContracts(); }
            set { _contracts = value; }
        }

        [Versioning("cooperations", Version = 2)]
        public List<Cooperation> Cooperations { get; set; }

        [Versioning("hubs")]
        public List<Hub> Hubs
        {
            private get { return GetHubs(); }
            set { _hubs = value; }
        }

        [Versioning("income")]
        public long Income { get; set; }

        public bool IsHub => GetHubs().Count > 0;

        [Versioning("lastexpansiondate")]
        public DateTime LastExpansionDate { get; set; }

        [Versioning("profile")]
        public AirportProfile Profile { get; set; }

        [Versioning("runways")]
        public List<Runway> Runways { get; set; }

        public AirportStatics Statics { get; set; }

        [Versioning("statistics")]
        public AirportStatistics Statistics { get; set; }

        [Versioning("terminals")]
        public Terminals Terminals { get; set; }

        [Versioning("weather")]
        public Weather[] Weather { get; set; }

        [Versioning("landingfee", Version = 3)]
        public double LandingFee { get; set; }

        #endregion

        #region Properties

        [Versioning("destinationcargo")]
        private List<DestinationDemand> DestinationCargo { get; }

        [Versioning("cargostatistics")]
        private Dictionary<Airport, double> DestinationCargoStatistics { get; }

        [Versioning("passengerstatistics")]
        private Dictionary<Airport, long> DestinationPassengerStatistics { get; }

        [Versioning("destinationpassengers")]
        private List<DestinationDemand> DestinationPassengers { get; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        public void AddAirlineContract(AirportContract contract)
        {
            lock (_contracts)
            {
                _contracts.Add(contract);
            }

            if (!contract.Airline.Airports.Contains(this))
            {
                contract.Airline.AddAirport(this);
            }
        }

        public void AddAirportFacility(Airline airline, AirportFacility facility, DateTime finishedDate)
        {
            //this.Facilities.RemoveAll(f => f.Airline == airline && f.Facility.Type == facility.Type);
            _facilities.Add(new AirlineAirportFacility(airline, this, facility, finishedDate));
        }

        //sets the facility for an airline
        public void AddAirportFacility(AirlineAirportFacility facility)
        {
            //this.Facilities.RemoveAll(f => f.Airline == facility.Airline && f.Facility.Type == facility.Facility.Type);
            _facilities.Add(facility);
        }

        public void AddCargoDestinationStatistics(Airport destination, double cargo)
        {
            lock (DestinationCargoStatistics)
            {
                if (!DestinationCargoStatistics.ContainsKey(destination))
                {
                    DestinationCargoStatistics.Add(destination, cargo);
                }
                else
                {
                    DestinationCargoStatistics[destination] += cargo;
                }
            }
        }

        public void AddCooperation(Cooperation cooperation)
        {
            Cooperations.Add(cooperation);
        }

        public void AddDestinationCargoRate(DestinationDemand cargo)
        {
            Statics.AddCargoDemand(cargo);
        }

        public void AddDestinationCargoRate(Airport destination, ushort rate)
        {
            lock (DestinationCargo)
            {
                DestinationDemand destinationCargo = GetDestinationCargoObject(destination);

                if (destinationCargo != null)
                {
                    destinationCargo.Rate += rate;
                }
                else
                {
                    DestinationCargo.Add(new DestinationDemand(destination.Profile.IATACode, rate));
                }
            }
        }

        public void AddDestinationPassengersRate(DestinationDemand passengers)
        {
            Statics.AddPassengerDemand(passengers);
        }

        public void AddDestinationPassengersRate(Airport destination, ushort rate)
        {
            lock (DestinationPassengers)
            {
                DestinationDemand destinationPassengers = GetDestinationPassengersObject(destination);

                if (destinationPassengers != null)
                {
                    destinationPassengers.Rate += rate;
                }
                else
                {
                    DestinationPassengers.Add(new DestinationDemand(destination.Profile.IATACode, rate));
                }
            }
        }

        public void AddHub(Hub hub)
        {
            _hubs.Add(hub);
        }

        //adds a major destination to the airport
        public void AddMajorDestination(string destination, int pax)
        {
            if (Profile.MajorDestionations.ContainsKey(destination))
            {
                Profile.MajorDestionations[destination] += pax;
            }
            else
            {
                Profile.MajorDestionations.Add(destination, pax);
            }
        }

        public void AddPassengerDestinationStatistics(Airport destination, long passengers)
        {
            lock (DestinationPassengerStatistics)
            {
                if (!DestinationPassengerStatistics.ContainsKey(destination))
                {
                    DestinationPassengerStatistics.Add(destination, passengers);
                }
                else
                {
                    DestinationPassengerStatistics[destination] += passengers;
                }
            }
        }

        public void AddTerminal(Terminal terminal)
        {
            Terminals.AddTerminal(terminal);
        }

        //returns a list of major destinations and pax

        //clears the list of airline contracts
        public void ClearAirlineContracts()
        {
            lock (_contracts)
            {
                AirlineContracts.Clear();
            }
        }

        public void ClearDestinationCargoStatistics()
        {
            DestinationCargoStatistics.Clear();
        }

        public void ClearDestinationPassengerStatistics()
        {
            DestinationPassengerStatistics.Clear();
        }

        public void ClearDestinationPassengers()
        {
            DestinationPassengers.Clear();
        }

        public void ClearFacilities()
        {
            _facilities = new List<AirlineAirportFacility>();
        }

        //cleares the list of facilities for an airline
        public void ClearFacilities(Airline airline)
        {
            _facilities.RemoveAll(f => f.Airline == airline);
        }

        //returns if an airline is building a facility
        public bool IsBuildingFacility(Airline airline, AirportFacility.FacilityType type)
        {
            var facilities = new List<AirlineAirportFacility>();
            lock (_facilities)
            {
                return facilities.Exists(f => f.Airline == airline && f.Facility.Type == type && f.FinishedDate > GameObject.GetInstance().GameTime);
            }
        }

        public AirlineAirportFacility GetAirlineAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            List<AirlineAirportFacility> facilities;
            lock (_facilities)
            {
                facilities = (from f in _facilities
                              where
                                  f.Airline == airline && f.Facility.Type == type
                                  && GameObject.GetInstance().GameTime >= f.FinishedDate
                              orderby f.FinishedDate descending
                              select f).ToList();

                if (!facilities.Any())
                {
                    AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find(f => f.TypeLevel == 0);
                    AddAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);

                    facilities.Add(GetAirlineAirportFacility(airline, type));
                }
            }
            return facilities.FirstOrDefault();
        }

        public AirportFacility GetAirlineBuildingFacility(Airline airline, AirportFacility.FacilityType type)
        {
            AirlineAirportFacility facility =
                _facilities.FirstOrDefault(
                    f =>
                    f.Airline == airline && f.Facility.Type == type
                    && GameObject.GetInstance().GameTime < f.FinishedDate);

            return facility?.Facility;
        }

        //adds an airline airport contract to the airport

        //returns the contracts for an airline
        public List<AirportContract> GetAirlineContracts(Airline airline)
        {
            return AirlineContracts.FindAll(a => a.Airline == airline);
        }

        //returns if an airline has a contract of a specific type

        //return all airline contracts
        public List<AirportContract> GetAirlineContracts()
        {
            List<AirportContract> contracts;
            lock (_contracts)
            {
                contracts = new List<AirportContract>(_contracts);
            }
            return contracts;
        }

        public double GetAirlineReputation(Airline airline)
        {
            //The score could be airport facilities for the airline, routes, connecting routes, hotels, service level per route etc
            double score = 0;

            score += Cooperations.Where(c => c.Airline == airline).Sum(c => c.Type.ServiceLevel*9);

            List<AirlineAirportFacility> facilities;

            lock (_facilities)
            {
                facilities = new List<AirlineAirportFacility>(_facilities);
            }
            score += facilities.Where(f => f.Airline == airline).Sum(f => f.Facility.ServiceLevel*10);

            IEnumerable<Route> airportRoutes =
                airline.Routes.Where(r => r.Destination1 == this || r.Destination2 == this);
            var enumerable = airportRoutes as Route[] ?? airportRoutes.ToArray();
            score += 7*enumerable.Count();
            score += 6
                     *enumerable.Where(r => r.Type == Route.RouteType.Passenger)
                                   .Sum(r => ((PassengerRoute) r).GetServiceLevel(AirlinerClass.ClassType.EconomyClass));

            score +=
                airline.Alliances.Sum(
                    a =>
                    5
                    *a.Members.SelectMany(m => m.Airline.Routes)
                      .Count(r => r.Destination1 == this || r.Destination2 == this));

            return airline.Codeshares.Select(codesharing => (codesharing.Airline1 == airline ? codesharing.Airline2 : codesharing.Airline1).Routes.Count(r => r.Destination2 == this || r.Destination1 == this)).Aggregate(score, (current, codesharingRoutes) => current + 4*codesharingRoutes);
        }

        public List<AirlineAirportFacility> GetAirportFacilities(Airline airline)
        {
            List<AirlineAirportFacility> fac;

            lock (_facilities)
            {
                fac = _facilities.FindAll(f => f.Airline == airline);
            }

            return fac;
        }

        //returns all facilities
        public List<AirlineAirportFacility> GetAirportFacilities()
        {
            return _facilities;
        }

        public AirportFacility GetAirportFacility(
            Airline airline,
            AirportFacility.FacilityType type,
            Boolean useAirport = false)
        {
            if (!useAirport)
            {
                return GetAirlineAirportFacility(airline, type).Facility;
            }
            AirportFacility airlineFacility = GetCurrentAirportFacility(airline, type);
            AirportFacility airportFacility = GetCurrentAirportFacility(null, type);

            return airportFacility == null || airlineFacility.TypeLevel > airportFacility.TypeLevel
                       ? airlineFacility
                       : airportFacility;
        }

        public List<AirportFacility> GetCurrentAirportFacilities(Airline airline)
        {
            return (from AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)) select GetCurrentAirportFacility(airline, type)).ToList();
        }

        public AirportFacility GetCurrentAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            List<AirportFacility> facilities;

            var tFacilities = new List<AirlineAirportFacility>(_facilities);
            lock (_facilities)
            {
                facilities = (from f in tFacilities
                              where
                                  f.Airline == airline && f.Facility.Type == type
                                  && f.FinishedDate <= GameObject.GetInstance().GameTime
                              orderby f.FinishedDate descending
                              select f.Facility).ToList();
                int numberOfFacilities = facilities.Count();

                if (numberOfFacilities == 0 && airline != null)
                {
                    AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find(f => f.TypeLevel == 0);
                    AddAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);

                    facilities.Add(noneFacility);
                }
            }
            return facilities.FirstOrDefault();
        }

        public DestinationDemand GetDestinationCargoObject(Airport destination)
        {
            return DestinationCargo.Find(a => a.Destination == destination.Profile.IATACode);
        }

        //returns the maximum value for the run ways

        //returns the destination cargo for a specific destination
        public ushort GetDestinationCargoRate(Airport destination)
        {
            DestinationDemand cargo = DestinationCargo.Find(a => a.Destination == destination.Profile.IATACode);

            if (cargo == null)
            {
                return Statics.GetDestinationCargoRate(destination);
            }
            return (ushort) (cargo.Rate + Statics.GetDestinationCargoRate(destination));
        }

        public double GetDestinationCargoStatistics(Airport destination)
        {
            double cargo = 0;
            lock (DestinationCargoStatistics)
            {
                if (DestinationCargoStatistics.ContainsKey(destination))
                {
                    cargo = DestinationCargoStatistics[destination];
                }
            }
            return cargo;
        }

        //adds a passenger rate for a destination

        //returns all airports where the airport has demand
        public List<Airport> GetDestinationDemands()
        {
            var destinations = new List<DestinationDemand>();

            destinations.AddRange(Statics.GetDemands());
            destinations.AddRange(DestinationCargo);
            destinations.AddRange(DestinationPassengers);

            return destinations.Select(d => Airports.GetAirport(d.Destination)).Distinct().ToList();
        }

        public long GetDestinationPassengerStatistics(Airport destination)
        {
            long passengers;
            lock (DestinationPassengerStatistics)
            {
                passengers = DestinationPassengerStatistics.ContainsKey(destination) ? DestinationPassengerStatistics[destination] : 0;
            }

            return passengers;
        }

        //returns if the destination has passengers rate

        //returns a destination passengers object
        public DestinationDemand GetDestinationPassengersObject(Airport destination)
        {
            return DestinationPassengers.Find(a => a.Destination == destination.Profile.IATACode);
        }

        public ushort GetDestinationPassengersRate(Airport destination, AirlinerClass.ClassType type)
        {
            DestinationDemand pax = DestinationPassengers.Find(a => a.Destination == destination.Profile.IATACode);

            Array values = Enum.GetValues(typeof (AirlinerClass.ClassType));

            int classFactor = 0;

            int i = 1;

            foreach (AirlinerClass.ClassType value in values)
            {
                if (value == type)
                {
                    classFactor = i;
                }
                i++;
            }

            if (pax == null)
            {
                return Statics.GetDestinationPassengersRate(destination, type);
            }
            return
                (ushort)
                (Statics.GetDestinationPassengersRate(destination, type) + (ushort) (pax.Rate/classFactor));
        }

        //returns a destination cargo object

        //returns the sum of passenger demand
        public int GetDestinationPassengersSum()
        {
            int sum;
            lock (DestinationPassengers)
            {
                sum = DestinationPassengers.Sum(d => d.Rate);
            }

            return sum + Statics.GetDestinationPassengersSum();
        }

        public List<DestinationDemand> GetDestinationsPassengers()
        {
            return DestinationPassengers;
        }

        //adds a number of passengers to destination to the statistics

        //returns the price for a gate
        public long GetGatePrice()
        {
            long sizeValue = 100 + 102*((int) Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(sizeValue));
        }

        public List<Hub> GetHubs(HubType.TypeOfHub type)
        {
            List<Hub> hubs;
            lock (_hubs) hubs = new List<Hub>(_hubs);

            return hubs.FindAll(h => h.Type.Type == type);
        }

        //returns all hubs
        public List<Hub> GetHubs()
        {
            List<Hub> hubs;
            lock (_hubs) hubs = new List<Hub>(_hubs);

            return hubs;
        }

        /*
        //returns the fee for landing at the airport
        public double getLandingFee()
        {
            long sizeValue = 151 * ((int)this.Profile.Size + 1);
            return GeneralHelpers.GetInflationPrice(sizeValue);
        }*/

        public Dictionary<Airport, int> GetMajorDestinations()
        {
            return Profile.MajorDestionations.ToDictionary(md => Airports.GetAirport(md.Key), md => md.Value);
        }

        public long GetMaxRunwayLength()
        {
            if (Runways.Count == 0)
            {
                return 0;
            }

            IEnumerable<long> query = from r in Runways
                                      where r.BuiltDate <= GameObject.GetInstance().GameTime
                                      select r.Length;
            return query.Max();
        }

        public long GetTerminalGatePrice()
        {
            long price = 75000*((int) Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));
        }

        public long GetTerminalPrice()
        {
            long price = 500000 + 15000*((int) Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));
        }

        /*
        //adds a facility to an airline
        public void addAirportFacility(Airline airline, AirportFacility facility, DateTime finishedDate)
        {
            lock (this.Facilities)
            {
                this.Facilities.Add(new AirlineAirportFacility(airline, this, facility, finishedDate));
            }
        }*/
        //sets the facility for an airline

        //returns if the airport has a facility for any airline
        public Boolean HasAirlineFacility()
        {
            return _facilities.Exists(f => f.Airline != null && f.Facility.TypeLevel > 0);
        }

        public Boolean HasAsHomebase(Airline airline)
        {
            return airline.Fleet.Any(airliner => airliner.Homebase == this);
        }

        public Boolean HasContractType(Airline airline, AirportContract.ContractType type)
        {
            return AirlineContracts.Exists(c => c.Airline == airline && c.Type == type);
        }

        public Boolean HasDestinationCargStatistics(Airport destination)
        {
            return DestinationCargoStatistics.ContainsKey(destination);
        }

        public Boolean HasDestinationPassengerStatistics(Airport destination)
        {
            return DestinationPassengerStatistics.ContainsKey(destination);
        }

        public Boolean HasDestinationPassengersRate(Airport destination)
        {
            return Statics.HasDestinationPassengersRate(destination)
                   || DestinationPassengers.Exists(a => a.Destination == destination.Profile.IATACode);
        }

        //returns the facilities being build for an airline

        //returns if an airline has any facilities at the airport
        public Boolean HasFacilities(Airline airline)
        {
            Boolean hasFacilities = false;
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)))
            {
                if (GetAirportFacility(airline, type).TypeLevel > 0)
                {
                    hasFacilities = true;
                }
            }
            return hasFacilities;
        }

        //returns if an airline has any facilities besides a specific type
        public Boolean HasFacilities(Airline airline, AirportFacility.FacilityType ftype)
        {
            Boolean hasFacilities = false;
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)))
            {
                if (type != ftype)
                {
                    if (GetAirportFacility(airline, type).TypeLevel > 0)
                    {
                        hasFacilities = true;
                    }
                }
            }
            return hasFacilities;
        }

        public Boolean HasHub(Airline airline)
        {
            return Hubs.Exists(h => h.Airline == airline);
        }

        public void RemoveAirlineContract(AirportContract contract)
        {
            lock (_contracts)
            {
                _contracts.Remove(contract);
            }

            if (!AirlineContracts.Exists(c => c.Airline == contract.Airline))
            {
                contract.Airline.RemoveAirport(this);
            }
        }

        public void RemoveCooperation(Cooperation cooperation)
        {
            Cooperations.Remove(cooperation);
        }

        //returns if an airline has any airliners with the airport as home base

        //removes the facility for an airline
        public void RemoveFacility(Airline airline, AirportFacility facility)
        {
            _facilities.RemoveAll(f => f.Airline == airline && f.Facility.Type == facility.Type);
        }

        //clears the list of facilites

        //removes a hub
        public void RemoveHub(Hub hub)
        {
            _hubs.Remove(hub);
        }

        //returns if an airline have a hub

        //removes a terminal from the airport
        public void RemoveTerminal(Terminal terminal)
        {
            AirportContract terminalContract =
                AirlineContracts.Find(c => c.Terminal != null && c.Terminal == terminal);

            if (terminalContract != null)
            {
                RemoveAirlineContract(terminalContract);
            }

            Terminals.RemoveTerminal(terminal);
        }

        #endregion

        //adds a cooperation to the airport
    }

    //the collection of airports
    public class Airports
    {
        #region Static Fields

        public static Dictionary<GeneralHelpers.Size, int> CargoAirportsSizes =
            new Dictionary<GeneralHelpers.Size, int>();

        public static double LargeAirports;

        public static double LargestAirports;

        public static double MediumAirports,
                             SmallAirports;

        public static double SmallestAirports;

        public static double VeryLargeAirports;

        public static double VerySmallAirports;

        private static List<Airport> _airports = new List<Airport>();

        #endregion

        #region Public Methods and Operators

        public static void AddAirport(Airport airport)
        {
            _airports.Add(airport);
        }

        //clears the list
        public static void Clear()
        {
            _airports = new List<Airport>();
        }

        //returns if a specific airport is in the list
        public static Boolean Contains(Airport airport)
        {
            return _airports.Contains(airport);
        }

        public static int Count()
        {
            return GetAllActiveAirports().Count;
        }

        public static double CountLarge()
        {
            return LargeAirports;
        }

        public static double CountLargest()
        {
            return LargestAirports;
        }

        public static double CountMedium()
        {
            return MediumAirports;
        }

        public static double CountSmall()
        {
            return SmallAirports;
        }

        public static double CountSmallest()
        {
            return SmallestAirports;
        }

        public static double CountVeryLarge()
        {
            return VeryLargeAirports;
        }

        public static double CountVerySmall()
        {
            return VerySmallAirports;
        }

        //adds an airport

        //returns an airport based on iata code
        public static Airport GetAirport(string iata)
        {
            List<Airport> tAirports = _airports.FindAll(a => a.Profile.IATACode == iata);

            if (tAirports.Count == 1)
            {
                return tAirports[0];
            }

            Airport tAirport = tAirports.Find(GeneralHelpers.IsAirportActive);

            return tAirport;
        }

        //returns an airport based on match
        public static Airport GetAirport(Predicate<Airport> match)
        {
            return _airports.Find(match);
        }

        //returns an airport based on id

        //returns a possible match for coordinates
        public static Airport GetAirport(GeoCoordinate coordinates)
        {
            return GetAllActiveAirports().Find(a => false);
        }

        public static Airport GetAirportFromID(string id)
        {
            Airport airport = _airports.Find(a => a.Profile.ID == id);

            if (airport != null)
            {
                return airport;
            }
            return _airports.Find(a => a.Profile.ID.StartsWith(id.Substring(0, 8)));
            //airports.Find(a=>a.Profile.ID.StartsWith(id.Substring(0, id.LastIndexOf('-'))));
        }

        //returns all airports with a specific size
        public static List<Airport> GetAirports(GeneralHelpers.Size size)
        {
            return GetAirports(airport => airport.Profile.Size == size);
        }

        //returns all airports from a specific country
        public static List<Airport> GetAirports(Country country)
        {
            return GetAirports(airport => airport.Profile.Country == country);
        }

        //returns all airports from a specific region
        public static List<Airport> GetAirports(Region region)
        {
            return GetAirports(airport => airport.Profile.Country.Region == region);
        }

        //returns a list of airports
        public static List<Airport> GetAirports(Predicate<Airport> match)
        {
            return GetAllActiveAirports().FindAll(match);
        }

        public static List<Airport> GetAllActiveAirports()
        {
            return _airports.FindAll(GeneralHelpers.IsAirportActive);
        }

        //returns all airports
        public static List<Airport> GetAllAirports(Predicate<Airport> match)
        {
            return _airports.FindAll(match);
        }

        public static List<Airport> GetAllAirports()
        {
            return _airports;
        }

        //returns the total number of airports

        //removes all airports with a specific match
        public static void RemoveAirports(Predicate<Airport> match)
        {
            _airports.RemoveAll(match);
        }

        #endregion
    }
}