
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel
{
    /*! Airliner class.
 * This class is used for a passenger class onboard of a airliner
 * The class needs parameters for the airliner, type of class and the seating capacity
 */
    [DataContract]
    public class AirlinerClass
    {
        public List<AirlinerFacility> AllFacilities
        {
            get
            { 
                return getFacilities(); 
            }
            private set { ;}
        }
        [DataMember]
        private Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities;
        
        public enum ClassType { 
            [EnumMember(Value="Economy")] Economy_Class=1921, 
            [EnumMember(Value="Business")] Business_Class = 1976, 
            [EnumMember(Value="First")] First_Class=1960}
        
        [DataMember]
        public ClassType Type { get; set; }
        [DataMember]
        public int RegularSeatingCapacity { get; set; }
        [DataMember]
        public int SeatingCapacity { get; set; }
        public AirlinerClass(ClassType type, int seatingCapacity)
        {
            this.Type = type;
            this.SeatingCapacity = seatingCapacity;
            this.RegularSeatingCapacity = seatingCapacity;
            this.Facilities = new Dictionary<AirlinerFacility.FacilityType, AirlinerFacility>();

        }
        //sets the facility for a facility type
        public void setFacility(Airline airline, AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

            if (airline != null)
                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.PricePerSeat * facility.PercentOfSeats / 100.0 * this.SeatingCapacity);
   

        }
        //force sets the facility for a facility type without cost
        public void forceSetFacility(AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

        }
        //returns the current facility for a facility type
        public AirlinerFacility getFacility(AirlinerFacility.FacilityType type)
        {
            return this.Facilities[type];
        }
        //returns all facilities for the class
        public List<AirlinerFacility> getFacilities()
        {
            return this.Facilities.Values.ToList();
        }
        //creates the basic facilities
        public void createBasicFacilities(Airline airline)
        {
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                this.setFacility(airline,AirlinerFacilities.GetBasicFacility(type));
                //this.Facilities.Add(type, AirlinerFacilities.GetBasicFacility(type));
            }

        }
        //returns all airliner types in the correct order
        public static List<ClassType> GetAirlinerTypes()
        {
            var types = new List<ClassType>();

            types.Add(ClassType.Economy_Class);
            types.Add(ClassType.Business_Class);
            types.Add(ClassType.First_Class);

            return types;
        }
    }
}
