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
using TheAirline.Model.PassengerModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlineHelpers
    {
        //clears the statistics for all routes for all airlines
        public static void ClearRoutesStatistics()
        {
            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes);

            foreach (Route route in routes)
                route.Statistics.clear();
        }
        //clears the statistics for all airlines
        public static void ClearAirlinesStatistics()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
                airline.Statistics.clear();
        }
        //creates an airliner for an airline
        public static FleetAirliner CreateAirliner(Airline airline, AirlinerType type)
        {
            Guid id = Guid.NewGuid();

            Airliner airliner = new Airliner(id.ToString(), type, airline.Profile.Country.TailNumbers.getNextTailNumber(), GameObject.GetInstance().GameTime);
            Airliners.AddAirliner(airliner);

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airline.Airports[0]);

            airliner.clearAirlinerClasses();

            AirlinerHelpers.CreateAirlinerClasses(airliner);

            return fAirliner;

        }
        //adds an invoice to an airline
        public static void AddAirlineInvoice(Airline airline, DateTime date, Invoice.InvoiceType type, double amount)
        {
            if (airline.IsHuman && GameObject.GetInstance().HumanAirline == airline)
            {
                GameObject.GetInstance().addHumanMoney(amount);
                GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(date, type, amount), false);
            }
            else
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

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airport);

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
            Guid id = Guid.NewGuid();

            foreach (AirlinerOrder order in orders)
            {
                for (int i = 0; i < order.Amount; i++)
                {
                    Airliner airliner = new Airliner(id.ToString(), order.Type, airline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);
                    Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    airline.addAirliner(pType, airliner, airport);

                    airliner.clearAirlinerClasses();

                    foreach (AirlinerClass aClass in order.Classes)
                    {
                        AirlinerClass tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            tClass.setFacility(airline, facility);

                        airliner.addAirlinerClass(tClass);
                    }


                }



            }

            int totalAmount = orders.Sum(o => o.Amount);
            double price = orders.Sum(o => o.Type.Price * o.Amount);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount))) * ((100 - discount) / 100);

            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);
        }

        //reallocate all gates and facilities from one airport to another - gates, facilities and routes
        public static void ReallocateAirport(Airport oldAirport, Airport newAirport, Airline airline)
        {

            //contract
            List<AirportContract> oldContracts = oldAirport.getAirlineContracts(airline);

            foreach (AirportContract oldContract in oldContracts)
            {

                oldAirport.removeAirlineContract(oldContract);

                oldContract.Airport = newAirport;

                newAirport.addAirlineContract(oldContract);

                for (int i = 0; i < oldContract.NumberOfGates; i++)
                {
                    Gate newGate = newAirport.Terminals.getGates().Where(g => g.Airline == null).First();
                    newGate.Airline = airline;

                    Gate oldGate = oldAirport.Terminals.getGates().Where(g => g.Airline == airline).First();
                    oldGate.Airline = null;
                }


            }
            //routes
            var obsoleteRoutes = (from r in airline.Routes where r.Destination1 == oldAirport || r.Destination2 == oldAirport select r);

            foreach (Route route in obsoleteRoutes)
            {
                if (route.Destination1 == oldAirport) route.Destination1 = newAirport;
                if (route.Destination2 == oldAirport) route.Destination2 = newAirport;


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

            for (int f = 0; f < fleet.Count; f++)
            {
                fleet[f].Airliner.Airline = airline.Airline;
                airline.Airline.addAirliner(fleet[f]);

            }

            var airports = airline.Airports;

            for (int i = 0; i < airports.Count; i++)
            {
                var contracts = airports[i].getAirlineContracts(airline);

                for (int j = 0; j < contracts.Count; j++)
                {
                    contracts[j].Airline = airline.Airline;
                }

                if (!airline.Airline.Airports.Contains(airports[i]))
                {
                    airline.Airline.addAirport(airports[i]);
                }

                foreach (AirportFacility facility in airports[i].getCurrentAirportFacilities(airline))
                {
                    if (airports[i].getAirlineAirportFacility(airline.Airline, facility.Type).Facility.TypeLevel < facility.TypeLevel)
                        airports[i].setAirportFacility(airline.Airline, facility, GameObject.GetInstance().GameTime);
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

            if (!AirportHelpers.HasFreeGates(airportHomeBase, sAirline))
                AirportHelpers.RentGates(airportHomeBase, sAirline, 2);

            Airlines.AddAirline(sAirline);

        }


        //creates a subsidiary airline for an airline
        public static SubsidiaryAirline CreateSubsidiaryAirline(Airline airline, double money, string name, string iata, Airline.AirlineMentality mentality, Airline.AirlineFocus market, Route.RouteType routefocus, Airport homebase)
        {
            AirlineProfile profile = new AirlineProfile(name, iata, airline.Profile.Color, airline.Profile.CEO, true, GameObject.GetInstance().GameTime.Year, 2199);
            profile.Country = airline.Profile.Country;

            SubsidiaryAirline sAirline = new SubsidiaryAirline(airline, profile, mentality, market, airline.License, routefocus);

            AddSubsidiaryAirline(airline, sAirline, money, homebase);

            return sAirline;
        }
        //hires the pilots for a specific airliner
        public static void HireAirlinerPilots(FleetAirliner airliner)
        {
            if (Pilots.GetNumberOfUnassignedPilots() < 10)
                GeneralHelpers.CreatePilots(50);

            while (airliner.Airliner.Type.CockpitCrew > airliner.NumberOfPilots)
            {
                var pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country == airliner.Airliner.Airline.Profile.Country);

                if (pilots.Count == 0)
                    pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country.Region == airliner.Airliner.Airline.Profile.Country.Region);

                if (pilots.Count == 0)
                    pilots = Pilots.GetUnassignedPilots();

                Pilot pilot = pilots.OrderByDescending(p => p.Rating).First();

                if (pilot != null)
                {
                    airliner.Airliner.Airline.addPilot(pilot);

                    pilot.Airliner = airliner;
                    airliner.addPilot(pilot);
                }
                else
                    GeneralHelpers.CreatePilots(50);
            }



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
        //returns if an airline can create a hub at an airport
        public static Boolean CanCreateHub(Airline airline, Airport airport, HubType type)
        {
            Boolean airlineHub = airport.getHubs().Exists(h => h.Airline == airline);

            int airlineValue = (int)airline.getAirlineValue() + 1;

            int totalAirlineHubs = airline.getHubs().Count;// 'Airports.GetAllActiveAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));
            double airlineGatesPercent = Convert.ToDouble(airport.Terminals.getNumberOfGates(airline)) / Convert.ToDouble(airport.Terminals.getNumberOfGates()) * 100;

            switch (type.Type)
            {
                case HubType.TypeOfHub.Focus_city:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type);
                case HubType.TypeOfHub.Regional_hub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) && (airport.Profile.Size == GeneralHelpers.Size.Large || airport.Profile.Size == GeneralHelpers.Size.Medium) && airport.getHubs().Count < 7;
                case HubType.TypeOfHub.Fortress_hub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) && (airport.Profile.Size > GeneralHelpers.Size.Medium) && airlineGatesPercent > 70 && (totalAirlineHubs < airlineValue);
                case HubType.TypeOfHub.Hub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) && (!airlineHub) && (airlineGatesPercent > 20) && (totalAirlineHubs < airlineValue) && (airport.getHubs(HubType.TypeOfHub.Hub).Count < (int)airport.Profile.Size);
            }

            return false;


        }

        //update the damage scores for an airline
        public static void UpdateMaintList(Airline airline)
        {
            foreach (FleetAirliner a in airline.Fleet)
            {
                airline.Scores.Maintenance.Add((int)a.Airliner.Condition);
            }

            if (airline.Scores.Maintenance.Count > (airline.Fleet.Count * 2))
            {
                airline.Scores.Maintenance.RemoveRange(0, airline.Fleet.Count);
            }
        }

        //updates the airlines ratings for an airline
        public static void UpdateRatings(Airline airline)
        {
            airline.Ratings.SafetyRating = (int)airline.Scores.Safety.Average();
            airline.Ratings.SecurityRating = (int)airline.Scores.Security.Average();
            airline.Ratings.CustomerHappinessRating = (int)airline.Scores.CHR.Average();
            airline.Ratings.EmployeeHappinessRating = (int)airline.Scores.EHR.Average();
            airline.Ratings.MaintenanceRating = (int)airline.Scores.Maintenance.Average();
        }
        //returns the max loan sum for an airline
        public static double GetMaxLoanAmount(Airline airline)
        {
            return airline.getValue() * 500000;
        }
        //returns if an airline can apply for a loan
        public static Boolean CanApplyForLoan(Airline airline, Loan loan)
        {
            double loans = airline.Loans.Sum(l => l.Amount);

            return loan.Amount + loans < GetMaxLoanAmount(airline);
        }
        //returns if an airline has licens for flying between two airports
        public static Boolean HasAirlineLicens(Airline airline, Airport airport1, Airport airport2)
        {
            Continent continent1 = Continents.GetContinent(airport1.Profile.Country.Region);
            Continent continent2 = Continents.GetContinent(airport2.Profile.Country.Region);
            Continent continentAirline = Continents.GetContinent(airline.Profile.Country.Region);

            Boolean continentsOk = true;//< continent1 == continentAirline || continent2 == continentAirline;
            Boolean isInUnion = Unions.GetUnions(airport1.Profile.Country, GameObject.GetInstance().GameTime).Intersect(Unions.GetUnions(airport2.Profile.Country, GameObject.GetInstance().GameTime)).Any();

            if (airline.License == Airline.AirlineLicense.Long_Haul && continentsOk)
                return true;

            if (airline.License == Airline.AirlineLicense.Short_Haul && (MathHelpers.GetDistance(airport1, airport2) < 2000 || isInUnion) && continentsOk)
                return true;

            if (airline.License == Airline.AirlineLicense.Domestic && airport1.Profile.Country == airport2.Profile.Country && continentsOk)
                return true;

            if (airline.License == Airline.AirlineLicense.Regional && (continent1 == continent2 || isInUnion) && continentsOk)
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

        //checks for any insurance claims up for settlement
        public static void CheckInsuranceSettlements(Airline airline)
        {
            foreach (InsuranceClaim claim in airline.InsuranceClaims)
            {
                if (claim.SettlementDate <= GameObject.GetInstance().GameTime)
                {
                    AirlineInsuranceHelpers.ReceiveInsurancePayout(airline, claim.Policy, claim);
                }
            }
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
            while (airlineFrom.Pilots.Count > 0)
            {
                Pilot pilot = airlineFrom.Pilots[0];
                airlineFrom.removePilot(pilot);

                pilot.Airline = airlineTo;
                airlineTo.addPilot(pilot);
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
        private enum RouteOkStatus { Ok, Missing_Cargo, Wrong_Distance, Appropriate_Type, Restrictions };
        //returns if a route can be created
        public static Boolean IsRouteDestinationsOk(Airline airline, Airport destination1, Airport destination2, Route.RouteType routeType, Airport stopover1 = null, Airport stopover2 = null)
        {
            var distances = new List<double>();
         
            Boolean stopoverOk = (stopover1 == null || routeType == Route.RouteType.Cargo ? true : AirportHelpers.HasFreeGates(stopover1, airline)) && (stopover2 == null || routeType == Route.RouteType.Cargo ? true : AirportHelpers.HasFreeGates(stopover2, airline));
          
            if ((AirportHelpers.HasFreeGates(destination1, airline) && AirportHelpers.HasFreeGates(destination2, airline) && stopoverOk) || routeType == Route.RouteType.Cargo)
            {
                RouteOkStatus routeOkStatus = RouteOkStatus.Ok;

                if (stopover1 == null && stopover2 == null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, destination2));
                    routeOkStatus = GetRouteStatus(destination1, destination2, routeType);
                }

                if (stopover1 == null && stopover2 != null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover2));
                    distances.Add(MathHelpers.GetDistance(stopover2, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover2, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover2, destination2, routeType);

                }

                if (stopover1 != null && stopover2 == null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover1));
                    distances.Add(MathHelpers.GetDistance(stopover1, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover1, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover1, destination2, routeType);
                }

                if (stopover1 != null && stopover2 != null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover1));
                    distances.Add(MathHelpers.GetDistance(stopover1, stopover2));
                    distances.Add(MathHelpers.GetDistance(stopover2, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover1, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover1, stopover2, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover2, destination2, routeType);
                }

                double maxDistance = distances.Max();
                double minDistance = distances.Min();

                var query = from a in AirlinerTypes.GetTypes(delegate(AirlinerType t) { return t.Produced.From < GameObject.GetInstance().GameTime; })
                            select a.Range;

                double maxFlightDistance = query.Max();

                if (minDistance <= 50 || maxDistance > maxFlightDistance)
                    routeOkStatus = RouteOkStatus.Wrong_Distance;

                if (routeOkStatus == RouteOkStatus.Ok)
                    return true;
                else
                {
                    
                    if (routeOkStatus == RouteOkStatus.Appropriate_Type)
                        throw new Exception("3002");
                    else if (routeOkStatus == RouteOkStatus.Wrong_Distance)
                        throw new Exception("3001");
                    else if (routeOkStatus == RouteOkStatus.Missing_Cargo)
                        throw new Exception("3003");
                    else if (routeOkStatus == RouteOkStatus.Restrictions)
                        throw new Exception("3004");

                    throw new Exception("3000");

                }
         }
            else
                throw new Exception("3000");

        }
        //returns if two airports can have route between them and if the airline has license for the route
        private static Boolean CheckRouteOk(Airport airport1, Airport airport2, Route.RouteType routeType)
        {
            Boolean isCargoRouteOk = true;
            if (routeType == Route.RouteType.Cargo)
                isCargoRouteOk = AIHelpers.IsCargoRouteDestinationsCorrect(airport1, airport2, GameObject.GetInstance().HumanAirline);

            return isCargoRouteOk && AirlineHelpers.HasAirlineLicens(GameObject.GetInstance().HumanAirline, airport1, airport2) && AIHelpers.IsRouteInCorrectArea(airport1, airport2) && !FlightRestrictions.HasRestriction(airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airport2.Profile.Country, airport1.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline, airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime);
        }
        private static RouteOkStatus GetRouteStatus(Airport airport1, Airport airport2, Route.RouteType routeType)
        {
            RouteOkStatus status = RouteOkStatus.Ok;

            if (routeType == Route.RouteType.Cargo)
                if (!AIHelpers.IsCargoRouteDestinationsCorrect(airport1, airport2, GameObject.GetInstance().HumanAirline))
                    status = RouteOkStatus.Missing_Cargo;

            if (status == RouteOkStatus.Ok)
            {
                if (AirlineHelpers.HasAirlineLicens(GameObject.GetInstance().HumanAirline, airport1, airport2) && AIHelpers.IsRouteInCorrectArea(airport1, airport2))
                    if (!FlightRestrictions.HasRestriction(airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airport2.Profile.Country, airport1.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline, airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime))
                        status = RouteOkStatus.Ok;
                    else
                        status = RouteOkStatus.Restrictions;
                else
                    status = RouteOkStatus.Appropriate_Type;
            }

            return status;

        }
    }
    //airline insurance helpers
    public class AirlineInsuranceHelpers
    {
        //add insurance policy
        public static AirlineInsurance CreatePolicy(Airline airline, AirlineInsurance.InsuranceType type, AirlineInsurance.InsuranceScope scope, AirlineInsurance.PaymentTerms terms, bool allAirliners, int length, int amount)
        {
            #region Method Setup
            Random rnd = new Random();
            double modifier = GetRatingModifier(airline);
            double hub = airline.getHubs().Count() * 0.1;
            AirlineInsurance policy = new AirlineInsurance(type, scope, terms, amount);
            policy.InsuranceEffective = GameObject.GetInstance().GameTime;
            policy.InsuranceExpires = GameObject.GetInstance().GameTime.AddYears(length);
            policy.PolicyIndex = GameObject.GetInstance().GameTime.ToString() + airline.ToString();
            policy.TermLength = length;
            switch (policy.InsTerms)
            {
                case AirlineInsurance.PaymentTerms.Monthly:
                    policy.RemainingPayments = length * 12;
                    break;
                case AirlineInsurance.PaymentTerms.Quarterly:
                    policy.RemainingPayments = length * 4;
                    break;
                case AirlineInsurance.PaymentTerms.Biannual:
                    policy.RemainingPayments = length * 2;
                    break;
                case AirlineInsurance.PaymentTerms.Annual:
                    policy.RemainingPayments = length;
                    break;
            }
            //sets up multipliers based on the type and scope of insurance policy
            Dictionary<AirlineInsurance.InsuranceType, Double> typeMultipliers = new Dictionary<AirlineInsurance.InsuranceType, double>();
            Dictionary<AirlineInsurance.InsuranceScope, Double> scopeMultipliers = new Dictionary<AirlineInsurance.InsuranceScope, double>();
            double typeMPublic = modifier;
            double typeMPassenger = modifier + 0.2;
            double typeMCSL = modifier + 0.5;
            double typeMFull = modifier + 1;

            double scMAirport = modifier;
            double scMDomestic = modifier + 0.2;
            double scMHub = modifier + hub + 0.5;
            double scMGlobal = modifier + hub + 1;
            #endregion
            #region Domestic/Int'l Airport Counter
            int i = 0; int j = 0;
            foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports)
            {
                if (airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                {
                    i++;
                }
                else j++;
            }
            #endregion
            // all the decision making for monthly payment amounts and deductibles
            #region Public Liability
            switch (type)
            {
                case AirlineInsurance.InsuranceType.Public_Liability:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;

                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;

            #endregion
                #region Passenger Liability

                case AirlineInsurance.InsuranceType.Passenger_Liability:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Combined Single Limit
                case AirlineInsurance.InsuranceType.Combined_Single_Limit:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Full Coverage
                case AirlineInsurance.InsuranceType.Full_Coverage:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                #endregion
                    break;
            }

            if (allAirliners == true)
            {
                amount *= airline.Fleet.Count();
                policy.PaymentAmount *= (airline.Fleet.Count() * 0.95);
            }
            return policy;
        }



        //gets insurance rate modifiers based on security, safety, and aircraft state of maintenance
        public static double GetRatingModifier(Airline airline)
        {
            double mod = 1;
            mod += (100 - airline.Ratings.MaintenanceRating) / 100;
            mod += (100 - airline.Ratings.SafetyRating) / 150;
            mod += (100 - airline.Ratings.SecurityRating) / 100;
            return mod;
        }


        //extend or modify policy
        public static void ModifyPolicy(Airline airline, string index, AirlineInsurance newPolicy)
        {
            //AirlineInsurance oldPolicy = airline.InsurancePolicies[index];
            //use the index to compare the new policy passed in to the existing one and make changes
        }

        public static void CheckExpiredInsurance(Airline airline)
        {
            DateTime date = GameObject.GetInstance().GameTime;
            foreach (AirlineInsurance policy in airline.InsurancePolicies)
            {
                if (policy.InsuranceExpires < date)
                {
                    airline.removeInsurance(policy);
                }
            }
        }

        public static void MakeInsurancePayment(Airline airline)
        {
            foreach (AirlineInsurance policy in airline.InsurancePolicies)
            {
                if (policy.RemainingPayments > 0)
                {
                    if (policy.NextPaymentDue.Month == GameObject.GetInstance().GameTime.Month)
                    {
                        airline.Money -= policy.PaymentAmount;
                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, policy.PaymentAmount);
                        policy.RemainingPayments--;
                        switch (policy.InsTerms)
                        {
                            case AirlineInsurance.PaymentTerms.Monthly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(1);
                                break;
                            case AirlineInsurance.PaymentTerms.Quarterly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(3);
                                break;
                            case AirlineInsurance.PaymentTerms.Biannual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(6);
                                break;
                            case AirlineInsurance.PaymentTerms.Annual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(12);
                                break;
                        }
                    }
                }

            }
        }


        //for general damage or claims
        public static void FileInsuranceClaim(Airline airline, AirlineInsurance policy, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, null, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);

        }

        //for damage and claims involving an airport or airport facility
        public static void FileInsuranceClaim(Airline airline, Airport airport, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, null, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }


        //for damage and claims involving an airliner
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, airliner, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }

        //for damage and claims involving an airliner and airport or airport facility
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, Airport airport, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, airliner, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }


        public static void ReceiveInsurancePayout(Airline airline, AirlineInsurance policy, InsuranceClaim claim)
        {
            if (claim.Damage > policy.Deductible)
            {
                claim.Damage -= (int)policy.Deductible;
                airline.Money -= policy.Deductible;
                policy.Deductible = 0;
                policy.InsuredAmount -= claim.Damage;
                airline.Money += claim.Damage;
                News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Payout", "You have received an insurance payout in the amount of $" + claim.Damage.ToString() + ". This was for claim number " + claim.Index);
            }

            else if (claim.Damage < policy.Deductible)
            {
                policy.Deductible -= claim.Damage;
                //Warnings.AddWarning("Low Damage", "The damage incurred was less than your deductible, so you will not receive an insurance payout for this claim! \n Reference: " + claim.Index);
            }
        }
    }
}
