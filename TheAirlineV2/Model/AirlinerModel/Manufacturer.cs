using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.GeneralModel;

namespace TheAirlineV2.Model.AirlinerModel
{
    //the class for an airliner manufacturer
    public class Manufacturer
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Country Country { get; set; }
        public Manufacturer(string name, string shortname, Country country)
        {
            this.Name = name;
            this.ShortName = shortname;
            this.Country = country;
        }
    }
    //the collection of manufacturers
    public class Manufacturers
    {
        private static Dictionary<string, Manufacturer> manufacturers = new Dictionary<string, Manufacturer>();
        //clears the list
        public static void Clear()
        {
            manufacturers = new Dictionary<string, Manufacturer>();
        }
        //adds a manufacturer to the collection
        public static void AddManufacturer(Manufacturer manufacturer)
        {
            manufacturers.Add(manufacturer.ShortName, manufacturer);
        }
        //returns a manufacturer
        public static Manufacturer GetManufacturer(string name)
        {
            return manufacturers[name];
        }
        //returns the list manufacturers
        public static List<Manufacturer> GetManufacturers()
        {
            return manufacturers.Values.ToList();
        }
    }
}
