using System;
using System.IO;
using System.Runtime.Serialization;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.General.Models.Countries
{
    //the base unit for countries and union members
    [Serializable]
    public class BaseUnit : BaseModel
    {
        private string _flag;

        #region Constructors and Destructors

        public BaseUnit(string uid, string shortname)
        {
            Uid = uid;
            ShortName = shortname;
        }

        protected BaseUnit(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

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

        public virtual string Name => Translator.GetInstance().GetString(Country.Section, Uid);

        [Versioning("shortname")]
        public string ShortName { get; set; }

        [Versioning("uid")]
        public string Uid { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static bool operator ==(BaseUnit a, BaseUnit b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }
            // if one is union and the other is not
            if ((a is Union && b is Country) || (a is Country && b is Union))
            {
                return false;
            }

            if (a is Union && b is Union)
            {
                return a.Uid == b.Uid;
            }

            // Return true if the fields match:
            if (a is TerritoryCountry && b is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry) a).MainCountry.Uid == b.Uid
                       || a.Uid == ((TerritoryCountry) b).MainCountry.Uid
                       || ((TerritoryCountry) a).MainCountry.Uid == ((TerritoryCountry) b).MainCountry.Uid;
            }
            if (a is TerritoryCountry)
            {
                return a.Uid == b.Uid || ((TerritoryCountry) a).MainCountry.Uid == b.Uid;
            }
            if (b is TerritoryCountry)
            {
                return a.Uid == b.Uid || a.Uid == ((TerritoryCountry) b).MainCountry.Uid;
            }

            return a.Uid == b.Uid; //a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(BaseUnit a, BaseUnit b)
        {
            return !(a == b);
        }

        public override bool Equals(object u)
        {
            // If parameter is null return false:
            if (!(u is BaseUnit))
            {
                return false;
            }

            // Return true if the fields match:
            return (Uid == ((BaseUnit) u).Uid);
        }

        public override int GetHashCode()
        {
            return Uid.GetHashCode() ^ ShortName.GetHashCode();
        }

        #endregion
    }
}