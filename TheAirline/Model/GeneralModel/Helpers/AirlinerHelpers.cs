namespace TheAirline.Model.GeneralModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;

    //the class for some general airline helpers
    public class AirlinerHelpers
    {
        #region Static Fields

        private static readonly Random rnd = new Random();

        #endregion

        /*! create a random airliner with a minimum range.
        */

        //creates the airliner classes for an airliner

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
            airliner.clearAirlinerClasses();

            List<AirlinerClass> classes = GetAirlinerClasses(airliner.Type);

            foreach (AirlinerClass aClass in classes)
            {
                airliner.addAirlinerClass(aClass);
            }
        }

        public static Airliner CreateAirlinerFromYear(int year)
        {
            Guid id = Guid.NewGuid();

            List<AirlinerType> types =
                AirlinerTypes.GetTypes(t => t.Produced.From.Year < year && t.Produced.To.Year > year);

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = year;

            var airliner = new Airliner(
                id.ToString(),
                type,
                country.TailNumbers.getNextTailNumber(),
                new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(1000, 100000);
            long km = kmPerYear * age;

            airliner.Flown = km;

            var engines = EngineTypes.GetEngineTypes(airliner.Type, GameObject.GetInstance().GameTime.Year);

            if (engines.Count > 0)
            {
                int engineNumber = rnd.Next(engines.Count);

                airliner.EngineType = engines[engineNumber];
            }

      
            CreateAirlinerClasses(airliner);

            return airliner;
        }

        public static void CreateStartUpAirliners()
        {
            int number =
                AirlinerTypes.GetTypes(
                    delegate(AirlinerType t)
                    {
                        return t.Produced.From <= GameObject.GetInstance().GameTime
                               && t.Produced.To.AddYears(-10) >= GameObject.GetInstance().GameTime.AddYears(-30);
                    })
                    .Count * rnd.Next(1, 3);

            int airlines = Airlines.GetNumberOfAirlines();

            number = (airlines * number) / 5;
            for (int i = 0; i < number; i++)
            {
                Airliners.AddAirliner(CreateAirliner(0));
            }
        }

        //returns the delivery date for a order of airliners

        //returns the code for an airliner class
        public static string GetAirlinerClassCode(AirlinerClass aClass)
        {
            string symbol = "Y";

            if (aClass.Type == AirlinerClass.ClassType.Business_Class)
            {
                symbol = "C";
            }

            if (aClass.Type == AirlinerClass.ClassType.First_Class)
            {
                symbol = "F";
            }

            if (aClass.Type == AirlinerClass.ClassType.Economy_Class)
            {
                symbol = "Y";
            }

            return string.Format("{0}{1}", aClass.SeatingCapacity, symbol);
        }

        public static List<AirlinerClass> GetAirlinerClasses(AirlinerType type)
        {
            var classes = new List<AirlinerClass>();

            if (type is AirlinerPassengerType)
            {
                Configuration airlinerTypeConfiguration =
                    Configurations.GetConfigurations(Configuration.ConfigurationType.AirlinerType)
                        .Find(
                            c =>
                                ((AirlinerTypeConfiguration)c).Airliner == type
                                && ((AirlinerTypeConfiguration)c).Period.From <= GameObject.GetInstance().GameTime
                                && ((AirlinerTypeConfiguration)c).Period.To > GameObject.GetInstance().GameTime);

                if (airlinerTypeConfiguration == null)
                {
                    AirlinerConfiguration configuration = null;

                    int numOfClasses = rnd.Next(0, ((AirlinerPassengerType)type).MaxAirlinerClasses) + 1;

                    if (GameObject.GetInstance().GameTime.Year >= (int)AirlinerClass.ClassType.Business_Class)
                    {
                        if (numOfClasses == 1)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("200");
                        }
                        if (numOfClasses == 2)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("202");
                        }
                        if (numOfClasses == 3)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("203");
                        }
                    }
                    else
                    {
                        if (numOfClasses == 1)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("200");
                        }
                        if (numOfClasses == 2)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("201");
                        }
                        if (numOfClasses == 3)
                        {
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("201");
                        }
                    }

                    foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                    {
                        var airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        airlinerClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                        {
                            airlinerClass.setFacility(null, facility);
                        }

                        foreach (
                            AirlinerFacility.FacilityType fType in Enum.GetValues(typeof(AirlinerFacility.FacilityType))
                            )
                        {
                            if (!aClass.Facilities.Exists(f => f.Type == fType))
                            {
                                airlinerClass.setFacility(null, AirlinerFacilities.GetBasicFacility(fType));
                            }
                        }

                        airlinerClass.SeatingCapacity =
                            Convert.ToInt16(
                                Convert.ToDouble(airlinerClass.RegularSeatingCapacity)
                                / airlinerClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses);

                        classes.Add(airlinerClass);
                    }

                    int seatingDiff = ((AirlinerPassengerType)type).MaxSeatingCapacity - configuration.MinimumSeats;

                    AirlinerClass economyClass = classes.Find(c => c.Type == AirlinerClass.ClassType.Economy_Class);
                    economyClass.RegularSeatingCapacity += seatingDiff;

                    AirlinerFacility seatingFacility = economyClass.getFacility(AirlinerFacility.FacilityType.Seat);

                    var extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                    economyClass.SeatingCapacity += extraSeats;
                }
                else
                {
                    foreach (
                        AirlinerClassConfiguration aClass in
                            ((AirlinerTypeConfiguration)airlinerTypeConfiguration).Classes)
                    {
                        var airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        airlinerClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                        {
                            airlinerClass.setFacility(null, facility);
                        }

                        airlinerClass.SeatingCapacity =
                            Convert.ToInt16(
                                Convert.ToDouble(airlinerClass.RegularSeatingCapacity)
                                / airlinerClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses);

                        classes.Add(airlinerClass);
                    }
                }
            }
            else if (type is AirlinerCargoType)
            {
                var cargoClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 0);
                classes.Add(cargoClass);
            }
            else if (type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
            {
                var helicopterClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 0);
                classes.Add(helicopterClass);
            }
            return classes;
        }

        //returns the cargo size for a passenger airliner if needed to converted

        //return the days for converting a passenger airliner to a cargo airliner
        public static int GetCargoConvertingDays(AirlinerPassengerType type)
        {
            return (int)(Convert.ToDouble(type.MaxSeatingCapacity) / 1.15);
        }

        //returns the price for converting a passenger airliner to a cargo airliner
        public static double GetCargoConvertingPrice(AirlinerPassengerType type)
        {
            double basePrice = 650000;

            if (type.Body == AirlinerType.BodyType.Single_Aisle)
            {
                basePrice = basePrice * 1.2;
            }

            if (type.Body == AirlinerType.BodyType.Narrow_Body)
            {
                basePrice = basePrice * 2.4;
            }

            if (type.Body == AirlinerType.BodyType.Wide_Body)
            {
                basePrice = basePrice * 3.6;
            }

            double paxRate = type.MaxSeatingCapacity * 800;

            return GeneralHelpers.GetInflationPrice(basePrice + paxRate);
        }

        public static DateTime GetOrderDeliveryDate(List<AirlinerOrder> orders)
        {
            double monthsToComplete = 0;

            foreach (AirlinerOrder order in orders)
            {
                double orderToComplete = Math.Ceiling(Convert.ToDouble(order.Amount) / order.Type.ProductionRate);

                if (orderToComplete > monthsToComplete)
                {
                    monthsToComplete = orderToComplete;
                }
            }

            var latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerOrder order in orders)
            {
                var date = new DateTime(
                    GameObject.GetInstance().GameTime.Year,
                    GameObject.GetInstance().GameTime.Month,
                    GameObject.GetInstance().GameTime.Day);
                int rate = order.Type.ProductionRate;
                if (order.Amount <= (rate / 4))
                {
                    date = date.AddMonths(3);
                }
                else
                {
                    for (int i = (rate / 4) + 1; i <= order.Amount; i++)
                    {
                        double iRate = 365 / rate;
                        date = date.AddDays(Math.Round(iRate, 0, MidpointRounding.AwayFromZero));
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
            return Convert.ToDouble(type.MaxSeatingCapacity) * 1.25;
        }

        public static FleetAirliner GetRandomAirliner(Airline airline)
        {
            return airline.Fleet[rnd.Next(airline.Fleet.Count)];
        }

        #endregion

        #region Methods

        private static Airliner CreateAirliner(double minRange)
        {
            Guid id = Guid.NewGuid();
            
            List<AirlinerType> types =
                AirlinerTypes.GetTypes(
                    delegate(AirlinerType t)
                    {
                        return t.Range >= minRange && t.Produced.From.Year < GameObject.GetInstance().GameTime.Year
                               && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30);
                    });

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = rnd.Next(
                Math.Max(type.Produced.From.Year, GameObject.GetInstance().GameTime.Year - 30),
                Math.Min(GameObject.GetInstance().GameTime.Year - 1, type.Produced.To.Year));

            var airliner = new Airliner(
                id.ToString(),
                type,
                country.TailNumbers.getNextTailNumber(),
                new DateTime(builtYear, 1, 1));

            if (airliner.TailNumber.Length < 2)
            {
                typeNumber = 0;
            }

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(100000, 1000000);
            long km = kmPerYear * age;

            airliner.Flown = km;

            CreateAirlinerClasses(airliner);

            var engines = EngineTypes.GetEngineTypes(airliner.Type, GameObject.GetInstance().GameTime.Year);

            if (engines.Count > 0)
            {
                int engineNumber = rnd.Next(engines.Count);

                airliner.EngineType = engines[engineNumber];
            }

            return airliner;
        }

        #endregion
    }
}