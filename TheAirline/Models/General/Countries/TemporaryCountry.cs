using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.General.Countries
{
    /// <summary>
    ///     The class for a temporary country
    /// </summary>
    [Serializable]
    public class TemporaryCountry : Country
    {
        #region Constructors and Destructors

        public TemporaryCountry(TemporaryType type, General.Countries.Country country, DateTime startDate, DateTime endDate)
            : base(Section, country.Uid, country.ShortName, country.Region, country.TailNumberFormat)
        {
            Type = type;
            StartDate = startDate;
            EndDate = endDate;
            Countries = new List<OneToManyCountry>();
            CountryAfter = this;
            CountryBefore = this;
        }

        private TemporaryCountry(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum TemporaryType
        {
            OneToMany,

            ManyToOne
        }

        #endregion

        #region Public Properties

        [Versioning("countries")]
        public List<OneToManyCountry> Countries { get; set; }

        [Versioning("after")]
        public General.Countries.Country CountryAfter { get; set; }

        [Versioning("before")]
        public General.Countries.Country CountryBefore { get; set; }

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        [Versioning("type")]
        public TemporaryType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public General.Countries.Country GetCurrentCountry(DateTime date, General.Countries.Country originalCountry)
        {
            if (Type == TemporaryType.ManyToOne)
            {
                if (date < StartDate)
                {
                    return CountryBefore;
                }
                if (date >= StartDate && date <= EndDate)
                {
                    return this;
                }
                if (date > EndDate)
                {
                    return CountryAfter;
                }
            }
            if (Type == TemporaryType.OneToMany)
            {
                OneToManyCountry tCountry = Countries.Find(c => c.Country == originalCountry);

                if (tCountry == null)
                {
                    return originalCountry;
                }

                if (date >= tCountry.StartDate && date <= tCountry.EndDate)
                {
                    return this;
                }
                return originalCountry;
            }
            return null;
        }

        #endregion
    }

    [Serializable]
    //the class for a one to many temporary country
    public class OneToManyCountry : BaseModel
    {
        #region Constructors and Destructors

        public OneToManyCountry(General.Countries.Country country, DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            Country = country;
        }

        private OneToManyCountry(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("country")]
        public General.Countries.Country Country { get; set; }

        [Versioning("endate")]
        public DateTime EndDate { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of temporary countries
    public class TemporaryCountries
    {
        #region Static Fields

        private static readonly List<TemporaryCountry> Countries = new List<TemporaryCountry>();

        #endregion

        #region Public Methods and Operators

        public static void AddCountry(TemporaryCountry country)
        {
            Countries.Add(country);
        }

        public static void Clear()
        {
            Countries.Clear();
        }

        //returns all temporary countries
        public static List<Country> GetCountries()
        {
            return Countries.Cast<General.Countries.Country>().ToList();
        }

        //returns a country
        public static General.Countries.Country GetCountry(string uid)
        {
            TemporaryCountry country = Countries.Find(t => t.Uid == uid);
            return country;
        }

        //returns a temporary country which a country is a part of
        public static TemporaryCountry GetTemporaryCountry(General.Countries.Country country, DateTime date)
        {
            if (country == null)
            {
                return null;
            }

            TemporaryCountry tCountry =
                Countries.Find(
                    c =>
                    c.StartDate < date && c.EndDate > date
                    && (c.CountryBefore == country || c.CountryAfter == country
                        || c.Countries.Find(tc => tc.Country.Uid == country.Uid) != null));
            return tCountry;
        }

        #endregion

        //adds a country to the list

        //clears the list
    }
}