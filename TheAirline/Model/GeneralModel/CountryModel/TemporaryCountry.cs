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
        public enum TemporaryType { OneToMany, ManyToOne }
        public TemporaryType Type { get; set; }
        public Country CountryBefore { get; set; }
        public Country CountryAfter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Country> Countries { get; set; }
        public TemporaryCountry(TemporaryType type, Country country, DateTime startDate, DateTime endDate)
            : base(Country.Section, country.Uid, country.ShortName, country.Region, country.TailNumberFormat)
        {
            this.Type = type;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Countries = new List<Country>();
            this.CountryAfter = this;
            this.CountryBefore = this;
        }
        //returns the current country for the country
        public Country getCurrentCountry(DateTime date)
        {
            if (this.Type == TemporaryType.ManyToOne)
            {
                if (date < this.StartDate)
                    return this.CountryBefore;
                if (date >= this.StartDate && date < this.EndDate)
                    return this;

                return this.CountryAfter;
            }
            if (this.Type == TemporaryType.OneToMany)
            {
                if (date >= this.StartDate && date <= this.EndDate)
                    return this;
                else
                    return null;
            }
            return null;

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
        //returns a temporary country which a country is a part of
        public static TemporaryCountry GetTemporaryCountry(Country country)
        {
            TemporaryCountry tCountry = tCountries.Find(c => c.CountryBefore == country || c.CountryAfter == country || c.Countries.Contains(country));
            return tCountry;
        }
        //clears the list
        public static void Clear()
        {
            tCountries.Clear();
        }
    }

}
