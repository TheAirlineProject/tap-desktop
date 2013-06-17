using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for some game settings
    [ProtoContract]
    public class Settings
    {
        private static Settings Instance;
        //the setting for which kind of airport code to show
        public enum AirportCode { IATA, ICAO }
        [ProtoMember(2)]
        public AirportCode AirportCodeDisplay { get; set; }
        [ProtoMember(3)]
        public Difficulty DifficultyDisplay { get; set; }
        //the setting for receiving mails on landings
        [ProtoMember(4)]
        public Boolean MailsOnLandings { get; set; }
        [ProtoMember(5)]
        public Boolean MailsOnBadWeather { get; set; }
        [ProtoMember(6)]
        public int MinutesPerTurn { get; set; }
        [ProtoMember(7)]
        public Boolean CurrencyShorten { get; set; }
        public enum Difficulty { Easy, Normal, Hard }

        private Settings()
        {
            this.AirportCodeDisplay = AirportCode.IATA;
            this.DifficultyDisplay = Difficulty.Normal;
            this.MailsOnLandings = false;
            this.MailsOnBadWeather = true;
            this.MinutesPerTurn = 60;
            this.CurrencyShorten = true;
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
