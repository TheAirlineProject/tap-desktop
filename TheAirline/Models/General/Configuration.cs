using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Models;

namespace TheAirline.Infrastructure
{
    //the class for a configuration
    [Serializable]
    public abstract class Configuration : BaseModel
    {
        #region Constructors and Destructors

        protected Configuration(ConfigurationType type, string name, Boolean standard)
        {
            Name = name;
            Type = type;
            Standard = standard;
            ID = name;
        }

        protected Configuration(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum ConfigurationType
        {
            Airliner,

            Routeclasses,

            AirlinerType,

            AirlinerSchedule
        }

        #endregion

        #region Public Properties

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("standard")]
        public Boolean Standard { get; set; }

        [Versioning("type")]
        public ConfigurationType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the collection of configurations
    public class Configurations
    {
        #region Static Fields

        private static List<Configuration> configurations = new List<Configuration>();

        #endregion

        #region Public Methods and Operators

        public static void AddConfiguration(Configuration configuration)
        {
            if (configurations.Find(c => c.Name == configuration.Name && !c.Standard) != null)
            {
                configurations.RemoveAll(c => c.Name == configuration.Name);
            }

            configurations.Add(configuration);
        }

        public static void Clear()
        {
            configurations = new List<Configuration>();
        }

        //returns a standard configuration with a specific name / or

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

        public static Configuration GetStandardConfiguration(string name)
        {
            return configurations.Find(c => (c.Name == name || c.ID == name) && c.Standard);
        }

        #endregion

        //adds a configuration to the list

        //clears the list of configurations
    }
}