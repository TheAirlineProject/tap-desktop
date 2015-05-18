using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Countries.Towns;
using TheAirline.Models.General.Environment;

namespace TheAirline.Models.Airports
{
    [Serializable]
    //the class for a profile for an airport
    public class AirportProfile : BaseModel
    {
        private string _logo;

        #region Constructors and Destructors

        public AirportProfile(
            string name,
            string code,
            string icaocode,
            AirportType type,
            Period<DateTime> period,
            Town town,
            TimeSpan offsetGMT,
            TimeSpan offsetDST,
            Coordinates coordinates,
            GeneralHelpers.Size cargo,
            double cargovolume,
            Weather.Season season)
        {
            PaxValues = new List<PaxValue>();

            Expansions = new List<AirportExpansion>();
            Name = name;
            Period = period;
            IATACode = code;
            ICAOCode = icaocode;
            Type = type;
            Town = town;
            Coordinates = coordinates;
            CargoVolume = cargovolume;
            MajorDestionations = new Dictionary<string, int>();
            Cargo = cargo;
            Logo = "";
            OffsetDST = offsetDST;
            OffsetGMT = offsetGMT;
            Season = season;
            ID =
                $"{char.ConvertToUtf32(IATACode, 0):00}-{char.ConvertToUtf32(IATACode, 1):00}-{char.ConvertToUtf32(IATACode, 2):00}-{name.Length:00}-{char.ConvertToUtf32(Name, Name.Length/2):00}-{(int) Cargo:00}";
        }

        private AirportProfile(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                Expansions = new List<AirportExpansion>();
        }

        #endregion

        #region Enums

        public enum AirportType
        {
            LongHaulInternational,

            Regional,

            Domestic,

            ShortHaulInternational
        }

        #endregion

        #region Public Properties

        [Versioning("expansions")]
        public List<AirportExpansion> Expansions { get; set; }

        [Versioning("cargo")]
        public GeneralHelpers.Size Cargo { get; set; }

        [Versioning("cargovolume")]
        public double CargoVolume { get; set; }

        [Versioning("coordinates")]
        public Coordinates Coordinates { get; set; }

        public Country Country => Town.Country;

        [Versioning("iata")]
        public string IATACode { get; set; }

        [Versioning("icao")]
        public string ICAOCode { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("logo")]
        public string Logo
        {
            get
            {
                if (!File.Exists(_logo))
                {
                    Logo = AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\" + IATACode + ".png";
                }
                return _logo;
            }
            set { _logo = value; }
        }

        [Versioning("majordestinations")]
        public Dictionary<string, int> MajorDestionations { get; set; }

        // chs, 2012-23-01 added for airport maps
        [Versioning("map")]
        public string Map { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("dst")]
        public TimeSpan OffsetDST { get; set; }

        [Versioning("gmt")]
        public TimeSpan OffsetGMT { get; set; }

        public double Pax => GetCurrentPaxValue();

        [Versioning("paxvalues")]
        public List<PaxValue> PaxValues { get; set; }

        [Versioning("period")]
        public Period<DateTime> Period { get; set; }

        [Versioning("season")]
        public Weather.Season Season { get; set; }

        public GeneralHelpers.Size Size => GetCurrentSize();

        public GameTimeZone TimeZone => GetTimeZone();

        [Versioning("town")]
        public Town Town { get; set; }

        [Versioning("type")]
        public AirportType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        //sets the pax value
        public void SetPaxValue(double pax)
        {
            PaxValue paxValue = PaxValues[0];

            PaxValues = new List<PaxValue>();

            var tPaxValue = new PaxValue(Period.From.Year, Period.To.Year, paxValue.Size, pax);

            PaxValues.Add(tPaxValue);
        }

        //adds an expansion to the airport
        public void AddExpansion(AirportExpansion expansion)
        {
            Expansions.Add(expansion);
        }

        #endregion

        #region Methods

        private double GetCurrentPaxValue()
        {
            int currentYear = GameObject.GetInstance().GameTime.Year;

            PaxValue currentPaxValue = GetCurrentPaxValueObject();

            double pax = currentPaxValue.Pax;

            if (!currentPaxValue.InflationAfterYear.Equals(0))
            {
                int yearDiff = currentYear - currentPaxValue.FromYear;

                pax = pax*Math.Pow((1 + (currentPaxValue.InflationAfterYear/100)), yearDiff);
            }

            if (!currentPaxValue.InflationBeforeYear.Equals(0))
            {
                int yearDiff = currentPaxValue.ToYear - currentYear;

                pax = pax*Math.Pow((1 - (currentPaxValue.InflationBeforeYear/100)), yearDiff);
            }


            return pax;
        }

        //return the current size (pax) of the airport

        //returns the current pax value object
        private PaxValue GetCurrentPaxValueObject()
        {
            int currentYear = GameObject.GetInstance().GameTime.Year;

            PaxValue currentPaxValue = PaxValues.Find(p => p.FromYear <= currentYear && p.ToYear >= currentYear);

            return currentPaxValue ?? PaxValues[0];
        }

        private GeneralHelpers.Size GetCurrentSize()
        {
            return AirportHelpers.ConvertAirportPaxToSize(GetCurrentPaxValue());
        }

        private GameTimeZone GetTimeZone()
        {
            GameTimeZone zone =
                TimeZones.GetTimeZones().Find(gtz => gtz.UTCOffset == OffsetDST);

            return zone;
        }

        #endregion

        //returns the current pax value
    }

    [Serializable]
    //the class for a pax value
    public class PaxValue : BaseModel
    {
        #region Constructors and Destructors

        public PaxValue(int fromYear, int toYear, GeneralHelpers.Size size, double pax)
        {
            Pax = pax;
            FromYear = fromYear;
            ToYear = toYear;
            Size = size;
        }

        private PaxValue(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("from")]
        public int FromYear { get; set; }

        [Versioning("inflationafter")]
        public double InflationAfterYear { get; set; }

        [Versioning("inflationbefore")]
        public double InflationBeforeYear { get; set; }

        [Versioning("pax")]
        public double Pax { get; set; }

        [Versioning("size")]
        public GeneralHelpers.Size Size { get; set; }

        [Versioning("to")]
        public int ToYear { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}