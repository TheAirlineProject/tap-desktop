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
            regions.Add(region.Uid, region);
        }
        //returns a region from the collection
        public static Region GetRegion(string uid)
        {
            if (regions.ContainsKey(uid))
                return regions[uid];
            else
                return null;
        }

        //returns the list of regions, without "All Regions"
        public static List<Region> GetRegions()
        {
            List<Region> netto = regions.Values.ToList();
            netto.Remove(GetRegion("100"));
            return netto;
        }

        //returns the list of regions
        public static List<Region> GetAllRegions()
        {
            return regions.Values.ToList();
        }

    }
}
