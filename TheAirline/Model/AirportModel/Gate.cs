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
    [Serializable]
 
    public class Gate
    {
        //public Boolean HasRoute { get; set; }
        //public const int RoutesPerGate = 5;
        public Airline Airline { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Gate(DateTime deliveryDate)
        {
             this.DeliveryDate = deliveryDate;
          }

    }
    // chs 11-04-11: changed for the possibility of extending a terminal
   //the collection of gates at an airport
    [Serializable]
    public class Gates
    {
        
        private List<Gate> TerminalGates;
       
        public int NumberOfGates { get { return this.TerminalGates.Count; } set { ;} }
        public int NumberOfOrderedGates { get { return this.getOrderedGates().Count;} set { ;} }
        public int NumberOfDeliveredGates { get { return this.NumberOfGates - this.NumberOfOrderedGates; } set { ;} }
        public Gates(int numberOfGates, DateTime deliveryDate,Airline airline)
        {
            this.TerminalGates = new List<Gate>();
            for (int i = 0; i < numberOfGates; i++)
            {
                Gate gate = new Gate(deliveryDate);
             
                this.TerminalGates.Add(gate);
            }
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
        //clears the gates
        public void clear()
        {
            this.TerminalGates = new List<Gate>();
        }
        //adds a gate
        public void addGate(Gate gate)
        {
            this.TerminalGates.Add(gate);

        }
    }
}
