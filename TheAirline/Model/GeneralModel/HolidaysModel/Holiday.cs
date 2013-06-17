using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HolidaysModel
{
    //the class for a holiday / vacation
    [ProtoContract]
    public class Holiday
    {
        public static string Section { get; set; }
        public enum TravelType { Travel, Normal, Both }
        [ProtoMember(1)]
        public TravelType Travel { get; set; }
        public enum HolidayType {Fixed_Date, Fixed_Week,Fixed_Month, Non_Fixed_Date}
        [ProtoMember(2)]
        public HolidayType Type { get; set; }
        [ProtoMember(3)]
        public int Week { get; set; }
        [ProtoMember(4)]
        public string LongName { get; set; }
        [ProtoMember(5)]
        public DateTime Date { get; set; }
        [ProtoMember(6)]
        public Country Country { get; set; }
        [ProtoMember(7)]
        public DayOfWeek Day { get; set; }
        [ProtoMember(8)]
        public int Month { get; set; }
        [ProtoMember(9)]
        public string Uid { get; set; }
        public Holiday(string section, string uid, HolidayType type, string name, TravelType travel, Country country)
        {
            this.Type = type;
            this.LongName = name;
            this.Travel = travel;
            this.Country = country;
            this.Uid = uid;
            Holiday.Section = section;
        }
        public virtual string Name
        {
            get { return Translator.GetInstance().GetString(Holiday.Section, this.Uid); }
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
