
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirportModel
{
    //the class for a type of hub
    [Serializable]
    public class HubType
    {
        
        public double Price { get; set; }
        
        public string Name { get; set; }
        public enum TypeOfHub { Hub, Regional_hub, Focus_city, Fortress_hub }
        
        public TypeOfHub Type { get; set; }
        public HubType(string name, double price, TypeOfHub type)
        {
            this.Name = name;
            this.Price = price;
            this.Type = type;
        }
    }
    //the list of hub types
    public class HubTypes
    {
        private static List<HubType> types = new List<HubType>();
        //adds a hub type to the list
        public static void AddHubType(HubType type)
        {
            types.Add(type);
        }
        //clears the list of hub types
        public static void Clear()
        {
            types.Clear();
        }
        //returns all hubtypes
        public static List<HubType> GetHubTypes()
        {
            return types;
        }
        //returns a hub type of a specific type
        public static HubType GetHubType(HubType.TypeOfHub type)
        {
            return types.Find(t => t.Type == type);
        }

    }
}
