using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//since: 0.5.0
namespace TheAirline.Model.Airline
{
    class Attributes
    {
        private Focus focus { set; get; }
        private Mentality mentality { set; get; }
        private License license { set; get; }
        private Schedule schedule { set; get; }
        private Value value {set; get;}
    }

    enum Focus 
    {
        Global,

        Regional,

        Domestic,

        Local
    }

    enum Mentality 
    {
        Safe,
        Moderate,
        Aggressive
    }

    enum License 
    {
        Domestic,

        Regional,

        Short_Haul,

        Long_Hau
    }

    enum Schedule 
    {
        Regular,

        Business,

        Charter,

        Sightseeing
    }

    enum Value
    {
        Very_low,
        Low,
        Normal,
        High,
        Very_high
    }
}
