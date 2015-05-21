using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.Airlines
{
    [Serializable]
    //the profile for an airline
    public class AirlineProfile : BaseModel
    {
        #region Constructors and Destructors

        public AirlineProfile(
            string name,
            string iata,
            string color,
            string ceo,
            bool isReal,
            int founded,
            int folded)
        {
            Name = name;
            IATACode = iata;
            CEO = ceo;
            Color = color;
            IsReal = isReal;
            Founded = founded;
            Folded = folded;
            Countries = new List<Country>();
            Logos = new List<AirlineLogo>();
            PreferedAircrafts = new List<AirlinerType>();
            PrimaryPurchasing = PreferedPurchasing.Random;
        }

        private AirlineProfile(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                PreferedAircrafts = new List<AirlinerType>();
            }

            if (Version == 2 || Version == 3)
            {
                PrimaryPurchasing = PreferedPurchasing.Random;
            }
        }

        #endregion

        #region Enums

        public enum PreferedPurchasing
        {
            Random,

            Leasing,

            Buying
        }

        #endregion

        #region Public Properties

        [Versioning("ceo")]
        public string CEO { get; set; }

        [Versioning("color")]
        public string Color { get; set; }

        [Versioning("countries")]
        public List<Country> Countries { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("folded")]
        public int Folded { get; set; }

        [Versioning("founded")]
        public int Founded { get; set; }

        [Versioning("iata")]
        public string IATACode { get; set; }

        [Versioning("isreal")]
        public bool IsReal { get; set; }

        public string Logo => GetCurrentLogo();

        [Versioning("logoname")]
        public string LogoName { get; set; }

        [Versioning("logos")]
        public List<AirlineLogo> Logos { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("narrative")]
        public string Narrative { get; set; }

        [Versioning("aircrafttypes", Version = 2)]
        public List<AirlinerType> PreferedAircrafts { get; set; }

        [Versioning("preferedairport")]
        public Airport PreferedAirport { get; set; }

        [Versioning("purchasing", Version = 3)]
        public PreferedPurchasing PrimaryPurchasing { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        //adds a logo to the airline
        public void AddLogo(AirlineLogo logo)
        {
            Logos.Add(logo);
        }

        public void AddPreferedAircraft(AirlinerType type)
        {
            PreferedAircrafts.Add(type);
        }

        #endregion

        #region Methods

        private string GetCurrentLogo()
        {
            AirlineLogo ret =
                Logos.Find(
                    l =>
                    l.FromYear <= GameObject.GetInstance().GameTime.Year
                    && l.ToYear >= GameObject.GetInstance().GameTime.Year);

            if (!File.Exists(ret.Path))
            {
                ret.Path = $"{AppSettings.GetDataPath()}\\graphics\\airlinelogos\\{LogoName}.png";
            }

            return ret.Path;
        }

        #endregion

        //returns the current logo for the airline
    }

    //the class for an airline logo
    [Serializable]
    public class AirlineLogo : BaseModel
    {
        #region Constructors and Destructors

        public AirlineLogo(int fromYear, int toYear, string path)
        {
            FromYear = fromYear;
            ToYear = toYear;
            Path = path;
        }

        public AirlineLogo(string path)
            : this(1900, 2199, path)
        {
        }

        private AirlineLogo(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("fromyear")]
        public int FromYear { get; set; }

        [Versioning("path")]
        public string Path { get; set; }

        [Versioning("toyear")]
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