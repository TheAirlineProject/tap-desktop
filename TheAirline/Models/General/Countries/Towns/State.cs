using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.General.Countries.Towns
{
    //the class for a state
    [Serializable]
    public class State : BaseModel
    {
        private string _flag;

        #region Constructors and Destructors

        public State(Country country, string name, string shortname, Boolean overseas)
        {
            Country = country;
            Name = name;
            ShortName = shortname;
            IsOverseas = overseas;
        }

        private State(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("flag")]
        public string Flag
        {
            get
            {
                if (!File.Exists(_flag))
                {
                    Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\" + Name + ".png";
                }
                return _flag;
            }
            set { _flag = value; }
        }

        [Versioning("isoverseas")]
        public bool IsOverseas { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("shortname")]
        public string ShortName { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of states
    public class States
    {
        #region Static Fields

        private static readonly List<State> _states = new List<State>();

        #endregion

        #region Public Methods and Operators

        public static void AddState(State state)
        {
            _states.Add(state);
        }

        //clears the list of states
        public static void Clear()
        {
            _states.Clear();
        }

        //returns a state with a short name and from a country
        public static State GetState(Country country, string shortname)
        {
            return _states.Find(s => s.Country == country && s.ShortName == shortname);
        }

        #endregion

        //adds a state to the list
    }
}