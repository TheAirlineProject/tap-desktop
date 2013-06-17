using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirportModel
{
    //the class for a runway at an airport
    [ProtoContract]
    public class Runway
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public long Length { get; set; }

        public enum SurfaceType { Asphalt, Concrete, Grass, Dirt, Gravel, Ice, Salt, Paved, Unpaved }
        [ProtoMember(3)]
        public SurfaceType Surface { get; set; }
        [ProtoMember(4)]
        public DateTime BuiltDate { get; set; }
        [ProtoMember(5)]
        public Boolean Standard { get; set; }
        public Runway(string name, long length, SurfaceType surface, DateTime builtDate, Boolean standard)
        {
            this.Name = name;
            this.Length = length;
            this.Surface = surface;
            this.BuiltDate = builtDate;
            this.Standard = standard;
        }
    }
}
