
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for some game settings
    [Serializable]
    public class Settings
    {
        private static Settings Instance;
        //the setting for which kind of airport code to show
        public enum AirportCode { IATA, ICAO }
        
        public AirportCode AirportCodeDisplay { get; set; }
        
        public Difficulty DifficultyDisplay { get; set; }
        //the setting for receiving mails on landings
        
        public Boolean MailsOnLandings { get; set; }
        
        public Boolean MailsOnBadWeather { get; set; }
        
        public int MinutesPerTurn { get; set; }
        
        public Boolean CurrencyShorten { get; set; }
        public enum Difficulty { Easy, Normal, Hard }

        public enum ScreenMode {Fullscreen, Windowed}
        public ScreenMode Mode { get; set; }
        private Settings()
        {
            this.AirportCodeDisplay = AirportCode.IATA;
            this.DifficultyDisplay = Difficulty.Normal;
            this.MailsOnLandings = false;
            this.MailsOnBadWeather = true;
            this.MinutesPerTurn = 60;
            this.CurrencyShorten = true;
            this.Mode = ScreenMode.Windowed;
        }
        //returns the settings instance
        public static Settings GetInstance()
        {
            if (Instance == null)
                Instance = new Settings();
            return Instance;
        }
    }
}
