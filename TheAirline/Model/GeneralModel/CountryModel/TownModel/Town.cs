using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel.TownModel
{
    //the class for a town / class
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
   
}
