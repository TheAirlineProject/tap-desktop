using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.RouteModel
{
    [Serializable]
    //the class for a passenger route
    public class PassengerRoute : Route
    {
        #region Constructors and Destructors

        public PassengerRoute(
            string id,
            Airport destination1,
            Airport destination2,
            DateTime startDate,
            double farePrice)
            : base(RouteType.Passenger, id, destination1, destination2, startDate)
        {
            Classes = new List<RouteAirlinerClass>();

            foreach (AirlinerClass.ClassType ctype in Enum.GetValues(typeof (AirlinerClass.ClassType)))
            {
                var cl = new RouteAirlinerClass(ctype, RouteAirlinerClass.SeatingType.ReservedSeating, farePrice);

                Classes.Add(cl);
            }
        }

        protected PassengerRoute(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("classes")]
        public List<RouteAirlinerClass> Classes { get; set; }

        public double IncomePerPassenger
        {
            get { return GetIncomePerPassenger(); }
        }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds a route airliner class to the route
        public void AddRouteAirlinerClass(RouteAirlinerClass aClass)
        {
            Classes.Add(aClass);
        }

        public double GetFarePrice(AirlinerClass.ClassType type)
        {
            return Classes.Find(c => c.Type == type).FarePrice;
        }

        public override double GetFillingDegree()
        {
            if (HasStopovers)
            {
                IEnumerable<Route> legs = Stopovers.SelectMany(s => s.Legs);
                Route[] enumerable = legs as Route[] ?? legs.ToArray();
                double fillingDegree = enumerable.Cast<PassengerRoute>().Sum(leg => leg.GetFillingDegree());
                return fillingDegree/enumerable.Count();
            }
            double passengers =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));

            double passengerCapacity =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

            return passengers/passengerCapacity;
        }

        //returns the route airliner class for a specific class type
        public RouteAirlinerClass GetRouteAirlinerClass(AirlinerClass.ClassType type)
        {
            return Classes.Find(cl => cl.Type == type);
        }

        public double GetServiceLevel(AirlinerClass.ClassType type)
        {
            return Classes.Find(c => c.Type == type).GetFacilities().Sum(f => f.ServiceLevel);
        }

        //returns the total number of cabin crew for the route based on airliner
        public int GetTotalCabinCrew()
        {
            int cabinCrew = 0;

            if (GetAirliners().Count > 0)
            {
                cabinCrew = GetAirliners().Sum(a => ((AirlinerPassengerType) a.Airliner.Type).CabinCrew);
            }

            return cabinCrew;
        }

        #endregion

        #region Methods

        private double GetIncomePerPassenger()
        {
            double totalPassengers =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));

            return Balance/totalPassengers;
        }

        #endregion

        //public FleetAirliner Airliner { get; set; }

        //get the degree of filling

        //gets the income per passenger

        //returns the service level for a specific class
    }

    [Serializable]
    //the class for a helicopter route
    public class HelicopterRoute : PassengerRoute
    {
        public HelicopterRoute(
            string id,
            Airport destination1,
            Airport destination2,
            DateTime startDate,
            double farePrice)
            : base(id, destination1, destination2, startDate, farePrice)
        {
            Type = RouteType.Helicopter;
        }

        protected HelicopterRoute(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}