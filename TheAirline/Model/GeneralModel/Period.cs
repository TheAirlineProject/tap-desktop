using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    [ProtoContract]
    //the class for a period
    public class Period<T>
    {
        [ProtoMember(1)]
        public T From { get; set; }
        [ProtoMember(2)]
        public T To { get; set; }
        public Period(T from, T to)
        {
            this.To = to;
            this.From = from;
        }
    }
   
}
