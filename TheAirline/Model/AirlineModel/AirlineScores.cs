
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    //the class the for score for an airline
    [Serializable]
    public class AirlineScores
    {
        
        public List<int> CHR { get; set; }
        
        public List<int> Safety { get; set; }
        
        public List<int> Security { get; set; }
        
        public List<int> EHR { get; set; }
        
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
