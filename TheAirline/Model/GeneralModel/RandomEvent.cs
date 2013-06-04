using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.Model.GeneralModel
{
    class RandomEvent
    {
        public enum EventType { Safety, Security, Maintenance, Customer, Employee, Political }
        public EventType Type { get; set; }
        public Airline airline { get; set; }
        public string EventName { get; set; }
        public string EventMessage { get; set; }
        public FleetAirliner airliner { get; set; }
        public Airport airport { get; set; }
        public Country country { get; set; }
        public Route route { get; set; }
        public bool CriticalEvent { get; set; }
        public DateTime DateOccurred { get; set; }
        public int CustomerHappinessEffect { get; set; } //0-100
        public int AircraftDamageEffect { get; set; } //0-100
        public int AirlineSecurityEffect { get; set; } //0-100
        public int AirlineSafetyEffect { get; set; } //0-100
        public int EmployeeHappinessEffect { get; set; } //0-100
        public int FinancialPenalty { get; set; } //dollar amount to be added or subtracted from airline cash
        public double PaxDemandEffect { get; set; } //0-2
        public double CargoDemandEffect { get; set; } //0-2
        public int EffectLength { get; set; } //should be defined in months
        public string EventID { get; set; }
        public int Frequency { get; set; } //frequency per 3 years
        public RandomEvent(EventType type, string name, string message, bool critical, int custHappiness, int aircraftDamage, int airlineSecurity, int airlineSafety, int empHappiness, int moneyEffect, double paxDemand, double cargoDemand, int length, string id, int frequency)
        {

            this.DateOccurred = GameObject.GetInstance().GameTime;
            this.CustomerHappinessEffect = 0;
            this.AircraftDamageEffect = 0;
            this.AirlineSecurityEffect = 0;
            this.EmployeeHappinessEffect = 0;
            this.FinancialPenalty = 0;
            this.PaxDemandEffect = 1;
            this.CargoDemandEffect = 1;
            this.EffectLength = 1;
            this.CriticalEvent = false;
            this.EventName = "";
            this.EventMessage = "";
            this.Type = type;
            this.EventID = GameObject.GetInstance().GameTime.ToString() + this.airline.ToString();
        }

        public void ExecuteEvent(Airline airline, RandomEvent rEvent) 
        {
            rEvent.airliner.Airliner.Damaged += AircraftDamageEffect;
            airline.Money += rEvent.FinancialPenalty;
            airline.scoresCHR.Add(rEvent.CustomerHappinessEffect);
            airline.scoresEHR.Add(rEvent.EmployeeHappinessEffect);
            airline.scoresSafety.Add(rEvent.AirlineSafetyEffect);
            airline.scoresSecurity.Add(rEvent.AirlineSecurityEffect);
            //add pax and cargo demand modifier
        }

        public void GenerateEvents(Airline airline)
        {
            Random rnd = new Random();
            int eFreq = 0;
            int i = 0;
            int totalRating = airline.CustomerHappinessRating + airline.EmployeeHappinessRating + airline.SafetyRating + airline.SecurityRating;
            if (totalRating < 300)
            {
                eFreq = (int)rnd.Next(1, 6);
            }
            else if (totalRating < 200)
            {
                eFreq = (int)rnd.Next(4, 10);
            }
            else if (totalRating < 100)
            {
                eFreq = (int)rnd.Next(8, 16);
            }
            else eFreq = (int)rnd.Next(0, 4);

            //need some code to populate the actual events

        }

        /*public RandomEvent GenerateRandomEvent()
        {
            //code needed

        }*/

        public void AddEvent(Airline airline, RandomEvent rEvent)
        {
            airline.EventLog.Add(rEvent.EventID, rEvent);
        }

        public void RemoveEvent(Airline airline, RandomEvent rEvent)
        {
            airline.EventLog.Remove(rEvent.EventID);
        }

        public void CheckExpired()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (RandomEvent rEvent in airline.EventLog.Values)
                {
                    DateTime expDate = GameObject.GetInstance().GameTime.AddMonths(rEvent.EffectLength);
                    if (expDate < GameObject.GetInstance().GameTime)
                    {
                        RemoveEvent(airline, rEvent);
                    }  }  }
        }
    }

    public class RandomEvents
    {
        private static Dictionary<string, RandomEvent> events = new Dictionary<string, RandomEvent>();

        public static void Clear()
        {
            events = new Dictionary<string, RandomEvent>();
        }

        public static void AddEvent(RandomEvent rEvent)
        {
            events.Add(rEvent.EventName, rEvent);
        }

        public static RandomEvent GetEvent(string name)
        {
            return events[name];
        }

        public static List<RandomEvent> GetEvents()
        {
            return events.Values.ToList();
        }

        public static List<RandomEvent> GetEvents(RandomEvent.EventType type)
        {
            return GetEvents().FindAll((delegate(RandomEvent rEvent) {return rEvent.Type ==type; }));
        }

    }


}
