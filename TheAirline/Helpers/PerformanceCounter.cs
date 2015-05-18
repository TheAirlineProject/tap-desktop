using System;
using System.Collections.Generic;
using System.Linq;

namespace TheAirline.Helpers
{
    //the class for a performance counter
    public class PagePerformanceCounter
    {
        #region Constructors and Destructors

        public PagePerformanceCounter(string page, DateTime timeSpamp, long counter)
        {
            TimeStamp = timeSpamp;
            Counter = counter;
            Page = page;
        }

        #endregion

        #region Public Properties

        public long Counter { get; set; }

        public string Page { get; set; }

        public DateTime TimeStamp { get; set; }

        #endregion
    }

    //the collection of performance counters
    public class PerformanceCounters
    {
        #region Static Fields

        private static readonly List<PagePerformanceCounter> Counters = new List<PagePerformanceCounter>();

        #endregion

        #region Public Methods and Operators

        public static void AddPerformanceCounter(PagePerformanceCounter counter)
        {
            Counters.Add(counter);
        }

        //returns all pages for the list
        public static List<string> GetPages()
        {
            return Counters.Select(c => c.Page).Distinct().ToList();
        }

        //returns all counters for a specific page
        public static List<PagePerformanceCounter> GetPerformanceCounters(string page)
        {
            return Counters.FindAll(c => c.Page == page);
        }

        #endregion

        //adds a counter to the list
    }
}