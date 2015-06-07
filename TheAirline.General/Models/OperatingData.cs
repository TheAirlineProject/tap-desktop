using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General
{
    //the class for operating data
    public class OperatingData : BaseModel
    {
        public OperatingData()
        {
            Values = new List<OperatingValue>();
        }

        private OperatingData(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        [Versioning("values")]
        public List<OperatingValue> Values { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //returns all the types in the data
        public List<string> GetTypes()
        {
            return Values.Select(v => v.Type).Distinct().ToList();
        }

        //returns the list of values ordered by date
        public List<OperatingValue> GetOrderedValues(string type)
        {
            return Values.Where(v => v.Type == type).OrderByDescending(v => v.Year).ThenByDescending(v => v.Month).ToList();
        }

        //adds a value to the data
        public void AddOperatingValue(OperatingValue value)
        {
            if (Values.Exists(v => v.Month == value.Month && v.Type == value.Type))
                Values.First(v => v.Month == value.Month && v.Type == value.Type).Value += value.Value;
            else
                Values.Add(new OperatingValue(value.Type, value.Year, value.Month, value.Value));
        }
    }

    //the class for an operating value
    public class OperatingValue : BaseModel
    {
        public OperatingValue(string type, int year, int month, double value)
        {
            Type = type;
            Year = year;
            Month = month;
            Value = value;
        }

        private OperatingValue(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        [Versioning("type")]
        public string Type { get; set; }

        [Versioning("month")]
        public int Month { get; set; }

        [Versioning("Year")]
        public int Year { get; set; }

        [Versioning("value")]
        public double Value { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }
    }
}