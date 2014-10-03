using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel
{
    //the class for a type of airliner
    [Serializable]
    public abstract class AirlinerType : BaseModel
    {
        #region Constructors and Destructors

        protected AirlinerType(
            Manufacturer manufacturer,
            TypeOfAirliner typeOfAirliner,
            string name,
            string family,
            int cockpitCrew,
            double speed,
            long range,
            double wingspan,
            double length,
            double weight,
            double consumption,
            long price,
            long minRunwaylength,
            long fuelcapacity,
            BodyType body,
            TypeRange rangeType,
            TypeOfEngine engine,
            Period<DateTime> produced,
            int prodRate,
            Boolean isConvertable,
            Boolean standardType)
        {
            TypeAirliner = typeOfAirliner;
            AirlinerFamily = family;
            Manufacturer = manufacturer;
            Name = name;
            CruisingSpeed = speed;
            Range = range;
            Wingspan = wingspan;
            Length = length;
            Weight = weight;
            CockpitCrew = cockpitCrew;
            Price = price;
            FuelConsumption = consumption;
            Produced = produced;
            Engine = engine;
            Body = body;
            RangeType = rangeType;
            MinRunwaylength = minRunwaylength;
            FuelCapacity = fuelcapacity;
            IsStandardType = standardType;
            ProductionRate = prodRate;
            IsConvertable = isConvertable;
        }

        protected AirlinerType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                IsConvertable = TypeAirliner != TypeOfAirliner.Cargo;
            }
            if (Version < 3)
                Weight = AirlinerHelpers.GetCalculatedWeight(this);
        }

        #endregion

        #region Enums

        public enum BodyType
        {
            NarrowBody,

            SingleAisle,

            WideBody,

            Helicopter
        }

        public enum TypeOfAirliner
        {
            Passenger,

            Cargo,

            Mixed,

            Helicopter
        }

        public enum TypeOfEngine
        {
            Jet,

            Turboprop
        }

        public enum TypeRange
        {
            Regional,

            ShortRange,

            MediumRange,

            LongRange
        }

        #endregion

        #region Public Properties

        [Versioning("family")]
        public string AirlinerFamily { get; set; }

        [Versioning("basetype")]
        public AirlinerType BaseType { get; set; }

        [Versioning("body")]
        public BodyType Body { get; set; }


        public int Capacity
        {
            get { return GetCapacity(); }
        }

        [Versioning("cockpit")]
        public int CockpitCrew { get; set; }

        [Versioning("engine")]
        public TypeOfEngine Engine { get; set; }

        [Versioning("fuelcapacity")]
        public long FuelCapacity { get; set; }

        [Versioning("image")]
        public string Image { get; set; }

        [Versioning("weight", Version = 3)]
        public double Weight { get; set; }

        [Versioning("isconvertable", Version = 2)]
        public Boolean IsConvertable { get; set; }

        [Versioning("isstandard")]
        public Boolean IsStandardType { get; set; }

        [Versioning("length")]
        public double Length { get; set; }

        public double Maintenance
        {
            get { return GetMaintenance(); }
        }

        [Versioning("manufacturer")]
        public Manufacturer Manufacturer { get; set; }

        [Versioning("speed")]
        public double CruisingSpeed { get; set; }

        [Versioning("range")]
        public long Range { get; set; }

        [Versioning("fuel")]
        public double FuelConsumption { get; set; }

        [Versioning("runway")]
        public long MinRunwaylength { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        public long Price
        {
            get { return Convert.ToInt64(GeneralHelpers.GetInflationPrice(APrice)); }
            set { APrice = value; }
        }

        [Versioning("produced")]
        public Period<DateTime> Produced { get; set; }

        [Versioning("production")]
        public int ProductionRate { get; set; }

        [Versioning("rangetype")]
        public TypeRange RangeType { get; set; }

        [Versioning("typeairliner")]
        public TypeOfAirliner TypeAirliner { get; set; }

        [Versioning("wingspan")]
        public double Wingspan { get; set; }

        [Versioning("price")]
        public long APrice { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        public abstract int GetCapacity();

        public long GetLeasingPrice()
        {
            const double months = 5*12;
            const double rate = 1.06;
            double leasingPrice = Price/months*rate;
            return Convert.ToInt64(leasingPrice);
        }

        public long GetMaintenance()
        {
            double maintenance = 0.0013*Price*12;
            return Convert.ToInt64(maintenance);
        }

        #endregion
    }

    //the class for a passenger airliner type
    [Serializable]
    public class AirlinerPassengerType : AirlinerType
    {
        #region Constructors and Destructors

        public AirlinerPassengerType(
            Manufacturer manufacturer,
            string name,
            string family,
            int seating,
            int cockpitcrew,
            int cabincrew,
            double speed,
            long range,
            double wingspan,
            double length,
            double weight,
            double consumption,
            long price,
            int maxAirlinerClasses,
            long minRunwaylength,
            long fuelcapacity,
            BodyType body,
            TypeRange rangeType,
            TypeOfEngine engine,
            Period<DateTime> produced,
            int prodRate,
            Boolean isConvertable,
            Boolean standardType = true)
            : base(
                manufacturer,
                TypeOfAirliner.Passenger,
                name,
                family,
                cockpitcrew,
                speed,
                range,
                wingspan,
                length,
                weight,
                consumption,
                price,
                minRunwaylength,
                fuelcapacity,
                body,
                rangeType,
                engine,
                produced,
                prodRate,
                isConvertable,
                standardType)
        {
            MaxSeatingCapacity = seating;
            CabinCrew = cabincrew;
            MaxAirlinerClasses = maxAirlinerClasses;
        }

        protected AirlinerPassengerType(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("cabincrew")]
        public int CabinCrew { get; set; }

        [Versioning("maxclasses")]
        public int MaxAirlinerClasses { get; set; }

        [Versioning("maxcapacity")]
        public int MaxSeatingCapacity { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public override int GetCapacity()
        {
            return MaxSeatingCapacity;
        }

        #endregion
    }

    //the class for a combi airliner type
    [Serializable]
    public class AirlinerCombiType : AirlinerPassengerType
    {
        #region Constructors and Destructors

        public AirlinerCombiType(
            Manufacturer manufacturer,
            string name,
            string family,
            int seating,
            int cockpitcrew,
            int cabincrew,
            double speed,
            long range,
            double wingspan,
            double length,
            double weight,
            double consumption,
            long price,
            int maxAirlinerClasses,
            long minRunwaylength,
            long fuelcapacity,
            BodyType body,
            TypeRange rangeType,
            TypeOfEngine engine,
            Period<DateTime> produced,
            int prodRate,
            double cargo,
            Boolean isConvertable,
            Boolean standardType = true)
            : base(
                manufacturer,
                name,
                family,
                seating,
                cockpitcrew,
                cabincrew,
                speed,
                range,
                wingspan,
                length,
                weight,
                consumption,
                price,
                maxAirlinerClasses,
                minRunwaylength,
                fuelcapacity,
                body,
                rangeType,
                engine,
                produced,
                prodRate,
                isConvertable,
                standardType)
        {
            CargoSize = cargo;
            TypeAirliner = TypeOfAirliner.Mixed;
        }

        private AirlinerCombiType(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("cargo")]
        public double CargoSize { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the class for a cargo airliner type
    [Serializable]
    public class AirlinerCargoType : AirlinerType
    {
        #region Constructors and Destructors

        public AirlinerCargoType(
            Manufacturer manufacturer,
            string name,
            string family,
            int cockpitcrew,
            double cargoSize,
            double speed,
            long range,
            double wingspan,
            double length,
            double weight,
            double consumption,
            long price,
            long minRunwaylength,
            long fuelcapacity,
            BodyType body,
            TypeRange rangeType,
            TypeOfEngine engine,
            Period<DateTime> produced,
            int prodRate,
            Boolean isConvertable,
            Boolean standardType = true)
            : base(
                manufacturer,
                TypeOfAirliner.Cargo,
                name,
                family,
                cockpitcrew,
                speed,
                range,
                wingspan,
                length,
                weight,
                consumption,
                price,
                minRunwaylength,
                fuelcapacity,
                body,
                rangeType,
                engine,
                produced,
                prodRate,
                isConvertable,
                standardType)
        {
            CargoSize = cargoSize;
        }

        private AirlinerCargoType(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("cargo")]
        public double CargoSize { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        public override int GetCapacity()
        {
            return (int) CargoSize;
        }

        #endregion
    }

    //the collection of airliner types
    public class AirlinerTypes
    {
        #region Static Fields

        private static List<AirlinerType> _types = new List<AirlinerType>();

        #endregion

        #region Public Methods and Operators

        public static void AddType(AirlinerType type)
        {
            _types.Add(type);
        }

        public static void Clear()
        {
            _types = new List<AirlinerType>();
        }

        //returns an airliner with a name

        //returns all airliner types
        public static List<AirlinerType> GetAllTypes()
        {
            return _types.FindAll(t => t.IsStandardType);
        }

        //returns a list of airliner types

        //returns all non standard airliner types
        public static List<AirlinerType> GetNonStandardTypes()
        {
            return _types.FindAll(t => !t.IsStandardType);
        }

        public static AirlinerType GetType(string name)
        {
            return _types.Find(t => t.Name == name || t.Name.ToUpper() == name);
        }

        public static List<AirlinerType> GetTypes(Predicate<AirlinerType> match)
        {
            return _types.FindAll(t => t.IsStandardType).FindAll(match);
        }

        //removes an airliner type
        public static void RemoveType(AirlinerType type)
        {
            _types.Remove(type);
        }

        #endregion

        //clears the list of airliner types

        //adds an airliner type to the list
    }
}