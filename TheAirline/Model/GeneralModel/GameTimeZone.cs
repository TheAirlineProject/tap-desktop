
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    /*! GameTimeZone.
* This is used for a time zone in the game
* The class needs parameters names of time zone and the utc offset
     */
    [Serializable]
    public class GameTimeZone
    {
        
        public TimeSpan UTCOffset { get; set; }
        
        public string ShortName { get; set; }
        
        public string Name { get; set; }
        public string DisplayName { get { return getDisplayName(); } set { ;} }
        public string ShortDisplayName { get { return getShortDisplayName(); } set { ;} }
        public GameTimeZone(string name, string shortName, TimeSpan utcOffset)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.UTCOffset = utcOffset;
        }
        //returns the display name
        private string getDisplayName()
        {
            return string.Format("{0} (UTC{1}{2:D2}:{3:D2})", this.Name, this.UTCOffset.Hours < 0 ? "" : "+", this.UTCOffset.Hours, Math.Abs(this.UTCOffset.Minutes));
        }
        //returns the short display name
        private string getShortDisplayName()
        {
            return string.Format("{0} (UTC{1}{2:D2}:{3:D2})", this.ShortName, this.UTCOffset.Hours < 0 ? "" : "+", this.UTCOffset.Hours, Math.Abs(this.UTCOffset.Minutes));

        }
    }
    //the list of time zones
    public class TimeZones
    {
       
        private static List<GameTimeZone> timeZones = new List<GameTimeZone>();
        //clears the list
        public static void Clear()
        {
            timeZones.Clear();
        }
        //adds a time zone to the list
        public static void AddTimeZone(GameTimeZone tz)
        {
            timeZones.Add(tz);
        }
        //returns the list of time zones
        public static List<GameTimeZone> GetTimeZones()
        {
            return timeZones;
        }
      
    }
}
