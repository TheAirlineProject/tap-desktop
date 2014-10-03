using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    //the class for an expansion of an airport
    [Serializable]
    public class AirportExpansion : BaseModel
    {
        public enum ExpansionType
        {
            NewTerminal,
            NewRunway,
            RunwayLength,
            Name,
            ExtraGates,
            CloseTerminal
        }

        public AirportExpansion(ExpansionType type, DateTime date, Boolean notifyOnChange)
        {
            Type = type;
            Date = date;
            NotifyOnChange = notifyOnChange;
        }

        private AirportExpansion(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        [Versioning("type")]
        public ExpansionType Type { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("notify")]
        public Boolean NotifyOnChange { get; set; }

        //for type==name && type==new_terminal (name) && type == new_runway (name) && extra_gates (name) && close_terminal (name)
        [Versioning("name")]
        public string Name { get; set; }

        //for type==new_terminal && type == extra_gates
        [Versioning("gates")]
        public int Gates { get; set; }

        //for type==new_runway
        [Versioning("length")]
        public long Length { get; set; }

        [Versioning("surface")]
        public Runway.SurfaceType Surface { get; set; }

        [Versioning("terminaltype")]
        public Terminal.TerminalType TerminalType { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }
    }
}