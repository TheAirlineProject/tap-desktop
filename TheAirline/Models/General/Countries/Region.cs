using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Countries
{
    /*! Region.
 * This is used for a region in the world.
 * The class needs parameter for the region name
 */

    [Serializable]
    public class Region : BaseModel
    {
        #region Constructors and Destructors

        public Region(string section, string uid, double fuelindex)
        {
            Section = section;
            Uid = uid;
            FuelIndex = fuelindex;
        }

        private Region(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                FuelIndex = 1;
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        public string Name => Translator.GetInstance().GetString(Section, Uid);

        [Versioning("uid")]
        public string Uid { get; set; }

        [Versioning("fuelindex", Version = 2)]
        public double FuelIndex { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of regions
    public class Regions
    {
        #region Static Fields

        private static List<Region> _regions = new List<Region>();

        #endregion

        #region Public Methods and Operators

        public static void AddRegion(Region region)
        {
            _regions.Add(region);
        }

        public static void Clear()
        {
            _regions = new List<Region>();
        }

        public static List<Region> GetAllRegions()
        {
            return _regions;
        }

        //returns a region from list
        public static Region GetRegion(string uid)
        {
            return _regions.Find(r => r.Uid == uid);
        }

        //returns the list of regions, without "All Regions"
        public static List<Region> GetRegions()
        {
            var netto = new List<Region>(_regions);
            netto.Remove(GetRegion("100"));
            return netto;
        }

        #endregion

        //clears the list

        //adds a region to the list

        //returns the list of regions
    }
}