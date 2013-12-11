using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using System.IO;
using TheAirline.Model.GeneralModel.CountryModel;
using System.Runtime.Serialization;
using TheAirline.GUIModel.HelpersModel;


namespace TheAirline.Model.GeneralModel
{
    //the class for a country
   [DataContract]
   [KnownType(typeof(TemporaryCountry))]
   [KnownType(typeof(TerritoryCountry))]
 
    public class Country : BaseUnit
    {
        public static string Section { get; set; }
        [DataMember]
        public Region Region { get; set; }
        //the format used for the tail number
        [DataMember]
        public string TailNumberFormat { get; set; }
        [DataMember]
        public CountryTailNumber TailNumbers { get; set; }
        [DataMember]
        public List<CountryCurrency> Currencies { get; set; }
        public Boolean HasLocalCurrency { get { return this.Currencies.Count > 0; } private set { ;} }
        public Country(string section, string uid, string shortName, Region region, string tailNumberFormat) : base(uid,shortName)
        {
            Country.Section = section;
            this.Region = region;
            this.TailNumberFormat = tailNumberFormat;
            this.TailNumbers = new CountryTailNumber(this);
            this.Currencies = new List<CountryCurrency>();
        }
        //adds a currency to the country
        public void addCurrency(CountryCurrency currency)
        {
            this.Currencies.Add(currency);
        }
        //returns the current currency
        public CountryCurrency getCurrency(DateTime date)
        {
            if (this.Currencies.Exists(c => c.DateFrom <= date && c.DateTo > date))
                return this.Currencies.Find(c => c.DateFrom <= date && c.DateTo > date);

            return null;
        }
        public override string Name
        {
            get
            {
                return Translator.GetInstance().GetString(Country.Section, this.Uid); ;
            }
        }
        /*
        public static bool operator ==(Country a, Country b)
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
            if (a is TerritoryCountry && b is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry)a).MainCountry.Uid == b.Uid || a.Uid == ((TerritoryCountry)b).MainCountry.Uid || ((TerritoryCountry)a).MainCountry.Uid == ((TerritoryCountry)b).MainCountry.Uid;
            }
            if (a is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry)a).MainCountry.Uid == b.Uid;
            }
            if (b is TerritoryCountry)
            {
                return a.Uid == b.Uid || a.Uid == ((TerritoryCountry)b).MainCountry.Uid;
            }
            
            return a.Uid == b.Uid;//a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Country a, Country b)
        {
            return !(a == b);
        }
         * */
      
    }
    [Serializable]
    //the class for a country which is a territory of another country
    public class TerritoryCountry : Country
    {
        public Country MainCountry { get; set; }
        public TerritoryCountry(string section, string uid, string shortName, Region region, string tailNumberFormat, Country mainCountry)
            : base(section,uid,shortName,region,tailNumberFormat)
        {
            this.MainCountry = mainCountry;
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
                if (TemporaryCountries.GetCountry(uid) != null)
                    return TemporaryCountries.GetCountry(uid);
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
            //netto.AddRange(TemporaryCountries.GetCountries());
            netto.Remove(GetCountry("100"));

            List<Country> tCountries = new List<Country>();
            foreach (Country country in netto)
                if (!(country is TerritoryCountry))
                    tCountries.Add((Country)new CountryCurrentCountryConverter().Convert(country));

            foreach (Country country in TemporaryCountries.GetCountries())
            {
                if (((TemporaryCountry)country).Type == TemporaryCountry.TemporaryType.ManyToOne)
                    tCountries.Add((Country)new CountryCurrentCountryConverter().Convert(country));
            }

            return tCountries.Distinct().ToList();
        }

        //returns the list of countries
        public static List<Country> GetAllCountries()
        {
            List<Country> tCountries = countries.Values.ToList();
            tCountries.AddRange(TemporaryCountries.GetCountries());
            return tCountries;
        }

        //returns the list of countries from a region
        public static List<Country> GetCountries(Region region)
        {
            return GetCountries().FindAll((delegate(Country country) { return country.Region == region; }));
        }
        //returns the number of countries
        public static int Count()
        {
            return countries.Values.Count - 1;//removes 100 (All)
        }
    }
}

