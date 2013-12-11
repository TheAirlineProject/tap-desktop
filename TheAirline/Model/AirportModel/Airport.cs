using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Runtime.Serialization;



namespace TheAirline.Model.AirportModel
{
    [DataContract]
    //the class for an airport
    public class Airport
    {
        [DataMember]
        public AirportProfile Profile { get; set; }
        // private List<Passenger> Passengers;
        //
        [DataMember]
        private List<DestinationDemand> DestinationPassengers { get; set; }
        [DataMember]
        private List<DestinationDemand> DestinationCargo { get; set; }
       [DataMember]
        private Dictionary<Airport, long> DestinationPassengerStatistics { get; set; }
       [DataMember]
        private Dictionary<Airport, double> DestinationCargoStatistics { get; set; }
        [DataMember]
        private List<AirlineAirportFacility> Facilities;
        [DataMember]
        public AirportStatistics Statistics { get; set; }
        [DataMember]
        public Weather[] Weather { get; set; }
        [DataMember]
        public List<Runway> Runways { get; set; }
        [DataMember]
        public Terminals Terminals { get; set; }
          [DataMember]
        private List<Hub> _Hubs;
        public List<Hub> Hubs { private get { return getHubs(); } set { this._Hubs = value; } }
        public Boolean IsHub { get { return getHubs().Count > 0; } set { ;} }
        [DataMember]
        public long Income { get; set; }
        [DataMember]
        public DateTime LastExpansionDate { get; set; }
        [DataMember]
        private List<AirportContract> _Contracts;
        public List<AirportContract> AirlineContracts { get { return getAirlineContracts();} set { this._Contracts = value; } }
        [IgnoreDataMember]
        public AirportStatics Statics { get; set; }
        public Airport(AirportProfile profile)
        {
            this.Profile = profile;
            this.Income = 0;
            this.DestinationPassengers = new List<DestinationDemand>();
            this.DestinationCargo = new List<DestinationDemand>();
            this.Facilities = new List<AirlineAirportFacility>();
            this.Statistics = new AirportStatistics();
            this.Weather = new Weather[5];
            this.Terminals = new Terminals(this);
            this.Runways = new List<Runway>();
            this._Hubs = new List<Hub>();
            this.DestinationPassengerStatistics = new Dictionary<Airport, long>();
            this.DestinationCargoStatistics = new Dictionary<Airport, double>();
            this.LastExpansionDate = new DateTime(1900, 1, 1);
            this.Statics = new AirportStatics(this);
            this.AirlineContracts = new List<AirportContract>();

        }
        //adds a major destination to the airport
        public void addMajorDestination(string destination, int pax)
        {
            if (this.Profile.MajorDestionations.ContainsKey(destination))
                this.Profile.MajorDestionations[destination] = pax;
            else
                this.Profile.MajorDestionations.Add(destination, pax);
        }
        //returns a list of major destinations and pax
        public Dictionary<Airport, int> getMajorDestinations()
        {
            Dictionary<Airport, int> majorDestinations = new Dictionary<Airport, int>();

            foreach (KeyValuePair<string, int> md in this.Profile.MajorDestionations)
            {
                majorDestinations.Add(Airports.GetAirport(md.Key), md.Value);
            }

            return majorDestinations;
        }
        //clears the list of airline contracts
        public void clearAirlineContracts()
        {
            lock (this._Contracts)
            {
                this.AirlineContracts.Clear();
            }
        }
        //adds an airline airport contract to the airport
        public void addAirlineContract(AirportContract contract)
        {
            lock (this._Contracts)
            {
                this._Contracts.Add(contract);
            }

            if (!contract.Airline.Airports.Contains(this))
                contract.Airline.addAirport(this);
        }
        //removes an airline airport contract from the airport
        public void removeAirlineContract(AirportContract contract)
        {

            lock (this._Contracts)
            {
                this._Contracts.Remove(contract);
            }

            if (!this.AirlineContracts.Exists(c => c.Airline == contract.Airline))
                contract.Airline.removeAirport(this);
        }
        //returns the contracts for an airline
        public List<AirportContract> getAirlineContracts(Airline airline)
        {
            return this.AirlineContracts.FindAll(a => a.Airline == airline);
        }
        //return all airline contracts
        public List<AirportContract> getAirlineContracts()
        {
            List<AirportContract> contracts;
            lock (this._Contracts)
            {
                contracts = new List<AirportContract>(this._Contracts);
             
            }
            return contracts;
        }
        //returns the maximum value for the run ways
        public long getMaxRunwayLength()
        {
            if (this.Runways.Count == 0)
                return 0;

            var query = from r in this.Runways
                        where r.BuiltDate <= GameObject.GetInstance().GameTime
                        select r.Length;
            return query.Max();


        }
        //returns the destination passengers for a specific destination for a class
        public ushort getDestinationPassengersRate(Airport destination, AirlinerClass.ClassType type)
        {
            DestinationDemand pax = this.DestinationPassengers.Find(a => a.Destination == destination.Profile.IATACode);

            var values = Enum.GetValues(typeof(AirlinerClass.ClassType));
 
            int classFactor = 0;

            int i = 1;

            foreach (AirlinerClass.ClassType value in values)
            {
                if (value == type)
                   classFactor = i;
                i++;
            }
                          
            if (pax == null)
                return this.Statics.getDestinationPassengersRate(destination,type);
            else
            {
                return (ushort)(this.Statics.getDestinationPassengersRate(destination,type)+(ushort)(pax.Rate / classFactor));
            }
      
        }
        //returns the destination cargo for a specific destination
        public ushort getDestinationCargoRate(Airport destination)
        {
            DestinationDemand cargo = this.DestinationCargo.Find(a => a.Destination == destination.Profile.IATACode);

         
            if (cargo == null)
                return this.Statics.getDestinationCargoRate(destination);
            else
                return (ushort)(cargo.Rate + (ushort)this.Statics.getDestinationCargoRate(destination));
            
        }
        //adds a passenger rate for a destination
        public void addDestinationPassengersRate(DestinationDemand passengers)
        {
            this.Statics.addPassengerDemand(passengers);
          
        }
        //adds a cargo rate for a destination
        public void addDestinationCargoRate(DestinationDemand cargo)
        {
            this.Statics.addCargoDemand(cargo);
        }
        //adds a passenger rate value to a destination
        public void addDestinationPassengersRate(Airport destination, ushort rate)
        {

            lock (this.DestinationPassengers)
            {
             
                DestinationDemand destinationPassengers = getDestinationPassengersObject(destination);

                if (destinationPassengers != null)
                    destinationPassengers.Rate += rate;
                else
                    this.DestinationPassengers.Add(new DestinationDemand(destination.Profile.IATACode, rate));
            }
        }
        //adds a cargo rate value to a destination
        public void addDestinationCargoRate(Airport destination, ushort rate)
        {
            lock (this.DestinationCargo) 
            {
                
                DestinationDemand destinationCargo = getDestinationCargoObject(destination);

                if (destinationCargo != null)
                    destinationCargo.Rate += rate;
                else
                    this.DestinationCargo.Add(new DestinationDemand(destination.Profile.IATACode, rate));
            }
        }
        //returns all airports where the airport has demand
        public List<Airport> getDestinationDemands()
        {
            var destinations = new List<DestinationDemand>();

            destinations.AddRange(this.Statics.getDemands());
            destinations.AddRange(this.DestinationCargo);
            destinations.AddRange(this.DestinationPassengers);

            return destinations.Select(d => Airports.GetAirport(d.Destination)).Distinct().ToList();
        }
        //returns if the destination has passengers rate
        public Boolean hasDestinationPassengersRate(Airport destination)
        {
            return this.Statics.hasDestinationPassengersRate(destination) || this.DestinationPassengers.Exists(a => a.Destination == destination.Profile.IATACode);
        }
        //returns a destination passengers object
        public DestinationDemand getDestinationPassengersObject(Airport destination)
        {
            return this.DestinationPassengers.Find(a => a.Destination == destination.Profile.IATACode);
        }
        //returns a destination cargo object
        public DestinationDemand getDestinationCargoObject(Airport destination)
        {
            return this.DestinationCargo.Find(a => a.Destination == destination.Profile.IATACode);
        }
        //clears the destination passengers
        public void clearDestinationPassengers()
        {
            this.DestinationPassengers.Clear();
        }
        //returns all destination passengers
        public List<DestinationDemand> getDestinationsPassengers()
        {
            return this.DestinationPassengers;
        }
        //returns the sum of passenger demand
        public int getDestinationPassengersSum()
        {
            int sum;
            lock (this.DestinationPassengers)
            {
                sum = this.DestinationPassengers.Sum(d => d.Rate);
            }

            return sum + this.Statics.getDestinationPassengersSum();
        }
        //adds a number of passengers to destination to the statistics
        public void addPassengerDestinationStatistics(Airport destination, long passengers)
        {
            lock (this.DestinationPassengerStatistics)
            {
                if (!this.DestinationPassengerStatistics.ContainsKey(destination))
                    this.DestinationPassengerStatistics.Add(destination, passengers);
                else
                    this.DestinationPassengerStatistics[destination] += passengers;
            }

        }
        //adds a number of cargo to destination to the statistics
        public void addCargoDestinationStatistics(Airport destination, double cargo)
        {
            lock (this.DestinationCargoStatistics)
            {
                if (!this.DestinationCargoStatistics.ContainsKey(destination))
                    this.DestinationCargoStatistics.Add(destination, cargo);
                else
                    this.DestinationCargoStatistics[destination] += cargo;
            }
        }
        //clears the destination statistics
        public void clearDestinationPassengerStatistics()
        {
            this.DestinationPassengerStatistics.Clear();
        }
        //clears the destination cargo statistics
        public void clearDestinationCargoStatistics()
        {
            this.DestinationCargoStatistics.Clear();
        }

        //returns the number of passengers to a destination
        public long getDestinationPassengerStatistics(Airport destination)
        {
            long passengers;
            lock (this.DestinationPassengerStatistics)
            {
                if (this.DestinationPassengerStatistics.ContainsKey(destination))
                    passengers = this.DestinationPassengerStatistics[destination];
                else
                    passengers = 0;
            }

            return passengers;
        }
        //returns the number of cargo to a destination
        public double getDestinationCargoStatistics(Airport destination)
        {
            double cargo = 0;
            lock (this.DestinationCargoStatistics)
            {
                if (this.DestinationCargoStatistics.ContainsKey(destination))
                    cargo = this.DestinationCargoStatistics[destination];
                
            }
            return cargo;
        }
        //returns if the destination have statistics
        public Boolean hasDestinationPassengerStatistics(Airport destination)
        {
            return this.DestinationPassengerStatistics.ContainsKey(destination);
        }
        //returns if the destination have cargo statistics
        public Boolean hasDestinationCargStatistics(Airport destination)
        {
            return this.DestinationCargoStatistics.ContainsKey(destination);
        }
        //returns the price for a gate
        public long getGatePrice()
        {
            long sizeValue = 100 + 102 * ((int)this.Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(sizeValue));
        }
        //returns the fee for landing at the airport
        public double getLandingFee()
        {
            long sizeValue = 151 * ((int)this.Profile.Size + 1);
            return GeneralHelpers.GetInflationPrice(sizeValue);
        }
        //adds a facility to an airline
        public void addAirportFacility(Airline airline, AirportFacility facility, DateTime finishedDate)
        {
            lock (this.Facilities)
            {
                this.Facilities.Add(new AirlineAirportFacility(airline, this, facility, finishedDate));
            }
        }
        //sets the facility for an airline
        public void setAirportFacility(Airline airline, AirportFacility facility, DateTime finishedDate)
        {
            this.Facilities.RemoveAll(f => f.Airline == airline && f.Facility.Type == facility.Type);
            this.Facilities.Add(new AirlineAirportFacility(airline, this, facility, finishedDate));
        }
        //sets the facility for an airline
        public void setAirportFacility(AirlineAirportFacility facility)
        {
            this.Facilities.RemoveAll(f => f.Airline == facility.Airline && f.Facility.Type == facility.Facility.Type);
            this.Facilities.Add(facility);
        }
        //returns the facility of a specific type for an airline - useAirport == true if it should also check for the airports facility
        public AirportFacility getAirportFacility(Airline airline, AirportFacility.FacilityType type, Boolean useAirport = false)
        {
            if (!useAirport)
                return getAirlineAirportFacility(airline, type).Facility;
            else
            {
                AirportFacility airlineFacility = getCurrentAirportFacility(airline, type);
                AirportFacility airportFacility = getCurrentAirportFacility(null, type);

                return airportFacility == null || airlineFacility.TypeLevel > airportFacility.TypeLevel ? airlineFacility : airportFacility;
            }
        }
         //returns the current airport facility of a specific type for an airlines
        public AirportFacility getCurrentAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            List<AirportFacility> facilities = new List<AirportFacility>();

            var tFacilities = new List<AirlineAirportFacility>(this.Facilities);
            lock (this.Facilities)
            {

                facilities = (from f in tFacilities where f.Airline == airline && f.Facility.Type == type && f.FinishedDate <= GameObject.GetInstance().GameTime orderby f.Facility.TypeLevel descending select f.Facility).ToList();
                int numberOfFacilities = facilities.Count();

                if (numberOfFacilities == 0 && airline != null)
                {
                    var noneFacility = AirportFacilities.GetFacilities(type).Find(f => f.TypeLevel == 0);
                    this.addAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);

                    facilities.Add(noneFacility);

                }
             }
            return facilities.FirstOrDefault();
        }
        //return the airport facility for a specific type for an airline
        public AirlineAirportFacility getAirlineAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            List<AirlineAirportFacility> facilities = new List<AirlineAirportFacility>();
            lock (this.Facilities)
            {
                facilities = (from f in this.Facilities where f.Airline == airline && f.Facility.Type == type orderby f.Facility.TypeLevel descending select f).ToList();

                if (facilities.Count() == 0)
                {
                    AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find(f => f.TypeLevel == 0);
                    this.addAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);

                    facilities.Add(getAirlineAirportFacility(airline,type));
                  }

                
            }
            return facilities.FirstOrDefault();
        }
        //return all the facilities for an airline
        public List<AirportFacility> getCurrentAirportFacilities(Airline airline)
        {
            List<AirportFacility> fs = new List<AirportFacility>();
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {
                fs.Add(getCurrentAirportFacility(airline, type));
            }

            return fs;
        }
        //return all the facilities for an airline
        public List<AirlineAirportFacility> getAirportFacilities(Airline airline)
        {
            var fac = new List<AirlineAirportFacility>();

            lock (this.Facilities)
            {
                fac = this.Facilities.FindAll(f => f.Airline == airline);
            }

            return fac;
        }
        //returns all facilities
        public List<AirlineAirportFacility> getAirportFacilities()
        {
            return this.Facilities;
        }
        //returns if an airline has any facilities at the airport
        public Boolean hasFacilities(Airline airline)
        {
            Boolean hasFacilities = false;
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {
                if (getAirportFacility(airline, type).TypeLevel > 0)
                    hasFacilities = true;
            }
            return hasFacilities;
        }
        //returns if an airline has any facilities besides a specific type
        public Boolean hasFacilities(Airline airline, AirportFacility.FacilityType ftype)
        {
            Boolean hasFacilities = false;
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {
                if (type != ftype)
                    if (getAirportFacility(airline, type).TypeLevel > 0)
                        hasFacilities = true;
            }
            return hasFacilities;
        }
        //returns if an airline has any airliners with the airport as home base
        public Boolean hasAsHomebase(Airline airline)
        {
            foreach (FleetAirliner airliner in airline.Fleet)
                if (airliner.Homebase == this)
                    return true;

            return false;
        }
        //downgrades the facility for a specific type for an airline
        public void downgradeFacility(Airline airline, AirportFacility.FacilityType type)
        {
            AirportFacility currentFacility = getAirportFacility(airline, type);
            AirlineAirportFacility aaf = getAirlineAirportFacility(airline, type);

            List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);

            facilities.Sort((delegate(AirportFacility f1, AirportFacility f2) { return f1.TypeLevel.CompareTo(f2.TypeLevel); }));

            int index = facilities.IndexOf(getAirportFacility(airline, type));

            addAirportFacility(airline, facilities[index - 1], GameObject.GetInstance().GameTime);

            this.Facilities.Remove(aaf);

        }
        //clears the list of facilites
        public void clearFacilities()
        {
            this.Facilities = new List<AirlineAirportFacility>();
        }
        //cleares the list of facilities for an airline
        public void clearFacilities(Airline airline)
        {
            this.Facilities.RemoveAll(f => f.Airline == airline);

        }
        //returns all hubs of a specific type
        public List<Hub> getHubs(HubType.TypeOfHub type)
        {
            List<Hub> hubs;
            lock (this._Hubs)
                hubs = new List<Hub>(this._Hubs);
            
            return hubs.FindAll(h=>h.Type.Type == type);
        }
        //returns all hubs
        public List<Hub> getHubs()
        {
            List<Hub> hubs;
            lock (this._Hubs)
                hubs = new List<Hub>(this._Hubs);

            return hubs;
        }
        //adds a hub to the airport
        public void addHub(Hub hub)
        {
            this._Hubs.Add(hub);
        }
        //removes a hub
        public void removeHub(Hub hub)
        {
            this._Hubs.Remove(hub);
        }
        //returns if an airline have a hub
        public Boolean hasHub(Airline airline)
        {
            return this.Hubs.Exists(h => h.Airline == airline);
        }
        // chs, 2011-31-10 added for pricing of a terminal
        //returns the price for a terminal
        public long getTerminalPrice()
        {
            long price = 500000 + 15000 * ((int)this.Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));

        }
        //returns the price for a gate at a bough terminal
        public long getTerminalGatePrice()
        {
            long price = 75000 * ((int)this.Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));

        }
        // chs, 2011-27-10 added for the possibility of purchasing a terminal
        //adds a terminal to the airport
        public void addTerminal(Terminal terminal)
        {
            this.Terminals.addTerminal(terminal);
        }
        //removes a terminal from the airport
        public void removeTerminal(Terminal terminal)
        {
            AirportContract terminalContract = this.AirlineContracts.Find(c => c.Terminal != null && c.Terminal == terminal);

            if (terminalContract != null)
                removeAirlineContract(terminalContract);

            this.Terminals.removeTerminal(terminal);
        }
    }
    //the collection of airports
    public class Airports
    {
        private static List<Airport> airports = new List<Airport>();
        public static double LargestAirports, VeryLargeAirports, LargeAirports, MediumAirports, SmallAirports, VerySmallAirports, SmallestAirports;
        public static Dictionary<GeneralHelpers.Size, int> CargoAirportsSizes = new Dictionary<GeneralHelpers.Size,int>();
        //clears the list
        public static void Clear()
        {
            airports = new List<Airport>();
        }
        //returns if a specific airport is in the list
        public static Boolean Contains(Airport airport)
        {
            return airports.Contains(airport);
        }
        //adds an airport
        public static void AddAirport(Airport airport)
        {
            airports.Add(airport);
        }
        //returns an airport based on iata code
        public static Airport GetAirport(string iata)
        {
            var tAirports = airports.FindAll(a => a.Profile.IATACode == iata);

            if (tAirports.Count == 1)
                return tAirports[0];

            var tAirport = tAirports.Find(a => GeneralHelpers.IsAirportActive(a));

            if (tAirport == null)
                return null;
            else
                return tAirport;
        }
        //returns an airport based on match
        public static Airport GetAirport(Predicate<Airport> match)
        {
            return airports.Find(match);
        }
        //returns an airport based on id
        public static Airport GetAirportFromID(string id)
        {
            Airport airport = airports.Find(a => a.Profile.ID == id);

            if (airport != null)
                return airport;
            else
            {
                return airports.Find(a => a.Profile.ID.StartsWith(id.Substring(0, 8)));//airports.Find(a=>a.Profile.ID.StartsWith(id.Substring(0, id.LastIndexOf('-'))));
            }
        }
        //returns all active airports
        public static List<Airport> GetAllActiveAirports()
        {
            return airports.FindAll(a => GeneralHelpers.IsAirportActive(a));
        }
        //returns all airports
        public static List<Airport> GetAllAirports(Predicate<Airport> match)
        {
            return airports.FindAll(match);
        }
        public static List<Airport> GetAllAirports()
        {
            return airports; ;
        }
        //returns a possible match for coordinates
        public static Airport GetAirport(GeoCoordinate coordinates)
        {
            return GetAllActiveAirports().Find(a => a.Profile.Coordinates.Equals(coordinates));

        }
        //returns all airports with a specific size
        public static List<Airport> GetAirports(GeneralHelpers.Size size)
        {

            return GetAirports(delegate(Airport airport) { return airport.Profile.Size == size; });

        }
        //returns all airports from a specific country
        public static List<Airport> GetAirports(Country country)
        {
            return GetAirports(delegate(Airport airport) { return airport.Profile.Country == country; });

        }
        //returns all airports from a specific region
        public static List<Airport> GetAirports(Region region)
        {
            return GetAirports(delegate(Airport airport) { return airport.Profile.Country.Region == region; });

        }
        //returns a list of airports
        public static List<Airport> GetAirports(Predicate<Airport> match)
        {
            return GetAllActiveAirports().FindAll(match);
        }
        //returns the total number of airports
        public static int Count()
        {
            return GetAllActiveAirports().Count;
        }
        //returns the total number of airports with a given size
        public static double CountLargest()
        {
            return LargestAirports;
        }

        public static double CountVeryLarge()
        {
            return VeryLargeAirports;
        }

        public static double CountLarge()
        {
            return LargeAirports;
        }

        public static double CountMedium()
        {
            return MediumAirports;
        }

        public static double CountSmall()
        {
            return SmallAirports;
        }

        public static double CountVerySmall()
        {
            return VerySmallAirports;
        }

        public static double CountSmallest()
        {
            return SmallestAirports;
        }


        //removes all airports with a specific match
        public static void RemoveAirports(Predicate<Airport> match)
        {
            airports.RemoveAll(match);
        }
    }

}
