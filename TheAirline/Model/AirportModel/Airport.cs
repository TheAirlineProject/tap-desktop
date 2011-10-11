using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlinerModel;


namespace TheAirline.Model.AirportModel
{
    //the class for an airport
    public class Airport
    {
        public AirportProfile Profile { get; set; }
        public Dictionary<Airport, int> Passengers { get; set; }
        public AirportStatistics Statistics { get; set; }
        public Gates Gates { get; set; }
        public Dictionary<Airline, Dictionary<AirportFacility.FacilityType, AirportFacility>> Facilities { get; private set; }
        public Weather Weather { get; set; }
        public Airport(AirportProfile profile)
        {
            this.Profile = profile;
            this.Gates = new Gates(this);
            this.Passengers = new Dictionary<Airport, int>();
            this.Facilities = new Dictionary<Airline, Dictionary<AirportFacility.FacilityType, AirportFacility>>();
            this.Statistics = new AirportStatistics();
            this.Weather = new Weather();
        }

        //clears the list of passengers
        public void clearPassengers()
        {
            this.Passengers = new Dictionary<Airport, int>();
        }
        /*
        //creates the passengers at the airport
        public void createPassengers(int coff)
        {
            for (int i = 0; i < coff*(int)this.Profile.Size; i++)
            {
                Airport airport = GameHelpers.GetRandomAirport(this);
                if (!this.Passengers.ContainsKey(airport))
                    this.Passengers.Add(airport, 0);
                this.Passengers[airport]++;
            }
        }
         * */
        //returns the number of passengers for a given destination
        public int getPassengers(Airport airport)
        {
            /*
            if (this.Passengers.ContainsKey(airport))
                return this.Passengers[airport];
            else
                return 0;
             * */
            Random rnd = new Random();

            int value = rnd.Next((int)this.Profile.Size * 100);
            return value;
        }
        //removes the number of passengers for a given destination
        public void removePassengers(Airport airport, int number)
        {
            if (this.Passengers.ContainsKey(airport))
                this.Passengers[airport] -= number;
        }
        //adds the number of passengers to a given destination
        public void addPassengers(Airport airport, int number)
        {
            if (!this.Passengers.ContainsKey(airport))
                this.Passengers.Add(airport, 0);
            this.Passengers[airport] += number;
        }
        //returns the price for a gate
        public long getGatePrice()
        {
            long sizeValue = 1000 + 1023 * ((int)this.Profile.Size + 1);
            return sizeValue;
        }
        //returns the fee for landing at the airport
        public double getLandingFee()
        {
            long sizeValue = 1543 * ((int)this.Profile.Size + 1);
            return sizeValue;
        }
        //sets a facility to an airline
        public void setAirportFacility(Airline airline, AirportFacility facility)
        {
            if (!this.Facilities.ContainsKey(airline))
                this.Facilities.Add(airline, new Dictionary<AirportFacility.FacilityType, AirportFacility>());
            if (!this.Facilities[airline].ContainsKey(facility.Type))
                this.Facilities[airline].Add(facility.Type, facility);
            else
                this.Facilities[airline][facility.Type] = facility;
        }
        //returns the facility of a specific type for an airline
        public AirportFacility getAirportFacility(Airline airline, AirportFacility.FacilityType type)
        {
            return this.Facilities[airline][type];
        }
        //return all the facilities for an airline
        public List<AirportFacility> getAirportFacilities(Airline airline)
        {
            List<AirportFacility> facilities = new List<AirportFacility>();
            foreach (AirportFacility.FacilityType type in this.Facilities[airline].Keys)
                facilities.Add(this.Facilities[airline][type]);

            return facilities;
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

            List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);

            facilities.Sort((delegate(AirportFacility f1, AirportFacility f2) { return f1.TypeLevel.CompareTo(f2.TypeLevel); }));

            int index = facilities.IndexOf(this.getAirportFacility(GameObject.GetInstance().HumanAirline, type));

            setAirportFacility(airline, facilities[index - 1]);



        }

    }
    //the collection of airports
    public class Airports
    {
        private static Dictionary<string, Airport> airports = new Dictionary<string, Airport>();
        //clears the list
        public static void Clear()
        {
            airports = new Dictionary<string, Airport>();
        }
        //adds an airport
        public static void AddAirport(Airport airport)
        {
            airports.Add(airport.Profile.IATACode, airport);
        }
        //returns an airport based on iata code
        public static Airport GetAirport(string iata)
        {
            if (airports.ContainsKey(iata))
                return airports[iata];
            else
                return null;
        }
        //returns a possible match for coordinates
        public static Airport GetAirport(Coordinates coordinates)
        {
            foreach (Airport airport in GetAirports())
                if (airport.Profile.Coordinates.CompareTo(coordinates) == 0)
                    return airport;
            return null;
        }
        //returns all airports with a specific size
        public static List<Airport> GetAirports(AirportProfile.AirportSize size)
        {
         
            return GetAirports().FindAll((delegate(Airport airport) { return airport.Profile.Size == size; }));
          
        }
        //returns all airports from a specific country
        public static List<Airport> GetAirports(Country country)
        {
             return GetAirports().FindAll((delegate(Airport airport) { return airport.Profile.Country == country; }));
  
        }
        //returns all airports from a specific region
        public static List<Airport> GetAirports(Region region)
        {
             return GetAirports().FindAll((delegate(Airport airport) { return airport.Profile.Country.Region == region; }));
  
        }
        //returns the list of airports
        public static List<Airport> GetAirports()
        {
            return airports.Values.ToList();
        }


    }
  
}
