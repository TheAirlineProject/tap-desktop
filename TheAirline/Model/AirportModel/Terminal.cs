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
    // chs, 2011-27-10 changed so a terminal has a devilery date
    public class Terminal
    {
        public string Name { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Airline Airline { get; set; }
        public Airport Airport { get; set; }
        public Gates Gates { get; set; }
        public Boolean IsBuilt { get{return isBuilt();} set{;} }
        public Boolean IsBuyable { get { return isBuyable(); } set { ;} }
        public Terminal(Airport airport, Airline airline,string name, int gates, DateTime deliveryDate)
        {
            this.Airport = airport;
            this.Airline = airline;
            this.Name = name;
            this.DeliveryDate = new DateTime(deliveryDate.Year, deliveryDate.Month, deliveryDate.Day);

            this.Gates = new Gates(airport, gates, this.DeliveryDate);
      
            if (this.Airline != null)
            {
                if (this.Airport.Terminals.getTotalNumberOfGates(airline) == 0)
                    airline.addAirport(this.Airport);

                for (int i = 0; i < this.Gates.getGates().Count; i++)
                    this.Gates.getGates()[i].Airline = airline;

               
            }
      
        }
        // chs 11-10-11: changed for the possibility of purchasing an existing terminal
        //returns if the terminal is buyalbe
        private Boolean isBuyable()
        {
            return this.Gates.getFreeGates() == this.Gates.NumberOfDeliveredGates && this.Airport.Terminals.getNumberOfAirportTerminals()>1;
        }
        //purchases a terminal for an airline
        public void purchaseTerminal(Airline airline)
        {
            this.Airline = airline;
            foreach (Gate gate in this.Gates.getGates())
            {
                gate.Airline = airline;
            }
            if (!airline.Airports.Contains(this.Airport))
                airline.Airports.Add(this.Airport);
            // chs 11-10-11: changed so old gates (from terminals owned by the airport) are moved into the new terminal
            //moves the "old" rented gates into the new terminal
            foreach (Terminal tTerminal in this.Airport.Terminals.getTerminals().FindAll((delegate(Terminal t) { return t.Airline == null; })))
            {
                foreach (Gate gate in tTerminal.Gates.getGates(this.Airline))
                {
                    Gate nGate = this.Gates.getEmptyGate(this.Airline);
                    if (nGate != null)
                    {
                        nGate.HasRoute = gate.HasRoute;

                        gate.Airline = null;
                        gate.HasRoute = false;
                    }


                }

            }
        }
        // chs 11-04-11: changed for the possibility of extending a terminal
        //extends a terminal with a number of gates
        public void extendTerminal(int gates)
        {
            DateTime deliveryDate = GameObject.GetInstance().GameTime.AddDays(gates * 10);
            for (int i = 0; i < gates; i++)
            {
                Gate gate = new Gate(this.Airport, deliveryDate);
                gate.Airline = this.Airline;
                this.Gates.addGate(gate);

                
            }
        }
        //returns if the terminal has been built
        private Boolean isBuilt()
        {
            return GameObject.GetInstance().GameTime > this.DeliveryDate;
        }
    }
    //the collection of terminals at an airport
    public class Terminals
    {
        public Airport Airport { get; set; }
        public List<Terminal> AirportTerminals;
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
        // chs 11-10-11: changed for the possibility of purchasing an existing terminal
        //returns the number of terminals owned by the airport
        public int getNumberOfAirportTerminals()
        {
            return getDeliveredTerminals().FindAll((delegate(Terminal terminal) { return terminal.Airline == null; })).Count;
        }
        //returns all delivered terminals
        public List<Terminal> getDeliveredTerminals()
        {
         
            return this.AirportTerminals.FindAll((delegate(Terminal terminal) { return terminal.DeliveryDate <= GameObject.GetInstance().GameTime; }));
         
           
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
           
     
        }
        //removes a terminal from the list
        public void removeTerminal(Terminal terminal)
        {
            while (terminal.Gates.getFreeGates(terminal.Airline) > 0)
                terminal.Gates.releaseGate(terminal.Airline);
                 
            this.AirportTerminals.Remove(terminal);
            
        }
        //rents a gate for an airline returns if it successed
        public Boolean rentGate(Airline airline)
        {
            if (getTotalNumberOfGates(airline) == 0)
                airline.addAirport(this.Airport);

            try
            {
                getFreeGate().Airline = airline;

                return true;
            }
            catch
            {
                if (getTotalNumberOfGates(airline) == 0)
                    airline.removeAirport(this.Airport);

                return false;
            }

        }
        //releases a gate for an airline
        public void releaseGate(Airline airline)
        {
            Gate gate = getEmptyGate(airline);
            gate.Airline = null;

            if (getTotalNumberOfGates(airline) == 0)
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
        //returns all used gates 
        public List<Gate> getUsedGates()
        {
            return this.getDeliveredTerminals().SelectMany(t => t.Gates.getUsedGates()).ToList();
        }
        //returns all used gates for an airline
        public List<Gate> getUsedGates(Airline airline)
        {
            return this.getDeliveredTerminals().SelectMany(t => t.Gates.getUsedGates(airline)).ToList();
        }
        //returns a used gate for an airline
        public Gate getUsedGate(Airline airline)
        {
            foreach (Terminal terminal in getDeliveredTerminals())
                if (terminal.Gates.getUsedGate(airline) != null)
                    return terminal.Gates.getUsedGate(airline);
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
        //returns the total number of gates for an airline
        public int getTotalNumberOfGates(Airline airline)
        {
            int number = 0;
            foreach (Terminal terminal in this.AirportTerminals)
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
        //returns the number of gates with route 
        public int getNumberOfRoutes()
        {
            return this.AirportTerminals.SelectMany(a => a.Gates.getGates()).Where(g=>g.HasRoute).Count();
        }
       
        //switches all gates from one airline to another
        public void switchAirline(Airline airlineFrom, Airline airlineTo)
        {
            while (getNumberOfGates(airlineFrom) > 0)
            {
                Gate gate = getGate(airlineFrom);
                gate.Airline = airlineTo;
            }
            airlineFrom.removeAirport(this.Airport);
            airlineTo.addAirport(this.Airport);
      
        }
       
        /*
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
        */
        //clears the gates
        public void clear()
        {
            foreach (Terminal terminal in this.AirportTerminals)
                terminal.Gates.clear();

            this.AirportTerminals = new List<Terminal>();

        }
        //returns if an airline has terminal
        public Boolean hasTerminal(Airline airline)
        {
            foreach (Terminal terminal in this.AirportTerminals)
            {
                if (terminal.Airline == airline)
                    return true;
            }
            return false;
        }
       
    }
}
