using System;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries.Towns;

namespace TheAirline.Models.Pilots
{
    //the class for the profile of a pilot
    [Serializable]
    public class PilotProfile : BaseModel
    {
        #region Constructors and Destructors

        public PilotProfile(string firstname, string lastname, DateTime birthdate, Town town)
        {
            Firstname = firstname;
            Lastname = lastname;
            Town = town;
            Birthdate = birthdate;
        }

        private PilotProfile(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public int Age => MathHelpers.GetAge(Birthdate);

        [Versioning("birthdate")]
        public DateTime Birthdate { get; set; }

        [Versioning("firstname")]
        public string Firstname { get; set; }

        [Versioning("lastname")]
        public string Lastname { get; set; }

        public string Name => $"{Firstname} {Lastname}";

        [Versioning("town")]
        public Town Town { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}