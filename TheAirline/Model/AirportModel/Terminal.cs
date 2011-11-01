using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    // chs, 2011-27-10 added for the possibility of purchasing a terminal
    /*!
     * Class for a terminal at an airport
     * Constructor needs parameter for airport, owner (airline), date of delivery and number of gates
     **/
    public class Terminal
    {
        public DateTime DevileryDate { get; set; }
        public Airline Airline { get; set; }
        public Airport Airport { get; set; }
        public Gates Gates { get; set; }
        public Terminal(Airport airport, Airline airline, int gates, DateTime deliveryDate)
        {
            this.Airport = airport;
            this.Airline = airline;
            this.Gates = new Gates(airport, gates);
            this.DevileryDate = deliveryDate;
                  
        }
    }
    // chs, 2011-27-10 changed so a terminal has a devilery date
    //the collection of terminals at an airport
    public class Terminals
    {
        public Airport Airport { get; set; }
        private List<Terminal> AirportTerminals;
        public Terminals(Airport airport)
        {
            this.Airport = airport;
            this.AirportTerminals = new List<Terminal>();
        }
        //returns all terminals
        public List<Terminal> getTerminals()
        {
            return this.AirportTerminals;
        }
        //returns all delivered terminals
        public List<Terminal> getDeliveredTerminals()
        {
            return this.AirportTerminals.FindAll((delegate(Terminal terminal) { return terminal.DevileryDate <= GameObject.GetInstance().GameTime; }));
        }
        //returns the list of gates
        public List<Gate> getGates()
        {
            List<Gate> gates = new List<Gate>();

            foreach (Terminal terminal in getDeliveredTerminals())
                foreach (Gate gate in terminal.Gates.getGates())
                    gates.Add(gate);


            return gates;
        }
        //adds a terminal to the list
        public void addTerminal(Terminal terminal)
        {
            this.AirportTerminals.Add(terminal);
            if (terminal.Airline != null)
                for (int i = 0; i < terminal.Gates.getGates().Count; i++)
                    terminal.Gates.rentGate(terminal.Airline);
     
        }
        //removes a terminal from the list
        public void removeTerminal(Terminal terminal)
        {
            for (int i = 0; i < terminal.Gates.getGates().Count; i++)
                terminal.Gates.releaseGate(terminal.Airline);
            this.AirportTerminals.Remove(terminal);
            
        }
        //rents a gate for an airline
        public void rentGate(Airline airline)
        {
            if (getNumberOfGates(airline) == 0)
                airline.addAirport(this.Airport);
            getFreeGate().Airline = airline;

        }
        //releases a gate for an airline
        public void releaseGate(Airline airline)
        {

            getEmptyGate(airline).Airline = null;

            if (getNumberOfGates(airline) == 0)
                airline.removeAirport(this.Airport);
        }
        //returns a empty gate for an airline
        public Gate getEmptyGate(Airline airline)
        {
            foreach (Terminal terminal in getDeliveredTerminals())
                if (terminal.Gates.getEmptyGate(airline) != null)
                    return terminal.Gates.getEmptyGate(airline);

            return null;
        }

        //returns a used gate for an airline
        public Gate getUsedGate(Airline airline)
        {
            foreach (Terminal terminal in getDeliveredTerminals())
                if (getUsedGate(airline) != null)
                    return getUsedGate(airline);
            return null;
        }
        //returns a gate for an airline
        public Gate getGate(Airline airline)
        {
            foreach (Terminal terminal in getDeliveredTerminals())
                foreach (Gate gate in terminal.Gates.getGates())
                    if (gate.Airline == airline)
                        return gate;
            return null;
        }
        //returns a free gate
        public Gate getFreeGate()
        {
            foreach (Terminal terminal in getDeliveredTerminals())
                if (terminal.Gates.getFreeGate() != null)
                    return terminal.Gates.getFreeGate();
            return null;
        }

        //returns the total number of free gates
        public int getFreeGates()
        {
            int count = 0;
            foreach (Terminal terminal in getDeliveredTerminals())
                count += terminal.Gates.getFreeGates();

            return count;
        }
        //returns the number of gates for an airline 
        public int getNumberOfGates(Airline airline)
        {
            int number = 0;
            foreach (Terminal terminal in getDeliveredTerminals())
                number += terminal.Gates.getNumberOfGates(airline);
            return number;
        }
        //returns the total number of gates
        public int getNumberOfGates()
        {
            return getGates().Count;
        }
        //returns the number of free gates for an airline (without a route)
        public int getFreeGates(Airline airline)
        {
            int number = 0;
            foreach (Terminal terminal in getDeliveredTerminals())
                number += terminal.Gates.getFreeGates(airline);
            return number;
        }
        //finds the routes assigned to the gates
        public List<Route> getRoutes()
        {
            List<Route> routes = new List<Route>();
            foreach (Terminal terminal in getDeliveredTerminals())
                foreach (Route route in terminal.Gates.getRoutes())
                    routes.Add(route);
            return routes;
        }
        public List<Route> getRoutes(Airline airline)
        {
            List<Route> routes = new List<Route>();
            foreach (Terminal terminal in getDeliveredTerminals())
                foreach (Route route in terminal.Gates.getRoutes(airline))
                    routes.Add(route);

            return routes;
        }
        //clears the gates
        public void clear()
        {
            foreach (Terminal terminal in this.AirportTerminals)
                terminal.Gates.clear();


        }
        /*
        //adds a gate
        public void addGate(Gate gate)
        {
            if (gate.Airline != null && getNumberOfGates(gate.Airline) == 0)
                gate.Airline.addAirport(this.Airport);

            this.gates.Add(gate);


        }
         * */
    }
}
