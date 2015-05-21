using System;
using System.Collections.Generic;
using System.Linq;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.Helpers
{
    //the class for some general airline helpers
    public class AirlinerHelpers
    {
        #region Static Fields

        private static readonly Random Rnd = new Random();

        #endregion

        #region Public Methods and Operators

        //returns a calculated weight for an airliner
        public static double GetCalculatedWeight(AirlinerType type)
        {
            return GetCalculatedWeight(type.Wingspan, type.Length, type.FuelCapacity);
        }

        public static double GetCalculatedWeight(double wingspan, double lenght, long fuel)
        {
            return (wingspan*lenght*4) + fuel;
        }

        public static void CreateAirlinerClasses(Airliner airliner)
        {
            airliner.ClearAirlinerClasses();

            List<AirlinerClass> classes = GetAirlinerClasses(airliner.Type);

            foreach (AirlinerClass aClass in classes)
            {
                airliner.AddAirlinerClass(aClass);
            }
        }

        public static Airliner CreateAirlinerFromYear(int year)
        {
            Guid id = Guid.NewGuid();

            List<AirlinerType> types =
                AirlinerTypes.GetTypes(t => t.Produced.From.Year < year && t.Produced.To.Year > year);

            int typeNumber = Rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = Rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = year;

            var airliner = new Airliner(
                id.ToString(),
                type,
                country.TailNumbers.GetNextTailNumber(),
                new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = Rnd.Next(1000, 100000);
            long km = kmPerYear*age;

            airliner.Flown = km;

            List<EngineType> engines = EngineTypes.GetEngineTypes(airliner.Type, GameObject.GetInstance().GameTime.Year);

            if (engines.Count > 0)
            {
                int engineNumber = Rnd.Next(engines.Count);

                airliner.EngineType = engines[engineNumber];
            }


            CreateAirlinerClasses(airliner);

            return airliner;
        }

        public static void CreateStartUpAirliners()
        {
            int number =
                AirlinerTypes.GetTypes(
                    t => t.Produced.From <= GameObject.GetInstance().GameTime
                         && t.Produced.To.AddYears(-10) >= GameObject.GetInstance().GameTime.AddYears(-30))
                             .Count*Rnd.Next(1, 3);

            int airlines = Airlines.GetNumberOfAirlines();

            number = (airlines*number)/10;
            for (int i = 0; i < number; i++)
            {
                Airliners.AddAirliner(CreateAirliner(0));
            }

            int airlinersForLease = number/20;

            for (int i = 0; i < airlinersForLease; i++)
            {
                Airliners.AddAirliner(CreateLeasingAirliner());
            }
        }

        //returns the delivery date for a order of airliners

        //returns the code for an airliner class
        public static string GetAirlinerClassCode(AirlinerClass aClass)
        {
            string symbol = "Y";

            if (aClass.Type == AirlinerClass.ClassType.BusinessClass)
            {
                symbol = "C";
            }

            if (aClass.Type == AirlinerClass.ClassType.FirstClass)
            {
                symbol = "F";
            }

            if (aClass.Type == AirlinerClass.ClassType.EconomyClass)
            {
                symbol = "Y";
            }

            return $"{aClass.SeatingCapacity}{symbol}";
        }

        public static List<AirlinerClass> GetAirlinerClasses(AirlinerType type)
        {
            var classes = new List<AirlinerClass>();

            var passengerType = type as AirlinerPassengerType;
            if (passengerType != null)
            {
                Configuration airlinerTypeConfiguration =
                    Configurations.GetConfigurations(Configuration.ConfigurationType.AirlinerType)
                                  .Find(
                                      c =>
                                      ((AirlinerTypeConfiguration) c).Airliner == type
                                      && ((AirlinerTypeConfiguration) c).Period.From <= GameObject.GetInstance().GameTime
                                      && ((AirlinerTypeConfiguration) c).Period.To > GameObject.GetInstance().GameTime);

                if (airlinerTypeConfiguration == null)
                {
                    int seatingDiff = 0;
                    AirlinerConfiguration configuration = null;

                    int numOfClasses = Rnd.Next(0, passengerType.MaxAirlinerClasses) + 1;

                    if (GameObject.GetInstance().GameTime.Year >= (int) AirlinerClass.ClassType.BusinessClass)
                    {
                        if (numOfClasses == 1)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("200");
                        }
                        if (numOfClasses == 2)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("202");
                        }
                        if (numOfClasses == 3)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("203");
                        }
                    }
                    else
                    {
                        if (numOfClasses == 1)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("200");
                        }
                        if (numOfClasses == 2)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("201");
                        }
                        if (numOfClasses == 3)
                        {
                            configuration = (AirlinerConfiguration) Configurations.GetStandardConfiguration("201");
                        }
                    }

                    if (configuration != null)
                    {
                        foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                        {
                            var airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity) {RegularSeatingCapacity = aClass.RegularSeatingCapacity};

                            foreach (AirlinerFacility facility in aClass.GetFacilities())
                            {
                                airlinerClass.SetFacility(null, facility);
                            }

                            foreach (
                                AirlinerFacility.FacilityType fType in Enum.GetValues(typeof (AirlinerFacility.FacilityType))
                                )
                            {
                                if (!aClass.Facilities.Exists(f => f.Type == fType))
                                {
                                    airlinerClass.SetFacility(null, AirlinerFacilities.GetBasicFacility(fType));
                                }
                            }

                            airlinerClass.SeatingCapacity =
                                Convert.ToInt16(
                                    Convert.ToDouble(airlinerClass.RegularSeatingCapacity)
                                    /airlinerClass.GetFacility(AirlinerFacility.FacilityType.Seat).SeatUses);

                            classes.Add(airlinerClass);
                        }

                        seatingDiff = passengerType.MaxSeatingCapacity - configuration.MinimumSeats;
                    }

                    AirlinerClass economyClass = classes.Find(c => c.Type == AirlinerClass.ClassType.EconomyClass);
                    economyClass.RegularSeatingCapacity += seatingDiff;

                    AirlinerFacility seatingFacility = economyClass.GetFacility(AirlinerFacility.FacilityType.Seat);

                    var extraSeats = (int) (seatingDiff/seatingFacility.SeatUses);

                    economyClass.SeatingCapacity += extraSeats;
                }
                else
                {
                    foreach (
                        AirlinerClassConfiguration aClass in
                            ((AirlinerTypeConfiguration) airlinerTypeConfiguration).Classes)
                    {
                        var airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity) {RegularSeatingCapacity = aClass.RegularSeatingCapacity};

                        foreach (AirlinerFacility facility in aClass.GetFacilities())
                        {
                            airlinerClass.SetFacility(null, facility);
                        }

                        airlinerClass.SeatingCapacity =
                            Convert.ToInt16(
                                Convert.ToDouble(airlinerClass.RegularSeatingCapacity)
                                /airlinerClass.GetFacility(AirlinerFacility.FacilityType.Seat).SeatUses);

                        classes.Add(airlinerClass);
                    }
                }
            }
            else if (type is AirlinerCargoType)
            {
                var cargoClass = new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 0);
                classes.Add(cargoClass);
            }
            else if (type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
            {
                var helicopterClass = new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 0);
                classes.Add(helicopterClass);
            }
            return classes;
        }

        //returns the cargo size for a passenger airliner if needed to converted

        //return the days for converting a passenger airliner to a cargo airliner
        public static int GetCargoConvertingDays(AirlinerPassengerType type)
        {
            return (int) (Convert.ToDouble(type.MaxSeatingCapacity)/1.15);
        }

        //returns the price for converting a passenger airliner to a cargo airliner
        public static double GetCargoConvertingPrice(AirlinerPassengerType type)
        {
            double basePrice = 650000;

            if (type.Body == AirlinerType.BodyType.SingleAisle)
            {
                basePrice = basePrice*1.2;
            }

            if (type.Body == AirlinerType.BodyType.NarrowBody)
            {
                basePrice = basePrice*2.4;
            }

            if (type.Body == AirlinerType.BodyType.WideBody)
            {
                basePrice = basePrice*3.6;
            }

            double paxRate = type.MaxSeatingCapacity*800;

            return GeneralHelpers.GetInflationPrice(basePrice + paxRate);
        }

        public static DateTime GetOrderDeliveryDate(List<AirlinerOrder> orders)
        {
            double monthsToComplete = orders.Select(order => Math.Ceiling(Convert.ToDouble(order.Amount)/order.Type.ProductionRate)).Concat(new double[] {0}).Max();

            var latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerOrder order in orders)
            {
                var date = new DateTime(
                    GameObject.GetInstance().GameTime.Year,
                    GameObject.GetInstance().GameTime.Month,
                    GameObject.GetInstance().GameTime.Day);
                int rate = order.Type.ProductionRate;
                if (order.Amount <= (rate/4))
                {
                    date = date.AddMonths(3);
                }
                else
                {
                    for (int i = (rate/4) + 1; i <= order.Amount; i++)
                    {
                        date = date.AddDays(365.0/rate);
                    }
                }

                if (date > latestDate)
                {
                    latestDate = date;
                }
            }

            return latestDate;
        }

        public static double GetPassengerCargoSize(AirlinerPassengerType type)
        {
            return Convert.ToDouble(type.MaxSeatingCapacity)*1.25;
        }

        public static FleetAirliner GetRandomAirliner(Airline airline)
        {
            return airline.Fleet[Rnd.Next(airline.Fleet.Count)];
        }

        #endregion

        #region Methods

        private static Airliner CreateLeasingAirliner()
        {
            int years = Rnd.Next(0, 4);

            Airliner airliner = CreateAirlinerFromYear(GameObject.GetInstance().GameTime.AddYears(-years).Year);

            airliner.Status = Airliner.StatusTypes.Leasing;

            return airliner;
        }

        private static Airliner CreateAirliner(double minRange)
        {
            Guid id = Guid.NewGuid();

            List<AirlinerType> types =
                AirlinerTypes.GetTypes(
                    t => t.Range >= minRange && t.Produced.From.Year < GameObject.GetInstance().GameTime.AddYears(-5).Year
                         && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-35));

            int typeNumber = Rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = Rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = Rnd.Next(
                Math.Max(type.Produced.From.Year, GameObject.GetInstance().GameTime.Year - 35),
                Math.Min(GameObject.GetInstance().GameTime.Year - 5, type.Produced.To.Year));

            var airliner = new Airliner(
                id.ToString(),
                type,
                country.TailNumbers.GetNextTailNumber(),
                new DateTime(builtYear, 1, 1));

            if (airliner.TailNumber.Length < 2)
            {
                typeNumber = 0;
            }

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = Rnd.Next(100000, 1000000);
            long km = kmPerYear*age;

            airliner.Flown = km;

            CreateAirlinerClasses(airliner);

            List<EngineType> engines = EngineTypes.GetEngineTypes(airliner.Type, GameObject.GetInstance().GameTime.Year);

            if (engines.Count > 0)
            {
                int engineNumber = Rnd.Next(engines.Count);

                airliner.EngineType = engines[engineNumber];
            }

            return airliner;
        }

        #endregion

        /*! create a random airliner with a minimum range.
        */

        //creates the airliner classes for an airliner
    }
}