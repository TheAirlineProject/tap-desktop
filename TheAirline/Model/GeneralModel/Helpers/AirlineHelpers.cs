using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlineHelpers
    {
        //buys an airliner to an airline
        public static FleetAirliner BuyAirliner(Airline airline, Airliner airliner, Airport airport)
        {
            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, airline, airliner, airliner.TailNumber, airport); 

            airline.addAirliner(fAirliner);

            airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.getPrice()));

            return fAirliner;
         
        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline,Dictionary<AirlinerType, int> orders, Airport airport, DateTime deliveryDate)
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
            double price = orders.Keys.Sum(t => t.Price);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

            airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice));
        }
    }
}
