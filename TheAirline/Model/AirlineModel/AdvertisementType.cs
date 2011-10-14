using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    /*! Airline Advertisement type class.
 * This class is used for a type of Advertisement for an airline
 * The class needs parameters for type, name, price per month and reputation level
 */
    public class AdvertisementType
    {
        public enum AirlineAdvertisementType { Newspaper, Radio, TV, Internet }
        public AirlineAdvertisementType Type { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int ReputationLevel { get; set; }
        public AdvertisementType(AirlineAdvertisementType type, string name, double price, int reputationLevel)
        {
            this.Type = type;
            this.Name = name;
            this.Price = price;
            this.ReputationLevel = reputationLevel;
        }
    }
    /*! Airline advertisering types class.
* This class is used for the list of Advertisement types
* The class needs no parameters and construction since it is static
*/
    public class AdvertisementTypes
    {
        private static List<AdvertisementType> types = new List<AdvertisementType>();
        /*!adds a type to the list
         * */
        public static void AddAdvertisementType(AdvertisementType type)
        {
            types.Add(type);
        }
        /*!clears the list of types
         */
        public static void Clear()
        {
            types = new List<AdvertisementType>();
        }
        /*!returns all advertisement types
         */
        public static List<AdvertisementType> GetTypes()
        {
            return types;
        }
        /*!returns the advertisement types for a specific type
         */
        public static List<AdvertisementType> GetTypes(AdvertisementType.AirlineAdvertisementType type)
        {
            return types.FindAll((delegate(AdvertisementType t) { return t.Type == type; }));
        }
        /*!returns the advertisement type for a specific type with a name
         */
        public static AdvertisementType GetType(AdvertisementType.AirlineAdvertisementType type, string name)
        {
            return GetTypes(type).Find((delegate(AdvertisementType t) { return t.Name == name; }));
        }
        /*!returns the basic advertisement
         */
        public static AdvertisementType GetBasicType(AdvertisementType.AirlineAdvertisementType type)
        {
            return GetTypes(type).Find((delegate(AdvertisementType t) { return t.ReputationLevel == 0; }));
        }
    }
}
