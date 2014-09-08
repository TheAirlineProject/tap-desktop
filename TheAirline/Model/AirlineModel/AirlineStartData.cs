namespace TheAirline.Model.AirlineModel
{
    using System.Collections.Generic;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

    //the start data for an airline
    public class AirlineStartData
    {
        #region Constructors and Destructors

        public AirlineStartData(Airline airline)
        {
            this.Airline = airline;
            this.Routes = new List<StartDataRoute>();
            this.Airliners = new List<StartDataAirliners>();
            this.OriginRoutes = new List<StartDataRoutes>();
        }

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public List<StartDataAirliners> Airliners { get; private set; }

        public List<StartDataRoutes> OriginRoutes { get; private set; }

        public List<StartDataRoute> Routes { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void addAirliners(StartDataAirliners airliners)
        {
            this.Airliners.Add(airliners);
        }

        //adds origin routes to the list
        public void addOriginRoutes(StartDataRoutes route)
        {
            this.OriginRoutes.Add(route);
        }

        //adds a route to the list
        public void addRoute(StartDataRoute route)
        {
            this.Routes.Add(route);
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
            this.Type = type;
            this.AirlinersEarly = airlinersEarly;
            this.AirlinersLate = airlinersLate;
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
            this.Countries = new List<Country>();
            this.Origin = origin;
            this.Destinations = destinations;
            this.MinimumSize = minimumsize;
            this.RouteType = routetype;
        }

        #endregion

        #region Public Properties

        public List<Country> Countries { get; set; }

        public int Destinations { get; set; }

        public GeneralHelpers.Size MinimumSize { get; set; }

        public string Origin { get; set; }

        public Route.RouteType RouteType { get; set; }

        #endregion

        //adds a country to the routes

        #region Public Methods and Operators

        public void addCountry(Country country)
        {
            this.Countries.Add(country);
        }

        #endregion
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
            this.Opened = opened;
            this.Closed = closed;
            this.Destination1 = destination1;
            this.Destination2 = destination2;
            this.RouteType = routetype;
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

        private static readonly List<AirlineStartData> startData = new List<AirlineStartData>();

        #endregion

        //adds start data to the list

        #region Public Methods and Operators

        public static void AddStartData(AirlineStartData data)
        {
            startData.Add(data);
        }

        //returns the start data for an airline
        public static AirlineStartData GetAirlineStartData(Airline airline)
        {
            return startData.Find(s => s.Airline == airline);
        }

        #endregion
    }
}