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

namespace TheAirline.Model.StatisticsModel
{
    //the general class for ratings
   public class Ratings
    {
       //standard deviation calculation for a list of values
       public static double getStandardDev(List<double> population)
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
       public static double getVariance(List<Double> population)
       {
           double var = 0.0;
           if (population.Count() > 0)
           {
               double u = population.Average();
               var = population.Sum(d => Math.Pow(d - u, 2));
           }
           return var;
       }
        //calcluates overall ticket price per distance
        public double getTotalTicketPPD()
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


        //calculates average AI ticket price
       public double getAIAvgTicketPPD()
        {
       
            List<Double> AIPrices = new List<Double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            
        {
            List<Double> aiEconPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
            List<Double> aiBusPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
            List<Double> aiFirstPrices = (from r in airline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
            List<Double> distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).ToList();
            double avgEP = aiEconPrices.Average();
            double avgBP = aiBusPrices.Average();
            double avgFP = aiFirstPrices.Average();
            double avgPrice = ((avgEP * 0.7) + (avgBP * 0.2) + (avgFP * 0.1)) / 3;
            double avgDistPrice = avgPrice / distance.Average();
            AIPrices.Add(avgDistPrice);
        }
            return AIPrices.Average();
}
       //calculate average human ticket price per distance
       public static double getHumanAvgTicketPPD()
       {
           List<Double> ePrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
           List<Double> bPrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
           List<Double> fPrices = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();
           List<Double> distance = (from r in GameObject.GetInstance().HumanAirline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).ToList();
           double avgHEP = ePrices.Average();
           double avgHBP = bPrices.Average();
           double avgFBP = fPrices.Average();
           double avgHPrice = ((avgHEP * 0.7) + (avgHBP * 0.2) + (avgFBP * 0.1)) / 3;
           double avgDistPrice = avgHPrice / distance.Average();
           return avgDistPrice;
       }

       //calculates average fill degree for all airlines
       public static double getTotalFillAverage()
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
       public static double getHumanFillAverage()
       {
       List<Double> fillDegree = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFillingDegree()).ToList();
       return fillDegree.Average();
       }

       //calculates AI average fill degree
       public static double getAIFillDegree()
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
       public static double getHumanOnTime()
       {
           List<Double> onTime = new List<Double>();
           double otp = GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
           onTime.Add(otp);
           
           return onTime.Average();
       }

       //calculates AI airline average on-time %
       //needs foreach loop needs changed to AI
       public static double getAIonTime()
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
       public static double getTotalOnTime()
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
       public static double getHumanOnTimeYear(int year)
       {
           double otp = GameObject.GetInstance().HumanAirline.Statistics.getStatisticsValue(year, StatisticsTypes.GetStatisticsType("On-Time%"));
           return otp;
       }

    } 
}
