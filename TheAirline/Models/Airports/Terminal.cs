using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Environment;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airports
{
    // chs, 2011-27-10 added for the possibility of purchasing a terminal
    /*!
     * Class for a terminal at an airport
     * Constructor needs parameter for airport, owner (airline), date of delivery and number of gates
     **/
    // chs, 2011-27-10 changed so a terminal has a devilery date
    [Serializable]
    public class Terminal : BaseModel
    {
        #region Constructors and Destructors

        public Terminal(Airport airport, string name, int gates, DateTime deliveryDate, TerminalType type)
            : this(airport, null, name, gates, deliveryDate, type)
        {
        }

        public Terminal(Airport airport, Airline airline, string name, int gates, DateTime deliveryDate, TerminalType type)
        {
            Airport = airport;
            Airline = airline;
            Name = name;
            DeliveryDate = new DateTime(deliveryDate.Year, deliveryDate.Month, deliveryDate.Day);
            Type = type;

            Gates = new Gates(gates, DeliveryDate, airline);
        }

        private Terminal(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                Type = TerminalType.Passenger;
        }

        #endregion

        #region Public Properties

        public enum TerminalType
        {
            Cargo,
            Passenger
        }

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("delivery")]
        public DateTime DeliveryDate { get; set; }

        [Versioning("gates")]
        public Gates Gates { get; set; }

        [Versioning("type", Version = 2)]
        public TerminalType Type { get; set; }

        public bool IsBuilt => GameObject.GetInstance().GameTime > DeliveryDate;

        public bool IsBuyable
        {
            get
            {
                int freeGates = Airport.Terminals.GetFreeGates(Type);

                return freeGates > Gates.NumberOfGates && Airport.Terminals.GetNumberOfAirportTerminals(Type) > 1
                       && Airline == null;
            }
        }

        [Versioning("name")]
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        // chs 11-04-11: changed for the possibility of extending a terminal
        //extends a terminal with a number of gates
        public void ExtendTerminal(int gates)
        {
            DateTime deliveryDate = GameObject.GetInstance().GameTime.AddDays(gates*10);
            for (int i = 0; i < gates; i++)
            {
                var gate = new Gate(deliveryDate) {Airline = Airline};

                Gates.AddGate(gate);
            }
        }

        //returns if the terminal has been built

        //returns the number of free gates
        public int GetFreeGates()
        {
            if (Airline != null)
            {
                return Gates.NumberOfGates;
            }
            int terminalIndex =
                Airport.Terminals.AirportTerminals.Where(
                    a => a.Airline == null && a.Type == Type && a.DeliveryDate <= GameObject.GetInstance().GameTime)
                       .ToList()
                       .IndexOf(this);

            int terminalGates =
                Airport.Terminals.AirportTerminals.Where(
                    a => a.Airline != null && a.Type == Type && a.DeliveryDate <= GameObject.GetInstance().GameTime)
                       .Sum(t => t.Gates.NumberOfGates);

            int contracts = Airport.AirlineContracts.Where(c => c.TerminalType == Type).Sum(c => c.NumberOfGates) - terminalGates;

            int gates = 0;

            int i = 0;
            while (gates < contracts)
            {
                gates +=
                    Airport.Terminals.AirportTerminals.Where(
                        a => a.Airline == null && a.Type == Type && a.DeliveryDate <= GameObject.GetInstance().GameTime).ToList()[i].Gates
                                                                                                                                    .NumberOfGates;

                if (gates < contracts)
                {
                    i++;
                }
            }

            if (terminalIndex > i || contracts == 0)
            {
                return Gates.NumberOfGates;
            }

            if (terminalIndex < i)
            {
                return 0;
            }

            if (terminalIndex == i)
            {
                return gates - contracts;
            }

            return Gates.NumberOfGates;
        }

        public void PurchaseTerminal(Airline airline)
        {
            Airline = airline;

            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(
                Airport,
                AirportContract.ContractType.Full,
                Gates.NumberOfGates,
                20);

            AirportHelpers.AddAirlineContract(
                new AirportContract(
                    Airline,
                    Airport,
                    AirportContract.ContractType.Full,
                    Type,
                    GameObject.GetInstance().GameTime,
                    Gates.NumberOfGates,
                    20,
                    yearlyPayment*0.60,
                    true));
        }

        #endregion
    }

    //the collection of terminals at an airport
    [Serializable]
    public class Terminals : BaseModel
    {
        #region Fields

        [Versioning("terminals")] public List<Terminal> AirportTerminals;

        #endregion

        #region Constructors and Destructors

        public Terminals(Airport airport)
        {
            Airport = airport;
            AirportTerminals = new List<Terminal>();
        }

        private Terminals(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airport")]
        public Airport Airport { get; set; }

        public int NumberOfFreeGates => GetFreeGates();

        public int NumberOfGates => GetNumberOfGates();

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddTerminal(Terminal terminal)
        {
            AirportTerminals.Add(terminal);
        }

        public void Clear()
        {
            foreach (Terminal terminal in AirportTerminals)
            {
                terminal.Gates.Clear();
            }

            AirportTerminals = new List<Terminal>();
        }

        //returns all terminals

        //returns all delivered terminals
        public List<Terminal> GetDeliveredTerminals()
        {
            return
                AirportTerminals.FindAll(
                    (terminal => terminal.DeliveryDate <= GameObject.GetInstance().GameTime));
        }

        public int GetFreeGates()
        {
            return GetFreeGates(Terminal.TerminalType.Cargo) + GetFreeGates(Terminal.TerminalType.Passenger);
        }

        public int GetFreeGates(Terminal.TerminalType type)
        {
            return GetNumberOfGates(type) - GetInuseGates(type);
        }

        public double GetFreeSlotsPercent(Airline airline, Terminal.TerminalType type)
        {
            const double numberOfSlots = (22 - 6)*4*7; //from 06.00 to 22.00 each quarter each day (7 days a week) 

            double usedSlots = AirportHelpers.GetOccupiedSlotTimes(Airport, airline, Weather.Season.AllYear, type).Count;

            double percent = ((numberOfSlots - usedSlots)/numberOfSlots)*100;

            return percent;
        }

        //returns the list of gates
        public List<Gate> GetGates()
        {
            return GetDeliveredTerminals().SelectMany(terminal => terminal.Gates.GetGates()).ToList();
        }

        public List<Gate> GetGates(Airline airline)
        {
            return GetDeliveredTerminals().SelectMany(terminal => terminal.Gates.GetGates().Where(a => a.Airline != null && a.Airline == airline)).ToList();
        }

        //adds a terminal to the list

        //returns the number of gates in use
        public int GetInuseGates(Terminal.TerminalType type)
        {
            return
                Airport.AirlineContracts.Where(c => c.ContractDate <= GameObject.GetInstance().GameTime && c.TerminalType == type)
                       .Sum(c => c.NumberOfGates);
        }

        public double GetInusePercent(Terminal.TerminalType type)
        {
            int freeGates = GetFreeGates(type);
            int totalGates = GetNumberOfGates(type);

            int usedGates = totalGates - freeGates;

            if (usedGates > 0)
            {
/*
                freeGates = 12;
*/
            }

            double inusePercent = Convert.ToDouble(usedGates)/Convert.ToDouble(totalGates)*100.0;

            return inusePercent;
        }

        public int GetNumberOfAirportTerminals()
        {
            return
                GetDeliveredTerminals()
                    .FindAll((terminal => terminal.Airline == null))
                    .Count;
        }

        public int GetNumberOfAirportTerminals(Terminal.TerminalType terminaltype)
        {
            return GetDeliveredTerminals().Count(t => t.Type == terminaltype);
        }

        //returns the number of free gates

        //returns the total number of gates
        public int GetNumberOfGates(Terminal.TerminalType type)
        {
            return AirportTerminals.Where(t => t.Type == type).Sum(t => t.Gates.NumberOfDeliveredGates);
        }

        public int GetNumberOfGates()
        {
            return GetNumberOfGates(Terminal.TerminalType.Passenger) + GetNumberOfGates(Terminal.TerminalType.Cargo);
        }

        //returns the number of gates for an airline
        public int GetNumberOfGates(Airline airline)
        {
            Terminal.TerminalType type = airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;
            return
                Airport.AirlineContracts.Where(
                    c => c.Airline == airline && c.TerminalType == type && c.ContractDate <= GameObject.GetInstance().GameTime)
                       .Sum(c => c.NumberOfGates);
        }

        public int GetOrdereredGates()
        {
            return AirportTerminals.Sum(a => a.Gates.NumberOfOrderedGates)
                   + AirportTerminals.FindAll(a => a.DeliveryDate > GameObject.GetInstance().GameTime)
                                     .Sum(t => t.Gates.NumberOfGates);
        }

        public List<Terminal> GetTerminals()
        {
            return AirportTerminals;
        }

        //returns the percent of free gate slots for the airport

        //returns if an airline has terminal
        public Boolean HasTerminal(Airline airline)
        {
            return AirportTerminals.Any(terminal => terminal.Airline == airline);
        }

        public void RemoveTerminal(Terminal terminal)
        {
            AirportTerminals.Remove(terminal);
        }

        public void SwitchAirline(Airline airlineFrom, Airline airlineTo)
        {
            List<AirportContract> contracts = Airport.GetAirlineContracts(airlineFrom);

            foreach (AirportContract contractFrom in contracts)
            {
                contractFrom.Airline = airlineTo;

                for (int i = 0; i < contractFrom.NumberOfGates; i++)
                {
                    Gate gate = contractFrom.Airport.Terminals.GetGates().First(g => g.Airline == airlineFrom);
                    gate.Airline = airlineTo;
                }
            }

            airlineFrom.RemoveAirport(Airport);

            if (!airlineTo.Airports.Contains(Airport))
            {
                airlineTo.AddAirport(Airport);
            }
        }

        #endregion
    }
}