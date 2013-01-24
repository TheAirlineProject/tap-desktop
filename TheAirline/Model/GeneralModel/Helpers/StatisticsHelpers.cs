using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.Helpers;
using MathNet.Numerics.Statistics;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class StatisticsHelpers
    {
        //standard deviation calculation for a list of values
        public static double GetStandardDev(List<double> population)
        {
            double sd = 0.0;
            if (population.Count() > 0)
            {
                double u = population.DefaultIfEmpty(0).Average();
                double sum = population.Sum(d => Math.Pow(d - u, 2));
                sd = Math.Sqrt((sum) / population.Count() - 1);
            }
            return sd;
        }

        //variance calculation for a list of values
        public static double GetVariance(List<Double> population)
        {
            double var = 0.0;
            if (population.Count() > 0)
            {
                double u = population.DefaultIfEmpty(0).Average();
                var = population.Sum(d => Math.Pow(d - u, 2));
            }
            return var;
        }  
        
        //generate a 1-100 scale for a list of values
        public static Dictionary<String, Double> GetRatingScale(IDictionary<Airline, Double> valueDictionary, double lower = 1, double upper = 100)
        {
            Dictionary<Airline, Double> scaleValues = new Dictionary<Airline, double>();
            Double max = valueDictionary.Values.DefaultIfEmpty(0).Max();
            Double min = valueDictionary.Values.DefaultIfEmpty(0).Min();
            Double avg = valueDictionary.Values.DefaultIfEmpty(0).Sum() / valueDictionary.Values.Count;
            string airline = valueDictionary.Keys.ToString();

            return valueDictionary.Values.ToDictionary(v => airline, x => (upper - lower) * ((x - min) / (max - min)) + lower);
        }
       /* public static List<Double> GetRatingScale(List<Double> values, Airline airline, double lower = 1, double upper = 100)
        {
            Double max = values.DefaultIfEmpty(0).Max();
            Double min = values.DefaultIfEmpty(0).Min();
            Double avg = values.DefaultIfEmpty(0).Sum() / values.Count;
            values.RemoveAll(v => Double.IsNaN(v));

            return new List<double>();// values.ToDictionary(x => (upper - lower) * ((x - min) / (max - min)) + lower);
        } */

        //returns dictionary of overall ticket price per distance
        public static Dictionary<Airline,Double> GetTotalPPD()
        {
            List<Double> ticketPPD = new List<Double>();
            Dictionary<Airline, Double> uDistPrice = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Double> econPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
                List<Double> busPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
                List<Double> firstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
                List<Double> distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).ToList();
                double avgDistPrice = ((econPrices.DefaultIfEmpty(0).Average() * 0.7) + (busPrices.DefaultIfEmpty(0).Average() * 0.2) + (firstPrices.DefaultIfEmpty(0).Average() * 0.1)) / distance.DefaultIfEmpty(0).Average();
                uDistPrice.Add(airline, avgDistPrice);
            }
            return uDistPrice;
        }

        //calculates maximum difference
        public static Dictionary<Airline,Double> GetPPDdifference()
        {
            Dictionary<Airline, Double> ppdDifference = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double avgPPD = 0;  // GetTotalTicketPPD();
                List<Double> aiEconPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
                List<Double> aiBusPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
                List<Double> aiFirstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
                double distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).DefaultIfEmpty(0).Average();
                double avgEP = aiEconPrices.DefaultIfEmpty(0).Average(); double avgBP = aiBusPrices.DefaultIfEmpty(0).Average(); double avgFP = aiFirstPrices.DefaultIfEmpty(0).Average();
                double avgDistPrice = ((avgEP * 0.7) + (avgBP * 0.2) + (avgFP * 0.1)) / distance;
                double avgDiff = avgDistPrice - avgPPD;
                ppdDifference.Add(airline, avgDiff);
            }
            return ppdDifference;
        }


        //calculates average AI ticket price
        public static double GetAIAvgTicketPPD()
        {

            List<Double> AIPrices = new List<Double>();
            List<Double> priceDiff = new List<Double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                List<Double> aiEconPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
                List<Double> aiBusPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
                List<Double> aiFirstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
                double distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).Average();
                double avgEP = aiEconPrices.DefaultIfEmpty(0).Average();
                double avgBP = aiBusPrices.DefaultIfEmpty(0).Average();
                double avgFP = aiFirstPrices.DefaultIfEmpty(0).Average();
                double avgPrice = ((avgEP * 0.7) + (avgBP * 0.2) + (avgFP * 0.1)) / 3;
                double avgDistPrice = avgPrice / distance;
                AIPrices.Add(avgDistPrice);
                
            }
            return AIPrices.Average();
        }
        //calculate average human ticket price per distance
        public static double GetHumanAvgTicketPPD()
        {
            List<Double> ePrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
            List<Double> bPrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
            List<Double> fPrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
            double distance = (from r in GameObject.GetInstance().HumanAirline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).DefaultIfEmpty(0).Average();
            double avgHEP = ePrices.DefaultIfEmpty(0).Average();
            double avgHBP = bPrices.DefaultIfEmpty(0).Average();
            double avgFBP = fPrices.DefaultIfEmpty(0).Average();
            double avgHPrice = ((avgHEP * 0.7) + (avgHBP * 0.2) + (avgFBP * 0.1)) / 3;
            return (((avgHEP * 0.7) + (avgHBP * 0.2) + (avgFBP * 0.1)) / 3) / distance;
        }

        //calculates average fill degree for all airlines
        public static Dictionary<Airline, Double> GetFillAverage()
        {
            Dictionary<Airline, Double> fillAverage = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {                List<Double> fillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                double uFillDegree = fillDegree.Average();
                fillAverage.Add(airline, uFillDegree);
            }
            return fillAverage;
        }

        //calculates average human fill degree
        public static double GetHumanFillAverage()
        {
            List<Double> fillDegree = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFillingDegree()).ToList();

            return fillDegree.DefaultIfEmpty(0).Average();
        }

        //calculates AI average fill degree
        public static Dictionary<Airline,Double> GetAIFillDegree()
        {
            Dictionary<Airline, Double> AIafd = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                List<Double> AIFillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                AIafd.Add(airline,AIFillDegree.Average());                

            }
            return AIafd;
        }

        //calculates human airline average on-time %
        public static double GetHumanOnTime()
        {
            List<Double> onTime = new List<Double>();
            double otp = GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
            onTime.Add(otp);

            return onTime.DefaultIfEmpty(0).Average();
        }

        //calculates AI airline average on-time %
        //needs foreach loop needs changed to AI
        public static Dictionary<Airline, Double> GetAIonTime()
        {
            Dictionary<Airline, Double> aiOnTime = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                double otp = airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                aiOnTime.Add(airline, otp);
            }
            return aiOnTime;
        }

        //calculates all airline average on-time %
        public static Dictionary<Airline, Double> GetTotalOnTime()
        {
            Dictionary<Airline, Double> totalOnTime = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double otp = airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                totalOnTime.Add(airline, otp);
            }
            return totalOnTime;
        }

        //calculates human airline average on-time % for given year
        public static double GetHumanOnTimeYear(int year)
        {
            return GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(year, StatisticsTypes.GetStatisticsType("On-Time%"));
        }

    }
}
