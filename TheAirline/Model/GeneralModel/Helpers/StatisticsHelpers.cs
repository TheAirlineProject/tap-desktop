using System;
using System.Collections.Generic;
using System.Linq;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public static class StatisticsHelpers
    {
        #region Public Methods and Operators

        public static List<int> GaussianList(int vals, int avg, double stdv = 1)
        {
            var dv = new List<int>();
            for (int i = 0; i <= vals; i++)
            {
                dv.Add((int) NextGaussian(new Random(), avg, stdv));
            }

            return dv;
        }

        public static List<double> GaussianList(int vals, double avg, double stdv = 1)
        {
            var dv = new List<double>();
            for (int i = 0; i <= vals; i++)
            {
                dv.Add(NextGaussian(new Random(), avg, stdv));
            }

            return dv;
        }

        //calculates maximum difference

        //calculates average AI ticket price
        public static double GetAIAvgTicketPPD()
        {
            var priceDiff = new List<Double>();
            var aiPrices = (from airline in Airlines.GetAirlines(a => !a.IsHuman)
                            let aiEconPrices = (from r in airline.Routes
                                                where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                                select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.EconomyClass)).ToList()
                            let aiBusPrices = (from r in airline.Routes
                                               where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                               select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.BusinessClass)).ToList()
                            let aiFirstPrices = (from r in airline.Routes
                                                 where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                                 select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.FirstClass)).ToList()
                            let distance = (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).DefaultIfEmpty(0).Average()
                            let avgEP = aiEconPrices.DefaultIfEmpty(0).Average()
                            let avgBP = aiBusPrices.DefaultIfEmpty(0).Average()
                            let avgFP = aiFirstPrices.DefaultIfEmpty(0).Average()
                            let avgPrice = ((avgEP*0.7) + (avgBP*0.2) + (avgFP*0.1))/3
                            select avgPrice/distance).ToList();
            return aiPrices.DefaultIfEmpty(0).Average();
        }

        //calculate average human ticket price per distance

        //returns dictionary of AI average fill degrees
        public static Dictionary<Airline, Double> GetAIFillDegree()
        {
            var aIafd = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                List<Double> aiFillDegree = (from r in airline.Routes select r.GetFillingDegree()).ToList();
                aIafd.Add(airline, aiFillDegree.DefaultIfEmpty(0).Average());
            }
            return aIafd;
        }

        //calculates human airline average on-time %

        //calculates AI airline average on-time %
        public static Dictionary<Airline, Double> GetAIonTime()
        {
            var aiOnTime = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAirlines(a => !a.IsHuman))
            {
                double otp = airline.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                aiOnTime.Add(airline, otp);
            }
            return aiOnTime;
        }

        public static double GetAirlineFillAverage(Airline airline)
        {
            List<Double> fillDegree = (from r in airline.Routes select r.GetFillingDegree()).ToList();

            double avg = fillDegree.DefaultIfEmpty(0).Average();
            return fillDegree.DefaultIfEmpty(0).Average();
        }

        //returns dictionary of all airline average on-time %

        //returns the average employee wages
        public static double GetAverageEmployeeWages()
        {
            int numberOfAirlines = Airlines.GetNumberOfAirlines();
            var airlineWages = new Dictionary<Airline, double>(GetEmployeeWages());

            double totalWages = Airlines.GetAllAirlines().Sum(airline => GetEmployeeWages()[airline]);

            return totalWages/numberOfAirlines;
        }

        //returns the average employee wages for the competitors for an airlines
        public static double GetAverageEmployeeWages(Airline airline)
        {
            int numberOfAirlines = Airlines.GetNumberOfAirlines() - 1;
            var airlineWages = new Dictionary<Airline, double>(GetEmployeeWages());

            double totalWages = Airlines.GetAllAirlines().Where(competitor => competitor != airline).Sum(competitor => GetEmployeeWages()[competitor]);

            return totalWages/numberOfAirlines;
        }

        public static int GetBestYear(Airline airline)
        {
            int high = 0;
            int year = 0;
            {
                int years = GameObject.GetInstance().GameTime.Year - GameObject.StartYear;
                for (int z = GameObject.StartYear; z < (z + years); z++)
                {
                    int sc = GetScore(airline, z);
                    if (sc > high)
                    {
                        high = sc;
                        year = z;
                    }
                }
            }

            return year;
        }

        //creates a dictionary of average employee discounts
        public static Dictionary<Airline, Double> GetEmployeeDiscounts()
        {
            var employeeDiscounts = new Dictionary<Airline, double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double eDiscount = airline.Fees.GetValue(FeeTypes.GetType("Employee Discount"));
                employeeDiscounts.Add(airline, eDiscount);
            }
            return employeeDiscounts;
        }

        public static Dictionary<Airline, Double> GetEmployeeWages()
        {
            var employeeWages = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double sWage = airline.Fees.GetValue(FeeTypes.GetType("Support Wage"));
                double mWage = airline.Fees.GetValue(FeeTypes.GetType("Maintenance Wage"));
                double pWage = airline.Fees.GetValue(FeeTypes.GetType("Pilot Base Salary")); // *airline.Pilots.Count();
                double cWage = airline.Fees.GetValue(FeeTypes.GetType("Cabin Wage"));
                double iWage = airline.Fees.GetValue(FeeTypes.GetType("Instructor Base Salary"));
                // *PilotModel.FlightSchool.MaxNumberOfInstructors;
                int cabinCrew =
                    airline.Routes.Where(r => r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed)
                           .Sum(r => ((PassengerRoute) r).GetTotalCabinCrew());
                int serviceCrew =
                    airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(airline))
                           .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support)
                           .Sum(a => a.NumberOfEmployees);
                int maintenanceCrew =
                    airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(airline))
                           .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                           .Sum(a => a.NumberOfEmployees);

                double averageWage = (sWage + mWage + pWage + cWage + iWage)/5;

                employeeWages.Add(airline, averageWage);
            }
            return employeeWages;
        }

        public static Double GetFillAverage()
        {
            var fillDegrees = Airlines.GetAllAirlines().Select(airline => (from r in airline.Routes select r.GetFillingDegree()).ToList()).Select(fillDegree => fillDegree.DefaultIfEmpty(0).Average()).ToList();
            return fillDegrees.DefaultIfEmpty(0).Average();
        }

        public static Dictionary<Airline, Double> GetFillAverages()
        {
            var fillAverage = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Double> fillDegree = (from r in airline.Routes
                                           where r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Passenger
                                           select r.GetFillingDegree()).ToList();
                double uFillDegree = fillDegree.DefaultIfEmpty(0).Average();
                fillAverage.Add(airline, uFillDegree);
            }
            return fillAverage;
        }

        public static double GetHumanAvgTicketPPD()
        {
            List<Double> ePrices = (from r in GameObject.GetInstance().HumanAirline.Routes
                                    where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                    select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.EconomyClass)).ToList();
            List<Double> bPrices = (from r in GameObject.GetInstance().HumanAirline.Routes
                                    where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                    select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.BusinessClass)).ToList();
            List<Double> fPrices = (from r in GameObject.GetInstance().HumanAirline.Routes
                                    where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                    select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.FirstClass)).ToList();
            double distance =
                (from r in GameObject.GetInstance().HumanAirline.Routes
                 select MathHelpers.GetDistance(r.Destination1, r.Destination2)).DefaultIfEmpty(0).Average();
            double avgHEP = ePrices.DefaultIfEmpty(0).Average();
            double avgHBP = bPrices.DefaultIfEmpty(0).Average();
            double avgFBP = fPrices.DefaultIfEmpty(0).Average();
            double avgHPrice = ((avgHEP*0.7) + (avgHBP*0.2) + (avgFBP*0.1))/3;
            return ((avgHEP*0.7) + (avgHBP*0.2) + (avgFBP*0.1))/distance;
        }

        public static double GetHumanFillAverage()
        {
            List<Double> fillDegree =
                (from r in GameObject.GetInstance().HumanAirline.Routes select r.GetFillingDegree()).ToList();

            return fillDegree.DefaultIfEmpty(0).Average();
        }

        public static double GetHumanOnTime()
        {
            return
                GameObject.GetInstance()
                          .HumanAirline.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
        }

        public static double GetHumanOnTimeYear(int year)
        {
            return GameObject.GetInstance()
                             .HumanAirline.Statistics.GetStatisticsValue(year, StatisticsTypes.GetStatisticsType("On-Time%"));
        }

        public static double GetOnTimePercent(Airline airline)
        {
            return airline.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
        }

        public static Dictionary<Airline, Double> GetPPDdifference()
        {
            var ppdDifference = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                const double avgPPD = 0; // GetTotalTicketPPD();
                List<Double> aiEconPrices = (from r in airline.Routes
                                             where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                             select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.EconomyClass)).ToList();
                List<Double> aiBusPrices = (from r in airline.Routes
                                            where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                            select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.BusinessClass)).ToList();
                List<Double> aiFirstPrices = (from r in airline.Routes
                                              where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                              select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.FirstClass)).ToList();
                double distance =
                    (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2))
                        .DefaultIfEmpty(0).Average();
                double avgEP = aiEconPrices.DefaultIfEmpty(0).Average();
                double avgBP = aiBusPrices.DefaultIfEmpty(0).Average();
                double avgFP = aiFirstPrices.DefaultIfEmpty(0).Average();
                double avgDistPrice = ((avgEP*0.7) + (avgBP*0.2) + (avgFP*0.1))/distance;
                double avgDiff = avgDistPrice - avgPPD;
                ppdDifference.Add(airline, avgDiff);
            }
            return ppdDifference;
        }

        public static IDictionary<Airline, Double> GetRatingScale(
            IDictionary<Airline, Double> valueDictionary,
            double lower = 1,
            double upper = 100)
        {
            foreach (Airline airline in valueDictionary.Keys.ToArray())
            {
                if (Double.IsNaN(valueDictionary[airline]))
                {
                    valueDictionary[airline] = 0;
                }
            }

            var scaleValues = new Dictionary<Airline, double>();
            Double max = valueDictionary.Values.DefaultIfEmpty(0).Max();
            Double min = valueDictionary.Values.DefaultIfEmpty(0).Min();
            Double avg = valueDictionary.Values.DefaultIfEmpty(0).Sum()/valueDictionary.Values.Count;

            Double gap = max - min;

            if (gap.Equals(0))
            {
                gap = 1;
            }

            return valueDictionary.ToDictionary(v => v.Key, x => (upper - lower)*((x.Value - min)/gap) + lower);
        }

        //returns a dictionary of unassigned pilots

        //just a helper method to get a score for a given year
        public static int GetScore(Airline airline, int year)
        {
            return airline.GameScores.Where(score => score.Key.Year == year).Sum(score => score.Value);
        }

        //helper method to get a score for a given month of a given year
        public static int GetScore(Airline airline, int year, int month)
        {
            return airline.GameScores.Where(score => score.Key.Month == month && score.Key.Year == year).Sum(score => score.Value);
        }

        //helper method to get the score inclusively for a provided date range

        public static int GetScore(Airline airline)
        {
            return airline.GameScores.Sum(score => score.Value);
        }

        public static Dictionary<Airline, Double> GetTotalOnTime()
        {
            var totalOnTime = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double otp = airline.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("On-Time%"));
                totalOnTime.Add(airline, otp);
            }
            return totalOnTime;
        }

        public static Dictionary<Airline, Double> GetTotalPPD()
        {
            var ticketPPD = new List<Double>();
            var uDistPrice = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Double> econPrices = (from r in airline.Routes
                                           where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                           select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.EconomyClass)).ToList();
                List<Double> busPrices = (from r in airline.Routes
                                          where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                          select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.BusinessClass)).ToList();
                List<Double> firstPrices = (from r in airline.Routes
                                            where r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed
                                            select ((PassengerRoute) r).GetFarePrice(AirlinerClass.ClassType.FirstClass)).ToList();
                List<Double> distance =
                    (from r in airline.Routes select MathHelpers.GetDistance(r.Destination1, r.Destination2)).ToList();
                double avgDistPrice = ((econPrices.DefaultIfEmpty(0).Average()*0.7)
                                       + (busPrices.DefaultIfEmpty(0).Average()*0.2)
                                       + (firstPrices.DefaultIfEmpty(0).Average()*0.1))
                                      /distance.DefaultIfEmpty(0).Average();
                uDistPrice.Add(airline, avgDistPrice);
            }
            return uDistPrice;
        }

        public static Dictionary<Airline, Double> GetUnassignedPilots()
        {
            var unassignedPilots = new Dictionary<Airline, Double>();
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                double unPilots = airline.Pilots.Count(p => p.Airliner == null);
                unassignedPilots.Add(airline, unPilots);
            }
            return unassignedPilots;
        }

        public static int GetWeeklyScore(Airline airline)
        {
            int score = 0;

            //some base score weights:
            //fleet: 5 per airliner
            // destinations: 2 per destination
            // cash: 1 per $1,000,000
            // happiness: raw value
            // employee happiness: raw value
            // maint, security, safety: raw values

            int fleet = airline.Fleet.Count();
            int destinations = airline.Airports.Count();
            int cash = (int) airline.Money/1000000;
            double happiness = airline.Ratings.CustomerHappinessRating;
            happiness = Math.Pow(happiness, 2)/2;
            double empHappiness = airline.Ratings.EmployeeHappinessRating;
            empHappiness = Math.Pow(empHappiness, 2)/4;
            double maint = airline.Ratings.MaintenanceRating;
            double security = airline.Ratings.SecurityRating;
            double safety = airline.Ratings.SafetyRating;

            score +=
                (int) ((fleet*5) + (destinations*2) + cash + happiness + empHappiness + maint + safety + security);
            airline.CountedScores++;
            return score;
        }

        public static int GetYearScore(Airline airline, int startYear, int endYear)
        {
            return airline.GameScores.Where(score => score.Key.Year >= startYear && score.Key.Year <= endYear).Sum(score => score.Value);
        }

        public static double NextGaussian(this Random r, double mean = 0, double stdv = 1)
        {
            //generate a couple randoms
            double u1 = r.NextDouble();
            double u2 = r.NextDouble();

            //stand dev
            double randStdNormal = Math.Sqrt(-2.0*Math.Log(u1))*Math.Sin(2.0*Math.PI*u2);

            //normalize
            double randNormal = mean + stdv*randStdNormal;

            return randNormal;
        }

        public static Dictionary<Country, double> GetCountryAirportsServed()

        {
            var countryAirports = new Dictionary<Country, double>();
            foreach (
                Country country in
                    GameObject.GetInstance().HumanAirline.Airports.Select(a => a.Profile.Country).Distinct())
            {
                double c = Airports.GetAirports(country).Count();
                double cHuman = GameObject.GetInstance().HumanAirline.Airports.Count(a => a.Profile.Country == country);
                countryAirports.Add(country, (c/cHuman));
            }
            return countryAirports;
        }

        public static double GetWorldAirportsServed()
        {
            double hAirports = GameObject.GetInstance().HumanAirline.Airports.Count();
            return hAirports/Airports.GetAllAirports().Count();
        }

        #endregion

        //generate a 1-100 scale for a list of values

        //returns the airlines best year in terms of score
    }
}