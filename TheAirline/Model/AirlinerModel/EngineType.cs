using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an engine type 
    [Serializable]
    public class EngineType : BaseModel
    {
        #region Constructors and Destructors

        public EngineType(
            string model,
            Manufacturer manufacturer,
            TypeOfEngine engine,
            NoiseLevel noise,
            double consumptation,
            long price,
            int maxspeed,
            int ceiling,
            double runway,
            double range,
            Period<int> produced)
        {
            Model = model;
            Manufacturer = manufacturer;
            Engine = engine;
            ConsumptationModifier = consumptation;
            Price = price;
            MaxSpeed = maxspeed;
            Ceiling = ceiling;
            RunwayModifier = runway;
            RangeModifier = range;
            Produced = produced;
            Types = new List<AirlinerType>();
            Noise = noise;
        }

        private EngineType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum NoiseLevel
        {
            VeryHigh,

            High,

            Medium,

            Low,

            VeryLow
        }

        public enum TypeOfEngine
        {
            Jet,

            Turboprop
        }

        #endregion

        #region Public Properties

        [Versioning("ceiling")]
        public int Ceiling { get; set; }

        [Versioning("consumptation")]
        public double ConsumptationModifier { get; set; }

        [Versioning("engine")]
        public TypeOfEngine Engine { get; set; }

        [Versioning("manufacturer")]
        public Manufacturer Manufacturer { get; set; }

        [Versioning("speed")]
        public int MaxSpeed { get; set; }

        [Versioning("model")]
        public string Model { get; set; }

        [Versioning("noise")]
        public NoiseLevel Noise { get; set; }

        [Versioning("price")]
        public long Price { get; set; }

        [Versioning("produced")]
        public Period<int> Produced { get; set; }

        [Versioning("range")]
        public double RangeModifier { get; set; }

        [Versioning("runway")]
        public double RunwayModifier { get; set; }

        [Versioning("types")]
        public List<AirlinerType> Types { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddAirlinerType(AirlinerType type)
        {
            Types.Add(type);
        }

        #endregion
    }

    //the list of engine types
    public class EngineTypes
    {
        #region Static Fields

        private static List<EngineType> _types = new List<EngineType>();

        #endregion

        #region Public Methods and Operators

        public static void Clear()
        {
            _types = new List<EngineType>();
        }

        public static void AddEngineType(EngineType type)
        {
            _types.Add(type);
        }

        //returns the list of engine types
        public static List<EngineType> GetEngineTypes()
        {
            return _types;
        }

        //returns the list of engine types for an airliner type
        public static List<EngineType> GetEngineTypes(AirlinerType type, int year)
        {
            return GetEngineTypes(type).FindAll(t => t.Produced.From <= year && t.Produced.To >= year);
        }

        public static List<EngineType> GetEngineTypes(AirlinerType type)
        {
            return (from t in _types from at in t.Types where at.Name == type.Name select t).ToList();
        }

        //returns the standard engine for an airliner type
        public static EngineType GetStandardEngineType(AirlinerType type, int year)
        {
            List<EngineType> allTypes = GetEngineTypes(type, year);

            if (allTypes.Count > 0)
            {
                return allTypes.OrderBy(t => t.Price).First();
            }

            return null;
        }

        public static EngineType GetStandardEngineType(AirlinerType type)
        {
            List<EngineType> allTypes = GetEngineTypes(type);

            if (allTypes.Count > 0)
            {
                return allTypes.OrderBy(t => t.Price).First();
            }

            return null;
        }

        #endregion

        //adds an engine type to the list
    }
}