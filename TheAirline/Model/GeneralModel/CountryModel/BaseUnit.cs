using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the base unit for countries and union members
    public class BaseUnit
    {
        public string Uid { get; set; }
        public string ShortName { get; set; }
        public string Flag { get; set; }
        public BaseUnit(string uid, string shortname)
        {
            this.Uid = uid;
            this.ShortName = shortname;
        }
        public string Name
        {
            get { return Translator.GetInstance().GetString(Country.Section, this.Uid); }
        }
    }
}
