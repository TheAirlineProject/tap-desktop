using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;


namespace TheAirline.Model.AirportModel
{
    //the class for an airport
    public class Airport
    {
        public AirportProfile Profile { get; set; }
       // private List<Passenger> Passengers;
        private List<DestinationPassengers> DestinationPassengers { get; set; }
        private Dictionary<Airport, long> DestinationStatistics { get; set; }
        private List<AirlineAirportFacility> Facilities;
        public AirportStatistics Statistics { get; set; }
        public Weather Weather { get; set; }
        public List<Runway> Runways { get; set; }
        public Terminals Terminals { get; set; }
        public List<Hub> Hubs { get; set; }
        public Boolean IsHub { get { return this.Hubs.Count > 0; } set { ;} }
        public long Income{ get; set; }
        public Airport(AirportProfile profile)
        {
            this.Profile = profile;
            this.Income = 0;
            this.DestinationPassengers = new List<DestinationPassengers>();
            this.Facilities = new List<AirlineAirportFacility>();
            this.Statistics = new AirportStatistics();
            this.Weather = new Weather();
            this.Terminals = new Terminals(this);
            this.Runways = new List<Runway>();
            this.Hubs = new List<Hub>();
            this.DestinationStatistics = new Dictionary<Airport, long>();

            /*
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {

                AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                oldAirport.setAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
            }
             * */
         }

        //returns the maximum value for the run ways
        public long getMaxRunwayLength()
        {
            var query = from r in this.Runways
                        select r.Length;
            return query.Max();
       

        }
        //returns the destination passengers for a specific destination for a class
        public GeneralHelpers.Rate getDestinationPassengersRate(Airport destination, AirlinerClass.ClassType type)
        {
            if (this.DestinationPassengers.Find(a => a.Destination == destination) == null)
                return GeneralHelpers.Rate.None;
            else
                return this.DestinationPassengers.Find(a => a.Destination == destination).Rate;
        }
        //adds a rate for a destination
        public void addDestinationPassengersRate(DestinationPassengers passengers)
        {
            this.DestinationPassengers.Add(passengers);
        }
        //returns a destination passengers object
        public DestinationPassengers getDestinationPassengersObject(Airport destination)
        {
            return this.DestinationPassengers.Find(a => a.Destination==destination);
        }
        //clears the destination passengers
        public void clearDestinationPassengers()
        {
            this.DestinationPassengers.Clear();
        }
        //adds a number of passengers to destination to the statistics
        public void addDestinationStatistics(Airport destination, long passengers)
        {
            if (!this.DestinationStatistics.ContainsKey(destination))
                this.DestinationStatistics.Add(destination, passengers);
            else
                this.DestinationStatistics[destination] += passengers;

        }
        //clears the destination statistics
        public void clearDestinationStatistics()
        {
            this.DestinationStatistics.Clear();
        }
        //returns the number of passengers to a destination
        public long getDestinationStatistics(Airport destination)
        {
            if (this.DestinationStatistics.ContainsKey(destination))
                return this.DestinationStatistics[destination];
            else
                return 0;
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
        //sets a facility to an airline
        public void setAirportFacility(Airline airline, AirportFacility facility, DateTime finishedDate)
        {
          
                this.Facilities.Add(new AirlineAirportFacility(airline,this, facility, finishedDate));
         }
      
        //returns the facility of a specific type for an airline
        public AirportFacility getAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
           
            return getAirlineAirportFacility(airline, type).Facility;
        }
        //returns the current airport facility of a specific type for an airlines
        public AirportFacility getCurrentAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            
            // AirportFacility facility = this.Facilities.Find(f => f.Airline == airline && f.Facility.Type == type && f.FinishedDate <= GameObject.GetInstance().GameTime).Facility;
            return (from f in this.Facilities where f.Airline == airline && f.Facility.Type == type && f.FinishedDate <= GameObject.GetInstance().GameTime orderby f.Facility.TypeLevel descending select f.Facility).First();
       
        }
        //return the airport facility for a specific type for an airline
        public AirlineAirportFacility getAirlineAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            return (from f in this.Facilities where f.Airline == airline && f.Facility.Type == type orderby f.Facility.TypeLevel descending select f).First();
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
            return this.Facilities.FindAll(f => f.Airline == airline);
            /*
            List<AirlineAirportFacility> fs = new List<AirlineAirportFacility>();
            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {
                fs.Add(getAirlineAirportFacility(airline, type));
            }

            return fs;
             * */
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

            setAirportFacility(airline, facilities[index - 1],GameObject.GetInstance().GameTime);

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

        //returns the price for a hub
        public long getHubPrice()
        {
            long price = 50000 + 25000 * ((int)this.Profile.Size);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));
        }
        // chs, 2011-31-10 added for pricing of a terminal
        //returns the price for a terminal
        public long getTerminalPrice()
        {
            long price = 200000 + 15000 * ((int)this.Profile.Size + 1);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));
   
        }
        //returns the price for a gate at a bough terminal
        public long getTerminalGatePrice()
        {
            long price = 12500 * ((int)this.Profile.Size + 1);
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
            this.Terminals.removeTerminal(terminal);
        }
    }
    //the collection of airports
    public class Airports
    {
        private static List<Airport> airports = new List<Airport>();
        //clears the list
        public static void Clear()
        {
            airports = new List<Airport>();
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
        //returns an airport based on id
        public static Airport GetAirportFromID(string id)
        {
            return airports.Find(a => a.Profile.ID == id);
        }
        //returns all active airports
        public static List<Airport> GetAllActiveAirports()
        {
            return airports.FindAll(a=>GeneralHelpers.IsAirportActive(a));
        }
        //returns all airports
        public static List<Airport> GetAllAirports(Predicate<Airport> match)
        {
            return airports.FindAll(match);
        }
        public static List<Airport> GetAllAirports()
        {
            return airports;
        }
        //returns a possible match for coordinates
        public static Airport GetAirport(Coordinates coordinates)
        {
            return GetAllActiveAirports().Find(a => a.Profile.Coordinates.CompareTo(coordinates) == 0);
      
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

    }
  
}
