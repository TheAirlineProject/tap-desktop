
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the base unit for countries and union members
    [DataContract]
    [KnownType(typeof(Country))]
     [KnownType(typeof(Union))]
    public class BaseUnit
    {

        [DataMember]
        public string Uid { get; set; }

        [DataMember]
        public string ShortName { get; set; }

        [DataMember]
        public string Flag { get; set; }
        public BaseUnit(string uid, string shortname)
        {
            this.Uid = uid;
            this.ShortName = shortname;
        }
        public virtual string Name
        {
            get { return Translator.GetInstance().GetString(Country.Section, this.Uid); }
        }
        public static bool operator ==(BaseUnit a, BaseUnit b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // if one is union and the other is not
            if ((a is Union && b is Country) || (a is Country && b is Union))
                return false;

            if (a is Union && b is Union)
                return a.Uid == b.Uid;
                      
            // Return true if the fields match:
            if (a is TerritoryCountry && b is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry)a).MainCountry.Uid == b.Uid || a.Uid == ((TerritoryCountry)b).MainCountry.Uid || ((TerritoryCountry)a).MainCountry.Uid == ((TerritoryCountry)b).MainCountry.Uid;
            }
            if (a is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry)a).MainCountry.Uid == b.Uid;
            }
            if (b is TerritoryCountry)
            {
                return a.Uid == b.Uid || a.Uid == ((TerritoryCountry)b).MainCountry.Uid;
            }

            return a.Uid == b.Uid;//a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(BaseUnit a, BaseUnit b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Uid.GetHashCode() ^ this.ShortName.GetHashCode();
        }
        public override bool Equals(object u)
        {
            // If parameter is null return false:
            if ((object)u == null || !(u is BaseUnit))
            {
                return false;
            }
           
            // Return true if the fields match:
            return (this.Uid == ((BaseUnit)u).Uid);
        }
    }
}
