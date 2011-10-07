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
        public enum AirportType { International, Regional, Domestic }
        public AirportType Type { get; set; }
        public enum AirportSize { Smallest, Very_small, Small, Medium, Large, Very_large, Largest }
        public string Name { get; set; }
        public string IATACode { get; set; }
        public string Town { get; set; }
        public Country Country { get; set; }
        public Coordinates Coordinates { get; set; }
        public AirportSize Size { get; set; }
        public int Gates { get; set; }
        public string Logo { get; set; }
        public TimeSpan OffsetGMT { get; set; }
        public TimeSpan OffsetDST { get; set; }
        public GameTimeZone TimeZone { get { return getTimeZone();} set { ;} }
        public AirportProfile(string name, string code, AirportType type,string town, Country country, TimeSpan offsetGMT, TimeSpan offsetDST, Coordinates coordinates, AirportSize size, int gates)
        {
            this.Name = name;
            this.IATACode = code;
            this.Type = type;
            this.Town = town;
            this.Country = country;
            this.Coordinates = coordinates;
            this.Size = size;
            this.Logo = "";
            this.Gates = gates;
            this.OffsetDST = offsetDST;
            this.OffsetGMT = offsetGMT;
            
        }
        //returns the time zone for the airport
        private GameTimeZone getTimeZone()
        {
            return TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == this.OffsetDST; });
        }
    }
}
