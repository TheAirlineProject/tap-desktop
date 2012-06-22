using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel
{
    /// <summary>
    /// The class for a temporary country
    /// </summary>
    public class TemporaryCountry : Country
    {
        public Country CountryBefore { get; set; }
        public Country CountryAfter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TemporaryCountry(Country country, DateTime startDate, DateTime endDate, Country countryBefore, Country countryAfter)
            : base(Country.Section, country.Uid, country.ShortName, country.Region, country.TailNumberFormat)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CountryAfter = countryAfter;
            this.CountryBefore = countryBefore;
        }
        //returns the current country for the country
        public Country getCurrentCountry(DateTime date)
        {

            if (date < this.StartDate)
                return this.CountryBefore;
            if (date >= this.StartDate && date < this.EndDate)
                return this;

            return this.CountryAfter;


        }
    }
    //the list of temporary countries
    public class TemporaryCountries
    {
        private static List<TemporaryCountry> tCountries = new List<TemporaryCountry>();
        //adds a country to the list
        public static void AddCountry(TemporaryCountry country)
        {
            tCountries.Add(country);
        }
        //returns all temporary countries
        public static List<Country> GetCountries()
        {
            List<Country> lCountries = new List<Country>();
            foreach (TemporaryCountry country in tCountries)
                lCountries.Add(country);
            return lCountries;

        }
        //returns a country
        public static Country GetCountry(string uid)
        {
            TemporaryCountry country = tCountries.Find(t => t.Uid == uid);
            return country;

        }
        //clears the list
        public static void Clear()
        {
            tCountries.Clear();
        }
    }

}
