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
