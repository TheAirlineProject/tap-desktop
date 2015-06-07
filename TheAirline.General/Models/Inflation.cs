using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General
{
    //the class for setting the inflation (prices etc.) for a specific year
    [Serializable]
    public class Inflation : BaseModel
    {
        #region Constructors and Destructors

        public Inflation(int year, double fuelprice, double inflationpercent, double modifier)
        {
            Year = year;
            FuelPrice = fuelprice;
            InflationPercent = inflationpercent;
            Modifier = modifier;
        }

        private Inflation(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("fuel")]
        public double FuelPrice { get; set; }

        [Versioning("percent")]
        public double InflationPercent { get; set; }

        [Versioning("modifier")]
        public double Modifier { get; set; }

        [Versioning("year")]
        public int Year { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of inflation years
    public class Inflations
    {
        #region Static Fields

        public static int BaseYear = 1960;

        private static readonly List<Inflation> inflations = new List<Inflation>();

        #endregion

        #region Public Methods and Operators

        public static void AddInflationYear(Inflation inflation)
        {
            inflations.Add(inflation);
        }

        //returns the inflation for an year

        //clears the list of inflations
        public static void Clear()
        {
            inflations.Clear();
        }

        public static Inflation GetInflation(int year)
        {
            Inflation inflation = inflations.Find(i => i.Year == year);

            if (inflation == null)
            {
                var rnd = new Random();

                double rndInflation = (((rnd.NextDouble()*5) - 1)/100.0);
                double inflationPercent = 1 + rndInflation;

                Inflation prevInflation = inflations.Find(i => i.Year == year - 1) ?? inflations.Last();

                var newInflation = new Inflation(
                    year,
                    prevInflation.FuelPrice*inflationPercent,
                    rndInflation,
                    prevInflation.Modifier*inflationPercent);
                AddInflationYear(newInflation);

                return newInflation;
            }
            return inflation;
        }

        #endregion

        //adds an inflation year to the list
    }
}