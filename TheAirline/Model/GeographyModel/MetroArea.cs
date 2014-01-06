using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeographyModel;

namespace TheAirline.Model.GeographyModel
{
    public class MetroArea
    {
        public List<Airport> Airports { get; set; }
        public List<Town> Towns { get; set; }
        public Town MainTown { get; set; }
        public Airport MainAirport { get; set; }
        public Country Country { get; set; }
        public int Population { get; set; }

        public MetroArea(List<Airport> airports, List<Town> towns, Airport mainAirport)
        {
            this.Airports = airports;
            this.Towns = towns;
            this.MainAirport = mainAirport;
            this.Population = this.Towns.Sum(_ => _.Population);
            this.Country = this.MainAirport.Profile.Country;
            this.MainTown = this.Towns.Aggregate((a1,a2) => a1.Population > a2.Population ? a1 : a2);
        }

        public void addTown(Town t)
        {
            this.Towns.Add(t);
        }

        public void removeTown(Town t)
        {
            this.Towns.Remove(t);
        }

        public void addAirport(Airport a)
        {
            this.Airports.Add(a);
        }

        public void removeAirport(Airport a)
        {
            this.Airports.Remove(a);
        }

        public Town calculateMainTown()
        {
            this.MainTown = this.Towns.Aggregate((a1, a2) => a1.Population > a2.Population ? a1 : a2);
            return this.MainTown;
        }

    }

    public class MetroAreas
    {
        private List<MetroArea> Areas { get; set; }

        public List<MetroArea> getAllAreas()
        {
            return this.Areas;
        }

        public void add(MetroArea m)
        {
            this.Areas.Add(m);
        }

        public void remove(MetroArea m)
        {
            this.Areas.Remove(m);
        }
        
    }
}
