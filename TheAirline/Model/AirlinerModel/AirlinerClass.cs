
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    [Serializable]
    public class AirlinerClass : ISerializable
    {
        public List<AirlinerFacility> AllFacilities
        {
            get
            {
                return getFacilities();
            }
            private set { ;}
        }
        [Versioning("facilities")]
        public Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities{ get; set; } 

        public enum ClassType
        {
            [EnumMember(Value = "Economy")]
            Economy_Class = 1921,
            [EnumMember(Value = "Business")]
            Business_Class = 1976,
            [EnumMember(Value = "First")]
            First_Class = 1960
        }

        [Versioning("type")]
        public ClassType Type { get; set; }
        [Versioning("regularseating")]
        public int RegularSeatingCapacity { get; set; }
        [Versioning("seats")]
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
            if (this.Facilities.ContainsKey(type))
                return this.Facilities[type];
            else
                return null;
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
                this.setFacility(airline, AirlinerFacilities.GetBasicFacility(type));
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
            private AirlinerClass(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


        }
    }
}
