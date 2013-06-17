
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    /*! Region.
 * This is used for a region in the world.
 * The class needs parameter for the region name
 */
    [Serializable]
    public class Region
    {
        public static string Section { get; set; }
        
        public string Uid { get; set; }
        public Region(string section, string uid)
        {
            Region.Section = section;
            this.Uid = uid;
        }

        public string Name
        {
            get { return Translator.GetInstance().GetString(Region.Section, this.Uid); }
        }
    }

    //the list of regions
    public class Regions
    {
        private static List<Region> regions = new List<Region>();
        //clears the list
        public static void Clear()
        {
            regions = new List<Region>();
        }
        //adds a region to the list
        public static void AddRegion(Region region)
        {
            regions.Add(region);
        }
        //returns a region from list
        public static Region GetRegion(string uid)
        {
            return regions.Find(r => r.Uid == uid);
        }

        //returns the list of regions, without "All Regions"
        public static List<Region> GetRegions()
        {
            List<Region> netto = new List<Region>(regions);
            netto.Remove(GetRegion("100"));
            return netto;
        }

        //returns the list of regions
        public static List<Region> GetAllRegions()
        {
            return regions;
        }

    }
}
