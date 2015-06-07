using System.Collections.Generic;
using TheAirline.General.Models.Countries;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.GUIModel.ObjectsModel
{
    //the object for the start data
    public class StartDataObject
    {
        #region Public Properties

        public Airline Airline { get; set; }

        public Airport Airport { get; set; }

        public string CEO { get; set; }

        public Continent Continent { get; set; }

        public DifficultyLevel Difficulty { get; set; }

        public Airline.AirlineFocus Focus { get; set; }

        public Country HomeCountry { get; set; }

        public bool IsPaused { get; set; }

        public bool LocalCurrency { get; set; }

        public bool MajorAirports { get; set; }

        public bool InternationalAirports { get; set; }

        public int NumberOfOpponents { get; set; }

        public List<Airline> Opponents { get; set; }

        public bool RandomOpponents { get; set; }

        public bool RealData { get; set; }

        public Region Region { get; set; }

        public bool SameRegion { get; set; }

        public GameTimeZone TimeZone { get; set; }

        public bool UseDayTurns { get; set; }

        public int Year { get; set; }

        public List<Country> SelectedCountries { get; set; }

        #endregion
    }
}