using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.AirlinerModel;

namespace TheAirlineV2.Model.GeneralModel
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
        public Country(string name, string shortName,Region region, string tailNumberFormat)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Region = region;
            this.TailNumberFormat = tailNumberFormat;
            this.TailNumbers = new CountryTailNumber(this);
        }
        //the class for handling the tail numbers for the country
        public class CountryTailNumber
        {
            private string LastTailNumber;
            public Country Country { get; set; }
            public CountryTailNumber(Country country)
            {
                this.Country = country;

            }
            //returns if a tail number matches the country
            public Boolean isMatch(string tailNumber)
            {
                string countryID = this.Country.TailNumberFormat.Split('-')[0];
                string numberFormat = this.Country.TailNumberFormat.Split('-')[1];

                int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

                string tailID = tailNumber.Split('-')[0];
                string tailFormat = tailNumber.Split('-')[1];

                return tailID == countryID && tailFormat.Length == length; 

            }
            //returns the next tail number
            public string getNextTailNumber()
            {
                string countryID = this.Country.TailNumberFormat.Split('-')[0];
                string numberFormat = this.Country.TailNumberFormat.Split('-')[1];

                int length = Convert.ToInt16(numberFormat.Substring(numberFormat.Length - 1));

                if (numberFormat.Contains("\\d"))
                {
                    int number;
                    if (LastTailNumber == null)
                        number = 0;
                    else
                        number = Convert.ToInt32(this.LastTailNumber.Split('-')[1]) + 1;
                    string format = countryID + "-{0:";
                    for (int i = 0; i < length; i++)
                        format += "0";
                    format += "}";
                    this.LastTailNumber = String.Format(format, number);
                }
                if (numberFormat.Contains("\\s"))
                {
                    if (LastTailNumber == null)
                    {
                        string code = countryID + "-";
                        for (int i = 0; i < length; i++)
                            code += "A";
                        this.LastTailNumber = code;
                    }
                    else
                    {
                        string lastCode = this.LastTailNumber.Split('-')[1];


                        int i = 0;
                        Boolean found = false;
                        while (!found && i < length)
                        {
                            if (lastCode[lastCode.Length - 1 - i] < 'Z')
                                found = true;
                            else
                                i++;
                        }
                   
              
                        char replaceChar = lastCode[lastCode.Length - 1 - i];
                        replaceChar++;

                        string newCode = lastCode.Substring(0, length - i - 1) + replaceChar + lastCode.Substring(length - i);

                        this.LastTailNumber = countryID + "-" + newCode;
                    }
                }
                return this.LastTailNumber;
            }
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
    //the class for a region
    public class Region
    {
        public string Name { get; set; }
        public Region(string name)
        {
            this.Name = name;
        }
    }
    //the collection of regions
    public class Regions
    {
        private static Dictionary<string, Region> regions = new Dictionary<string, Region>();
        //clears the list
        public static void Clear()
        {
            regions = new Dictionary<string, Region>();
        }
        //adds a region to the collection
        public static void AddRegion(Region region)
        {
            regions.Add(region.Name,region);
        }
        //returns a region from the collection
        public static Region GetRegion(string name)
        {
            if (regions.ContainsKey(name))
                return regions[name];
            else
                return null;
        }
        //returns the list of regions
        public static List<Region> GetRegions()
        {
            return regions.Values.ToList();
        }
      
    }
    //the class for a time zone
    public class GameTimeZone
    {
        public TimeSpan UTCOffset { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get { return getDisplayName(); } set { ;} }
        public string ShortDisplayName { get { return getShortDisplayName(); } set { ;} }
        public GameTimeZone(string name, string shortName, TimeSpan utcOffset)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.UTCOffset = utcOffset;
        }
        //returns the display name
        private string getDisplayName()
        {
            return string.Format("{0} (UTC{1}{2:D2}:{3:D2})", this.Name, this.UTCOffset.Hours < 0 ? "" : "+", this.UTCOffset.Hours, this.UTCOffset.Minutes);
        }
        //returns the short display name
        private string getShortDisplayName()
        {
            return string.Format("{0} (UTC{1}{2:D2}:{3:D2})", this.ShortName, this.UTCOffset.Hours < 0 ? "" : "+", this.UTCOffset.Hours, this.UTCOffset.Minutes);
      
        }
    }
    //the list of time zones
    public class TimeZones
    {
        private static List<GameTimeZone> timeZones = new List<GameTimeZone>();
        //clears the list
        public static void Clear()
        {
            timeZones.Clear();
        }
        //adds a time zone to the list
        public static void AddTimeZone(GameTimeZone tz)
        {
            timeZones.Add(tz);
        }
        //returns the list of time zones
        public static List<GameTimeZone> GetTimeZones()
        {
            return timeZones;
        }
    }
}

