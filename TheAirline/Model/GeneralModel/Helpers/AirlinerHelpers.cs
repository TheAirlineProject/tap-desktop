using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;

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
            List<AirlinerType> types = AirlinerTypes.GetTypes(delegate(AirlinerType t) { return t.Range >= minRange && t.Produced.From.Year < GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30); });

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = rnd.Next(Math.Max(type.Produced.From.Year, GameObject.GetInstance().GameTime.Year - 30), Math.Min(GameObject.GetInstance().GameTime.Year-1, type.Produced.To.Year));

            Airliner airliner = new Airliner(type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(100000, 1000000);
            long km = kmPerYear * age;

            airliner.Flown = km;

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
            List<AirlinerType> types = AirlinerTypes.GetTypes(t=>t.Produced.From.Year < year && t.Produced.To.Year > year);

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = year;

            Airliner airliner = new Airliner(type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(1000, 100000);
            long km = kmPerYear * age;

            airliner.Flown = km;

            return airliner;
        }

    }
}
