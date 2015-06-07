using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airports;
using TheAirline.Models.Routes;

namespace TheAirline.Models.General
{
    /*the class for a special contract type*/

    public class SpecialContractType : BaseModel
    {
        public SpecialContractType(string name, string text, long payment, bool asbonus, long penalty, bool isfixeddate)
        {
            Name = name;
            Text = text;
            IsFixedDate = isfixeddate;
            Routes = new List<SpecialContractRoute>();
            Payment = payment;
            Penalty = penalty;
            AsBonus = asbonus;
            Requirements = new List<ContractRequirement>();
        }

        private SpecialContractType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("text")]
        public string Text { get; set; }

        [Versioning("fixed")]
        public bool IsFixedDate { get; set; }

        [Versioning("period")]
        public Period<DateTime> Period { get; set; }

        [Versioning("frequency")]
        public int Frequency { get; set; }

        //how often this contract type is available per year
        [Versioning("routes")]
        public List<SpecialContractRoute> Routes { get; set; }

        [Versioning("payment")]
        public long Payment { get; set; }

        [Versioning("bonus")]
        public bool AsBonus { get; set; }

        [Versioning("requirements")]
        public List<ContractRequirement> Requirements { get; set; }

        [Versioning("lastdate")]
        public DateTime LastDate { get; set; }

        [Versioning("penalty")]
        public long Penalty { get; set; }
    }

    /*the class for a contract route*/

    public class SpecialContractRoute : BaseModel
    {
        public SpecialContractRoute(Airport destination1, Airport destination2, long passengers, Route.RouteType routetype, bool bothways)
        {
            Departure = destination1;
            Destination = destination2;
            BothWays = bothways;
            PassengersPerDay = passengers;
            RouteType = routetype;
        }

        private SpecialContractRoute(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        [Versioning("departure")]
        public Airport Departure { get; set; }

        [Versioning("destination")]
        public Airport Destination { get; set; }

        [Versioning("bothways")]
        public bool BothWays { get; set; }

        [Versioning("passengers")]
        public long PassengersPerDay { get; set; }

        [Versioning("type")]
        public Route.RouteType RouteType { get; set; }
    }

    /*the class for a requirement for a contract*/

    public class ContractRequirement : BaseModel
    {
        public enum RequirementType
        {
            Destination,
            ClassType
        }

        public ContractRequirement(RequirementType type)
        {
            Type = type;
        }

        private ContractRequirement(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        [Versioning("type")]
        public RequirementType Type { get; set; }

        [Versioning("classtype")]
        public AirlinerClass.ClassType ClassType { get; set; }

        [Versioning("seats")]
        public int MinSeats { get; set; }

        [Versioning("destination")]
        public Airport Destination { get; set; }

        [Versioning("departure")]
        public Airport Departure { get; set; }
    }

    /*the list of special contracts*/

    public class SpecialContractTypes
    {
        private static readonly List<SpecialContractType> Types = new List<SpecialContractType>();
        //adds a special contract type
        public static void AddType(SpecialContractType type)
        {
            Types.Add(type);
        }

        //returns the list of types
        public static List<SpecialContractType> GetTypes()
        {
            return Types;
        }

        //returns all random contract types
        public static List<SpecialContractType> GetRandomTypes()
        {
            return Types.FindAll(t => !t.IsFixedDate);
        }

        //clears the list
        public static void Clear()
        {
            Types.Clear();
        }
    }
}