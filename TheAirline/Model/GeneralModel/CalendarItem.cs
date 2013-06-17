
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    [Serializable]
     public class CalendarItem
    {
        
        public string Header { get; set; }
        
        public DateTime Date { get; set; }
        
        public string Subject { get; set; }
        public enum ItemType { Holiday, Airliner_Order, Airport_Opening, Airport_Closing }
        
        public ItemType Type { get; set; }
        public CalendarItem(ItemType type, DateTime date, string header, string subject)
        {
            this.Type = type;
            this.Header = header;
            this.Date = date;
            this.Subject = subject;
        }

    }
    public class CalendarItems
    {
        private static List<CalendarItem> items = new List<CalendarItem>();
        //adds a calendar item to the list
        public static void AddCalendarItem(CalendarItem item)
        {
            items.Add(item);
        }
        //returns all calendar items for a date
        public static List<CalendarItem> GetCalendarItems(DateTime date)
        {
            return items.FindAll(i => i.Date.ToShortDateString() == date.ToShortDateString());
        }
        //returns all calendar items
        public static List<CalendarItem> GetCalendarItems()
        {
            return items;
        }
        //clears the list of calendar items
        public static void Clear()
        {
            items.Clear();
        }
    }

}
