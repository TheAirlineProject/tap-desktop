
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Device.Location;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Reflection;

namespace TheAirline.Model.AirportModel
{
    [Serializable]
    //the class for a profile for an airport
    public class AirportProfile : ISerializable
    {
        public enum AirportType { Long_Haul_International, Regional, Domestic, Short_Haul_International }
        [Versioning("type")]
        public AirportType Type { get; set; }
        //public enum AirportSize { Smallest, Very_small, Small, Medium, Large, Very_large, Largest }
        public GeneralHelpers.Size Size { get { return getCurrentSize(); } private set { ;} }
        [Versioning("cargo")]
        public GeneralHelpers.Size Cargo { get; set; }
        [Versioning("season")]
        public Weather.Season Season { get; set; }
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("iata")]
        public string IATACode { get; set; }
        [Versioning("icao")]
        public string ICAOCode { get; set; }
        [Versioning("town")]
        public Town Town { get; set; }
        
        public Country Country { get { return this.Town.Country; } private set { ;} }
        [Versioning("coordinates")]
        public Coordinates Coordinates { get; set; }
        [Versioning("logo")]
        public string Logo { get; set; }
        // chs, 2012-23-01 added for airport maps
        [Versioning("map")]
        public string Map { get; set; }
        [Versioning("gmt")]
        public TimeSpan OffsetGMT { get; set; }
        [Versioning("dst")]
        public TimeSpan OffsetDST { get; set; }
        public GameTimeZone TimeZone { get { return getTimeZone(); } set { ;} }
        [Versioning("period")]
        public Period<DateTime> Period { get; set; }
        [Versioning("id")]
        public string ID { get; set; }
        public double Pax { get { return getCurrentPaxValue(); } private set { ;} }
        [Versioning("paxvalues")]
        public List<PaxValue> PaxValues { get; set; }
        [Versioning("cargovolume")]
        public double CargoVolume { get; set; }
        [Versioning("majordestinations")]
        public Dictionary<string, int> MajorDestionations { get; set; }
        public AirportProfile(string name, string code, string icaocode, AirportType type, Period<DateTime> period, Town town, TimeSpan offsetGMT, TimeSpan offsetDST, Coordinates coordinates, GeneralHelpers.Size cargo, double cargovolume, Weather.Season season)
        {
            this.PaxValues = new List<PaxValue>();

            this.Name = name;
            this.Period = period;
            this.IATACode = code;
            this.ICAOCode = icaocode;
            this.Type = type;
            this.Town = town;
            this.Coordinates = coordinates;
            this.CargoVolume = cargovolume;
            this.MajorDestionations = new Dictionary<string, int>();
            this.Cargo = cargo;
            this.Logo = "";
            this.OffsetDST = offsetDST;
            this.OffsetGMT = offsetGMT;
            this.Season = season;
            this.ID = string.Format("{0:00}-{1:00}-{2:00}-{3:00}-{4:00}-{5:00}", char.ConvertToUtf32(this.IATACode, 0), char.ConvertToUtf32(this.IATACode, 1), char.ConvertToUtf32(this.IATACode, 2), name.Length, char.ConvertToUtf32(this.Name, this.Name.Length / 2), (int)this.Cargo);


        }
        //returns the time zone for the airport
        private GameTimeZone getTimeZone()
        {
            GameTimeZone zone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == this.OffsetDST; });

            return zone;
        }
        //sets the pax value
        public void setPaxValue(double pax)
        {

            PaxValue paxValue = this.PaxValues[0];

            this.PaxValues = new List<PaxValue>();

            PaxValue tPaxValue = new PaxValue(this.Period.From.Year, this.Period.To.Year, paxValue.Size, pax);

            this.PaxValues.Add(tPaxValue);
        }
        //returns the current pax value
        private double getCurrentPaxValue()
        {

            int currentYear = GameObject.GetInstance().GameTime.Year;

            PaxValue currentPaxValue = getCurrentPaxValueObject();

            double pax = currentPaxValue.Pax;

            if (currentPaxValue.InflationAfterYear != 0)
            {
                int yearDiff = currentYear - currentPaxValue.FromYear;

                pax = pax * Math.Pow((1 + (currentPaxValue.InflationAfterYear / 100)), yearDiff);
            }

            if (currentPaxValue.InflationBeforeYear != 0)
            {
                int yearDiff = currentPaxValue.ToYear - currentYear;

                pax = pax * Math.Pow((1 - (currentPaxValue.InflationBeforeYear / 100)), yearDiff);
            }



            return pax;
        }
        //return the current size (pax) of the airport
        private GeneralHelpers.Size getCurrentSize()
        {

            return AirportHelpers.ConvertAirportPaxToSize(getCurrentPaxValue());
        }
        //returns the current pax value object
        private PaxValue getCurrentPaxValueObject()
        {

            int currentYear = GameObject.GetInstance().GameTime.Year;

            PaxValue currentPaxValue = this.PaxValues.Find(p => p.FromYear <= currentYear && p.ToYear >= currentYear);

            return currentPaxValue == null ? this.PaxValues[0] : currentPaxValue;
        }
           private AirportProfile(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(Versioning)) != null && ((Versioning)p.GetCustomAttribute(typeof(Versioning))).AutoGenerated));

            foreach (SerializationEntry entry in info)
            {
                PropertyInfo prop = props.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                    prop.SetValue(this, entry.Value);
            }

            var notSetProps = props.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (PropertyInfo prop in notSetProps)
            {
                Versioning ver = (Versioning)prop.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                    prop.SetValue(this, ver.DefaultValue);

            }




        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties().Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(this, null);

                Versioning att = (Versioning)prop.GetCustomAttribute(typeof(Versioning));

                if (prop.PropertyType.IsSerializable)
                    info.AddValue(att.Name, propValue);
                else
                    Console.WriteLine(prop.Name + " is not serialized");
            }

        }

    }
    [Serializable]
    //the class for a pax value
    public class PaxValue : ISerializable
    {
        [Versioning("pax")]
        public double Pax { get; set; }
        [Versioning("from")]
        public int FromYear { get; set; }
        [Versioning("to")]
        public int ToYear { get; set; }
        [Versioning("inflationbefore")]
        public double InflationBeforeYear { get; set; }
        [Versioning("inflationafter")]
        public double InflationAfterYear { get; set; }
        [Versioning("size")]
        public GeneralHelpers.Size Size { get; set; }
        public PaxValue(int fromYear, int toYear, GeneralHelpers.Size size, double pax)
        {
            this.Pax = pax;
            this.FromYear = fromYear;
            this.ToYear = toYear;
            this.Size = size;
        }
           private PaxValue(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }



        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


        }
    }
}
