using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Airports
{
    //the class for a type of hub
    [Serializable]
    public class HubType : BaseModel
    {
        #region Constructors and Destructors

        public HubType(string name, double price, TypeOfHub type)
        {
            Name = name;
            Price = price;
            Type = type;
        }

        private HubType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum TypeOfHub
        {
            Hub,

            RegionalHub,

            FocusCity,

            FortressHub
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("price")]
        public double Price { get; set; }

        [Versioning("type")]
        public TypeOfHub Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of hub types
    public class HubTypes
    {
        #region Static Fields

        private static readonly List<HubType> Types = new List<HubType>();

        #endregion

        #region Public Methods and Operators

        public static void AddHubType(HubType type)
        {
            Types.Add(type);
        }

        //clears the list of hub types
        public static void Clear()
        {
            Types.Clear();
        }

        //returns all hubtypes

        //returns a hub type of a specific type
        public static HubType GetHubType(HubType.TypeOfHub type)
        {
            return Types.Find(t => t.Type == type);
        }

        public static List<HubType> GetHubTypes()
        {
            return Types;
        }

        #endregion

        //adds a hub type to the list
    }
}