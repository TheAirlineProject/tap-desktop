using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel
{
    [Serializable]
    public class RandomEvent : BaseModel
    {
        #region Constructors and Destructors

        public RandomEvent(
            EventType type,
            Focus focus,
            int airlineSafety,
            int moneyEffect,
            string id,
            int frequency,
            DateTime stat,
            DateTime end,
            string name = "",
            string message = "",
            bool critical = false,
            int custHappiness = 0,
            int aircraftDamage = 0,
            int airlineSecurity = 0,
            int empHappiness = 0,
            double paxDemand = 1,
            double cargoDemand = 1,
            int length = 1)
        {
            DateOccurred = GameObject.GetInstance().GameTime;
            CustomerHappinessEffect = custHappiness;
            AircraftDamageEffect = aircraftDamage;
            AirlineSecurityEffect = airlineSecurity;
            EmployeeHappinessEffect = empHappiness;
            FinancialPenalty = 0;
            PaxDemandEffect = paxDemand;
            CargoDemandEffect = cargoDemand;
            EffectLength = length;
            CriticalEvent = critical;
            EventName = name;
            EventMessage = message;
            Type = type;

            EventID = id;
        }

        private RandomEvent(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum EventType
        {
            Safety,

            Security,

            Maintenance,

            Customer,

            Employee,

            Political
        }

        public enum Focus
        {
            Aircraft,

            Airport,

            Airline
        }

        #endregion

        #region Public Properties

        public int AircraftDamageEffect { get; set; }

        [Versioning("airline")]
        public Airline Airline { get; set; }

        public int AirlineSafetyEffect { get; set; }

        public int AirlineSecurityEffect { get; set; }

        [Versioning("airliner")]
        public FleetAirliner Airliner { get; set; }

        [Versioning("airport")]
        public Airport Airport { get; set; }

        public double CargoDemandEffect { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        public bool CriticalEvent { get; set; }

        public int CustomerHappinessEffect { get; set; }

        public DateTime DateOccurred { get; set; }

        //0-100

        //0-2

        public int EffectLength { get; set; }

        public int EmployeeHappinessEffect { get; set; }

        public DateTime End { get; set; }

        //should be defined in months

        public string EventID { get; set; }

        [Versioning("message")]
        public string EventMessage { get; set; }

        [Versioning("name")]
        public string EventName { get; set; }

        public int FinancialPenalty { get; set; }

        public int Frequency { get; set; }

        public double PaxDemandEffect { get; set; }

        [Versioning("route")]
        public Route Route { get; set; }

        //frequency per 3 years

        public DateTime Start { get; set; }

        [Versioning("type")]
        public EventType Type { get; set; }

        [Versioning("focus")]
        public Focus focus { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static void CheckExpired()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (RandomEvent rEvent in airline.EventLog)
                {
                    DateTime expDate = GameObject.GetInstance().GameTime.AddMonths(rEvent.EffectLength);
                    if (expDate < GameObject.GetInstance().GameTime)
                    {
                        PassengerHelpers.ChangePaxDemand(airline, (1/rEvent.PaxDemandEffect));
                        RemoveEvent(airline, rEvent);
                    }
                }
            }
        }

        public static void RemoveEvent(Airline airline, RandomEvent rEvent)
        {
            airline.EventLog.Remove(rEvent);
        }

        public void AddEvent(Airline airline, RandomEvent rEvent)
        {
            airline.EventLog.Add(rEvent);
        }

        public void ExecuteEvents(Airline airline, DateTime time)
        {
            var rnd = new Random();
            foreach (RandomEvent rEvent in airline.EventLog)
            {
                if (rEvent.DateOccurred.DayOfYear == time.DayOfYear)
                {
                    rEvent.Airliner.Airliner.Condition += AircraftDamageEffect;
                    airline.Money += rEvent.FinancialPenalty;
                    airline.Scores.CHR.Add(rEvent.CustomerHappinessEffect);
                    airline.Scores.EHR.Add(rEvent.EmployeeHappinessEffect);
                    airline.Scores.Safety.Add(rEvent.AirlineSafetyEffect);
                    airline.Scores.Security.Add(rEvent.AirlineSecurityEffect);
                    PassengerHelpers.ChangePaxDemand(airline, (rEvent.PaxDemandEffect*rnd.Next(9, 11)/10));
                }
            }
        }

        #endregion

        //checks if an event's effects are expired
    }

    public class RandomEvents
    {
        #region Static Fields

        private static Dictionary<string, RandomEvent> _events = new Dictionary<string, RandomEvent>();

        #endregion

        #region Public Methods and Operators

        public static void AddEvent(RandomEvent rEvent)
        {
            _events.Add(rEvent.EventName, rEvent);
        }

        public static void Clear()
        {
            _events = new Dictionary<string, RandomEvent>();
        }

        //gets a single event by name
        public static RandomEvent GetEvent(string name)
        {
            return _events[name];
        }

        //gets a list of all events
        public static List<RandomEvent> GetEvents()
        {
            return _events.Values.ToList();
        }

        //gets all events of a given type
        public static List<RandomEvent> GetEvents(RandomEvent.EventType type)
        {
            return GetEvents().FindAll((rEvent => rEvent.Type == type));
        }

        //gets x number of random events of a given type
        public static List<RandomEvent> GetEvents(RandomEvent.EventType type, int number, Airline airline)
        {
            var rnd = new Random();
            var rEvents = new Dictionary<int, RandomEvent>();
            List<RandomEvent> tEvents = GetEvents(type);
            int i = 1;
            int j = 0;
            foreach (RandomEvent r in tEvents)
            {
                if (r.Start <= GameObject.GetInstance().GameTime && r.End >= GameObject.GetInstance().GameTime)
                {
                    {
                        r.DateOccurred = MathHelpers.GetRandomDate(
                            GameObject.GetInstance().GameTime,
                            GameObject.GetInstance().GameTime.AddMonths(12));
                        r.Airline = airline;
                        r.Airliner = AirlinerHelpers.GetRandomAirliner(airline);
                        r.Route = r.Airliner.Routes[rnd.Next(r.Airliner.Routes.Count())];
                        r.Country = r.Route.Destination1.Profile.Country;
                        r.Airport = r.Route.Destination1;

                        if (r.focus == RandomEvent.Focus.Airline)
                        {
                            r.Airliner = null;
                            r.Airport = null;
                            r.Country = null;
                            r.Route = null;
                        }

                        rEvents.Add(i, r);
                        i++;
                    }
                }
            }

            tEvents.Clear();

            while (j < number)
            {
                int item = rnd.Next(rEvents.Count());
                tEvents.Add(rEvents[item]);
                j++;
            }

            return tEvents;
        }

        #endregion
    }
}