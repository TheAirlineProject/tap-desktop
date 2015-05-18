using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;

namespace TheAirline.Models.Airports
{
    // chs 11-04-11: changed for the possibility of extending a terminal
    //the class for a gate at an airport   
    [Serializable]
    public class Gate : BaseModel
    {
        #region Constructors and Destructors

        public Gate(DateTime deliveryDate)
        {
            DeliveryDate = deliveryDate;
        }

        private Gate(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airlines.Airline Airline { get; set; }

        [Versioning("date")]
        public DateTime DeliveryDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //public Boolean HasRoute { get; set; }
        //public const int RoutesPerGate = 5;
    }

    // chs 11-04-11: changed for the possibility of extending a terminal
    //the collection of gates at an airport
    [Serializable]
    public class Gates : BaseModel
    {
        #region Fields

        [Versioning("gates")] private List<Gate> _terminalGates;

        #endregion

        #region Constructors and Destructors

        public Gates(int numberOfGates, DateTime deliveryDate, Airlines.Airline airline)
        {
            _terminalGates = new List<Gate>();
            for (int i = 0; i < numberOfGates; i++)
            {
                var gate = new Gate(deliveryDate) {Airline = airline};

                _terminalGates.Add(gate);
            }
        }

        private Gates(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public int NumberOfDeliveredGates => NumberOfGates - NumberOfOrderedGates;

        public int NumberOfGates => _terminalGates.Count;

        public int NumberOfOrderedGates => GetOrderedGates().Count;

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddGate(Gate gate)
        {
            _terminalGates.Add(gate);
        }

        public void Clear()
        {
            _terminalGates = new List<Gate>();
        }

        //returns all delivered gats
        public List<Gate> GetDeliveredGates()
        {
            return
                _terminalGates.FindAll(
                    (gate => gate.DeliveryDate <= GameObject.GetInstance().GameTime));
        }

        // chs 11-07-11: changed for the possibility of extending a terminal
        //returns the ordered gates

        //returns the list of gates
        public List<Gate> GetGates()
        {
            return GetDeliveredGates();
        }

        #endregion

        #region Methods

        private List<Gate> GetOrderedGates()
        {
            return
                _terminalGates.FindAll(
                    (gate => gate.DeliveryDate > GameObject.GetInstance().GameTime));
        }

        #endregion

        //clears the gates
    }
}