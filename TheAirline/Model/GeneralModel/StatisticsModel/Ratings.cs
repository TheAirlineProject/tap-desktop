using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Math;

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
       public static double getVariance(List<double> population)
       {
           double var = 0.0;
           if (population.Count() > 0)
           {
               double u = population.Average();
               var = population.Sum(d => Math.Pow(d - u, 2));
           }
           return var;
       }
           
    }
}
