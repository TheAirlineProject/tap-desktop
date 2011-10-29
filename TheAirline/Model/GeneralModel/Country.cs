using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using System.IO;

namespace TheAirline.Model.GeneralModel
{
    //the class for a country
    public class Country
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Region Region { get; set; }
        //the format used for the tail number
        public string TailNumberFormat { get; set; }
        public CountryTailNumber TailNumbers { get; set; }
        public string Flag { get; set; }
        public Country(string name, string shortName, Region region, string tailNumberFormat)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Region = region;
            this.TailNumberFormat = tailNumberFormat;
            this.TailNumbers = new CountryTailNumber(this);
        }
    }
    
    //the collection of countries
    public class Countries
    {
        private static Dictionary<string, Country> countries = new Dictionary<string, Country>();
        
        //clears the list
        public static void Clear()
        {
            countries = new Dictionary<string, Country>();
        }

        //adds a country to the list
        public static void AddCountry(Country country)
        {
            countries.Add(country.Name, country);
        }

        //retuns a country
        public static Country GetCountry(string name)
        {
            if (countries.ContainsKey(name))
                return countries[name];
            else
                return null;
        }

        //returns a country with a specific tailnumberformat
        public static Country GetCountryFromTailNumber(string tailnumber)
        {
            return GetCountries().Find((delegate(Country country) { return country.TailNumbers.isMatch(tailnumber); }));
        }

        //returns the list of countries
        public static List<Country> GetCountries()
        {
            return countries.Values.ToList();
        }

        //returns the list of countries from a region
        public static List<Country> GetCountries(Region region)
        {
            return GetCountries().FindAll((delegate(Country country) { return country.Region == region; }));
        }
    }
}

