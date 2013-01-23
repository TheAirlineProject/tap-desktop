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
                double u = population.Average();
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
                double u = population.Average();
                var = population.Sum(d => Math.Pow(d - u, 2));
            }
            return var;
        }  
        
        //generate a 1-100 scale for a list of values
        public static IDictionary<Double, Double> GetRatingScale(List<Double> values, double lower = 1, double upper = 100)
        {
            Double max = values.Max();
            Double min = values.Min();
            Double avg = values.Sum() / values.Count;
            return values.ToDictionary(x => x, x => (upper - lower) * ((x - min) / (max - min)) + lower);
        }

        //calcluates overall ticket price per distance
        public static double GetTotalTicketPPD()
        {
            List<Double> ticketPPD = new List<Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Double> econPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
                List<Double> busPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
                List<Double> firstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
                List<Double> distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).ToList();
                double avgDistPrice = ((econPrices.Average() * 0.7) + (busPrices.Average() * 0.2) + (firstPrices.Average() * 0.1)) / distance.Average();
                ticketPPD.Add(avgDistPrice);
            }
            return ticketPPD.Average();
        }

        //calculates maximum difference
        public static List<Double> GetPPDdifference()
        {
            List<Double> ppdDifference = new List<Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double avgPPD = GetTotalTicketPPD();
                List<Double> aiEconPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
                List<Double> aiBusPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
                List<Double> aiFirstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
                double distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).Average();
                double avgEP = aiEconPrices.Average(); double avgBP = aiBusPrices.Average(); double avgFP = aiFirstPrices.Average();
                double avgDistPrice = (((avgEP * 0.7) + (avgBP * 0.2) + (avgFP * 0.1)) / 3) / distance;
                double avgDiff = avgDistPrice - avgPPD;
                ppdDifference.Add(avgDiff);
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
                double avgEP = aiEconPrices.Average();
                double avgBP = aiBusPrices.Average();
                double avgFP = aiFirstPrices.Average();
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
            double distance = (from r in GameObject.GetInstance().HumanAirline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).Average();
            double avgHEP = ePrices.Average();
            double avgHBP = bPrices.Average();
            double avgFBP = fPrices.Average();
            double avgHPrice = ((avgHEP * 0.7) + (avgHBP * 0.2) + (avgFBP * 0.1)) / 3;
            return avgHPrice / distance;
        }

        //calculates average fill degree for all airlines
        public static double GetTotalFillAverage()
        {
            List<Double> totalFillAverage = new List<Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Double> fillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                totalFillAverage.Add(fillDegree.Average());
            }
            return totalFillAverage.Average();
        }

        //calculates average human fill degree
        public static double GetHumanFillAverage()
        {
            List<Double> fillDegree = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFillingDegree()).ToList();
            return fillDegree.Average();
        }

        //calculates AI average fill degree
        public static double GetAIFillDegree()
        {
            List<Double> AIafd = new List<Double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                List<Double> AIFillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                AIafd.Add(AIFillDegree.Average());

            }
            return AIafd.Average();
        }

        //calculates human airline average on-time %
        public static double GetHumanOnTime()
        {
            List<Double> onTime = new List<Double>();
            double otp = GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
            onTime.Add(otp);

            return onTime.Average();
        }

        //calculates AI airline average on-time %
        //needs foreach loop needs changed to AI
        public static double GetAIonTime()
        {
            List<Double> onTime = new List<Double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                double otp = airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                onTime.Add(otp);
            }
            return onTime.Average();
        }

        //calculates all airline average on-time %
        public static double GetTotalOnTime()
        {
            List<Double> onTime = new List<Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double otp = airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                onTime.Add(otp);
            }
            return onTime.Average();
        }

        //calculates human airline average on-time % for given year
        public static double GetHumanOnTimeYear(int year)
        {
            return GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(year, StatisticsTypes.GetStatisticsType("On-Time%"));
        }

    }
}
