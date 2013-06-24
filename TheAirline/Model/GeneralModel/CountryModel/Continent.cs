using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for a continent
    [Serializable]public class Continent
    {
        public string Name { get; set; }
        public string Uid{ get; set; }
        public List<Region> Regions { get; set; }
        public Continent(string uid, string name)
        {
            this.Uid = uid;
            this.Name = name;
            this.Regions = new List<Region>();
        }
        //adds a region to the continent
        public void addRegion(Region region)
        {
            this.Regions.Add(region);
        }
        //returns if a country contains a region
        public Boolean hasRegion(Region region)
        {
            return this.Regions.Contains(region);
        }
    }
    //the list of continents
    public class Continents
    {
        private static List<Continent> continents = new List<Continent>();
        //adds a continent to the list
        public static void AddContinent(Continent continent)
        {
            continents.Add(continent);
        }
        //returns the continent for a region
        public static Continent GetContinent(Region region)
        {
            return continents.Where(c => c.Regions.Exists(r=>r.Uid == region.Uid)).First();
        }
        //returns the list of continents
        public static List<Continent> GetContinents()
        {
            return continents;
        }
        //clears the list of continents
        public static void Clear()
        {
            continents.Clear();
        }
        
    }
}
