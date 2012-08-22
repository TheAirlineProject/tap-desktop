using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for setting the inflation (prices etc.) for a specific year
    public class Inflation
    {
        public int Year { get; set; }
        public double FuelPrice { get; set; }
        public double InflationPercent { get; set; }
        public double Modifier { get; set; }
        public Inflation(int year, double fuelprice, double inflationpercent, double modifier)
        {
            this.Year = year;
            this.FuelPrice = fuelprice;
            this.InflationPercent = inflationpercent;
            this.Modifier = modifier;
        }
    }
    //the list of inflation years
    public class Inflations
    {
        public static int BaseYear = 2011;
        private static List<Inflation> inflations = new List<Inflation>();
        //adds an inflation year to the list
        public static void AddInflationYear(Inflation inflation)
        {
            inflations.Add(inflation);
                
        }
        //returns the inflation for an year
        public static Inflation GetInflation(int year)
        {
            Inflation inflation = inflations.Find(i => i.Year == year);

            return inflation;
        }
        //clears the list of inflations
        public static void Clear()
        {
            inflations.Clear();
        }
    }
}
