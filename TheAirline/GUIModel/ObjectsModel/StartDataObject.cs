using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GUIModel.ObjectsModel
{
    //the object for the start data
    public class StartDataObject
    {
        public int Year { get; set; }
        public Continent Continent { get; set; }
        public Region Region { get; set; }
        public int NumberOfOpponents { get; set; }
        public List<Airline> Opponents { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public Boolean UseDayTurns { get; set; }
        public Airline.AirlineFocus Focus { get; set; }
        public Boolean SameRegion { get; set; }
        public Boolean RandomOpponents { get; set; }
        public Boolean IsPaused { get; set; }
        public Boolean RealData { get; set; }
        public Boolean MajorAirports { get; set; }

        public Airport Airport { get; set; }
        public Airline Airline { get; set; }
        public Country HomeCountry { get; set; }
        public string CEO { get; set; }
        public GameTimeZone TimeZone { get; set; }
        public Boolean LocalCurrency { get; set; }
       
    }
}
