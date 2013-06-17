
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel.TownModel
{
    //the class for a state
    [Serializable]
    public class State
    {
        
        public Country Country { get; set; }
        
        public string Name { get; set; }
        
        public string ShortName { get; set; }
        
        public string Flag { get; set; }
        
        public Boolean IsOverseas { get; set; }
        public State(Country country, string name, string shortname,Boolean overseas)
        {
            this.Country = country;
            this.Name = name;
            this.ShortName = shortname;
            this.IsOverseas = overseas;
        }
    }
    //the list of states
    public class States
    {
        private static List<State> states = new List<State>();
        //adds a state to the list
        public static void AddState(State state)
        {
            states.Add(state);
        }
        //clears the list of states
        public static void Clear()
        {
            states.Clear();
        }
        //returns a state with a short name and from a country
        public static State GetState(Country country, string shortname)
        {
            return states.Find(s => s.Country == country && s.ShortName == shortname);
        }
    }
}
