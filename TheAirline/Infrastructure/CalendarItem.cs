using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;
using TheAirline.Models;

namespace TheAirline.Infrastructure
{
    [Serializable]
    public class CalendarItem : BaseModel
    {
        #region Constructors and Destructors

        public CalendarItem(ItemType type, DateTime date, string header, string subject)
        {
            Type = type;
            Header = header;
            Date = date;
            Subject = subject;
        }

        private CalendarItem(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum ItemType
        {
            Holiday,

            AirlinerOrder,

            AirportOpening,

            AirportClosing
        }

        #endregion

        #region Public Properties

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("header")]
        public string Header { get; set; }

        [Versioning("subject")]
        public string Subject { get; set; }

        [Versioning("type")]
        public ItemType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    public class CalendarItems
    {
        #region Static Fields

        private static readonly List<CalendarItem> Items = new List<CalendarItem>();

        #endregion

        #region Public Methods and Operators

        public static void AddCalendarItem(CalendarItem item)
        {
            Items.Add(item);
        }

        public static void Clear()
        {
            Items.Clear();
        }

        //returns all calendar items for a date
        public static List<CalendarItem> GetCalendarItems(DateTime date)
        {
            return Items.FindAll(i => i.Date.ToShortDateString() == date.ToShortDateString());
        }

        //returns all calendar items
        public static List<CalendarItem> GetCalendarItems()
        {
            return Items;
        }

        #endregion

        //adds a calendar item to the list

        //clears the list of calendar items
    }
}