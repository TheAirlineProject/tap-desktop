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
        public int MinimumSeats { get; set; }
        public List<AirlinerClassConfiguration> Classes { get; set; }
        public AirlinerConfiguration(string name, int minimumSeats)
        {
            this.Name = name;
            this.MinimumSeats = minimumSeats;
            this.Classes = new List<AirlinerClassConfiguration>();
        }
        //returns the number of classes
        public int getNumberOfClasses()
        {
            return this.Classes.Count;
        }
        //adds an airliner class configuration to the configuration
        public void addClassConfiguration(AirlinerClassConfiguration conf)
        {
            this.Classes.Add(conf);
        }
        //returns the total number of seats needed for the configuration
     

    }
    //the configuration of an airliner class
    public class AirlinerClassConfiguration
    {
        public AirlinerClass.ClassType Type { get; set; }
        public int SeatingCapacity { get; set; }
        public int RegularSeatingCapacity { get; set; }
        public List<AirlinerFacility> Facilities { get; set; }
        public AirlinerClassConfiguration(AirlinerClass.ClassType type, int seating, int regularseating)
        {
            this.SeatingCapacity = seating;
            this.RegularSeatingCapacity = regularseating;
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
    //the list of configurations
    public class AirlinerConfigurations
    {
        private static List<AirlinerConfiguration> configurations = new List<AirlinerConfiguration>();
        //adds a configuration to the list
        public static void AddConfiguration(AirlinerConfiguration configuration)
        {
            if (configurations.Find(c => c.Name == configuration.Name) != null)
                configurations.RemoveAll(c => c.Name == configuration.Name);

            configurations.Add(configuration);
        }
        //returns the list of configuraitons
        public static List<AirlinerConfiguration> GetConfigurations()
        {
            return configurations;
        }
        //returns the list of configurations
        public static List<AirlinerConfiguration> GetConfigurations(Predicate<AirlinerConfiguration> match)
        {
            return configurations.FindAll(match);
        }
        //clears the list of configurations
        public static void Clear()
        {
            configurations = new List<AirlinerConfiguration>();
        }

    }
}
