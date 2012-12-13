using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.CountryModel.TownModel
{
    //the class for a town / city
    public class Town
    {
        public string Name { get; set; }
        public Country Country { get; set; }
        public State State { get; set; }
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
            return a.Name == b.Name && a.Country == b.Country;
        }
        public static bool operator !=(Town a,Town b)
        {
            return !(a == b);
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
        //returns a town
        public static Town GetTown(string name)
        {
            return Airports.GetAirport(a => a.Profile.Town.Name == name).Profile.Town;
        }
        public static Town GetTown(string name, State state)
        {
            return Airports.GetAirport(a => a.Profile.Town.Name == name && a.Profile.Town.State == state).Profile.Town;
        }
    }
   
}
