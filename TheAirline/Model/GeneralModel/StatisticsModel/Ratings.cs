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
       //standard deviation calculation
       public static double getStandardDev(List<double> population)
       {
           double M = 0.0;
           double S = 0.0;
           int k = 1;
           foreach (double value in population)
           {
               double tmpM = M;
               M += (value - tmpM) / k;
               S += (value - tmpM) * (value - M);
               k++;
           }
           return Math.Sqrt(S / (k - 1));
       }

       //variance calculation
       public static double getVariance(List<double> population)
       {
           double M = 0.0;
           double S = 0.0;
           int k = 1;
           foreach (double value in population)
           {
               double tmpM = M;
               M += (value - tmpM) / k;
               S += (value - tmpM) * (value - M);
               k++;
           }
           return S / (k - 1);
       }
    }
}
