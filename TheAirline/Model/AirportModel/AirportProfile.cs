using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    //the class for a profile for an airport
    public class AirportProfile
    {
        public enum AirportType { Long_Haul_International, Regional, Domestic,Short_Haul_International }
        public AirportType Type { get; set; }
        //public enum AirportSize { Smallest, Very_small, Small, Medium, Large, Very_large, Largest }
        public GeneralHelpers.Size Size { get; set; }
        public GeneralHelpers.Size Cargo { get; set; }
        public Weather.Season Season { get; set; }
        public string Name { get; set; }
        public string IATACode { get; set; }
        public string ICAOCode { get; set; }
        public string Town { get; set; }
        public Country Country { get; set; }
        public Coordinates Coordinates { get; set; }
        public string Logo { get; set; }
        // chs, 2012-23-01 added for airport maps
        public string Map { get; set; }
        public TimeSpan OffsetGMT { get; set; }
        public TimeSpan OffsetDST { get; set; }
        public GameTimeZone TimeZone { get { return getTimeZone();} set { ;} }
        public Period Period { get; set; }
        public string ID { get; set; }
        public AirportProfile(string name, string code, string icaocode, AirportType type, Period period, string town, Country country, TimeSpan offsetGMT, TimeSpan offsetDST, Coordinates coordinates, GeneralHelpers.Size size, GeneralHelpers.Size cargo, Weather.Season season)
        {
            this.Name = name;
            this.Period = period;
            this.IATACode = code;
            this.ICAOCode = icaocode;
            this.Type = type;
            this.Town = town;
            this.Country = country;
            this.Coordinates = coordinates;
            this.Size = size;
            this.Cargo = cargo;
            this.Logo = "";
            this.OffsetDST = offsetDST;
            this.OffsetGMT = offsetGMT;
            this.Season = season;
            this.ID = string.Format("{0:00}-{1:00}-{2:00}-{3:00}-{4:00}-{5:00}", char.ConvertToUtf32(this.IATACode, 0), char.ConvertToUtf32(this.IATACode, 1), char.ConvertToUtf32(this.IATACode, 2), name.Length, char.ConvertToUtf32(this.Name, this.Name.Length / 2),(int)this.Size);

            
        }
        //returns the time zone for the airport
        private GameTimeZone getTimeZone()
        {
            GameTimeZone zone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == this.OffsetDST; });
          
            return zone;
        }
       
    }
}
