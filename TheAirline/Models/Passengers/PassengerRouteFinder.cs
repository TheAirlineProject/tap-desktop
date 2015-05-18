using System.Collections.Generic;
using System.Linq;
using TheAirline.Helpers;
using TheAirline.Models.Airports;

namespace TheAirline.Models.Passengers
{
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
            Nodes = nodes;
            Basis = new List<Airport>();
            Dist = new Dictionary<string, double>();
            Previous = new Dictionary<string, Airport>();

            foreach (Airport n in Nodes)
            {
                Previous.Add(n.Profile.IATACode, null);
                Basis.Add(n);
                Dist.Add(n.Profile.IATACode, double.MaxValue);
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
        public void CalculateDistance(Airport start)
        {
            Dist[start.Profile.IATACode] = 0;

            while (Basis.Count > 0)
            {
                Airport u = GetNodeWithSmallestDistance();
                if (u == null)
                {
                    Basis.Clear();
                }
                else
                {
                    foreach (Airport v in GetNeighbors(u))
                    {
                        double alt = Dist[u.Profile.IATACode] + getDistanceBetween(u, v);
                        if (alt < Dist[v.Profile.IATACode])
                        {
                            Dist[v.Profile.IATACode] = alt;
                            Previous[v.Profile.IATACode] = u;
                        }
                    }
                    Basis.Remove(u);
                }
            }
        }

        /// <summary>
        ///     Returns the airport with the smallest distance
        /// </summary>
        /// <returns></returns>
        public Airport GetNodeWithSmallestDistance()
        {
            double distance = double.MaxValue;
            Airport smallest = null;

            foreach (Airport n in Basis)
            {
                if (Dist[n.Profile.IATACode] < distance)
                {
                    distance = Dist[n.Profile.IATACode];
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
        /// </param>
        /// <returns></returns>
        public List<Airport> GetPathTo(Airport d)
        {
            var path = new List<Airport>();

            path.Insert(0, d);

            while ((Previous.Count != 0) && (Previous[d.Profile.IATACode] != null))
            {
                d = Previous[d.Profile.IATACode];
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
        private IEnumerable<Airport> GetNeighbors(Airport n)
        {
            return AirportHelpers.GetAirportRoutes(n).Select(route => route.Destination1 == n ? route.Destination2 : route.Destination1).Where(destination => Basis.Contains(n)).ToList();
        }

        #endregion
    }
}