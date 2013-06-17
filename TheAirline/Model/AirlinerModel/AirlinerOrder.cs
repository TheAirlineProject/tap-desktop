using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner order
    [ProtoContract]
    public class AirlinerOrder
    {
        [ProtoMember(1)]
        public List<AirlinerClass> Classes { get; set; }
        [ProtoMember(2)]
        public AirlinerType Type { get; set; }
        [ProtoMember(3)]
        public int Amount { get; set; }
        public AirlinerOrder(AirlinerType type, List<AirlinerClass> classes, int amount)
        {
            this.Type = type;
            this.Amount = amount;
            this.Classes = classes;
        }
    }
}
