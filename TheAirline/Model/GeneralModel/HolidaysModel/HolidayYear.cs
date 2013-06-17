
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HolidaysModel
{
    //the class for an event (holiday) in a year
    [Serializable]
    public class HolidayYearEvent
    {
        
        public DateTime Date { get; set; }
        
        public Holiday Holiday { get; set; }
        
        public int Length { get; set; } //in days
        public HolidayYearEvent(DateTime date, Holiday holiday, int length)
        {
            this.Date = date;
            this.Holiday = holiday;
            this.Length = length;
        }
    }
    //the class for the current holiday year
    public class HolidayYear
    {
        private static List<HolidayYearEvent> holidays = new List<HolidayYearEvent>();
        //adds a holiday to the year
        public static void AddHoliday(HolidayYearEvent holiday)
        {
            holidays.Add(holiday);
        }
        //returns all holidays
        public static List<HolidayYearEvent> GetHolidays()
        {
            return holidays;
        }
        //returns all holidays
        public static List<HolidayYearEvent> GetHolidays(Predicate<HolidayYearEvent> match)
        {
            return holidays.FindAll(match);
        }
        //returns all holidays for a specific day
        public static List<HolidayYearEvent> GetHolidays(DateTime date)
        {
            DateTime currentDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            return holidays.FindAll(h => h.Date <= currentDate && h.Date.AddDays(h.Length) > currentDate);
        }
        //returns all holidays for a country
        public static List<HolidayYearEvent> GetHolidays(Country country)
        {
            return holidays.FindAll(h => h.Holiday.Country == country);
        }
        //returns if there is a for a given date and country
        public static Boolean IsHoliday(Country country, DateTime date)
        {
            return GetHoliday(country, date) != null;
        }
        //returns a holiday for a given date and country
        public static HolidayYearEvent GetHoliday(Country country, DateTime date)
        {
            DateTime currentDate = new DateTime(date.Year,date.Month,date.Day,0,0,0);
           
            return holidays.Find(h => h.Date <= currentDate && h.Date.AddDays(h.Length)>currentDate && h.Holiday.Country == country);
        }
        //clears the list of holidays
        public static void Clear()
        {
            holidays.Clear();
        }
    }
}
