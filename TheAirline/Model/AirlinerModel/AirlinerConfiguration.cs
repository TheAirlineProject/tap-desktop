using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlinerModel
{
    //the configuration of an airliner 
    public class AirlinerConfiguration
    {
        public string Name { get; set; }
        public List<AirlinerClassConfiguration> Classes { get; set; }
        public AirlinerConfiguration(string name)
        {
            this.Name = name;
        }
        //returns the number of classes
        public int getNumberOfClasses()
        {
            return this.Classes.Count;
        }

    }
    //the configuration of an airliner class
    public class AirlinerClassConfiguration
    {
        public AirlinerClass.ClassType Type { get; set; }
        public int Seating { get; set; }
        public List<AirlinerFacility> Facilities { get; set; }
        public AirlinerClassConfiguration(AirlinerClass.ClassType type, int seating)
        {
            this.Seating = seating;
            this.Type = type;
            this.Facilities = new List<AirlinerFacility>();
        }
        //adds a facility to the configuration
        public void addFacility(AirlinerFacility facility)
        {
            this.Facilities.Add(facility);
        }
        //returns all facilities
        public List<AirlinerFacility> getFacilities()
        {
            return this.Facilities;
        }
        //returns the facility of a specific type
        public AirlinerFacility getFacility(AirlinerFacility.FacilityType type)
        {
            return this.Facilities.Find(f => f.Type == type);
        }
    
    }
}
