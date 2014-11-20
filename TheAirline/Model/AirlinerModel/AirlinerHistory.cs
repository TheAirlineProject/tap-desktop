using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for the history of an airliner
    public class AirlinerHistory
    {
        public AirlinerHistory(AirlinerType type, string serialnumber, DateTime enddate)
        {
            this.Type = type;
            this.SerialNumber = serialnumber;
            this.EndDate = enddate;
            this.AirlineHistories = new List<AirlinerAirlineHistory>();
        }
        public List<AirlinerAirlineHistory> AirlineHistories { get; set; }
        public AirlinerType Type { get; set; }
        public string SerialNumber { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate
        {
            get
            {
                return getStartDate();
            }
            private set{;}
        }
    
        private DateTime getStartDate()
        {
           
            return this.AirlineHistories.Min(h => h.Date);
        }
        public Airline getAirline(DateTime date)
        {
            if (this.getStartDate() > date || this.EndDate < date)
                return null;

            if (this.AirlineHistories.Count == 1)
                return this.AirlineHistories[0].Airline;

            for (int i = 0; i < this.AirlineHistories.Count - 1; i++)
                if (this.AirlineHistories[i].Date <= date && this.AirlineHistories[i + 1].Date >= date)
                    return this.AirlineHistories[i].Airline;

            return this.AirlineHistories.Last().Airline;


        }
    }
    //the class for the airline history of an airliner
    public class AirlinerAirlineHistory
    {
        public Airline Airline { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public AirlinerAirlineHistory(DateTime date, Airline airline, string title)
        {
            this.Airline = airline;
            this.Date = date;
            this.Title = title;
        }
    }
    //the list of airliner histories
    public class AirlinerHistories
    {
        private static List<AirlinerHistory> histories = new List<AirlinerHistory>();
        //adds a history to the list
        public static void AddHistory(AirlinerHistory history)
        {
            histories.Add(history);
        }
        //returns all the histories
        public static List<AirlinerHistory> GetHistories()
        {
            return histories;
        }
        //clears the histories
        public static void Clear()
        {
            histories.Clear();
        }
    }
}
