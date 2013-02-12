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
using TheAirline.Model.AirportModel;


namespace TheAirline.Model.GeneralModel.Helpers
{
    public class StatisticsHelpers
    {
        
            //generate a 1-100 scale for a list of values
            public static IDictionary<Airline, Double> GetRatingScale(IDictionary<Airline, Double> valueDictionary, double lower = 1, double upper = 100)
            {
                foreach (Airline airline in valueDictionary.Keys.ToArray())
                    if (Double.IsNaN(valueDictionary[airline]))
                        valueDictionary[airline] = 0;

                Dictionary<Airline, Double> scaleValues = new Dictionary<Airline, double>();
                Double max = valueDictionary.Values.DefaultIfEmpty(0).Max();
                Double min = valueDictionary.Values.DefaultIfEmpty(0).Min();
                Double avg = valueDictionary.Values.DefaultIfEmpty(0).Sum() / valueDictionary.Values.Count;

                Double gap = max - min;

                if (gap == 0)
                    gap = 1;

                return valueDictionary.ToDictionary(v => v.Key, x => (upper - lower) * ((x.Value - min) / gap) + lower);
            }

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
                    double distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).DefaultIfEmpty(0).Average();
                    double avgEP = aiEconPrices.DefaultIfEmpty(0).Average();
                    double avgBP = aiBusPrices.DefaultIfEmpty(0).Average();
                    double avgFP = aiFirstPrices.DefaultIfEmpty(0).Average();
                    double avgPrice = ((avgEP * 0.7) + (avgBP * 0.2) + (avgFP * 0.1)) / 3;
                    double avgDistPrice = avgPrice / distance;
                    AIPrices.Add(avgDistPrice);
                
                }
                return AIPrices.DefaultIfEmpty(0).Average();
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
                return ((avgHEP * 0.7) + (avgHBP * 0.2) + (avgFBP * 0.1)) / distance;
            }

            //returns dictionary of average fill degrees for all airlines
            //FINISHED
            public static Dictionary<Airline, Double> GetFillAverages()
            {
                Dictionary<Airline, Double> fillAverage = new Dictionary<Airline, Double>();
                foreach (Airline airline in Airlines.GetAllAirlines())
                {                
                    List<Double> fillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                    double uFillDegree = fillDegree.DefaultIfEmpty(0).Average();
                    fillAverage.Add(airline, uFillDegree);
                }
                return fillAverage;
            }

            //returns the average fill degree of all airlines
            //FINISHED
            public static Double GetFillAverage()
            {
                List<Double> fillDegrees = new List<Double>();
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    List<Double> fillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                    fillDegrees.Add(fillDegree.DefaultIfEmpty(0).Average());
                }
                return fillDegrees.DefaultIfEmpty(0).Average();
            }
                

            //calculates average human fill degree
            public static double GetHumanFillAverage()
            {
                List<Double> fillDegree = (from r in GameObject.GetInstance().HumanAirline.Routes select r.getFillingDegree()).ToList();

                return fillDegree.DefaultIfEmpty(0).Average();
            }

            //returns dictionary of AI average fill degrees
            public static Dictionary<Airline,Double> GetAIFillDegree()
            {
                Dictionary<Airline, Double> AIafd = new Dictionary<Airline, double>();
                foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
                {
                    List<Double> AIFillDegree = (from r in airline.Routes select r.getFillingDegree()).ToList();
                    AIafd.Add(airline, AIFillDegree.DefaultIfEmpty(0).Average());                

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

            //returns dictionary of all airline average on-time %
            public static Dictionary<Airline, Double> GetTotalOnTime()
            {
                Dictionary<Airline, Double> totalOnTime = new Dictionary<Airline, Double>();
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

        /*-------------------------------------------------------------------------------------------------------------------------------------------
         * -------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------END OF CUSTOMER HAPPINESS METHODS-------------------------------------------------------------*/
    
        //creates dictionary of average employee wages
            public static Dictionary<Airline, Double> GetEmployeeWages()
            {
                Dictionary<Airline,Double>employeeWages = new Dictionary<Airline, Double>();
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    double sWage = airline.Fees.getValue(FeeTypes.GetType("Support Wage"));
                    double mWage = airline.Fees.getValue(FeeTypes.GetType("Maintenance Wage"));
                    double pWage = airline.Fees.getValue(FeeTypes.GetType("Pilot Base Salary"));// *airline.Pilots.Count();
                    double cWage = airline.Fees.getValue(FeeTypes.GetType("Cabin Wage"));
                    double iWage = airline.Fees.getValue(FeeTypes.GetType("Instructor Base Salary"));// *PilotModel.FlightSchool.MaxNumberOfInstructors;
                    int cabinCrew = airline.Routes.Sum(r => r.getTotalCabinCrew());
                    int serviceCrew = airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportModel.AirportFacility.EmployeeTypes.Support).Sum(a => a.NumberOfEmployees);
                    int maintenanceCrew = airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportModel.AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees);
                    double averageWage = ((sWage * serviceCrew) + (mWage * maintenanceCrew) + pWage + iWage + (cWage * cabinCrew)) / 5;

                    averageWage = (sWage + mWage + pWage + cWage + iWage) / 5;
                    
                    employeeWages.Add(airline, averageWage);
                }
                return employeeWages;
            }

        //creates a dictionary of average employee discounts
            public static Dictionary<Airline, Double> GetEmployeeDiscounts()
            {
                Dictionary<Airline, Double> employeeDiscounts = new Dictionary<Airline,double>();
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    double eDiscount = airline.Fees.getValue(FeeTypes.GetType("Employee Discount"));
                    employeeDiscounts.Add(airline, eDiscount);
                }
                return employeeDiscounts;
            }

        //returns a dictionary of unassigned pilots
            public static Dictionary<Airline, Double> GetUnassignedPilots()
            {
                Dictionary<Airline, Double> unassignedPilots = new Dictionary<Airline, Double>();
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    double unPilots = airline.Pilots.Count(p => p.Airliner == null);
                    unassignedPilots.Add(airline, unPilots);
                }
                return unassignedPilots;
            }

        /*-------------------------------------------------------------------------------------------------------------------------------------------
 * -------------------------------------------------------------------------------------------------------------------------------------------
 * ------------------------------------------END OF EMPLOYEE HAPPINESS METHODS-------------------------------------------------------------*/
       //calculates the human airports served worldwide
            public static double getWorldAirportsServed()
            {
                int hAirports = GameObject.GetInstance().HumanAirline.Airports.Count();
                return hAirports / AirportModel.Airports.GetAllAirports().Count();                
            }

        //calculates the human airports served for a given country
            public static Dictionary<Country,double> getCountryAirportsServed()
                
            {
                Dictionary<Country,double> countryAirports = new Dictionary<Country,double>();
                foreach (Country country in GameObject.GetInstance().HumanAirline.Airports.Select(a => a.Profile.Country).Distinct())
                {
                    double c = Airports.GetAirports(country).Count();
                    double cHuman = GameObject.GetInstance().HumanAirline.Airports.Count(a => a.Profile.Country == country);
                    countryAirports.Add(country, (c / cHuman));
                }
                return countryAirports;
            }

        //calculates human daily flights worldwide
        /*    public static double getHumanDailyFlights()
            {
                Dictionary<Airline,double> uFlights = new Dictionary<Airline,double>();
                foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports)
                {
                    double flights = AirportHelpers.GetAirportRoutes(airport).Sum(r => r.TimeTable.Entries.Count) / 7;
                    double hFlights = AirportHelpers.GetAirportRoutes(airport, GameObject.GetInstance().HumanAirline).Sum(r=>r.TimeTable.Entries.Count) / 7;
                    uFlights.Add(Airline,(hFlights/flights);
                }

                return hFlights / 
            }*/

    }
}
