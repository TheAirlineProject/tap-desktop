
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirportModel
{
    // chs, 2011-27-10 added for the possibility of purchasing a terminal
    /*!
     * Class for a terminal at an airport
     * Constructor needs parameter for airport, owner (airline), date of delivery and number of gates
     **/
    // chs, 2011-27-10 changed so a terminal has a devilery date
    [Serializable]
    public class Terminal
    {
        
        public string Name { get; set; }
        
        public DateTime DeliveryDate { get; set; }
        
        public Airline Airline { get; set; }
        public Airport Airport { get; set; }
        
        public Gates Gates { get; set; }
        public Boolean IsBuilt { get{return isBuilt();} set{;} }
        public Boolean IsBuyable { get { return isBuyable(); } set { ;} }
        public Terminal(Airport airport, string name, int gates, DateTime deliveryDate)
            : this(airport, null, name, gates, deliveryDate)
        {
        }
        public Terminal(Airport airport, Airline airline,string name, int gates, DateTime deliveryDate)
        {
            this.Airport = airport;
            this.Airline = airline;
            this.Name = name;
            this.DeliveryDate = new DateTime(deliveryDate.Year, deliveryDate.Month, deliveryDate.Day);
        
            this.Gates = new Gates(gates, this.DeliveryDate,airline);
        }
        // chs 11-10-11: changed for the possibility of purchasing an existing terminal
        //returns if the terminal is buyalbe
        private Boolean isBuyable()
        {
            int freeGates = this.Airport.Terminals.getFreeGates();

            return freeGates > this.Gates.NumberOfGates && this.Airport.Terminals.getNumberOfAirportTerminals()>1 && this.Airline == null;
        }
        //purchases a terminal for an airline
        public void purchaseTerminal(Airline airline)
        {
            this.Airline = airline;

            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(this.Airport,this.Gates.NumberOfGates,20);

            this.Airport.addAirlineContract(new AirportContract(this.Airline,this.Airport,GameObject.GetInstance().GameTime,this.Gates.NumberOfGates,20,yearlyPayment * 0.75));
           
        }
        // chs 11-04-11: changed for the possibility of extending a terminal
        //extends a terminal with a number of gates
        public void extendTerminal(int gates)
        {
            DateTime deliveryDate = GameObject.GetInstance().GameTime.AddDays(gates * 10);
            for (int i = 0; i < gates; i++)
            {
                Gate gate = new Gate(deliveryDate);
                gate.Airline = this.Airline;

                this.Gates.addGate(gate);
            }
        }
        //returns if the terminal has been built
        private Boolean isBuilt()
        {
            return GameObject.GetInstance().GameTime > this.DeliveryDate;
        }
        //returns the number of free gates
        public int getFreeGates()
        {
            //4 fordelt på 24 gates og 5 gates - burde være 20/24 og 5/5
            if (this.Airline != null)
                return this.Gates.NumberOfGates;

            int terminalIndex = this.Airport.Terminals.AirportTerminals.Where(a=>a.Airline==null).ToList().IndexOf(this);

            int terminalGates = this.Airport.Terminals.AirportTerminals.Where(a => a.Airline != null).Sum(t => t.Gates.NumberOfGates);
                        
            int contracts = this.Airport.AirlineContracts.Sum(c => c.NumberOfGates) - terminalGates;
            
            int gates = 0;

            int i = 0;
            while (gates < contracts)
            {
                gates += this.Airport.Terminals.AirportTerminals.Where(a => a.Airline == null).ToList()[i].Gates.NumberOfGates;

                if (gates < contracts)
                    i++;
            }
            if (terminalIndex > i || contracts == 0)
                return this.Gates.NumberOfGates;

            if (terminalIndex < i)
                return 0;

            if (terminalIndex == i)
                return gates - contracts;

            return this.Gates.NumberOfGates;
          
        }
    }
    //the collection of terminals at an airport
    [Serializable]
    public class Terminals
    {
        public Airport Airport { get; set; }
        public int NumberOfGates { get { return getNumberOfGates(); } private set { ;} }
        public int NumberOfFreeGates { get { return getFreeGates(); } private set { ; } }
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
        //returns the number of gates in order
        public int getOrdereredGates()
        {
            return this.AirportTerminals.Sum(a => a.Gates.NumberOfOrderedGates) + this.AirportTerminals.FindAll(a=>a.DeliveryDate>GameObject.GetInstance().GameTime).Sum(t=>t.Gates.NumberOfGates);
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
        public List<Gate> getGates(Airline airline)
        {
            List<Gate> gates = new List<Gate>();

            foreach (Terminal terminal in getDeliveredTerminals())
                foreach (Gate gate in terminal.Gates.getGates().Where(a=>a.Airline != null && a.Airline == airline))
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
                  
            this.AirportTerminals.Remove(terminal);
            
        }
       
        //returns the percent of gates which are in use
        public double getInusePercent()
        {
            int freeGates = getFreeGates();
            int totalGates = getNumberOfGates();

            int usedGates = totalGates - freeGates;

            if (usedGates > 0)
                freeGates = 12;

            double inusePercent = Convert.ToDouble(usedGates) / Convert.ToDouble(totalGates) * 100.0;

            return inusePercent;
        }
        //returns the number of gates in use
        public int getInuseGates()
        {
            return this.Airport.AirlineContracts.Where(c=>c.ContractDate<= GameObject.GetInstance().GameTime).Sum(c => c.NumberOfGates); 
        }
        //returns the number of free gates
        public int getFreeGates()
        {
            return getNumberOfGates() - getInuseGates();
        }
        //returns the total number of gates
        public int getNumberOfGates()
        {
            return this.AirportTerminals.Sum(t=>t.Gates.NumberOfDeliveredGates);
        }
        //returns the number of gates for an airline
        public int getNumberOfGates(Airline airline)
        {
            return this.Airport.AirlineContracts.Where(c => c.Airline == airline && c.ContractDate <= GameObject.GetInstance().GameTime).Sum(c=>c.NumberOfGates);
        }
        //returns the percent of free gate slots for the airport
        public double getFreeSlotsPercent(Airline airline)
        {
            double numberOfSlots = (22 - 6) * 4 * 7; //from 06.00 to 22.00 each quarter each day (7 days a week) 

            double usedSlots = AirportHelpers.GetOccupiedSlotTimes(this.Airport, airline).Count;

            double percent = ((numberOfSlots - usedSlots) / numberOfSlots) * 100;

            return percent;

        }
        /*
        //returns the number of free gates for an airport
        public int getNumberOfFreeGates(Airline airline)
        {
            var contracts = this.Airport.getAirlineContracts(airline).Where(c => c.ContractDate <= GameObject.GetInstance().GameTime);

            if (contracts.Count() == 0)
                return 0;

            int gates = contracts.Sum(c => c.NumberOfGates);

            return gates -  (AirportHelpers.GetAirportRoutes(this.Airport, airline).Count / Gate.RoutesPerGate);
        }*/
        //switches from one airline to another
        public void switchAirline(Airline airlineFrom, Airline airlineTo)
        {
            List<AirportContract> contracts = this.Airport.getAirlineContracts(airlineFrom);
            
            foreach (AirportContract contractFrom in contracts)
            {
                contractFrom.Airline = airlineTo;

                for (int i = 0; i < contractFrom.NumberOfGates; i++)
                {
                    Gate gate = contractFrom.Airport.Terminals.getGates().Where(g => g.Airline == airlineFrom).First();
                    gate.Airline = airlineTo;
                }
            }
          
            airlineFrom.removeAirport(this.Airport);

            if (!airlineTo.Airports.Contains(this.Airport))
                airlineTo.addAirport(this.Airport);
      
        }
       
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
