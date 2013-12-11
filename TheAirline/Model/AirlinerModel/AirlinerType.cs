
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
     
     //the class for a type of airliner
    [DataContract]
    [KnownType(typeof(AirlinerPassengerType))]
    [KnownType(typeof(AirlinerCargoType))]
 
    public abstract class AirlinerType
    {
        public double Maintenance { get { return getMaintenance(); } private set { ;} }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double CruisingSpeed { get; set; }

        [DataMember]
        public long Range { get; set; }

        [DataMember]
        public string Image { get; set; }

        [DataMember]
        public double Length { get; set; }

        [DataMember]
        public double Wingspan { get; set; }

        [DataMember]
        public int CockpitCrew { get; set; }

        [DataMember]
        private long APrice;
        public long Price { get { return Convert.ToInt64(GeneralHelpers.GetInflationPrice(this.APrice)); } set { this.APrice = value; } }

        [DataMember]
        public Period<DateTime> Produced { get; set; }

        [DataMember]
        public double FuelConsumption { get; set; }

        [DataMember]
        public Manufacturer Manufacturer { get; set; }
        public enum BodyType {Narrow_Body, Single_Aisle,Wide_Body} 
        public enum TypeRange {Regional, Short_Range, Medium_Range, Long_Range}
        public enum EngineType {Jet, Turboprop}
        public enum TypeOfAirliner { Passenger, Cargo, Mixed, Convertible }
        [DataMember]
        public BodyType Body { get; set; }
        [DataMember]
        public TypeRange RangeType { get; set; }
        [DataMember]
        public EngineType Engine { get; set; }
        [DataMember]
        public long MinRunwaylength { get; set; }
        [DataMember]
        public long FuelCapacity { get; set; }
        [DataMember]
        public TypeOfAirliner TypeAirliner { get; set; }
        [DataMember]
        public Boolean IsStandardType { get; set; }
        [DataMember]
        public AirlinerType BaseType { get; set; }
        [DataMember]
        public int ProductionRate { get; set; }
        [DataMember]
        public string AirlinerFamily { get; set; }
        public int Capacity { get { return getCapacity(); } private set { ;} }
        public AirlinerType(Manufacturer manufacturer,TypeOfAirliner typeOfAirliner, string name,string family, int cockpitCrew, double speed, long range, double wingspan, double length, double consumption, long price,long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced,int prodRate, Boolean standardType)
        {
            this.TypeAirliner = typeOfAirliner;
            this.AirlinerFamily = family;
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
       
        //returns the yearly maintenance
        public long getMaintenance()
        {
            double maintenance = 0.0013 * (double)this.Price * 12;
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
        //returns the capacity of the airliner
        public abstract int getCapacity();
       
    }
    //the class for a passenger airliner type
    [Serializable]
    public class AirlinerPassengerType : AirlinerType
    {
        public int MaxSeatingCapacity { get; set; }
        public int CabinCrew { get; set; }
        public int MaxAirlinerClasses { get; set; }
        public AirlinerPassengerType(Manufacturer manufacturer, string name,string family, int seating, int cockpitcrew, int cabincrew, double speed, long range, double wingspan, double length, double consumption, long price, int maxAirlinerClasses, long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced, int prodRate, Boolean standardType = true) : base(manufacturer,TypeOfAirliner.Passenger,name,family,cockpitcrew,speed,range,wingspan,length,consumption,price,minRunwaylength,fuelcapacity,body,rangeType,engine,produced, prodRate,standardType)
        {
            this.MaxSeatingCapacity = seating;
            this.CabinCrew = cabincrew;
            this.MaxAirlinerClasses = maxAirlinerClasses;
        }

        public override int getCapacity()
        {
            return this.MaxSeatingCapacity;
        }
    }
    //the class for a cargo airliner type
    [Serializable]
    public class AirlinerCargoType : AirlinerType
    {
        public double CargoSize { get; set; }
        public AirlinerCargoType(Manufacturer manufacturer, string name,string family, int cockpitcrew, double cargoSize,  double speed, long range, double wingspan, double length, double consumption, long price, long minRunwaylength, long fuelcapacity, BodyType body, TypeRange rangeType, EngineType engine, Period<DateTime> produced, int prodRate, Boolean standardType = true) : base(manufacturer,TypeOfAirliner.Cargo,name,family,cockpitcrew,speed,range,wingspan,length,consumption,price,minRunwaylength,fuelcapacity,body,rangeType,engine,produced, prodRate,standardType)
        {
            this.CargoSize = cargoSize;
        }

        public override int getCapacity()
        {
            return (int)this.CargoSize;
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
        //removes an airliner type
        public static void RemoveType(AirlinerType type)
        {
            types.Remove(type);
        }
    }
}
