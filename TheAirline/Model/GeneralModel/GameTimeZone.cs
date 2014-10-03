using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel
{
    /*! GameTimeZone.
* This is used for a time zone in the game
* The class needs parameters names of time zone and the utc offset
     */

    [Serializable]
    public class GameTimeZone : BaseModel
    {
        #region Constructors and Destructors

        public GameTimeZone(string name, string shortName, TimeSpan utcOffset)
        {
            Name = name;
            ShortName = shortName;
            UTCOffset = utcOffset;
        }

        private GameTimeZone(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public string DisplayName
        {
            get { return GetDisplayName(); }
        }

        [Versioning("name")]
        public string Name { get; set; }

        public string ShortDisplayName
        {
            get { return GetShortDisplayName(); }
        }

        [Versioning("shortname")]
        public string ShortName { get; set; }

        [Versioning("offset")]
        public TimeSpan UTCOffset { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        #region Methods

        private string GetDisplayName()
        {
            return string.Format(
                "{0} (UTC{1}{2:D2}:{3:D2})",
                Name,
                UTCOffset.Hours < 0 ? "" : "+",
                UTCOffset.Hours,
                Math.Abs(UTCOffset.Minutes));
        }

        //returns the short display name
        private string GetShortDisplayName()
        {
            return string.Format(
                "{0} (UTC{1}{2:D2}:{3:D2})",
                ShortName,
                UTCOffset.Hours < 0 ? "" : "+",
                UTCOffset.Hours,
                Math.Abs(UTCOffset.Minutes));
        }

        #endregion

        //returns the display name
    }

    //the list of time zones
    public class TimeZones
    {
        #region Static Fields

        private static readonly List<GameTimeZone> timeZones = new List<GameTimeZone>();

        #endregion

        #region Public Methods and Operators

        public static void AddTimeZone(GameTimeZone tz)
        {
            timeZones.Add(tz);
        }

        public static void Clear()
        {
            timeZones.Clear();
        }

        //returns the list of time zones
        public static List<GameTimeZone> GetTimeZones()
        {
            return timeZones;
        }

        #endregion

        //clears the list

        //adds a time zone to the list
    }
}