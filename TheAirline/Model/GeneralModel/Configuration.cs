using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a configuration
    public abstract class Configuration
    {
        public enum ConfigurationType { Airliner, Route }
        public ConfigurationType Type { get; set; }
        public string Name { get; set; }
        public Configuration(ConfigurationType type, string name)
        {
            this.Name = name;
            this.Type = type;
        }
    }
    //the collection of configurations
    public class Configurations
    {
        private static List<Configuration> configurations = new List<Configuration>();
        //adds a configuration to the list
        public static void AddConfiguration(Configuration configuration)
        {
            if (configurations.Find(c => c.Name == configuration.Name) != null)
                configurations.RemoveAll(c => c.Name == configuration.Name);

            configurations.Add(configuration);
        }
        //returns the list of configuraitons
        public static List<Configuration> GetConfigurations()
        {
            return configurations;
        }
        //returns the list of configurations for a specific type
        public static List<Configuration> GetConfigurations(Configuration.ConfigurationType type)
        {
            return configurations.FindAll(c => c.Type == type);
        }
        //returns the list of configurations
        public static List<Configuration> GetConfigurations(Predicate<Configuration> match)
        {
            return configurations.FindAll(match);
        }
        //clears the list of configurations
        public static void Clear()
        {
            configurations = new List<Configuration>();
        }

    }
}
