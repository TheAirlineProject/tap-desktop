using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.Airliners
{
    //the class for an airliner manufacturer
    [Serializable]
    public class Manufacturer : BaseModel
    {
        #region Constructors and Destructors

        public Manufacturer(string name, string shortname, Country country, bool isReal)
        {
            Name = name;
            ShortName = shortname;
            Country = country;
            IsReal = isReal;
        }

        private Manufacturer(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("isreal")]
        public bool IsReal { get; set; }

        [Versioning("logo")]
        public string Logo { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("shortname")]
        public string ShortName { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the collection of manufacturers
    public class Manufacturers
    {
        #region Static Fields

        private static List<Manufacturer> _manufacturers = new List<Manufacturer>();

        #endregion

        #region Public Methods and Operators

        public static void AddManufacturer(Manufacturer manufacturer)
        {
            _manufacturers.Add(manufacturer);
        }

        public static void Clear()
        {
            _manufacturers = new List<Manufacturer>();
        }

        //returns a manufacturer

        //returns the list manufacturers
        public static List<Manufacturer> GetAllManufacturers()
        {
            return _manufacturers;
        }

        public static Manufacturer GetManufacturer(string name)
        {
            return _manufacturers.Find(m => m.Name == name || m.ShortName == name);
        }

        public static List<Manufacturer> GetManufacturers(Predicate<Manufacturer> match)
        {
            return _manufacturers.FindAll(match);
        }

        #endregion

        //clears the list

        //adds a manufacturer to the collection
    }
}