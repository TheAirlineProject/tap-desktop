using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    public class OptionsMVVM
    {
        public Language SelectedLanguage { get; set; }
        public Settings.AirportCode SelectedAirportCode { get; set; }
        public Boolean MailsOnLandings { get; set; }
        public Boolean MailsOnBadWeather { get; set; }
        public Boolean ShortenCurrency { get; set; }
        public Boolean HourRoundEnabled { get; set; }
        public int SelectedGameMinutes { get; set; }
        public List<int> GameMinutes { get; set; }
        public List<Language> AllLanguages { get { return Languages.GetLanguages().FindAll(l => l.IsEnabled); } private set { ;} }
        public OptionsMVVM()
        {
            this.SelectedLanguage = AppSettings.GetInstance().getLanguage();
            this.SelectedAirportCode = Settings.GetInstance().AirportCodeDisplay;
            this.MailsOnLandings = Settings.GetInstance().MailsOnLandings;
            this.MailsOnBadWeather = Settings.GetInstance().MailsOnBadWeather;
            this.ShortenCurrency = Settings.GetInstance().CurrencyShorten;
            this.HourRoundEnabled = !GameObject.GetInstance().DayRoundEnabled;
            this.SelectedGameMinutes = Settings.GetInstance().MinutesPerTurn;
            this.GameMinutes = new List<int>() { 15, 30, 60 };
         
                
        }
    }
}
