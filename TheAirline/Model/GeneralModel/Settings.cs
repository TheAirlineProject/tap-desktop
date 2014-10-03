using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.GeneralModel
{
    //the class for some game settings
    [Serializable]
    public class Settings : BaseModel
    {
        #region Static Fields

        private static Settings _instance;

        #endregion

        #region Constructors and Destructors

        private Settings()
        {
            AirportCodeDisplay = AirportCode.IATA;
            DifficultyDisplay = Difficulty.Normal;
            GameSpeed = GeneralHelpers.GameSpeedValue.Normal;
            MailsOnLandings = false;
            MailsOnAirlineRoutes = false;
            MailsOnBadWeather = true;
            MinutesPerTurn = 60;
            CurrencyShorten = true;
            Mode = ScreenMode.Windowed;

            ClearStats = Intervals.Monthly;
            AutoSave = Intervals.Never;
        }

        private Settings(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum AirportCode
        {
            IATA,

            ICAO
        }

        public enum Difficulty
        {
            Easy,

            Normal,

            Hard
        }

        public enum Intervals
        {
            Daily,

            Monthly,

            Yearly,

            Never
        }

        public enum ScreenMode
        {
            Fullscreen,

            Windowed
        }

        #endregion

        #region Public Properties

        [Versioning("airportcode")]
        public AirportCode AirportCodeDisplay { get; set; }

        [Versioning("autosave")]
        public Intervals AutoSave { get; set; }

        [Versioning("clearstats")]
        public Intervals ClearStats { get; set; }

        [Versioning("currencyshorten")]
        public Boolean CurrencyShorten { get; set; }

        [Versioning("difficulty")]
        public Difficulty DifficultyDisplay { get; set; }

        [Versioning("gamespeed")]
        public GeneralHelpers.GameSpeedValue GameSpeed { get; private set; }

        //the setting for receiving mails on landings

        [Versioning("mailsonroutes")]
        public Boolean MailsOnAirlineRoutes { get; set; }

        [Versioning("mailsonweather")]
        public Boolean MailsOnBadWeather { get; set; }

        [Versioning("mailsonlandings")]
        public Boolean MailsOnLandings { get; set; }

        [Versioning("minutes")]
        public int MinutesPerTurn { get; set; }

        [Versioning("mode")]
        public ScreenMode Mode { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static Settings GetInstance()
        {
            return _instance ?? (_instance = new Settings());
        }

        public static void SetInstance(Settings instance)
        {
            _instance = instance;
        }

        //sets the speed of the game

        public void SetGameSpeed(GeneralHelpers.GameSpeedValue gameSpeed)
        {
            GameSpeed = gameSpeed;
        }

        #endregion

        //the setting for which kind of airport code to show

        //returns the settings instance
    }
}