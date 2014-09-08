namespace TheAirline.Model.PassengerModel
{
    using System.Collections.Generic;

    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel.Helpers;

    //the route finder for the passengers
    public class PassengerRouteFinder
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="nodes">All airports</param>
        public PassengerRouteFinder(List<Airport> nodes)
        {
            this.Nodes = nodes;
            this.Basis = new List<Airport>();
            this.Dist = new Dictionary<string, double>();
            this.Previous = new Dictionary<string, Airport>();

            foreach (Airport n in this.Nodes)
            {
                this.Previous.Add(n.Profile.IATACode, null);
                this.Basis.Add(n);
                this.Dist.Add(n.Profile.IATACode, double.MaxValue);
            }
        }

        #endregion

        #region Public Properties

        public List<Airport> Basis { get; set; }

        public Dictionary<string, double> Dist { get; set; }

        public List<Airport> Nodes { get; set; }

        public Dictionary<string, Airport> Previous { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Calculates the distance from
        ///     the start airport to all other airports
        /// </summary>
        /// <param name="start">Startknoten</param>
        public void calculateDistance(Airport start)
        {
            this.Dist[start.Profile.IATACode] = 0;

            while (this.Basis.Count > 0)
            {
                Airport u = this.getNodeWithSmallestDistance();
                if (u == null)
                {
                    this.Basis.Clear();
                }
                else
                {
                    foreach (Airport v in this.getNeighbors(u))
                    {
                        double alt = this.Dist[u.Profile.IATACode] + this.getDistanceBetween(u, v);
                        if (alt < this.Dist[v.Profile.IATACode])
                        {
                            this.Dist[v.Profile.IATACode] = alt;
                            this.Previous[v.Profile.IATACode] = u;
                        }
                    }
                    this.Basis.Remove(u);
                }
            }
        }

        /// <summary>
        ///     Returns the airport with the smallest distance
        /// </summary>
        /// <returns></returns>
        public Airport getNodeWithSmallestDistance()
        {
            double distance = double.MaxValue;
            Airport smallest = null;

            foreach (Airport n in this.Basis)
            {
                if (this.Dist[n.Profile.IATACode] < distance)
                {
                    distance = this.Dist[n.Profile.IATACode];
                    smallest = n;
                }
            }

            return smallest;
        }

        /// <summary>
        ///     Gets the path to an airport
        /// </summary>
        /// <param name="d">
        ///     Destination
        ///     <n/ param>
        ///         <returns></returns>
        public List<Airport> getPathTo(Airport d)
        {
            var path = new List<Airport>();

            path.Insert(0, d);

            while ((this.Previous.Count != 0) && (this.Previous[d.Profile.IATACode] != null))
            {
                d = this.Previous[d.Profile.IATACode];
                path.Insert(0, d);
            }

            return path;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Returns the distance between two airports
        /// </summary>
        /// <param name="o">Start airport</param>
        /// <param name="d">Destination airport</param>
        /// <returns></returns>
        private double getDistanceBetween(Airport o, Airport d)
        {
            if (AirportHelpers.GetAirportRoutes(o).Find(r => r.Destination1 == d || r.Destination2 == d) != null)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        ///     Returns all airports with a route from the an airport
        /// </summary>
        /// <param name="n">Airport</param>
        /// <returns></returns>
        private List<Airport> getNeighbors(Airport n)
        {
            var neighbors = new List<Airport>();

            foreach (Route route in AirportHelpers.GetAirportRoutes(n))
            {
                Airport destination = route.Destination1 == n ? route.Destination2 : route.Destination1;
                if (this.Basis.Contains(n))
                {
                    neighbors.Add(destination);
                }
            }

            return neighbors;
        }

        #endregion
    }
}