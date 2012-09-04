using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

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
            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought,GameObject.GetInstance().GameTime, airline, airliner, airliner.TailNumber, airport); 

            airline.addAirliner(fAirliner);

            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.getPrice());

            return fAirliner;
         
        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline,Dictionary<AirlinerType, int> orders, Airport airport, DateTime deliveryDate)
        {
             
            foreach (KeyValuePair<AirlinerType, int> order in orders)
            {
                for (int i = 0; i < order.Value; i++)
                {
                    Airliner airliner = new Airliner(order.Key, airline.Profile.Country.TailNumbers.getNextTailNumber(),deliveryDate);
                    Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    airline.addAirliner(pType, airliner, airliner.TailNumber, airport);

                }
              


            }

            int totalAmount = orders.Values.Sum();
            double price = orders.Keys.Sum(t => t.Price * orders[t]);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);
         }
        //reallocate all gates and facilities from one airport to another - gates, facilities and routes
        public static void ReallocateAirport(Airport oldAirport, Airport newAirport, Airline airline)
        {
            
            //gates
            foreach (Gate gate in oldAirport.Terminals.getUsedGates(airline))
            {
                gate.Route = null;
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

                newAirport.Terminals.getEmptyGate(airline).Route = route;
                
                var entries = route.TimeTable.Entries.FindAll(e=>e.Destination.Airport == oldAirport);

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
        //merges and clears all invoices from the previous month
        public static void MergeInvoicesMonthly(Airline airline)
        {
            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
    {
        double sum=0;
        if (type != Invoice.InvoiceType.Total)
    {
       foreach (Invoice invoice in airline.getInvoices(GameObject.GetInstance().GameTime.AddMonths(-1),GameObject.GetInstance().GameTime.AddMinutes(-1))) sum += invoice.Amount;

        airline.clearInvoices(GameObject.GetInstance().GameTime.AddMonths(-1),GameObject.GetInstance().GameTime.AddMinutes(-1));
        airline.setInvoice(new Invoice(GameObject.GetInstance().GameTime.AddMinutes(-1), type, sum));
    }
}
}
    }
}
