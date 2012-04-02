using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    /*! Airliner class.
 * This class is used for a passenger class onboard of a airliner
 * The class needs parameters for the airliner, type of class and the seating capacity
 */
    public class AirlinerClass
    {
        private Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities;
        public enum ClassType { Economy_Class = 1, Business_Class = 3, First_Class = 6 }
        public ClassType Type { get; set; }
        public int RegularSeatingCapacity { get; set; }
        public int SeatingCapacity { get; set; }
        public Airliner Airliner { get; set; }
        public AirlinerClass(Airliner airliner, ClassType type, int seatingCapacity)
        {
            this.Type = type;
            this.Airliner = airliner;
            this.SeatingCapacity = seatingCapacity;
            this.RegularSeatingCapacity = seatingCapacity;
            this.Facilities = new Dictionary<AirlinerFacility.FacilityType, AirlinerFacility>();


            createBasicFacilities();
        }
        //sets the facility for a facility type
        public void setFacility(AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

            if (this.Airliner.Airline != null)
                this.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.PricePerSeat * facility.PercentOfSeats / 100.0 * this.SeatingCapacity));
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
        //creates the basic facilities
        private void createBasicFacilities()
        {
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                this.setFacility(AirlinerFacilities.GetBasicFacility(type));
                //this.Facilities.Add(type, AirlinerFacilities.GetBasicFacility(type));
            }

        }
    }
}
