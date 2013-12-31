
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using System.Device.Location;

namespace TheAirline.Model.GeneralModel.CountryModel.TownModel
{
    //the class for a town / city
    [Serializable]
    public class Town
    {
        
        public string Name { get; set; }
        
        public Country Country { get; set; }
        
        public State State { get; set; }
        public int Population { get; set; }
        public GeoCoordinate Coordinates { get; set; }
        public Town(string name, Country country) : this(name,country,null)
        {
            
        }
        public Town(string name, Country country, State state)
        {
          
            this.Name = name;
            this.Country = country;
            this.State = state;
        }
        public static bool operator ==(Town a, Town b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Name == b.Name && a.Name == b.Name && ((a.State == null && b.State == null) || (a.State == b.State));
        }
        public static bool operator !=(Town a,Town b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Country.GetHashCode();
        }
        public override bool Equals(object u)
        {

            // If parameter is null return false:
            if (u == null || !(u is Town))
            {
                return false;
            }

            // Return true if the fields match:
            return (this == (Town)u);
        }

        /*
         * returns a list of nearby towns
         *  this can be replaced with a foreach/if loop depending on performance 
        */
        public List<Town> GetNearbyTowns(int d)
        {
            List<Town> towns = new List<Town>();
            foreach (Town t in Towns.GetTowns())
            {
                if(this.Coordinates.GetDistanceTo(t.Coordinates) < d )
                {
                    towns.Add(t);
                }
            }
            return towns;
        }


        /*
         * returns a list of nearby airports
         * again, can be replaced with a foreach/if for performance
         */
        public List<Airport> GetNearbyAirports(int d)
        {
            List<Airport> airports = Airports.GetAllActiveAirports();
            var nearby =
                from a in airports
                where this.Coordinates.GetDistanceTo(a.Profile.Coordinates) > d
                select a;
            foreach (Airport a in nearby) { airports.Remove(a); }
            return airports;
        }

    }
    //the class which finds a town based on name
    public class Towns
    {
        //returns all towns
        public static List<Town> GetTowns()
        {
            return Airports.GetAllAirports().Select(a => a.Profile.Town).Distinct().ToList();
        }
        //returns all towns from a specific country
        public static List<Town> GetTowns(Country country)
        {
            return Airports.GetAllAirports().Where(a=>a.Profile.Country == country).Select(a => a.Profile.Town).Distinct().ToList();
        }
        //returns a town
        public static Town GetTown(string name)
        {
            if (name.Contains(','))
            {
                string town = name.Split(',')[0].Trim();
                string state = name.Split(',')[1].Trim();

                if (Airports.GetAirport(a => a.Profile.Town.Name == town && a.Profile.Town.State != null && a.Profile.Town.State.ShortName == state) == null)
                    return null;

                return Airports.GetAirport(a => a.Profile.Town.Name == town && a.Profile.Town.State != null && a.Profile.Town.State.ShortName == state).Profile.Town;
            }
            else
            {
                if (Airports.GetAirport(a => a.Profile.Town.Name == name) == null)
                    return null;

                return Airports.GetAirport(a => a.Profile.Town.Name == name).Profile.Town;
            }
        }
        public static Town GetTown(string name, State state)
        {
            return Airports.GetAirport(a => a.Profile.Town.Name == name && a.Profile.Town.State == state).Profile.Town;
        }
    }
   
}
