using System;
using System.Collections.Generic;
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

        public Boolean IsPaused { get; set; }

        public Boolean LocalCurrency { get; set; }

        public Boolean MajorAirports { get; set; }

        public Boolean InternationalAirports { get; set; }

        public int NumberOfOpponents { get; set; }

        public List<Airline> Opponents { get; set; }

        public Boolean RandomOpponents { get; set; }

        public Boolean RealData { get; set; }

        public Region Region { get; set; }

        public Boolean SameRegion { get; set; }

        public GameTimeZone TimeZone { get; set; }

        public Boolean UseDayTurns { get; set; }

        public int Year { get; set; }

        public List<Country> SelectedCountries { get; set; }

        #endregion
    }
}