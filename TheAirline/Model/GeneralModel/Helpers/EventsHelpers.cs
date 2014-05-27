namespace TheAirline.Model.GeneralModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TheAirline.Model.AirlineModel;

    //the helpers class for events
    public class EventsHelpers
    {
        //returns a list of proportions of events based on current ratings

        //generates x number of events for each event type for the current year. Should be called only from OnNewYear

        #region Public Methods and Operators

        public static void GenerateEvents(Airline airline)
        {
            var rnd = new Random();
            var eventOccurences = new Dictionary<RandomEvent.EventType, double>();
            int eFreq = 0;
            double secEvents;
            double safEvents;
            double polEvents;
            double maintEvents;
            double custEvents;
            double empEvents;

            //sets an overall event frequency based on an airlines total overall rating
            int totalRating = airline.Ratings.CustomerHappinessRating + airline.Ratings.EmployeeHappinessRating
                              + airline.Ratings.SafetyRating + airline.Ratings.SecurityRating;
            if (totalRating < 300)
            {
                eFreq = rnd.Next(1, 6);
            }
            else if (totalRating < 200)
            {
                eFreq = rnd.Next(4, 10);
            }
            else if (totalRating < 100)
            {
                eFreq = rnd.Next(8, 16);
            }
            else
            {
                eFreq = rnd.Next(0, 4);
            }

            //gets the event proportions and multiplies them by total # events to get events per type
            List<double> probs = GetEventProportions(airline);
            custEvents = eFreq * probs[0];
            empEvents = eFreq * probs[1];
            secEvents = eFreq * probs[2];
            safEvents = eFreq * probs[3];
            maintEvents = eFreq * probs[4];
            polEvents = eFreq - custEvents - empEvents - secEvents - maintEvents;
            eventOccurences.Add(RandomEvent.EventType.Customer, custEvents);
            eventOccurences.Add(RandomEvent.EventType.Employee, empEvents);
            eventOccurences.Add(RandomEvent.EventType.Maintenance, maintEvents);
            eventOccurences.Add(RandomEvent.EventType.Safety, safEvents);
            eventOccurences.Add(RandomEvent.EventType.Security, secEvents);
            eventOccurences.Add(RandomEvent.EventType.Political, polEvents);

            /*
            //generates the given number of events for each type
            foreach (KeyValuePair<RandomEvent.EventType, double> v in eventOccurences)
            {
                int k = (int)v.Value;
                List<RandomEvent> list = RandomEvents.GetEvents(v.Key, k, airline);
                foreach (RandomEvent e in list)
                {
                    airline.EventLog.Add(e);
                }
            }
            */
        }

        public static List<double> GetEventProportions(Airline airline)
        {
            //chr 0 ehr 1 scr 2 sfr 3 total 4
            var ratings = new List<int>();
            ratings.Add(100 - airline.Ratings.CustomerHappinessRating);
            ratings.Add(100 - airline.Ratings.EmployeeHappinessRating);
            ratings.Add(100 - airline.Ratings.SecurityRating);
            ratings.Add(100 - airline.Ratings.SafetyRating);
            ratings.Add(100 - airline.Ratings.MaintenanceRating);
            ratings.Add(500 - ratings.Sum());
            double pCHR = ratings[0] / ratings[5];
            double pEHR = ratings[1] / ratings[5];
            double pSCR = ratings[2] / ratings[5];
            double pSFR = ratings[3] / ratings[5];
            double pMTR = ratings[4] / ratings[5];
            ratings.Clear();
            var pRatings = new List<double>();
            pRatings.Add(pCHR);
            pRatings.Add(pEHR);
            pRatings.Add(pSCR);
            pRatings.Add(pSFR);
            pRatings.Add(pMTR);
            return pRatings;
        }

        #endregion
    }
}