using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an engine type 
    public class EngineType
    {
        public enum TypeOfEngine { Jet, Turboprop }
        public enum NoiseLevel { High, Medium, Low }
        public string Model { get; set; }
     //   public Period<int> Produced { get; set; }
        public int MaxSpeed { get; set; }
        public double RunwayModifier { get; set; }
        public double RangeModifier { get; set; }
        public double ConsumptationModifier { get; set; }
        public int Ceiling { get; set; }
        public long Price { get; set; }
       // public EngineType(

            //ryk til 040Dev
    }
    /*
     * <engine manufacturer="Rolls-Royce" model="Trent 800" >
	  <specs type="jet" noise="high" consumptionModifier=".99" price="21150000" />
	  <performance maxSpeed="930" ceiling="43000" runwaylengthrequiredModifier=".99" rangeModifier=".97" />
	  <aircraft models="Boeing 777-200, Boeing 777-300, Boeing 777-200ER" />
	  <produced from="1995" to="2024" />
  </engine>*/
}
