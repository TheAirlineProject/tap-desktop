using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
     
     //the class for a type of airliner
    [ProtoContract]
    [ProtoInclude(100,typeof(AirlinerPassengerType))]
    [ProtoInclude(101,typeof(AirlinerCargoType))]
 
    public abstract class AirlinerType
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public double CruisingSpeed { get; set; }
        [ProtoMember(3)]
        public long Range { get; set; }
        [ProtoMember(4)]
        public string Image { get; set; }
        [ProtoMember(5)]
        public double Length { get; set; }
        [ProtoMember(6)]
        public double Wingspan { get; set; }
        [ProtoMember(7)]
        public int CockpitCrew { get; set; }
        [ProtoMember(8)]
        private long APrice;
        public long Price { get { return Convert.ToInt64(GeneralHelpers.GetInflationPrice(this.APrice)); } set { this.APrice = value; } }
        [ProtoMember(9)]
        public Period<DateTime> Produced { get; set; }
        [ProtoMember(10)]
        public double FuelConsumption { get; set; }
        [ProtoMember(11)]
        public Manufacturer Manufacturer { get; set; }
        public enum BodyType {Narrow_Body, Single_Aisle,Wide_Body} 
        public enum TypeRange {Regional, Short_Range, Medium_Range, Long_Range}
        public enum EngineType {Jet, Turboprop}
        public enum TypeOfAirliner { Passenger, Cargo, Mixed, Convertible }
        [ProtoMember(12)]
        public BodyType Body { get; set; }
        [ProtoMember(13)]
        public TypeRange RangeType { get; set; }
        [ProtoMember(14)]
        public EngineType Engine { get; set; }
        [ProtoMember(15)]
        public long MinRunwaylength { get; set; }
        [ProtoMember(16)]
        public long FuelCapacity { get; set; }
        [ProtoMember(17)]
        public TypeOfAirliner TypeAirliner { get; set; }
        [ProtoMember(18)]
        public Boolean IsStandardType { get; set; }
        public AirlinerType BaseType { get; set; }
        [ProtoMember(19)]
        public int ProductionRate { get; set; }
        public AirlinerType(Manufacturer manufacturer,TypeOfAirliner typeOfAirliner, string name, int cockpitCrew, double speed, long range, double wingspan, double length, double consumption, long price,long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced,int prodRate, Boolean standardType)
        {
            this.TypeAirliner = typeOfAirliner;
            this.Manufacturer = manufacturer;
            this.Name = name;
            this.CruisingSpeed = speed;
            this.Range = range;
            this.Wingspan = wingspan;
            this.Length = length;
            this.CockpitCrew = cockpitCrew;
            this.Price = price;
            this.FuelConsumption = consumption;
            this.Produced = produced;
            this.Engine = engine;
            this.Body = body;
            this.RangeType = rangeType;
            this.MinRunwaylength = minRunwaylength;
            this.FuelCapacity = fuelcapacity;
            this.IsStandardType = standardType;
            this.ProductionRate = prodRate;
        }
       
        //returns the montly maintenance
        public long getMaintenance()
        {
            double maintenance = 0.0013 * (double)this.Price;
            return Convert.ToInt64(maintenance);
        }
        // chs, 2011-10-10 changed the leasing price
        //returns the leasing price based on 5 years with 6% in rate 
        public long getLeasingPrice()
        {
            double months = 5 * 12;
            double rate = 1.06;
            double leasingPrice = this.Price / months * rate;
            return Convert.ToInt64(leasingPrice);

        }
       
    }
    //the class for a passenger airliner type
    [ProtoContract]
    public class AirlinerPassengerType : AirlinerType
    {
        [ProtoMember(30)]
        public int MaxSeatingCapacity { get; set; }
        [ProtoMember(31)]
        public int CabinCrew { get; set; }
        [ProtoMember(32)]
        public int MaxAirlinerClasses { get; set; }
        public AirlinerPassengerType(Manufacturer manufacturer, string name, int seating, int cockpitcrew, int cabincrew, double speed, long range, double wingspan, double length, double consumption, long price, int maxAirlinerClasses, long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced, int prodRate, Boolean standardType = true) : base(manufacturer,TypeOfAirliner.Passenger,name,cockpitcrew,speed,range,wingspan,length,consumption,price,minRunwaylength,fuelcapacity,body,rangeType,engine,produced, prodRate,standardType)
        {
            this.MaxSeatingCapacity = seating;
            this.CabinCrew = cabincrew;
            this.MaxAirlinerClasses = maxAirlinerClasses;
        }
    }
    //the class for a cargo airliner type
    [ProtoContract]
    public class AirlinerCargoType : AirlinerType
    {
        [ProtoMember(40)]
        public double CargoSize { get; set; }
        public AirlinerCargoType(Manufacturer manufacturer, string name, int cockpitcrew, double cargoSize,  double speed, long range, double wingspan, double length, double consumption, long price, long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced, int prodRate, Boolean standardType = true) : base(manufacturer,TypeOfAirliner.Cargo,name,cockpitcrew,speed,range,wingspan,length,consumption,price,minRunwaylength,fuelcapacity,body,rangeType,engine,produced, prodRate,standardType)
        {
            this.CargoSize = cargoSize;
        }
    }
  
    //the collection of airliner types
    public class AirlinerTypes
    {
        private static List<AirlinerType> types = new List<AirlinerType>();
        //clears the list of airliner types
        public static void Clear()
        {
            types = new List<AirlinerType>();
        }
        //adds an airliner type to the list
        public static void AddType(AirlinerType type)
        {

            types.Add(type);
        }
        //returns an airliner with a name
        public static AirlinerType GetType(string name)
        {
            return types.Find(t => t.Name == name || t.Name.ToUpper() == name);
        }
        //returns all airliner types
        public static List<AirlinerType> GetAllTypes()
        {
            return types.FindAll(t => t.IsStandardType);
        }
        //returns a list of airliner types
        public static List<AirlinerType> GetTypes(Predicate<AirlinerType> match)
        {
            return types.FindAll(t=>t.IsStandardType).FindAll(match);
        }
        //returns all non standard airliner types
        public static List<AirlinerType> GetNonStandardTypes()
        {
            return types.FindAll(t => !t.IsStandardType);
        }
    }
}
