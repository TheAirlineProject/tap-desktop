using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;

namespace TheAirline.Models.Airliners
{
    /*! Airliner class.
 * This class is used for a passenger class onboard of a airliner
 * The class needs parameters for the airliner, type of class and the seating capacity
 */

    [Serializable]
    public class AirlinerClass : BaseModel
    {
        #region Constructors and Destructors

        public AirlinerClass(ClassType type, int seatingCapacity)
        {
            Type = type;
            SeatingCapacity = seatingCapacity;
            RegularSeatingCapacity = seatingCapacity;
            Facilities = new Dictionary<AirlinerFacility.FacilityType, AirlinerFacility>();
        }

        private AirlinerClass(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum ClassType
        {
            [EnumMember(Value = "Economy")] EconomyClass = 1921,

            [EnumMember(Value = "Business")] BusinessClass = 1976,

            [EnumMember(Value = "First")] FirstClass = 1960
        }

        #endregion

        #region Public Properties

        public List<AirlinerFacility> AllFacilities => GetFacilities();

        [Versioning("facilities")]
        public Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities { get; set; }

        [Versioning("regularseating")]
        public int RegularSeatingCapacity { get; set; }

        [Versioning("seats")]
        public int SeatingCapacity { get; set; }

        [Versioning("type")]
        public ClassType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static List<ClassType> GetAirlinerTypes()
        {
            var types = new List<ClassType> {ClassType.EconomyClass, ClassType.BusinessClass, ClassType.FirstClass};

            return types;
        }

        public void CreateBasicFacilities(Airline airline)
        {
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof (AirlinerFacility.FacilityType)))
            {
                SetFacility(airline, AirlinerFacilities.GetBasicFacility(type));
                //this.Facilities.Add(type, AirlinerFacilities.GetBasicFacility(type));
            }
        }

        public void ForceSetFacility(AirlinerFacility facility)
        {
            Facilities[facility.Type] = facility;
        }

        public List<AirlinerFacility> GetFacilities()
        {
            return Facilities.Values.ToList();
        }

        public AirlinerFacility GetFacility(AirlinerFacility.FacilityType type)
        {
            if (Facilities.ContainsKey(type))
            {
                return Facilities[type];
            }
            return null;
        }

        public void SetFacility(Airline airline, AirlinerFacility facility)
        {
            Facilities[facility.Type] = facility;

            if (airline != null)
            {
                AirlineHelpers.AddAirlineInvoice(
                    airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Purchases,
                    -facility.PricePerSeat*facility.PercentOfSeats/100.0*SeatingCapacity);
            }
        }

        #endregion

        //returns all airliner types in the correct order
    }
}