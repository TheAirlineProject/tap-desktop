
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirportModel
{
    //the class for a runway at an airport
    [Serializable]
    public class Runway
    {
        
        public string Name { get; set; }
        
        public long Length { get; set; }

        public enum SurfaceType { Asphalt, Concrete, Grass, Dirt, Gravel, Ice, Salt, Paved, Unpaved }
        
        public SurfaceType Surface { get; set; }
        
        public DateTime BuiltDate { get; set; }
        
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
