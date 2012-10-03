using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlineHelpers
    {
        //adds an invoice to an airline
        public static void AddAirlineInvoice(Airline airline, DateTime date, Invoice.InvoiceType type, double amount)
        {
            airline.addInvoice(new Invoice(date, type, amount));
        }
        //buys an airliner to an airline
        public static FleetAirliner BuyAirliner(Airline airline, Airliner airliner, Airport airport)
        {
            return BuyAirliner(airline, airliner, airport, 0);

        }
        public static FleetAirliner BuyAirliner(Airline airline, Airliner airliner, Airport airport, double discount)
        {
            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airliner.TailNumber, airport);

            airline.addAirliner(fAirliner);

            double price = airliner.getPrice() * ((100 - discount) / 100);

            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

            return fAirliner;

        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, Dictionary<AirlinerType, int> orders, Airport airport, DateTime deliveryDate)
        {
            OrderAirliners(airline, orders, airport, deliveryDate, 0);

        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, Dictionary<AirlinerType, int> orders, Airport airport, DateTime deliveryDate, double discount)
        {
            foreach (KeyValuePair<AirlinerType, int> order in orders)
            {
                for (int i = 0; i < order.Value; i++)
                {
                    Airliner airliner = new Airliner(order.Key, airline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);
                    Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    airline.addAirliner(pType, airliner, airliner.TailNumber, airport);

                }



            }

            int totalAmount = orders.Values.Sum();
            double price = orders.Keys.Sum(t => t.Price * orders[t]);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount))) * ((100 - discount) / 100);

            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);
        }
        //reallocate all gates and facilities from one airport to another - gates, facilities and routes
        public static void ReallocateAirport(Airport oldAirport, Airport newAirport, Airline airline)
        {

            //gates
            foreach (Gate gate in oldAirport.Terminals.getUsedGates(airline))
            {
                gate.HasRoute = false;
            }
            while (oldAirport.Terminals.getTotalNumberOfGates(airline) > 0)
            {
                oldAirport.Terminals.releaseGate(airline);
                newAirport.Terminals.rentGate(airline);
            }
            //routes
            var obsoleteRoutes = (from r in airline.Routes where r.Destination1 == oldAirport || r.Destination2 == oldAirport select r);

            foreach (Route route in obsoleteRoutes)
            {
                if (route.Destination1 == oldAirport) route.Destination1 = newAirport;
                if (route.Destination2 == oldAirport) route.Destination2 = newAirport;

                newAirport.Terminals.getEmptyGate(airline).HasRoute = true;

                var entries = route.TimeTable.Entries.FindAll(e => e.Destination.Airport == oldAirport);

                foreach (RouteTimeTableEntry entry in entries)
                    entry.Destination.Airport = newAirport;

                foreach (FleetAirliner airliner in route.getAirliners())
                {
                    if (airliner.Homebase == oldAirport)
                        airliner.Homebase = newAirport;
                }
            }

            //facilities
            foreach (AirportFacility facility in oldAirport.getCurrentAirportFacilities(airline))
            {
                newAirport.setAirportFacility(airline, facility, GameObject.GetInstance().GameTime);
            }

            oldAirport.clearFacilities(airline);

            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {

                AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                oldAirport.setAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
            }
        }
        //returns all route facilities for a given airline and type
        public static List<RouteFacility> GetRouteFacilities(Airline airline, RouteFacility.FacilityType type)
        {
            return RouteFacilities.GetFacilities(type).FindAll(f => f.Requires == null || airline.Facilities.Contains(f.Requires));
        }
        //adds a subsidiary airline to an airline
        public static void AddSubsidiaryAirline(Airline airline, SubsidiaryAirline sAirline, double money, Airport airportHomeBase)
        {
            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -money);
            sAirline.Money = money;
            sAirline.StartMoney = money;

            sAirline.Fees = new AirlineFees();

            airline.addSubsidiaryAirline(sAirline);

            //sets all the facilities at an airport to none for all airlines
            foreach (Airport airport in Airports.GetAllAirports())
            {

                foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
                {
                    AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                    airport.setAirportFacility(sAirline, noneFacility, GameObject.GetInstance().GameTime);
                }

            }

            AirportFacility serviceFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);
            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

            airportHomeBase.setAirportFacility(sAirline, serviceFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.setAirportFacility(sAirline, checkinFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.Terminals.rentGate(sAirline);

            Airlines.AddAirline(sAirline);

        }
        //creates a subsidiary airline for an airline
        public static SubsidiaryAirline CreateSubsidiaryAirline(Airline airline, double money, string name, string iata, Airline.AirlineMentality mentality, Airline.AirlineMarket market, Airport homebase)
        {
            AirlineProfile profile = new AirlineProfile(name, iata, airline.Profile.Color, airline.Profile.Country, airline.Profile.CEO);
          
            SubsidiaryAirline sAirline = new SubsidiaryAirline(airline, profile, mentality, market);

            AddSubsidiaryAirline(airline, sAirline,money,homebase);

            return sAirline;
        }
    }
}
