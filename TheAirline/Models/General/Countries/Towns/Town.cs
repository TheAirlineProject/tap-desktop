using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.General.Countries.Towns
{
    //the class for a town / city
    [Serializable]
    public class Town : BaseModel
    {
        #region Constructors and Destructors

        public Town(string name, Country country)
            : this(name, country, null)
        {
        }

        public Town(string name, Country country, State state)
        {
            Name = name;
            Country = country;
            State = state;
        }

        private Town(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("state")]
        public State State { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static bool operator ==(Town a, Town b)
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

            // Return true if the fields match:
            return a.Name == b.Name && a.Name == b.Name
                   && ((a.State == null && b.State == null) || (a.State == b.State));
        }

        public static bool operator !=(Town a, Town b)
        {
            return !(a == b);
        }

        public override bool Equals(object u)
        {
            // If parameter is null return false:
            if (!(u is Town))
            {
                return false;
            }

            // Return true if the fields match:
            return (this == (Town) u);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Country.GetHashCode();
        }

        #endregion
    }

    //the class which finds a town based on name
    public class Towns
    {
        #region Public Methods and Operators

        public static Town GetTown(string name)
        {
            if (name.Contains(','))
            {
                string town = name.Split(',')[0].Trim();
                string state = name.Split(',')[1].Trim();

                if (
                    Airports.Airports.GetAirport(
                        a =>
                        a.Profile.Town.Name == town && a.Profile.Town.State != null
                        && a.Profile.Town.State.ShortName == state) == null)
                {
                    if (Airports.Airports.GetAirport(a => a.Profile.Town.Name == town && a.Profile.Country.Uid == state) != null)
                    {
                        return Airports.Airports.GetAirport(a => a.Profile.Town.Name == town && a.Profile.Country.Uid == state).Profile.Town;
                    }
                    return null;
                }

                return
                    Airports.Airports.GetAirport(
                        a =>
                        a.Profile.Town.Name == town && a.Profile.Town.State != null
                        && a.Profile.Town.State.ShortName == state).Profile.Town;
            }
            if (Airports.Airports.GetAirport(a => a.Profile.Town.Name == name) == null)
            {
                return null;
            }

            return Airports.Airports.GetAirport(a => a.Profile.Town.Name == name).Profile.Town;
        }

        public static Town GetTown(string name, State state)
        {
            return Airports.Airports.GetAirport(a => a.Profile.Town.Name == name && a.Profile.Town.State == state).Profile.Town;
        }

        public static List<Town> GetTowns()
        {
            return Airports.Airports.GetAllAirports().Select(a => a.Profile.Town).Distinct().ToList();
        }

        //returns all towns from a specific country
        public static List<Town> GetTowns(Country country)
        {
            return
                Airports.Airports.GetAllAirports()
                        .Where(a => a.Profile.Country == country)
                        .Select(a => a.Profile.Town)
                        .Distinct()
                        .ToList();
        }

        #endregion

        //returns all towns

        //returns a town
    }
}