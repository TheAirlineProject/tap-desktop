using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    /*the class for some general route helpers*/
    public class RouteHelpers
    {
        /*returns the price score for the route*/
        public static double GetRoutePriceScore(Route route)
        {
            double basePrice = PassengerHelpers.GetPassengerPrice(route.Destination1, route.Destination2);

            double price = ((PassengerRoute)route).getFarePrice(AirlinerClass.ClassType.Economy_Class);

            double priceFactor = price / basePrice; 

            double priceLevel = 10 * priceFactor;

            return Math.Max(1, 16 - priceLevel);
        }
        /*returns the seats score for the route*/
        public static double GetRouteSeatsScore(Route route)
        {
            TimeSpan flightTime = MathHelpers.GetFlightTime(route.Destination1, route.Destination2, route.getAirliners()[0].Airliner.Type);

            AirlinerFacility seats = route.getAirliners()[0].Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Seat);

            var seatfacilities = AirlinerFacilities.GetFacilities(AirlinerFacility.FacilityType.Seat).Where(f => f.FromYear >= GameObject.GetInstance().GameTime.Year).OrderBy(f => f.ServiceLevel);

            int facilitynumber = seatfacilities.Count() - seatfacilities.ToList().IndexOf(seats) - 1; //max == 6

            double seatlevel;

            if (flightTime.Hours < 1)
            {
                seatlevel = 13 - facilitynumber;
            }
            else if (flightTime.Hours >= 1 && flightTime.Hours < 3)
            {
                seatlevel = 12 - facilitynumber;
            }
            else if (flightTime.Hours >= 3 && flightTime.Hours < 7)
            {
                seatlevel = 11 - facilitynumber;
            }
            else
            {
                seatlevel = 10 - facilitynumber;
            }

            return Math.Min(10, seatlevel);

        }
        /*returns the meal score for the route*/
        public static double GetRouteMealScore(Route route)
        {
            TimeSpan flightTime = MathHelpers.GetFlightTime(route.Destination1, route.Destination2, route.getAirliners()[0].Airliner.Type);

            RouteFacility food = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(RouteFacility.FacilityType.Food);

            double foodlevel;

            if (flightTime.Hours < 1)
            {
                if (food.ServiceLevel < 0)
                    foodlevel = 5;
                else
                    foodlevel = 5 + (food.ServiceLevel / 10);
            }
            else if (flightTime.Hours >= 1 && flightTime.Hours < 3)
            {
                if (food.ServiceLevel < 0)
                    foodlevel = 4;
                else
                    foodlevel = 4 + (food.ServiceLevel / 10);
            }
            else if (flightTime.Hours >= 3 && flightTime.Hours < 7)
            {
                if (food.ServiceLevel < 0)
                    foodlevel = 2;
                else
                    foodlevel = 3 + (food.ServiceLevel / 10);
            }
            else
            {
                if (food.ServiceLevel < 0)
                    foodlevel = 1;
                else
                    foodlevel = 2 + (food.ServiceLevel / 10);
            }

            return Math.Min(10, foodlevel);


        }
        /*returns the plane type score for the route*/
        public static double GetRoutePlaneTypeScore(Route route)
        {
            TimeSpan flightTime = MathHelpers.GetFlightTime(route.Destination1, route.Destination2, route.getAirliners()[0].Airliner.Type);

            AirlinerType airlinertype = route.getAirliners()[0].Airliner.Type;

            int oldTypeFactor = airlinertype.Produced.To < GameObject.GetInstance().GameTime ? 1 : 0;

            int airlinerLevel;

            int paxLevel = ((AirlinerPassengerType)airlinertype).MaxSeatingCapacity / 40; //maks 10??

            if (flightTime.Hours < 1)
            {
                airlinerLevel = 4 + paxLevel - oldTypeFactor;
            }
            else if (flightTime.Hours >= 1 && flightTime.Hours < 3)
            {
                airlinerLevel = 3 + paxLevel - oldTypeFactor;
            }
            else if (flightTime.Hours >= 3 && flightTime.Hours < 7)
            {
                airlinerLevel = 2 + paxLevel - oldTypeFactor;
            }
            else
            {
                airlinerLevel = 1 + paxLevel - oldTypeFactor;
            }

            return Math.Min(10, airlinerLevel);
        }
        /*returns the plane age score for the route*/
        public static double GetPlaneAgeScore(Route route)
        {
            FleetAirliner airliner = route.getAirliners()[0];

            int age = airliner.Airliner.Age;

            double score = 10 - (age / 2);

            return Math.Max(1, score);
        }
        /*returns the luggage score for the route*/
        public static double GetRouteLuggageScore(Route route)
        {
           double bagFee = GameObject.GetInstance().HumanAirline.Fees.getValue(FeeTypes.GetType("1 Bag"));

           if (bagFee == 0)
               return 8;
           else
               return 3;
               
            
        }
        /*returns the total score of the route*/
        public static double GetRouteTotalScore(Route route)
        {
            double score = GetPlaneAgeScore(route) + GetRouteInflightScore(route) + GetRouteMealScore(route) + GetRoutePlaneTypeScore(route) + (5*GetRoutePriceScore(route)) + GetRouteSeatsScore(route) + GetRouteLuggageScore(route);

            if ((int)RouteFacility.FacilityType.WiFi <= GameObject.GetInstance().GameTime.Year)
            {
                double wifiScore = GetRouteWifiScore(route);
                score += wifiScore;

                return score /12;
            }
            return score / 11;
        }
        /*returns the inflight demand score for the route*/
        public static double GetRouteInflightScore(Route route)
        {
            TimeSpan flightTime = MathHelpers.GetFlightTime(route.Destination1, route.Destination2, route.getAirliners()[0].Airliner.Type);

            AirlinerFacility inflight = route.getAirliners()[0].Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Video);

            var videofacilities = AirlinerFacilities.GetFacilities(AirlinerFacility.FacilityType.Video).Where(f => f.FromYear >= GameObject.GetInstance().GameTime.Year).OrderBy(f=>f.ServiceLevel);

            int facilitynumber = videofacilities.Count() - videofacilities.ToList().IndexOf(inflight) - 1; 

            double inflightlevel;

            if (flightTime.Hours < 1)
            {
                inflightlevel = 9 - facilitynumber;
            }
            else if (flightTime.Hours >= 1 && flightTime.Hours < 3)
            {
                inflightlevel = 8 - facilitynumber;
            }
            else if (flightTime.Hours >= 3 && flightTime.Hours < 7)
            {
                inflightlevel = 7 - facilitynumber;
            }
            else
            {
                inflightlevel = 10 - (2*facilitynumber);
            }

            return Math.Min(10, inflightlevel);


        }
        /*returns the wifi score for the route*/
        public static double GetRouteWifiScore(Route route)
        {
            RouteFacility wifi = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(RouteFacility.FacilityType.WiFi);

            if (wifi.Name == "None")
                return 3;
            if (wifi.Name == "Buyable")
                return 6;
            else
                return 9;
        }
    }
}
