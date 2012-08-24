using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a period
    public class Period
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Period(DateTime from, DateTime to)
        {
            this.To = to;
            this.From = from;
        }
    }
}
