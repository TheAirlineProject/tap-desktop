using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.AirportModel
{
    // chs 11-04-11: changed for the possibility of extending a terminal
    //the class for a gate at an airport
    public class Gate
    {
        public Airport Airport { get; set; }
        public Airline Airline { get; set; }
        public Route Route { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Gate(Airport airport, DateTime deliveryDate)
        {
            this.Airport = airport;
            this.Route = null;
            this.DeliveryDate = deliveryDate;
        }

    }
    // chs 11-04-11: changed for the possibility of extending a terminal
   //the collection of gates at an airport
    public class Gates
    {
        private List<Gate> TerminalGates;
        public Airport Airport { get; set; }
       
        public int NumberOfGates { get { return this.TerminalGates.Count; } set { ;} }
        public int NumberOfOrderedGates { get { return this.getOrderedGates().Count;} set { ;} }
        public int NumberOfDeliveredGates { get { return this.NumberOfGates - this.NumberOfOrderedGates; } set { ;} }
        public int NumberOfUsedGates { get { return this.NumberOfGates - getFreeGates(); } set { ;} }
        public Gates(Airport airport, int numberOfGates, DateTime deliveryDate)
        {
            this.Airport = airport;
            this.TerminalGates = new List<Gate>();
            for (int i = 0; i < numberOfGates; i++)
                this.TerminalGates.Add(new Gate(this.Airport,deliveryDate));
        }
        //returns all delivered gats
        public List<Gate> getDeliveredGates()
        {
            return this.TerminalGates.FindAll((delegate(Gate gate) { return gate.DeliveryDate <= GameObject.GetInstance().GameTime; }));
       
        }
        // chs 11-07-11: changed for the possibility of extending a terminal
        //returns the ordered gates
        private List<Gate> getOrderedGates()
        {
            return this.TerminalGates.FindAll((delegate(Gate gate) { return gate.DeliveryDate > GameObject.GetInstance().GameTime; }));
        }
        //returns the list of gates
        public List<Gate> getGates()
        {
            return getDeliveredGates();
        }
        //returns all gates for an airline
        public List<Gate> getGates(Airline airline)
        {
            return getDeliveredGates().FindAll((delegate(Gate gate) { return gate.Airline == airline; }));
        }
        //rents a gate for an airline
        public void rentGate(Airline airline)
        {
            if (this.Airport.Terminals.getTotalNumberOfGates(airline) == 0 && !this.Airport.Terminals.hasTerminal(airline))
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
            foreach (Gate gate in getDeliveredGates())
                if (gate.Airline == airline && gate.Route == null)
                    return gate;

            return null;
        }
        //returns all used gates
        public List<Gate> getUsedGates()
        {
            return getDeliveredGates().FindAll(g => g.Airline != null);
        }
        //returns all gates for an airline
        public List<Gate> getUsedGates(Airline airline)
        {
            return getDeliveredGates().FindAll(g => g.Airline == airline);
        }
        //returns a used gate for an airline
        public Gate getUsedGate(Airline airline)
        {
            foreach (Gate gate in getDeliveredGates())
                if (gate.Airline == airline && gate.Route != null)
                    return gate;

            return null;
        }
        //returns a gate for an airline
        public Gate getGate(Airline airline)
        {
            foreach (Gate gate in getDeliveredGates())
                if (gate.Airline == airline)
                    return gate;
            return null;
        }
        //returns a free gate
        public Gate getFreeGate()
        {
            foreach (Gate gate in getDeliveredGates())
                if (gate.Airline == null)
                    return gate;
            return null;
        }
      
        //returns the number of gates for an airline 
        public int getNumberOfGates(Airline airline)
        {
            int number = 0;
            foreach (Gate gate in getDeliveredGates())
            {
                if (gate.Airline != null && gate.Airline == airline)
                    number++;
            }
            return number;
        }

        //returns the total number of free gates
        public int getFreeGates()
        {
            return getDeliveredGates().FindAll((delegate(Gate gate) { return gate.Airline == null; })).Count;
        }
        //returns the number of free gates for an airline (without a route)
        public int getFreeGates(Airline airline)
        {
            int number = 0;
            foreach (Gate gate in getDeliveredGates())
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
            foreach (Gate gate in getDeliveredGates())
            {
                if (gate.Route != null)
                    routes.Add(gate.Route);
         
            }
            return routes;
        }
        public List<Route> getRoutes(Airline airline)
        {
            List<Route> routes = new List<Route>();
            foreach (Gate gate in getDeliveredGates())
            {
                if (gate.Route != null && gate.Airline == airline)
                    routes.Add(gate.Route);

            }
            return routes;
        }
        //clears the gates
        public void clear()
        {
            this.TerminalGates = new List<Gate>();
        }
        //adds a gate
        public void addGate(Gate gate)
        {
            if (gate.Airline != null && !gate.Airline.Airports.Contains(this.Airport))
                gate.Airline.addAirport(this.Airport);

            this.TerminalGates.Add(gate);

            
        }
    }
}
