using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    //the profile for an airline
    public class AirlineProfile
    {
        public string Name { get; set; }
        public string CEO { get; set; }
        public string IATACode { get; set; }
        public Country Country { get; set; }
        public string Color { get; set; }
        public string Logo { get; set; }
        public AirlineProfile(string name, string iata, string color, Country country, string ceo)
        {
            this.Name = name;
            this.IATACode = iata;
            this.CEO = ceo;
            this.Color = color;
            this.Country = country;
        }
    }
}
