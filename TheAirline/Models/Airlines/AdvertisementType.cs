using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    /*! Airline Advertisement type class.
 * This class is used for a type of Advertisement for an airline
 * The class needs parameters for type, name, price per month and reputation level
 */

    [Serializable]
    public class AdvertisementType : BaseModel
    {
        #region Fields

        [Versioning("price")] private double _aPrice;

        #endregion

        #region Constructors and Destructors

        public AdvertisementType(AirlineAdvertisementType type, string name, double price, int reputationLevel)
        {
            Type = type;
            Name = name;
            Price = price;
            ReputationLevel = reputationLevel;
        }

        private AdvertisementType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum AirlineAdvertisementType
        {
            Newspaper = 1930,

            Radio = 1940,

            Tv = 1950,

            Internet = 1995
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        public double Price
        {
            get { return GeneralHelpers.GetInflationPrice(_aPrice); }
            set { _aPrice = value; }
        }

        [Versioning("level")]
        public int ReputationLevel { get; set; }

        [Versioning("type")]
        public AirlineAdvertisementType Type { get; set; }

        #endregion
    }

    /*! Airline advertisering types class.
* This class is used for the list of Advertisement types
* The class needs no parameters and construction since it is static
*/

    public class AdvertisementTypes
    {
        #region Static Fields

        private static readonly List<AdvertisementType> Advertisements = new List<AdvertisementType>();

        #endregion

        #region Public Methods and Operators

        public static void AddAdvertisementType(AdvertisementType type)
        {
            Advertisements.Add(type);
        }

        /*!clears the list of types
         */

        public static void Clear()
        {
            Advertisements.Clear();
        }

        public static AdvertisementType GetBasicType(AdvertisementType.AirlineAdvertisementType type)
        {
            return GetTypes(type).Find((t => t.ReputationLevel == 0));
        }

        public static AdvertisementType GetType(AdvertisementType.AirlineAdvertisementType type, string name)
        {
            return GetTypes(type).Find((t => t.Name == name));
        }

        /*!returns all advertisement types
         */

        public static List<AdvertisementType> GetTypes()
        {
            return Advertisements;
        }

        /*!returns the advertisement types for a specific type
         */

        public static List<AdvertisementType> GetTypes(AdvertisementType.AirlineAdvertisementType type)
        {
            return Advertisements.FindAll((t => t.Type == type));
        }

        #endregion

        /*!adds a type to the list
         * */

        /*!returns the advertisement type for a specific type with a name
         */
    }
}