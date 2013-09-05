using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlinerHelpers
    {
        private static Random rnd = new Random();
       
        /*! create a random airliner with a minimum range.
        */
        private static Airliner CreateAirliner(double minRange)
        {
            Guid id = Guid.NewGuid();

            List<AirlinerType> types = AirlinerTypes.GetTypes(delegate(AirlinerType t) { return t.Range >= minRange && t.Produced.From.Year < GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30); });

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = rnd.Next(Math.Max(type.Produced.From.Year, GameObject.GetInstance().GameTime.Year - 30), Math.Min(GameObject.GetInstance().GameTime.Year-1, type.Produced.To.Year));

            Airliner airliner = new Airliner(id.ToString(), type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

            if (airliner.TailNumber.Length < 2)
                typeNumber = 0;

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(100000, 1000000);
            long km = kmPerYear * age;

            airliner.Flown = km;
            
            CreateAirlinerClasses(airliner);

            return airliner;
        }

        /*! create some game airliners.
         */
        public static void CreateStartUpAirliners()
        {
            int number = AirlinerTypes.GetTypes(delegate(AirlinerType t) { return t.Produced.From <= GameObject.GetInstance().GameTime && t.Produced.To >= GameObject.GetInstance().GameTime.AddYears(-30); }).Count * 25;
            for (int i = 0; i < number; i++)
            {
                Airliners.AddAirliner(CreateAirliner(0));
            }

       
        }
        /*!creates an airliner from a specific year
         */
        public static Airliner CreateAirlinerFromYear(int year)
        {
            Guid id = Guid.NewGuid();

            List<AirlinerType> types = AirlinerTypes.GetTypes(t=>t.Produced.From.Year < year && t.Produced.To.Year > year);

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = year;

            Airliner airliner = new Airliner(id.ToString(), type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(1000, 100000);
            long km = kmPerYear * age;

            airliner.Flown = km;

            CreateAirlinerClasses(airliner);

            return airliner;
        }
        //converts the passenger capacity for an airliner type to cargo capacity
        public static double ConvertPassengersToCargoSize(AirlinerPassengerType type)
        {
            return type.MaxSeatingCapacity * 2;
        }
        //creates the airliner classes for an airliner
        public static void CreateAirlinerClasses(Airliner airliner)
        {
            
            if (airliner.Type is AirlinerPassengerType)
            {
                Configuration airlinerTypeConfiguration = Configurations.GetConfigurations(Configuration.ConfigurationType.AirlinerType).Find(c=>((AirlinerTypeConfiguration)c).Airliner == airliner.Type && ((AirlinerTypeConfiguration)c).Period.From <= airliner.BuiltDate && ((AirlinerTypeConfiguration)c).Period.To> airliner.BuiltDate) ;

                if (airlinerTypeConfiguration == null)
                {
                    airliner.clearAirlinerClasses();

                    AirlinerConfiguration configuration = null;

                    int classes = rnd.Next(0, ((AirlinerPassengerType)airliner.Type).MaxAirlinerClasses) +1;

                    if (GameObject.GetInstance().GameTime.Year >= (int)AirlinerClass.ClassType.Business_Class)
                    {
                        if (classes == 1)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("200");
                        if (classes == 2)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("202");
                        if (classes == 3)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("203");
                    }
                    else
                    {
                        if (classes == 1)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("200");
                        if (classes == 2)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("201");
                        if (classes == 3)
                            configuration = (AirlinerConfiguration)Configurations.GetStandardConfiguration("201");
           
                    }

                    foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                    {
                        AirlinerClass airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        airlinerClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            airlinerClass.setFacility(airliner.Airline, facility);

                        airlinerClass.SeatingCapacity = Convert.ToInt16(Convert.ToDouble(airlinerClass.RegularSeatingCapacity) / airlinerClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses); 

                         airliner.addAirlinerClass(airlinerClass);
                    }
                    
                    int seatingDiff = ((AirlinerPassengerType)airliner.Type).MaxSeatingCapacity - configuration.MinimumSeats;

                    airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).RegularSeatingCapacity += seatingDiff;

                    AirlinerFacility seatingFacility = airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Seat);

                    int extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                    airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).SeatingCapacity += extraSeats;
                }
                else
                {
                    airliner.clearAirlinerClasses();

                    foreach (AirlinerClassConfiguration aClass in ((AirlinerTypeConfiguration)airlinerTypeConfiguration).Classes)
                    {
                        AirlinerClass airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        airlinerClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            airlinerClass.setFacility(airliner.Airline, facility);

                        airlinerClass.SeatingCapacity = Convert.ToInt16(Convert.ToDouble(airlinerClass.RegularSeatingCapacity) / airlinerClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses); 

                        airliner.addAirlinerClass(airlinerClass);
                    }
                    
                }
            }
            else if (airliner.Type is AirlinerCargoType)
            {
                airliner.clearAirlinerClasses();

                AirlinerClass cargoClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 0);
                airliner.addAirlinerClass(cargoClass);
            }
          
        }

        public static FleetAirliner GetRandomAirliner(Airline airline)
        {
            return airline.Fleet[rnd.Next(airline.Fleet.Count)];
         
        }
       
    }
}
