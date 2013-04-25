using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    //some static values for an airport
    public class AirportStatics
    {
        private Dictionary<Airport, double> AirportDistances;
        public Airport Airport { get; set; }
        public AirportStatics(Airport airport)
        {
            this.AirportDistances = new Dictionary<Airport, double>();
            this.Airport = airport;
        }
        //adds a distance to the class
        public void addDistance(Airport airport, double distance)
        {
            lock (this.AirportDistances)
            {
                if (!this.AirportDistances.ContainsKey(airport))
                    this.AirportDistances.Add(airport, distance);
            }
        }
        //returns the distance for an airport
        public double getDistance(Airport airport)
        {
            double distance;
            lock (this.AirportDistances)
            {
                if (this.AirportDistances.ContainsKey(airport))
                    distance = this.AirportDistances[airport];
                else
                    distance =0;
            }

            return distance;
        }
        //returns all airports within a range
        public List<Airport> getAirportsWithin(double range)
        {
            List<Airport> airports;
            lock (this.AirportDistances)
            {
                if (this.AirportDistances.Count == 0)
                {
                    foreach (Airport airport in Airports.GetAllAirports())
                        addDistance(airport, MathHelpers.GetDistance(this.Airport, airport));
                }
                airports = new List<Airport>(from a in this.AirportDistances where a.Value <= range select a.Key);
            }

            return airports;
        }
    }
}
