
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

namespace TheAirline.Model.AirportModel
{
    [Serializable]
    //the class for a profile for an airport
    public class AirportProfile
    {
        public enum AirportType { Long_Haul_International, Regional, Domestic, Short_Haul_International }
        public AirportType Type { get; set; }
        //public enum AirportSize { Smallest, Very_small, Small, Medium, Large, Very_large, Largest }
        public GeneralHelpers.Size Size { get { return getCurrentSize(); } private set { ;} }
        public GeneralHelpers.Size Cargo { get; set; }
        public Weather.Season Season { get; set; }
        public string Name { get; set; }
        public string IATACode { get; set; }
        public string ICAOCode { get; set; }
        public Town Town { get; set; }
        public Country Country { get { return this.Town.Country; } private set { ;} }
        public GeoCoordinate Coordinates { get; set; }
        public string Logo { get; set; }
        // chs, 2012-23-01 added for airport maps
        public string Map { get; set; }
        public TimeSpan OffsetGMT { get; set; }
        public TimeSpan OffsetDST { get; set; }
        public GameTimeZone TimeZone { get { return getTimeZone(); } set { ;} }
        public Period<DateTime> Period { get; set; }
        public string ID { get; set; }
        public double Pax { get { return getCurrentPaxValue(); } private set { ;} }
        public List<PaxValue> PaxValues { get; set; }
        public double CargoVolume { get; set; }

        public Dictionary<string, int> MajorDestionations { get; set; }
        public AirportProfile(string name, string code, string icaocode, AirportType type, Period<DateTime> period, Town town, TimeSpan offsetGMT, TimeSpan offsetDST, GeoCoordinate coordinates, GeneralHelpers.Size cargo, double cargovolume, Weather.Season season)
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

    }
    [Serializable]
    //the class for a pax value
    public class PaxValue
    {

        public double Pax { get; set; }

        public int FromYear { get; set; }

        public int ToYear { get; set; }

        public double InflationBeforeYear { get; set; }

        public double InflationAfterYear { get; set; }

        public GeneralHelpers.Size Size { get; set; }
        public PaxValue(int fromYear, int toYear, GeneralHelpers.Size size, double pax)
        {
            this.Pax = pax;
            this.FromYear = fromYear;
            this.ToYear = toYear;
            this.Size = size;
        }
    }
}
