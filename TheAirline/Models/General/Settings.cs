using TheAirline.Infrastructure.Enums;

namespace TheAirline.Models.General
{
    public class Settings
    {
        public int Id { get; set; }
        public AirportCode AirportCodeDisplay { get; set; }
        public Intervals AutoSave { get; set; }
        public Intervals ClearStats { get; set; }
        public bool CurrencyShorten { get; set; }
        public Difficulty DifficultyDisplay { get; set; }
        public GameSpeeds GameSpeed { get; set; }
        public bool MailsOnAirlineRoutes { get; set; }
        public bool MailsOnBadWeather { get; set; }
        public bool MailsOnLandings { get; set; }
        public ScreenMode Mode { get; set; }
        public string Language { get; set; }
    }
}
