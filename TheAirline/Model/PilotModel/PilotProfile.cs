using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;

namespace TheAirline.Model.PilotModel
{
    //the class for the profile of a pilot
    public class PilotProfile
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public Town Town { get; set; }
        public DateTime Birthdate { get; set; }
        public PilotProfile(string firstname, string lastname, DateTime birthdate, Town town)
        {
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Town = town;
            this.Birthdate = birthdate;
        }
    }
}
