using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

       //passenger happiness calculation
       public static int getPaxHappiness()
       {
           //create a list of route ticket prices
           List<Double> ePrices = (from r in GeneralModel.GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Economy_Class)).ToList();
           List<Double> bPrices = (from r in GeneralModel.GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.Business_Class)).ToList();
           List<Double> fPrices = (from r in GeneralModel.GameObject.GetInstance().HumanAirline.Routes select r.getFarePrice(AirlinerModel.AirlinerClass.ClassType.First_Class)).ToList();

           //variance of econoomy, business, and first class
           double eVar = getVariance(ePrices);
           double bVar = getVariance(bPrices);
           double fVar = getVariance(fPrices);

           //compiled variance for all classes
           double totalVariance = (eVar * 0.7) + (bVar * 0.2) + (fVar * 0.1);
           int variance = (int)totalVariance;

           return variance;
       }

           
    }
}
