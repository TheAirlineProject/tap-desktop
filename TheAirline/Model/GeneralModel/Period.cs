
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    [Serializable]
    //the class for a period
    public class Period<T>
    {
        
        public T From { get; set; }
        
        public T To { get; set; }
        public Period(T from, T to)
        {
            this.To = to;
            this.From = from;
        }
    }
   
}
