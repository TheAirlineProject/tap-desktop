using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for a performance counter
    public class PagePerformanceCounter
    {
        public long Counter { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Page { get; set; }
        public PagePerformanceCounter(string page, DateTime timeSpamp, long counter)
        {
            this.TimeStamp = timeSpamp;
            this.Counter = counter;
            this.Page = page;
        }

    }
    //the collection of performance counters
    public class PerformanceCounters
    {
        private static List<PagePerformanceCounter> counters = new List<PagePerformanceCounter>();
        //adds a counter to the list
        public static void AddPerformanceCounter(PagePerformanceCounter counter)
        {
            counters.Add(counter);
        }
        //returns all pages for the list
        public static List<string> GetPages()
        {
            return counters.Select(c => c.Page).Distinct().ToList();
        }
        //returns all counters for a specific page
        public static List<PagePerformanceCounter> GetPerformanceCounters(string page)
        {
            return counters.FindAll(c => c.Page == page);
        }
    }
}
