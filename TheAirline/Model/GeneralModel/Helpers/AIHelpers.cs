using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for the AI
    public class AIHelpers
    {
        private static Random rnd = new Random();
        //updates a cpu airline
        public static void UpdateCPUAirline(Airline airline)
        {
            int newRouteInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newRouteInterval = 10000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newRouteInterval = 100000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newRouteInterval = 1000000;
                    break;
            }

            Boolean newRoute = rnd.Next(newRouteInterval) == 0;

            //creates a new route for the airline
            if (newRoute)
            {
                List<Airport> homeAirports = airline.Airports.FindAll(a => a.getAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);
                homeAirports.AddRange(airline.Airports.FindAll(a => a.Hubs.Count(h => h.Airline == airline) > 0)); //hubs

                Airport airport = homeAirports.Find(a => a.Terminals.getFreeGates(airline) > 0);
                
                if (airport == null)
                {
                    airport = homeAirports.Find(a => a.Terminals.getFreeGates() > 0);
                    if (airport !=null)
                       airport.Terminals.rentGate(airline);
                    
                }

                if (airport != null)
                {
                    Airport destination = GetDestinationAirport(airline, airport);

            
                    if (destination != null)
                    {

                        Airliner airliner = GetAirlinerForRoute(airline, airport, destination);

                        if (airliner != null)
                        {
                            if (destination.Terminals.getFreeGates(airline) == 0) destination.Terminals.rentGate(airline);

                            if (!airline.Airports.Contains(destination)) airline.addAirport(destination);

                            double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                            Guid id = Guid.NewGuid();

                            Route route = new Route(id.ToString(), airport, destination, price, airline.getNextFlightCode(), airline.getNextFlightCode());

                            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                            {
                                route.getRouteAirlinerClass(type).CabinCrew = 2;
                                route.getRouteAirlinerClass(type).DrinksFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Count)];// RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
                                route.getRouteAirlinerClass(type).FoodFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Count)];//RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Food);
                            }

                            airline.addRoute(route);

                            airport.Terminals.getEmptyGate(airline).Route = route;
                            destination.Terminals.getEmptyGate(airline).Route = route;

                            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();

                            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, airline, airliner, airliner.TailNumber, airline.Airports[0]);

                            RouteAirliner rAirliner = new RouteAirliner(fAirliner, route);

                            airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.getPrice()));

                            fAirliner.RouteAirliner = rAirliner;

                            airline.Fleet.Add(fAirliner);

                            rAirliner.Status = RouteAirliner.AirlinerStatus.To_route_start;
                        }
                    }
                }



            }
        }
        //returns the destination for an airline with a start airport
        public static Airport GetDestinationAirport(Airline airline, Airport airport)
        {
            double maxDistance = (from a in Airliners.GetAirlinersForSale()
                                  select a.Type.Range).Max();

     
            List<Airport> airports = new List<Airport>();
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            switch (airline.MarketFocus)
            {
                case Airline.AirlineMarket.Global:
                    airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates,airport.Profile.Coordinates)>100);
                    break;
                case Airline.AirlineMarket.Local:
                    airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 1000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
                    break;
                case Airline.AirlineMarket.Regional:
                    airports = Airports.GetAirports(airport.Profile.Country.Region).FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
            }
           
            Airport destination = null;
            int counter = 0;

            if (airports.Count == 0)
                airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 2000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
     

            while (destination == null && counter < airports.Count)
            {
                destination = airports[counter];

                if ((routes.Find(r => r.Destination1 == destination || r.Destination2 == destination) != null) || (destination.Terminals.getFreeGates()==0 && destination.Terminals.getFreeGates(airline)==0) || (destination == airport)) destination = null;
                counter++;
                
            }
            return destination;
        }
        //returns the best fit for an airliner for sale for a route
        public static Airliner GetAirlinerForRoute(Airline airline, Airport destination1, Airport destination2)
        {
            
            double distance = MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates);

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(distance);

            List<Airliner> airliners = Airliners.GetAirlinersForSale().FindAll(a => a.getPrice() < airline.Money - 1000000 && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

            if (airliners.Count > 0)
                return (from a in airliners orderby a.Type.Range select a).First();
            else
                return null;
   
        }
    }
}
