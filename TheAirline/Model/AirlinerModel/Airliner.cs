﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner
    [Serializable]
    public class Airliner : ISerializable
    {
        #region Enums

        public enum StatusTypes
        {
            Normal,
            Leasing
        }

        #endregion

        #region Fields

        private readonly Random _rnd = new Random();

        #endregion

        #region Constructors and Destructors

        public Airliner(string id, AirlinerType type, string tailNumber, DateTime builtDate)
        {
            ID = id;
            BuiltDate = new DateTime(builtDate.Year, builtDate.Month, builtDate.Day);
            Type = type;
            LastServiceCheck = 0;
            TailNumber = tailNumber;
            Flown = 0;
            Condition = _rnd.Next(90, 100);
            Status = StatusTypes.Normal;

            Classes = new List<AirlinerClass>();

            if (Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                var aClass = new AirlinerClass(
                    AirlinerClass.ClassType.EconomyClass,
                    ((AirlinerPassengerType) Type).MaxSeatingCapacity);
                aClass.CreateBasicFacilities(Airline);
                Classes.Add(aClass);
            }

            if (Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
            {
                var aClass = new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 0);
                aClass.CreateBasicFacilities(Airline);
                Classes.Add(aClass);
            }
        }

        private Airliner(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                    {
                        ((FieldInfo) prop).SetValue(this, entry.Value);
                    }
                    else
                    {
                        ((PropertyInfo) prop).SetValue(this, entry.Value);
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning) p.GetCustomAttribute(typeof (Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning) notSet.GetCustomAttribute(typeof (Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo) notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }

            Classes.RemoveAll(c => c == null);

            var doubleClasses =
                new List<AirlinerClass.ClassType>(
                    Classes.Where(c => Classes.Count(cc => cc.Type == c.Type) > 1).Select(c => c.Type));

            foreach (AirlinerClass.ClassType doubleClassType in doubleClasses)
            {
                AirlinerClass dClass = Classes.Last(c => c.Type == doubleClassType);
                Classes.Remove(dClass);
            }

            if (version == 1)
                EngineType = null;
            if (version < 3)
                FlownHours = new TimeSpan();
            if (version < 4)
                Status = StatusTypes.Normal;
            if (version < 5)
            {
                Owner = Airline;
            }
        }

        #endregion

        #region Public Properties

        public int Age
        {
            get { return GetAge(); }
        }

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("built")]
        public DateTime BuiltDate { get; set; }

        [Versioning("classes")]
        public List<AirlinerClass> Classes { get; set; }

        [Versioning("enginetype", Version = 2)]
        public EngineType EngineType { get; set; }

        public string CabinConfiguration
        {
            get
            {
                if (Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                {
                    return string.Format(
                        "{0}F | {1}C | {2}Y",
                        GetSeatingCapacity(AirlinerClass.ClassType.FirstClass),
                        GetSeatingCapacity(AirlinerClass.ClassType.BusinessClass),
                        GetSeatingCapacity(AirlinerClass.ClassType.EconomyClass));
                }

                if (Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
                {
                    return string.Format("{0} t", GetCargoCapacity());
                }

                return string.Format(
                    "{0}F | {1}C | {2}Y | {3} t",
                    GetSeatingCapacity(AirlinerClass.ClassType.FirstClass),
                    GetSeatingCapacity(AirlinerClass.ClassType.BusinessClass),
                    GetSeatingCapacity(AirlinerClass.ClassType.EconomyClass),
                    GetCargoCapacity());
            }
        }

        [Versioning("condition")]
        public double Condition { get; set; }

        [Versioning("flownhours")]
        public TimeSpan FlownHours { get; set; }

        [Versioning("flown")]
        public double Flown { get; set; }

        [Versioning("fuelcapacity")]
        public long FuelCapacity { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        //distance flown by the airliner

        [Versioning("lastservice")]
        public double LastServiceCheck { get; set; }

        //the km were the airliner was last at service

        public long LeasingPrice
        {
            get { return GetLeasingPrice(); }
        }

        public long Price
        {
            get { return GetPrice(); }
        }

        public Country Registered
        {
            get { return Countries.GetCountryFromTailNumber(TailNumber); }
        }

        public double FuelConsumption
        {
            get
            {
                if (EngineType == null)
                    return Type.FuelConsumption;
                else
                    return Type.FuelConsumption*EngineType.ConsumptationModifier;
            }
        }

        public double CruisingSpeed
        {
            get
            {
                if (EngineType == null)
                    return Type.CruisingSpeed;
                else
                    return Math.Min(Type.CruisingSpeed, EngineType.MaxSpeed);
            }
        }

        public long MinRunwaylength
        {
            get
            {
                if (EngineType == null)
                    return Type.MinRunwaylength;
                else
                    return Convert.ToInt64(Type.MinRunwaylength*EngineType.RunwayModifier);
            }
        }

        public long Range
        {
            get
            {
                if (EngineType == null)
                    return Type.Range;
                else
                    return Convert.ToInt64(Type.Range*EngineType.RangeModifier);
            }
        }

        [Versioning("status", Version = 4)]
        public StatusTypes Status { get; set; }

        [Versioning("tailnumber")]
        public string TailNumber { get; set; }

        [Versioning("type")]
        public AirlinerType Type { get; set; }

        [Versioning("owner", Version = 4)]
        public Airline Owner { get; set; }

        private int GetSeatingCapacity(AirlinerClass.ClassType classType)
        {
            return Classes.Exists(x => x.Type == classType) ? Classes.Find(x => x.Type == classType).SeatingCapacity : 0;
        }

        #endregion

        #region Public Methods and Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 5);

            Type myType = GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                      .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                          .Where(p => p.GetCustomAttribute(typeof (Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo) member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo) member).GetValue(this, null);
                }

                var att = (Versioning) member.GetCustomAttribute(typeof (Versioning));

                info.AddValue(att.Name, propValue);
            }
        }

        //adds a new airliner class to the airliner
        public void AddAirlinerClass(AirlinerClass airlinerClass)
        {
            if (airlinerClass != null)
            {
                if (Classes.Exists(c => c.Type == airlinerClass.Type))
                {
                    Classes[Classes.FindIndex(c => c.Type == airlinerClass.Type)] = airlinerClass;
                }
                else
                {
                    Classes.Add(airlinerClass);
                    if (airlinerClass.GetFacilities().Count == 0)
                    {
                        airlinerClass.CreateBasicFacilities(Airline);
                    }
                }
            }
        }

        //removes an airliner class from the airliner

        //clear the list of airliner classes
        public void ClearAirlinerClasses()
        {
            Classes.Clear();
        }

        public int GetAge()
        {
            return MathHelpers.CalculateAge(BuiltDate, GameObject.GetInstance().GameTime);
        }

        //returns the total amount of seat capacity

        //returns the airliner class for the airliner
        public AirlinerClass GetAirlinerClass(AirlinerClass.ClassType type)
        {
            if (Classes.Exists(c => c.Type == type))
            {
                return Classes.Find(c => c.Type == type);
            }
            return Classes[0];
        }

        public double GetCargoCapacity()
        {
            if (Type is AirlinerCargoType)
            {
                return ((AirlinerCargoType) Type).CargoSize;
            }

            if (Type is AirlinerCombiType)
            {
                return ((AirlinerCombiType) Type).CargoSize;
            }

            return 0;
        }

        public long GetLeasingPrice()
        {
            double months = 12*15;
            double rate = 1.30;

            double leasingPrice = (GetPrice()*rate/months);
            return Convert.ToInt64(leasingPrice);
        }

        public long GetPrice()
        {
            double basePrice = Type.Price;

            double facilityPrice = 0;

            var classes = new List<AirlinerClass>(Classes);

            foreach (AirlinerClass aClass in classes)
            {
                AirlinerFacility audioFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Audio);
                AirlinerFacility videoFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Video);
                AirlinerFacility seatFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Seat);

                double audioPrice = audioFacility.PricePerSeat*audioFacility.PercentOfSeats*aClass.SeatingCapacity;
                double videoPrice = videoFacility.PricePerSeat*videoFacility.PercentOfSeats*aClass.SeatingCapacity;
                double seatPrice = seatFacility.PricePerSeat*seatFacility.PercentOfSeats*aClass.SeatingCapacity;

                facilityPrice += audioPrice + videoPrice + seatPrice;
            }

            basePrice += facilityPrice;

            double diffEnginePrice;

            if (EngineType == null)
                diffEnginePrice = 0;
            else
            {
                double enginePrice = EngineType.Price;
                double standardEnginePrice = EngineTypes.GetStandardEngineType(Type).Price;

                diffEnginePrice = enginePrice - standardEnginePrice;
            }

            basePrice += diffEnginePrice;

            int age = GetAge();
            double devaluationPercent = 1 - (0.02*age);

            return Convert.ToInt64(basePrice*devaluationPercent*(Condition/100));
        }

        public int GetTotalSeatCapacity()
        {
            int capacity = 0;
            foreach (AirlinerClass aClass in Classes)
            {
                capacity += aClass.SeatingCapacity;
            }

            return capacity;
        }

        public long GetValue()
        {
            if (GetAge() < 25)
            {
                return GetPrice()*(1 - (long) GetAge()*(3/100));
            }
            return GetPrice()*(20/100);
        }

        public void RemoveAirlinerClass(AirlinerClass airlinerClass)
        {
            Classes.Remove(airlinerClass);
        }

        #endregion
    }

    //the list of airliners
    public class Airliners
    {
        #region Static Fields

        private static List<Airliner> airliners = new List<Airliner>();

        #endregion

        #region Public Methods and Operators

        public static void AddAirliner(Airliner airliner)
        {
            lock (airliners)
            {
                //if (airliners.Exists(a => a.ID == airliner.ID))
                //  throw new Exception("Airliner element already exists exception");

                airliners.Add(airliner);
            }
        }

        public static void Clear()
        {
            airliners = new List<Airliner>();
        }

        //returns an airliner
        public static Airliner GetAirliner(string tailnumber)
        {
            return airliners.Find(delegate(Airliner airliner) { return airliner.TailNumber == tailnumber; });
        }

        //returns the list of airliners

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale()
        {
            return airliners.FindAll(a => a.Airline == null && a.Status == Airliner.StatusTypes.Normal);
        }

        //returns the list of airliners for leasing
        public static List<Airliner> GetAirlinersForLeasing()
        {
            return airliners.FindAll(a => a.Status == Airliner.StatusTypes.Leasing);
        }

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale(Predicate<Airliner> match)
        {
            return airliners.FindAll(a => a.Airline == null).FindAll(match);
        }

        public static List<Airliner> GetAllAirliners()
        {
            return airliners;
        }

        //removes an airliner from the list
        public static void RemoveAirliner(Airliner airliner)
        {
            airliners.Remove(airliner);
        }

        #endregion

        //clears the list

        //adds an airliner to the list
    }
}