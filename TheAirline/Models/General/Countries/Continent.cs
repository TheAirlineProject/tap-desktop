using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Countries
{
    //the class for a continent
    [Serializable]
    public class Continent : BaseModel
    {
        #region Constructors and Destructors

        public Continent(string uid, string name)
        {
            Uid = uid;
            Name = name;
            Regions = new List<Region>();
        }

        private Continent(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("regions")]
        public List<Region> Regions { get; set; }

        [Versioning("uid")]
        public string Uid { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds a region to the continent
        public void AddRegion(Region region)
        {
            Regions.Add(region);
        }

        //returns if a country contains a region
        public bool HasRegion(Region region)
        {
            return Regions.Contains(region);
        }

        #endregion
    }

    //the list of continents
    public class Continents
    {
        #region Static Fields

        private static readonly List<Continent> continents = new List<Continent>();

        #endregion

        #region Public Methods and Operators

        public static void AddContinent(Continent continent)
        {
            continents.Add(continent);
        }

        public static void Clear()
        {
            continents.Clear();
        }

        //returns the continent for a region
        public static Continent GetContinent(Region region)
        {
            return continents.First(c => c.Regions.Exists(r => r.Uid == region.Uid));
        }

        //returns the list of continents
        public static List<Continent> GetContinents()
        {
            return continents;
        }

        #endregion

        //adds a continent to the list

        //clears the list of continents
    }
}