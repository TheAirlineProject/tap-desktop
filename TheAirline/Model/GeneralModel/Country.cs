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
        public static string Section { get; set; }
        public string Uid { get; set; }
        public string ShortName { get; set; }
        public Region Region { get; set; }
        //the format used for the tail number
        public string TailNumberFormat { get; set; }
        public CountryTailNumber TailNumbers { get; set; }
        public string Flag { get; set; }
        public Country(string section, string uid, string shortName, Region region, string tailNumberFormat)
        {
            Country.Section = section;
            this.Uid = uid;
            this.ShortName = shortName;
            this.Region = region;
            this.TailNumberFormat = tailNumberFormat;
            this.TailNumbers = new CountryTailNumber(this);
        }

        public string Name
        {
            get { return Translator.GetInstance().GetString(Country.Section, this.Uid); }
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
            countries.Add(country.Uid, country);
        }

        //retuns a country
        public static Country GetCountry(string uid)
        {
            if (countries.ContainsKey(uid))
                return countries[uid];
            else
                return null;
        }

        //returns a country with a specific tailnumberformat
        public static Country GetCountryFromTailNumber(string tailnumber)
        {
            return GetCountries().Find(co => co.TailNumbers.isMatch(tailnumber));


        }

        //returns the list of countries
        public static List<Country> GetCountries()
        {
            List<Country> netto = countries.Values.ToList();
            netto.Remove(GetCountry("100"));
            return netto;
        }

        //returns the list of countries
        public static List<Country> GetAllCountries()
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

