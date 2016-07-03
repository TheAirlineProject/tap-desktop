using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.Airliners
{
    //the class for an airliner
    [Serializable]
    public class Airliner : BaseModel
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

        private Airliner(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            Classes.RemoveAll(c => c == null);

            var doubleClasses =
                new List<AirlinerClass.ClassType>(
                    Classes.Where(c => Classes.Count(cc => cc.Type == c.Type) > 1).Select(c => c.Type));

            foreach (var doubleClassType in doubleClasses)
            {
                var dClass = Classes.Last(c => c.Type == doubleClassType);
                Classes.Remove(dClass);
            }

            if (Version == 1)
                EngineType = null;
            if (Version < 3)
                FlownHours = new TimeSpan();
            if (Version < 4)
                Status = StatusTypes.Normal;
            if (Version < 5)
            {
                Owner = Airline;
            }
        }

        #endregion

        #region Public Properties

        public int Age => GetAge();

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
                    return
                        $"{GetSeatingCapacity(AirlinerClass.ClassType.FirstClass)}F | {GetSeatingCapacity(AirlinerClass.ClassType.BusinessClass)}C | {GetSeatingCapacity(AirlinerClass.ClassType.EconomyClass)}Y";
                }

                if (Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
                {
                    return $"{GetCargoCapacity()} t";
                }

                return
                    $"{GetSeatingCapacity(AirlinerClass.ClassType.FirstClass)}F | {GetSeatingCapacity(AirlinerClass.ClassType.BusinessClass)}C | {GetSeatingCapacity(AirlinerClass.ClassType.EconomyClass)}Y | {GetCargoCapacity()} t";
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

        public long LeasingPrice => GetLeasingPrice();

        public long Price => GetPrice();

        public Country Registered => Countries.GetCountryFromTailNumber(TailNumber);

        public double FuelConsumption
        {
            get
            {
                if (EngineType == null)
                    return Type.FuelConsumption;
                return Type.FuelConsumption*EngineType.ConsumptationModifier;
            }
        }

        public double CruisingSpeed => EngineType == null ? Type.CruisingSpeed : Math.Min(Type.CruisingSpeed, EngineType.MaxSpeed);

        public long MinRunwaylength => EngineType == null
            ? Type.MinRunwaylength
            : Convert.ToInt64(Type.MinRunwaylength*EngineType.RunwayModifier);

        public long Range => EngineType == null ? Type.Range : Convert.ToInt64(Type.Range*EngineType.RangeModifier);

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 5);

            base.GetObjectData(info, context);
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
            const double months = 12*15;
            const double rate = 1.30;

            var leasingPrice = (GetPrice()*rate/months);
            return Convert.ToInt64(leasingPrice);
        }

        public long GetPrice()
        {
            double basePrice = Type.Price;

            var classes = new List<AirlinerClass>(Classes);

            var facilityPrice = (from aClass in classes
                let audioFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Audio)
                let videoFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Video)
                let seatFacility = aClass.GetFacility(AirlinerFacility.FacilityType.Seat)
                let audioPrice = audioFacility.PricePerSeat*audioFacility.PercentOfSeats*aClass.SeatingCapacity
                let videoPrice = videoFacility.PricePerSeat*videoFacility.PercentOfSeats*aClass.SeatingCapacity
                let seatPrice = seatFacility.PricePerSeat*seatFacility.PercentOfSeats*aClass.SeatingCapacity
                select audioPrice + videoPrice + seatPrice).Sum();

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

            var age = GetAge();
            var devaluationPercent = 1 - (0.02*age);

            return Convert.ToInt64(basePrice*devaluationPercent*(Condition/100));
        }

        public int GetTotalSeatCapacity()
        {
            return Classes.Sum(aClass => aClass.SeatingCapacity);
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

        private static List<Airliner> _airliners = new List<Airliner>();

        #endregion

        #region Public Methods and Operators

        public static void AddAirliner(Airliner airliner)
        {
            lock (_airliners)
            {
                //if (airliners.Exists(a => a.ID == airliner.ID))
                //  throw new Exception("Airliner element already exists exception");

                _airliners.Add(airliner);
            }
        }

        public static void Clear()
        {
            _airliners = new List<Airliner>();
        }

        //returns an airliner
        public static Airliner GetAirliner(string tailnumber)
        {
            return _airliners.Find(airliner => airliner.TailNumber == tailnumber);
        }

        //returns the list of airliners

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale()
        {
            return _airliners.FindAll(a => a.Airline == null && a.Status == Airliner.StatusTypes.Normal);
        }

        //returns the list of airliners for leasing
        public static List<Airliner> GetAirlinersForLeasing()
        {
            return _airliners.FindAll(a => a.Status == Airliner.StatusTypes.Leasing);
        }

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale(Predicate<Airliner> match)
        {
            return _airliners.FindAll(a => a.Airline == null).FindAll(match);
        }

        public static List<Airliner> GetAllAirliners()
        {
            return _airliners;
        }

        //removes an airliner from the list
        public static void RemoveAirliner(Airliner airliner)
        {
            _airliners.Remove(airliner);
        }

        #endregion
    }
}