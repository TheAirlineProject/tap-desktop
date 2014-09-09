﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TheAirline.GUIModel.HelpersModel;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for a country
    [Serializable]
    public class Country : BaseUnit
    {
        #region Constructors and Destructors

        public Country(string section, string uid, string shortName, Region region, string tailNumberFormat)
            : base(uid, shortName)
        {
            Section = section;
            Region = region;
            TailNumberFormat = tailNumberFormat;
            TailNumbers = new CountryTailNumber(this);
            Currencies = new List<CountryCurrency>();
        }

        protected Country(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (entry.Name.ToLower() == "region")
                    {
                        var region = ((Region) entry.Value);

                        Region = Regions.GetRegion(region.Uid);
                    }
                    else
                    {
                        if (prop is FieldInfo)
                        {
                            ((FieldInfo) prop).SetValue(this, entry.Value);
                        }
                        else
                        {
                            ((PropertyInfo) prop).SetValue(this, entry.Value);
                        }
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning) notSet.GetCustomAttribute(typeof (Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("currencies")]
        public List<CountryCurrency> Currencies { get; set; }

        public Boolean HasLocalCurrency
        {
            get { return Currencies.Count > 0; }
        }

        public override string Name
        {
            get
            {
                return Translator.GetInstance().GetString(Section, Uid);
            }
        }

        [Versioning("region")]
        public Region Region { get; set; }

        //the format used for the tail number
        [Versioning("tailnumberformat")]
        public string TailNumberFormat { get; set; }

        [Versioning("tailnumbers")]
        public CountryTailNumber TailNumbers { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                      .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                          .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo) member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo) member).GetValue(this, null);
                }

                var att = (Versioning) member.GetCustomAttribute(typeof (Versioning));

                info.AddValue(att.Name, propValue);
            }

            base.GetObjectData(info, context);
        }

        //adds a currency to the country
        public void AddCurrency(CountryCurrency currency)
        {
            Currencies.Add(currency);
        }

        //returns the current currency
        public CountryCurrency GetCurrency(DateTime date)
        {
            if (Currencies.Exists(c => c.DateFrom <= date && c.DateTo > date))
            {
                return Currencies.Find(c => c.DateFrom <= date && c.DateTo > date);
            }

            return null;
        }

        #endregion
    }

    [Serializable]
    //the class for a country which is a territory of another country
    public class TerritoryCountry : Country
    {
        #region Constructors and Destructors

        public TerritoryCountry(
            string section,
            string uid,
            string shortName,
            Region region,
            string tailNumberFormat,
            Country mainCountry)
            : base(section, uid, shortName, region, tailNumberFormat)
        {
            MainCountry = mainCountry;
        }

        private TerritoryCountry(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            int version = info.GetInt16("version");

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    GetType()
                        .GetProperties()
                        .Where(
                            p =>
                            p.GetCustomAttribute(typeof (Versioning)) != null
                            && ((Versioning) p.GetCustomAttribute(typeof (Versioning))).AutoGenerated));

            foreach (SerializationEntry entry in info)
            {
                PropertyInfo prop =
                    props.FirstOrDefault(p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    prop.SetValue(this, entry.Value);
                }
            }

            IEnumerable<PropertyInfo> notSetProps =
                props.Where(p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Version > version);

            foreach (PropertyInfo prop in notSetProps)
            {
                var ver = (Versioning) prop.GetCustomAttribute(typeof (Versioning));

                if (ver.AutoGenerated)
                {
                    prop.SetValue(this, ver.DefaultValue);
                }
            }
        }

        #endregion

        #region Public Properties

        [Versioning("maincountry")]
        public Country MainCountry { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = GetType();
            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties().Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(this, null);

                var att = (Versioning) prop.GetCustomAttribute(typeof (Versioning));

                info.AddValue(att.Name, propValue);
            }

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the collection of countries
    public class Countries
    {
        #region Static Fields

        private static Dictionary<string, Country> countries = new Dictionary<string, Country>();

        #endregion

        #region Public Methods and Operators

        public static void AddCountry(Country country)
        {
            countries.Add(country.Uid, country);
        }

        public static void Clear()
        {
            countries = new Dictionary<string, Country>();
        }

        public static int Count()
        {
            return countries.Values.Count - 1; //removes 100 (All)
        }

        //retuns a country

        //returns the list of countries
        public static List<Country> GetAllCountries()
        {
            List<Country> tCountries = countries.Values.ToList();
            tCountries.AddRange(TemporaryCountries.GetCountries());
            return tCountries;
        }

        public static List<Country> GetCountries()
        {
            List<Country> netto = countries.Values.ToList();
            //netto.AddRange(TemporaryCountries.GetCountries());
            netto.Remove(GetCountry("100"));

            var tCountries = new List<Country>();
            foreach (Country country in netto)
            {
                if (!(country is TerritoryCountry))
                {
                    tCountries.Add((Country) new CountryCurrentCountryConverter().Convert(country));
                }
            }

            foreach (Country country in TemporaryCountries.GetCountries())
            {
                if (((TemporaryCountry) country).Type == TemporaryCountry.TemporaryType.ManyToOne)
                {
                    tCountries.Add((Country) new CountryCurrentCountryConverter().Convert(country));
                }
            }

            return tCountries.Distinct().ToList();
        }

        //returns the list of countries from a region
        public static List<Country> GetCountries(Region region)
        {
            return GetCountries().FindAll((delegate(Country country) { return country.Region == region; }));
        }

        public static Country GetCountry(string uid)
        {
            if (countries.ContainsKey(uid))
            {
                return countries[uid];
            }
            if (TemporaryCountries.GetCountry(uid) != null)
            {
                return TemporaryCountries.GetCountry(uid);
            }
            return null;
        }

        //returns a country with a specific tailnumberformat
        public static Country GetCountryFromTailNumber(string tailnumber)
        {
            Country country = GetCountries().Find(co => co.TailNumbers.IsMatch(tailnumber));

            if (country != null)
                return country;
            else
                return null;
        }

        #endregion

        //clears the list

        //adds a country to the list

        //returns the number of countries
    }
}