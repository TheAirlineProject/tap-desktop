using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.AirportModel
{
    //the class for a gate at an airport
    public class Gate
    {
        public Airport Airport { get; set; }
        public Airline Airline { get; set; }
        public Route Route { get; set; }
        public Gate(Airport airport)
        {
            this.Airport = airport;
            this.Route = null;
        }

    }
    //the collection of gates at an airport
    public class Gates
    {
        private List<Gate> gates;
        public Airport Airport { get; set; }
        public int NumberOfGates { get { return this.gates.Count; } set { ;} }
        public Gates(Airport airport, int numberOfGates)
        {
            this.Airport = airport;
            gates = new List<Gate>();
            for (int i = 0; i < numberOfGates; i++)
                gates.Add(new Gate(this.Airport));
        }
        //returns the list of gates
        public List<Gate> getGates()
        {
            return gates;
        }
        //returns all gates for an airline
        public List<Gate> getGates(Airline airline)
        {
            return gates.FindAll((delegate(Gate gate) { return gate.Airline == airline; }));
        }
        //rents a gate for an airline
        public void rentGate(Airline airline)
        {
            if (this.Airport.Terminals.getTotalNumberOfGates(airline) == 0)
                airline.addAirport(this.Airport);
            getFreeGate().Airline = airline;
          
        }
         
        //releases a gate for an airline
        public void releaseGate(Airline airline)
        {
            
            getEmptyGate(airline).Airline = null;

            if (this.Airport.Terminals.getTotalNumberOfGates(airline) == 0)
                airline.removeAirport(this.Airport);
        }
        //returns a empty gate for an airline
        public Gate getEmptyGate(Airline airline)
        {
            foreach (Gate gate in this.gates)
                if (gate.Airline == airline && gate.Route == null)
                    return gate;

            return null;
        }

        //returns a used gate for an airline
        public Gate getUsedGate(Airline airline)
        {
            foreach (Gate gate in this.gates)
                if (gate.Airline == airline && gate.Route != null)
                    return gate;

            return null;
        }
        //returns a gate for an airline
        public Gate getGate(Airline airline)
        {
            foreach (Gate gate in this.gates)
                if (gate.Airline == airline)
                    return gate;
            return null;
        }
        //returns a free gate
        public Gate getFreeGate()
        {
            foreach (Gate gate in this.gates)
                if (gate.Airline == null)
                    return gate;
            return null;
        }
        
        //returns the total number of free gates
        public int getFreeGates()
        {
            return gates.FindAll((delegate(Gate gate) { return gate.Airline==null; })).Count;
        }
        //returns the number of gates for an airline 
        public int getNumberOfGates(Airline airline)
        {
            int number = 0;
            foreach (Gate gate in this.gates)
            {
                if (gate.Airline != null && gate.Airline == airline)
                    number++;
            }
            return number;
        }
      
        //returns the number of free gates for an airline (without a route)
        public int getFreeGates(Airline airline)
        {
            int number = 0;
            foreach (Gate gate in this.gates)
            {
                if (gate.Airline != null && gate.Airline == airline && gate.Route == null)
                    number++;
            }
            return number;
        }
        //finds the routes assigned to the gates
        public List<Route> getRoutes()
        {
            List<Route> routes = new List<Route>();
            foreach (Gate gate in gates)
            {
                if (gate.Route != null)
                    routes.Add(gate.Route);
         
            }
            return routes;
        }
        public List<Route> getRoutes(Airline airline)
        {
            List<Route> routes = new List<Route>();
            foreach (Gate gate in gates)
            {
                if (gate.Route != null && gate.Airline == airline)
                    routes.Add(gate.Route);

            }
            return routes;
        }
        //clears the gates
        public void clear()
        {
            this.gates = new List<Gate>();
        }
        //adds a gate
        public void addGate(Gate gate)
        {
            if (gate.Airline != null && !gate.Airline.Airports.Contains(this.Airport))
                gate.Airline.addAirport(this.Airport);

            this.gates.Add(gate);

            
        }
    }
}
