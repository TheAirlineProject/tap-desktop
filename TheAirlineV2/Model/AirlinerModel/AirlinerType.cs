using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.GeneralModel;

namespace TheAirlineV2.Model.AirlinerModel
{
     

    //the class for a type of airliner
    public class AirlinerType
    {
        public string Name { get; set; }
        public double CruisingSpeed { get; set; }
        public long Range { get; set; }
        public double Length { get; set; }
        public double Wingspan { get; set; }
        public int MaxSeatingCapacity { get; set; }
        public int CockpitCrew { get; set; }
        public int CabinCrew { get; set; }
        public long Price { get; set; }
        public ProductionPeriod Produced { get; set; }
        public double FuelConsumption { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public int MaxAirlinerClasses { get; set; }
        public enum BodyType {Narrow_Body, Single_Aisle,Wide_Body} 
        public enum TypeRange {Regional, Short_Range, Medium_Range, Long_Range}
        public enum EngineType {Jet, Turboprop}
        public BodyType Body { get; set; }
        public TypeRange RangeType { get; set; }
        public EngineType Engine { get; set; }
        public AirlinerType(Manufacturer manufacturer, string name, int seating, int cockpitcrew,int cabincrew, double speed, long range, double wingspan, double length, double consumption, long price,int maxAirlinerClasses, BodyType body, TypeRange rangeType, EngineType engine, ProductionPeriod produced)
        {
            this.Manufacturer = manufacturer;
            this.Name = name;
            this.CruisingSpeed = speed;
            this.Range = range;
            this.Wingspan = wingspan;
            this.Length = length;
            this.MaxSeatingCapacity = seating;
            this.CockpitCrew = cockpitcrew;
            this.CabinCrew = cabincrew;
            this.Price = price;
            this.FuelConsumption = consumption;
            this.Produced = produced;
            this.MaxAirlinerClasses = maxAirlinerClasses;
            this.Engine = engine;
            this.Body = body;
            this.RangeType = rangeType;
        }
       
        //returns the montly maintenance
        public long getMaintenance()
        {
            double maintenance = 0.0013 * (double)this.Price;
            return Convert.ToInt64(maintenance);
        }
        //returns the leasing price based on 5 years with 6% in rate 
        public long getLeasingPrice()
        {
            double months = 20 * 12;
            double rate = 1.06;
            double leasingPrice = this.Price / months * rate;
            return Convert.ToInt64(leasingPrice);

        }
       
    }

    //the class for a production period (year from - year to)
    public class ProductionPeriod
    {
        public int From { get; set; }
        public int To { get; set; }
        public ProductionPeriod(int from, int to)
        {
            this.From = from;
            this.To = to;
        }
        public override string ToString()
        {
            return string.Format("{0}-{1}", this.From > GameObject.GetInstance().GameTime.Year ? "?" : this.From.ToString(), this.To > GameObject.GetInstance().GameTime.Year ? "?" : this.To.ToString());
        }
    }
    //the collection of airliner types
    public class AirlinerTypes
    {
        private static Dictionary<string, AirlinerType> types = new Dictionary<string, AirlinerType>();
        //clears the list
        public static void Clear()
        {
            types = new Dictionary<string, AirlinerType>();
        }
        public static void AddType(AirlinerType type)
        {
            types.Add(type.Name, type);
        }
        public static AirlinerType GetType(string name)
        {
            return types[name];
        }
        public static List<AirlinerType> GetTypes()
        {
            return types.Values.ToList();
        }
        
    }
}
