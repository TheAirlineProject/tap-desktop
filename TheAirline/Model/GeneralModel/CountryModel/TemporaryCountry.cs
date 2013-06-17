
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace TheAirline.Model.GeneralModel
{
    /// <summary>
    /// The class for a temporary country
    /// </summary>
    ///
    [Serializable]
    public class TemporaryCountry : Country
    {
        
        public enum TemporaryType { OneToMany, ManyToOne }
        public TemporaryType Type { get; set; }
        public Country CountryBefore { get; set; }
        public Country CountryAfter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OneToManyCountry> Countries { get; set; }
        public TemporaryCountry(TemporaryType type, Country country, DateTime startDate, DateTime endDate)
            : base(Country.Section, country.Uid, country.ShortName, country.Region, country.TailNumberFormat)
        {
            this.Type = type;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Countries = new List<OneToManyCountry>();
            this.CountryAfter = this;
            this.CountryBefore = this;
        }
        //returns the current country for the country
        public Country getCurrentCountry(DateTime date, Country originalCountry)
        {
            if (this.Type == TemporaryType.ManyToOne)
            {
                if (date < this.StartDate)
                    return this.CountryBefore;
                if (date >= this.StartDate && date <= this.EndDate)
                    return this;
                if (date > this.EndDate)
                    return this.CountryAfter;
            }
            if (this.Type == TemporaryType.OneToMany)
            {
                OneToManyCountry tCountry = this.Countries.Find(c => c.Country == originalCountry);

                if (tCountry == null)
                    return originalCountry;

                if (date >= tCountry.StartDate && date <= tCountry.EndDate)
                    return this;
                else
                    return originalCountry;
            }
            return null;

        }
    }
    [Serializable]
    //the class for a one to many temporary country
    public class OneToManyCountry
    {
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public Country Country { get; set; }
        public OneToManyCountry(Country country, DateTime startDate, DateTime endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Country = country;
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
        public static TemporaryCountry GetTemporaryCountry(Country country,DateTime date)
        {
            TemporaryCountry tCountry = tCountries.Find(c => c.StartDate<date && c.EndDate>date && (c.CountryBefore == country || c.CountryAfter == country || c.Countries.Find(tc=>tc.Country.Uid==country.Uid)!=null));
            return tCountry;
        }
        //clears the list
        public static void Clear()
        {
            tCountries.Clear();
        }
    }

}
