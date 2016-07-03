using System.Collections.Generic;
using System.Linq;

namespace TheAirline.Models.General.Countries
{
    //the class for a continent
    public sealed class Continent
    {
        #region Constructors and Destructors

        public Continent(string uid, string name)
        {
            Uid = uid;
            Name = name;
            Regions = new List<Region>();
        }

        public Continent()
        {
        }

        #endregion

        #region Public Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public List<Region> Regions { get; set; }

        public string Uid { get; set; }

        #endregion

        #region Public Methods and Operators

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
    }
}