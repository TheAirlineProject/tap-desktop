namespace TheAirline.Model.AirportModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.PassengerModel;

    //some static values for an airport
    public class AirportStatics
    {
        #region Fields

        private readonly Dictionary<Airport, double> AirportDistances;

        private readonly List<DestinationDemand> CargoDemand;

        private readonly List<DestinationDemand> PassengerDemand;

        #endregion

        #region Constructors and Destructors

        public AirportStatics(Airport airport)
        {
            this.AirportDistances = new Dictionary<Airport, double>();
            this.PassengerDemand = new List<DestinationDemand>();
            this.CargoDemand = new List<DestinationDemand>();
            this.Airport = airport;
            this.HasDemand = false;
        }

        #endregion

        #region Public Properties

        public Airport Airport { get; set; }
        public Boolean HasDemand { get; set; }

        #endregion

        //adds passenger demand to the airport

        //adds cargo demand to the airport

        #region Public Methods and Operators

        public void addCargoDemand(DestinationDemand demand)
        {
            lock (this.CargoDemand) this.CargoDemand.Add(demand);
        }

        //adds a distance to the airport
        public void addDistance(Airport airport, double distance)
        {
            lock (this.AirportDistances)
            {
                if (!this.AirportDistances.ContainsKey(airport))
                {
                    this.AirportDistances.Add(airport, distance);
                }
            }
        }
        public Boolean hasDistance(Airport airport)
        {
            return this.AirportDistances.ContainsKey(airport);
        }
        public void addPassengerDemand(DestinationDemand demand)
        {
            lock (this.PassengerDemand) this.PassengerDemand.Add(demand);
        }

        public List<Airport> getAirportsWithin(double range)
        {
            List<Airport> airports;
            lock (this.AirportDistances)
            {
                if (this.AirportDistances.Count == 0)
                {
                    foreach (Airport airport in Airports.GetAllAirports())
                    {
                        this.addDistance(airport, MathHelpers.GetDistance(this.Airport, airport));
                    }
                }
                airports = new List<Airport>(from a in this.AirportDistances where a.Value <= range select a.Key);
            }

            return airports;
        }

        public List<DestinationDemand> getDemands()
        {
            var demands = new List<DestinationDemand>();

            demands.AddRange(this.CargoDemand);
            demands.AddRange(this.PassengerDemand);

            return demands;
        }
        public List<DestinationDemand> getPassengersDemand()
        {
            return this.PassengerDemand;
        }
        //returns if the airport have passenger demand to another airport

        //returns the destination cargo for a specific destination
        public ushort getDestinationCargoRate(Airport destination)
        {
            DestinationDemand cargo = this.CargoDemand.Find(a => a.Destination == destination.Profile.IATACode);

            if (cargo == null)
            {
                return 0;
            }
            return cargo.Rate;
        }

        //returns the destination passengers for a specific destination for a class
        public ushort getDestinationPassengersRate(Airport destination, AirlinerClass.ClassType type)
        {
            DestinationDemand pax = this.PassengerDemand.Find(a => a.Destination == destination.Profile.IATACode);

            Array values = Enum.GetValues(typeof(AirlinerClass.ClassType));

            int classFactor = 0;

            int i = 1;

            foreach (AirlinerClass.ClassType value in values)
            {
                if (value == type)
                {
                    classFactor = i;
                }
                i++;
            }

            if (pax == null)
            {
                return 0;
            }
            return (ushort)(pax.Rate / classFactor);
        }

        public int getDestinationPassengersSum()
        {
            int sum = 0;
            lock (this.PassengerDemand)
            {
                sum = this.PassengerDemand.Sum(p => p.Rate);
            }
            return sum;
        }

        //returns all demands

        //returns the distance for an airport
        public double getDistance(Airport airport)
        {
            double distance;
            lock (this.AirportDistances)
            {
                if (this.AirportDistances.ContainsKey(airport))
                {
                    distance = this.AirportDistances[airport];
                }
                else
                {
                    distance = 0;
                }
            }

            return distance;
        }

        public Boolean hasDestinationCargoRate(Airport destination)
        {
            return this.CargoDemand.Exists(a => a.Destination == destination.Profile.IATACode);
        }

        public Boolean hasDestinationPassengersRate(Airport destination)
        {
            return this.PassengerDemand.Exists(a => a.Destination == destination.Profile.IATACode);
        }

        #endregion

        //returns all airports within a range
    }
}