using System.Collections.Generic;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    //the start data for an airline
    public class AirlineStartData
    {
        #region Constructors and Destructors

        public AirlineStartData(Airline airline)
        {
            Airline = airline;
            Routes = new List<StartDataRoute>();
            Airliners = new List<StartDataAirliners>();
            OriginRoutes = new List<StartDataRoutes>();
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public List<StartDataAirliners> Airliners { get; private set; }

        public List<StartDataRoutes> OriginRoutes { get; private set; }

        public List<StartDataRoute> Routes { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void AddAirliners(StartDataAirliners airliners)
        {
            Airliners.Add(airliners);
        }

        //adds origin routes to the list
        public void AddOriginRoutes(StartDataRoutes route)
        {
            OriginRoutes.Add(route);
        }

        //adds a route to the list
        public void AddRoute(StartDataRoute route)
        {
            Routes.Add(route);
        }

        #endregion

        //adds an airliners to the list
    }

    //the airliners for the start data
    public class StartDataAirliners
    {
        #region Constructors and Destructors

        public StartDataAirliners(string type, int airlinersEarly, int airlinersLate)
        {
            Type = type;
            AirlinersEarly = airlinersEarly;
            AirlinersLate = airlinersLate;
        }

        #endregion

        #region Public Properties

        public int AirlinersEarly { get; set; }

        public int AirlinersLate { get; set; }

        public string Type { get; set; }

        #endregion
    }

    //the routes for the start data
    public class StartDataRoutes
    {
        #region Constructors and Destructors

        public StartDataRoutes(
            string origin,
            int destinations,
            GeneralHelpers.Size minimumsize,
            Route.RouteType routetype)
        {
            Countries = new List<Country>();
            Origin = origin;
            Destinations = destinations;
            MinimumSize = minimumsize;
            RouteType = routetype;
        }

        #endregion

        #region Public Properties

        public List<Country> Countries { get; set; }

        public int Destinations { get; set; }

        public GeneralHelpers.Size MinimumSize { get; set; }

        public string Origin { get; set; }

        public Route.RouteType RouteType { get; set; }

        #endregion

        #region Public Methods and Operators

        public void AddCountry(Country country)
        {
            Countries.Add(country);
        }

        #endregion

        //adds a country to the routes
    }

    //the route for the start data
    public class StartDataRoute
    {
        #region Constructors and Destructors

        public StartDataRoute(
            string destination1,
            string destination2,
            int opened,
            int closed,
            Route.RouteType routetype)
        {
            Opened = opened;
            Closed = closed;
            Destination1 = destination1;
            Destination2 = destination2;
            RouteType = routetype;
        }

        #endregion

        #region Public Properties

        public int Closed { get; set; }

        public string Destination1 { get; set; }

        public string Destination2 { get; set; }

        public int Opened { get; set; }

        public Route.RouteType RouteType { get; set; }

        public AirlinerType Type { get; set; }

        #endregion
    }

    //the list of start data
    public class AirlineStartDatas
    {
        #region Static Fields

        private static readonly List<AirlineStartData> StartData = new List<AirlineStartData>();

        #endregion

        #region Public Methods and Operators

        public static void AddStartData(AirlineStartData data)
        {
            StartData.Add(data);
        }

        //returns the start data for an airline
        public static AirlineStartData GetAirlineStartData(Airline airline)
        {
            return StartData.Find(s => s.Airline == airline);
        }

        #endregion

        //adds start data to the list
    }
}