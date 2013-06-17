using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    //the class the for score for an airline
    [ProtoContract]
    public class AirlineScores
    {
        [ProtoMember(1)]
        public List<int> CHR { get; set; }
        [ProtoMember(2)]
        public List<int> Safety { get; set; }
        [ProtoMember(3)]
        public List<int> Security { get; set; }
        [ProtoMember(4)]
        public List<int> EHR { get; set; }
        [ProtoMember(5)]
        public List<int> Maintenance { get; set; }
        public AirlineScores()
        {
            this.CHR = new List<int>();
            this.EHR = new List<int>();
            this.Maintenance = new List<int>();
            this.Safety = new List<int>();
            this.Security = new List<int>();
        }
    }
}
