using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HolidaysModel
{
    //the class for a holiday / vacation
    public class Holiday
    {
        public enum TravelType { Travel, Normal, Both }
        public TravelType Travel { get; set; }
        public enum HolidayType {Fixed_Date, Fixed_Week,Fixed_Month, Non_Fixed_Date}
        public HolidayType Type { get; set; }
        public int Week { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Country Country { get; set; }
        public DayOfWeek Day { get; set; }
        public int Month { get; set; }
        public Holiday(HolidayType type, string name,TravelType travel, Country country)
        {
            this.Type = type;
            this.Name = name;
            this.Travel = travel;
            this.Country = country;
        }
    }
    //the list of holidays / vacations
    public class Holidays
    {
        private static List<Holiday> holidays = new List<Holiday>();
        //adds a holiday to the list
        public static void AddHoliday(Holiday holiday)
        {
            holidays.Add(holiday);
        }
        //returns all holidays
        public static List<Holiday> GetHolidays()
        {
            return holidays;
        }
        //clears the list of holidays
        public static void Clear()
        {
            holidays.Clear();
        }
    }
}
