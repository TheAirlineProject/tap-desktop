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
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlineHelpers
    {
        //creates an airliner for an airline
        public static FleetAirliner CreateAirliner(Airline airline, AirlinerType type)
        {
            Airliner airliner = new Airliner(type, airline.Profile.Country.TailNumbers.getNextTailNumber(), GameObject.GetInstance().GameTime);
            Airliners.AddAirliner(airliner);

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airliner.TailNumber, airline.Airports[0]);

            airliner.clearAirlinerClasses();

            AirlinerHelpers.CreateAirlinerClasses(airliner);

            return fAirliner;

        }
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
            FleetAirliner fAirliner = AddAirliner(airline, airliner, airport);

            double price = airliner.getPrice() * ((100 - discount) / 100);

            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

            return fAirliner;

        }
        public static FleetAirliner AddAirliner(Airline airline, Airliner airliner, Airport airport)
        {
       
            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();
            
            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airliner.TailNumber, airport);
             
            airline.addAirliner(fAirliner);

            return fAirliner;
        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, List<AirlinerOrder> orders, Airport airport, DateTime deliveryDate)
        {
            OrderAirliners(airline, orders, airport, deliveryDate, 0);

        }
        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, List<AirlinerOrder> orders, Airport airport, DateTime deliveryDate, double discount)
        {
            
            foreach (AirlinerOrder order in orders)
            {
                for (int i = 0; i < order.Amount; i++)
                {
                    Airliner airliner = new Airliner(order.Type, airline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);
                    Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    airline.addAirliner(pType, airliner, airliner.TailNumber, airport);

                    airliner.clearAirlinerClasses();

                    foreach (AirlinerClass aClass in order.Classes)
                    {
                        AirlinerClass tClass = new AirlinerClass(aClass.Type,aClass.SeatingCapacity);
                        tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            tClass.setFacility(airline, facility);

                        airliner.addAirlinerClass(tClass);
                    }


                }



            }

            int totalAmount = orders.Sum(o => o.Amount)  ;
            double price = orders.Sum(o => o.Type.Price * o.Amount); 

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
                newAirport.addAirportFacility(airline, facility, GameObject.GetInstance().GameTime);
            }

            oldAirport.clearFacilities(airline);

            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {

                AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                oldAirport.addAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
            }
        }
        //returns all route facilities for a given airline and type
        public static List<RouteFacility> GetRouteFacilities(Airline airline, RouteFacility.FacilityType type)
        {
            return RouteFacilities.GetFacilities(type).FindAll(f => f.Requires == null || airline.Facilities.Contains(f.Requires));
        }
        //launches a subsidiary to operate on its own
        public static void MakeSubsidiaryAirlineIndependent(SubsidiaryAirline airline)
        {
            airline.Airline.removeSubsidiaryAirline(airline);

            airline.Airline = null;

            airline.Profile.CEO = string.Format("{0} {1}", Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName());

            if (!Airlines.ContainsAirline(airline))
                Airlines.AddAirline(airline);
        }
        //closes a subsidiary airline for an airline
        public static void CloseSubsidiaryAirline(SubsidiaryAirline airline)
        {
            AddAirlineInvoice(airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, airline.Money);
          
            airline.Airline.removeSubsidiaryAirline(airline);
            Airlines.RemoveAirline(airline);

            var fleet = airline.Fleet; 

            for (int f=0;f<fleet.Count;f++)
            {
                fleet[f].Airliner.Airline = airline.Airline;
                airline.Airline.addAirliner(fleet[f]);

            }

            var airports = airline.Airports;

            for (int i=0;i<airports.Count;i++)
            {
                var gates = airports[i].Terminals.getUsedGates(airline);

                for (int g = 0;g<gates.Count;g++)
                {
                    gates[g].Airline = airline.Airline;
                    gates[g].HasRoute = false;
                }
                if (!airline.Airline.Airports.Contains(airports[i]))
                {
                    airline.Airline.addAirport(airports[i]);
                }

                airports[i].clearFacilities(airline);

       
            }

  
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

                    airport.addAirportFacility(sAirline, noneFacility, GameObject.GetInstance().GameTime);
                }

            }

            foreach (AirlinePolicy policy in airline.Policies)
                sAirline.addAirlinePolicy(policy);

           
            AirportFacility serviceFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);
            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

            airportHomeBase.addAirportFacility(sAirline, serviceFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.addAirportFacility(sAirline, checkinFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.Terminals.rentGate(sAirline);

            Airlines.AddAirline(sAirline);

        }


        //creates a subsidiary airline for an airline
        public static SubsidiaryAirline CreateSubsidiaryAirline(Airline airline, double money, string name, string iata, Airline.AirlineMentality mentality, Airline.AirlineFocus market, Airport homebase)
        {
            AirlineProfile profile = new AirlineProfile(name, iata, airline.Profile.Color, airline.Profile.CEO,true,GameObject.GetInstance().GameTime.Year,2199);
            profile.Country = airline.Profile.Country;
         
            SubsidiaryAirline sAirline = new SubsidiaryAirline(airline, profile, mentality, market, airline.License);

            AddSubsidiaryAirline(airline, sAirline,money,homebase);

            return sAirline;
        }
        //hires the pilots for a specific airliner
        public static void HireAirlinerPilots(FleetAirliner airliner)
        {
            while (airliner.Airliner.Type.CockpitCrew > airliner.NumberOfPilots)
           {
                var pilots = Pilots.GetUnassignedPilots(p=>p.Profile.Town.Country == airliner.Airliner.Airline.Profile.Country);

                if (pilots.Count == 0)
                    pilots = Pilots.GetUnassignedPilots(p=>p.Profile.Town.Country.Region == airliner.Airliner.Airline.Profile.Country.Region);

                if (pilots.Count == 0)
                    pilots = Pilots.GetUnassignedPilots();

                Pilot pilot = pilots.OrderByDescending(p => p.Rating).First();
                airliner.Airliner.Airline.addPilot(pilot);

                pilot.Airliner = airliner;
                airliner.addPilot(pilot);
            }
            
            if (Pilots.GetNumberOfUnassignedPilots() < 10)
                GeneralHelpers.CreatePilots(50);
            
        }
        //returns the discount factor for a manufactorer for an airline and for a period
        public static double GetAirlineManufactorerDiscountFactor(Airline airline, int length, Boolean forReputation) 
        {
            double score = 0;

            if (forReputation)
                score = 0.3 * (1 + (int)airline.getReputation());
            else
                score = 0.005 * (1 + (int)airline.getValue());

            double discountFactor = (Convert.ToDouble(length) / 20) + (score);
            double discount = Math.Pow(discountFactor, 5);

            if (discount > 30)
                discount = length * 3;
      
            return discount;


        }
        //returns if an airline has licens for flying between two airports
        public static Boolean HasAirlineLicens(Airline airline, Airport airport1, Airport airport2)
        {
            Boolean isInUnion = Unions.GetUnions(airport1.Profile.Country, GameObject.GetInstance().GameTime).Intersect(Unions.GetUnions(airport2.Profile.Country, GameObject.GetInstance().GameTime)).Any();

            if (airline.License == Airline.AirlineLicense.Long_Haul)
                return true;

            if (airline.License == Airline.AirlineLicense.Short_Haul && (MathHelpers.GetDistance(airport1, airport2) < 2000 || isInUnion))
                return true;

            if (airline.License == Airline.AirlineLicense.Domestic && airport1.Profile.Country == airport2.Profile.Country)
                return true;

            if (airline.License == Airline.AirlineLicense.Regional && (airport1.Profile.Country.Region == airport2.Profile.Country.Region || isInUnion))
                return true;

            return false;
        }
        //returns the monthly payroll for an airline
        public static double GetMonthlyPayroll(Airline airline)
        {
            double instructorFee = airline.FlightSchools.Sum(f => f.NumberOfInstructors) * airline.Fees.getValue(FeeTypes.GetType("Instructor Base Salary"));

            double cockpitCrewFee = airline.Pilots.Count * airline.Fees.getValue(FeeTypes.GetType("Cockpit Wage"));

            double cabinCrewFee = airline.Routes.Where(r => r.Type == Route.RouteType.Passenger).Sum(r => ((PassengerRoute)r).getTotalCabinCrew()) * airline.Fees.getValue(FeeTypes.GetType("Cabin Wage"));

            double serviceCrewFee = airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support).Sum(a => a.NumberOfEmployees) * airline.Fees.getValue(FeeTypes.GetType("Support Wage"));
            double maintenanceCrewFee = airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees) * airline.Fees.getValue(FeeTypes.GetType("Maintenance Wage"));

            return instructorFee + cockpitCrewFee + cabinCrewFee + serviceCrewFee + maintenanceCrewFee;

        }
        //returns the number of routes out of an airport for an airline
        public static int GetAirportOutboundRoutes(Airline airline, Airport airport)
        {
            var routes = new List<Route>(airline.Routes);
            return routes.Count(r => r.Destination1 == airport || r.Destination2 == airport);

        }
        //switches from one airline to another airline
        public static void SwitchAirline(Airline airlineFrom, Airline airlineTo)
        {
            while (airlineFrom.Alliances.Count > 0)
            {
                Alliance alliance = airlineFrom.Alliances[0];
                alliance.removeMember(airlineFrom);
                alliance.addMember(new AllianceMember(airlineTo, GameObject.GetInstance().GameTime));
            }
            while (airlineFrom.Facilities.Count > 0)
            {
                AirlineFacility airlineFacility = airlineFrom.Facilities[0];
                airlineFrom.removeFacility(airlineFacility);
                airlineTo.addFacility(airlineFacility);
            }


            while (airlineFrom.getFleetSize() > 0)
            {
                FleetAirliner airliner = airlineFrom.Fleet[0];
                airlineFrom.removeAirliner(airliner);
                airlineTo.addAirliner(airliner);
                airliner.Airliner.Airline = airlineTo;
            }

            while (airlineFrom.Routes.Count > 0)
            {
                Route route = airlineFrom.Routes[0];
                route.Airline = airlineTo;

                airlineFrom.removeRoute(route);
                airlineTo.addRoute(route);
            }

            while (airlineFrom.Airports.Count > 0)
            {
                Airport airport = airlineFrom.Airports[0];
                airport.Terminals.switchAirline(airlineFrom, airlineTo);

                foreach (AirportFacility facility in airport.getCurrentAirportFacilities(airlineFrom))
                {
                    if (facility.TypeLevel > airport.getCurrentAirportFacility(airlineTo, facility.Type).TypeLevel)
                        airport.addAirportFacility(airlineTo, facility, GameObject.GetInstance().GameTime);

                    AirportFacility noneFacility = AirportFacilities.GetFacilities(facility.Type).Find(f => f.TypeLevel == 0);

                    airport.setAirportFacility(airlineFrom, noneFacility, GameObject.GetInstance().GameTime);

                }
            }
        }
        
    }
}
