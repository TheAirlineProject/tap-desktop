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
            regions.Add(region.Name, region);
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
}
