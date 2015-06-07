using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.General.Holidays
{
    //the class for a holiday / vacation
    [Serializable]
    public class Holiday : BaseModel
    {
        #region Constructors and Destructors

        public Holiday(string section, string uid, HolidayType type, string name, TravelType travel, Country country)
        {
            Type = type;
            LongName = name;
            Travel = travel;
            Country = country;
            Uid = uid;
            Section = section;
        }

        private Holiday(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum HolidayType
        {
            FixedDate,

            FixedWeek,

            FixedMonth,

            NonFixedDate
        }

        public enum TravelType
        {
            Travel,

            Normal,

            Both
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("day")]
        public DayOfWeek Day { get; set; }

        [Versioning("longname")]
        public string LongName { get; set; }

        [Versioning("month")]
        public int Month { get; set; }

        public virtual string Name => Translator.GetInstance().GetString(Section, Uid);

        [Versioning("travel")]
        public TravelType Travel { get; set; }

        [Versioning("type")]
        public HolidayType Type { get; set; }

        [Versioning("uid")]
        public string Uid { get; set; }

        [Versioning("week")]
        public int Week { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of holidays / vacations
    public class Holidays
    {
        #region Static Fields

        private static readonly List<Holiday> holidays = new List<Holiday>();

        #endregion

        #region Public Methods and Operators

        public static void AddHoliday(Holiday holiday)
        {
            holidays.Add(holiday);
        }

        //returns all holidays

        //clears the list of holidays
        public static void Clear()
        {
            holidays.Clear();
        }

        public static List<Holiday> GetHolidays()
        {
            return holidays;
        }

        #endregion

        //adds a holiday to the list
    }
}