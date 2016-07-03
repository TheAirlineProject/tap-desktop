using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.General.Holidays
{
    //the class for an event (holiday) in a year
    [Serializable]
    public class HolidayYearEvent : BaseModel
    {
        #region Constructors and Destructors

        public HolidayYearEvent(DateTime date, Holiday holiday, int length)
        {
            Date = date;
            Holiday = holiday;
            Length = length;
        }

        private HolidayYearEvent(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("holiday")]
        public Holiday Holiday { get; set; }

        [Versioning("length")]
        public int Length { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //in days
    }

    //the class for the current holiday year
    public class HolidayYear
    {
        #region Static Fields

        private static readonly List<HolidayYearEvent> Holidays = new List<HolidayYearEvent>();

        #endregion

        #region Public Methods and Operators

        public static void AddHoliday(HolidayYearEvent holiday)
        {
            Holidays.Add(holiday);
        }

        public static void Clear()
        {
            Holidays.Clear();
        }

        public static HolidayYearEvent GetHoliday(Country country, DateTime date)
        {
            var currentDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            return
                Holidays.Find(
                    h => h.Date <= currentDate && h.Date.AddDays(h.Length) > currentDate && h.Holiday.Country == country);
        }

        //returns all holidays
        public static List<HolidayYearEvent> GetHolidays()
        {
            return Holidays;
        }

        //returns all holidays
        public static List<HolidayYearEvent> GetHolidays(Predicate<HolidayYearEvent> match)
        {
            return Holidays.FindAll(match);
        }

        //returns all holidays for a specific day
        public static List<HolidayYearEvent> GetHolidays(DateTime date)
        {
            var currentDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            return Holidays.FindAll(h => h.Date <= currentDate && h.Date.AddDays(h.Length) > currentDate);
        }

        //returns all holidays for a country
        public static List<HolidayYearEvent> GetHolidays(Country country)
        {
            return Holidays.FindAll(h => h.Holiday.Country == country);
        }

        //returns if there is a for a given date and country
        public static bool IsHoliday(Country country, DateTime date)
        {
            return GetHoliday(country, date) != null;
        }

        #endregion

        //adds a holiday to the year

        //returns a holiday for a given date and country
    }
}