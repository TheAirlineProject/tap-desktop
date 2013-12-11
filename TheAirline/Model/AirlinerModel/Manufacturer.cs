
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner manufacturer
    [Serializable]
    public class Manufacturer
    {
        
        public string Name { get; set; }
        
        public string ShortName { get; set; }
        
        public Country Country { get; set; }
        
        public string Logo { get; set; }
        public Boolean IsReal { get; set; }
        public Manufacturer(string name, string shortname, Country country, Boolean isReal)
        {
            this.Name = name;
            this.ShortName = shortname;
            this.Country = country;
            this.IsReal = isReal;

           

        }
    }
    //the collection of manufacturers
    public class Manufacturers
    {
        private static List<Manufacturer> manufacturers = new List<Manufacturer>();
        //clears the list
        public static void Clear()
        {
            manufacturers = new List<Manufacturer>();
        }
        //adds a manufacturer to the collection
        public static void AddManufacturer(Manufacturer manufacturer)
        {
            manufacturers.Add(manufacturer);
        }
        //returns a manufacturer
        public static Manufacturer GetManufacturer(string name)
        {
            return manufacturers.Find(m => m.Name == name || m.ShortName == name);
        }
        //returns the list manufacturers
        public static List<Manufacturer> GetAllManufacturers()
        {
            return manufacturers;
        }
        public static List<Manufacturer> GetManufacturers(Predicate<Manufacturer> match)
        {
            return manufacturers.FindAll(match);
        }
    }
}
