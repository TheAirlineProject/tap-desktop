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

            Boolean newRoute = rnd.Next(newRouteInterval)/1000 == 0;

            //creates a new route for the airline
            if (newRoute)
            {
                List<Airport> homeAirports = airline.Airports.FindAll(a => a.getAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);
                homeAirports.AddRange(airline.Airports.FindAll(a => a.Hubs.Count(h => h.Airline == airline) > 0)); //hubs

                Airport airport = homeAirports.Find(a => a.Terminals.getFreeGates(airline) > 0);
                
                if (airport == null)
                {
                    airport = homeAirports.Find(a => a.Terminals.getFreeGates() > 0);
                    if (airport != null)
                        airport.Terminals.rentGate(airline);
                }

                if (airport != null)
                {
                    Airport destination = GetDestinationAirport(airline, airport);

                    if (destination != null)
                    {
                     
                        if (destination.Terminals.getFreeGates(airline) == 0) destination.Terminals.rentGate(airline);

                        if (!airline.Airports.Contains(destination)) airline.addAirport(destination);

                        double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                        Guid id = Guid.NewGuid();

                        Route route = new Route(id.ToString(), airport, destination, price, airline.getNextFlightCode(), airline.getNextFlightCode());

                        airline.addRoute(route);

                        //flight
                    }
                }



            }
        }
        //returns the destination for an airline with a start airport
        private static Airport GetDestinationAirport(Airline airline, Airport airport)
        {
            double maxDistance = (from a in AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From < GameObject.GetInstance().GameTime.Year; }))
                                  select a.Range).Max();

            //       return (dest1.Profile.Country == dest2.Profile.Country || distance < 1000 || (dest1.Profile.Country.Region == dest2.Profile.Country.Region && (dest1.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International) && (dest2.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International)) || (dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International && dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International));

            List<Airport> airports = new List<Airport>();
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            switch (airline.MarketFocus)
            {
                case Airline.AirlineMarket.Global:
                    airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance);
                    break;
                case Airline.AirlineMarket.Local:
                    airports = Airports.GetAirports(airport.Profile.Country).FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance);
                    break;
                case Airline.AirlineMarket.Regional:
                    airports = Airports.GetAirports(airport.Profile.Country.Region).FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance);
                    break;
            }
            Airport destination = null;
            int counter = 0;

            while (destination == null && counter < airports.Count)
            {
                destination = airports[counter];

                if ((routes.Find(r => r.Destination1 == destination || r.Destination2 == destination) != null || destination.Terminals.getFreeGates()==0) && destination.Terminals.getFreeGates(airline)==0) destination = null;
                counter++;
                
            }
            return destination;
        }
    }
}
