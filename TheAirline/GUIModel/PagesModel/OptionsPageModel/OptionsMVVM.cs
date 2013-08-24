using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    public class OptionsMVVM : INotifyPropertyChanged
    {
        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { _selectedLanguage = value; NotifyPropertyChanged("SelectedLanguage"); }
        }

        private Settings.AirportCode _selectedAirportCode;
        public Settings.AirportCode SelectedAirportCode 
        {
            get { return _selectedAirportCode; }
            set { _selectedAirportCode = value; NotifyPropertyChanged("SelectedAirportCode"); }
        }

        private Boolean _mailsOnLanding;
        public Boolean MailsOnLandings 
        {
            get { return _mailsOnLanding; }
            set { _mailsOnLanding = value; NotifyPropertyChanged("MailsOnLandings"); } 
        }

        private Boolean _mailsOnBadWeather;
        public Boolean MailsOnBadWeather
        {
            get { return _mailsOnBadWeather; }
            set { _mailsOnBadWeather = value; NotifyPropertyChanged("MailsOnBadWeather"); }
        }

        private Boolean _shortenCurrency;
        public Boolean ShortenCurrency
        {
            get { return _shortenCurrency; }
            set { _shortenCurrency = value; NotifyPropertyChanged("ShortenCurrency"); }
        }
    
        public Boolean HourRoundEnabled { get; set; }

        private int _selectedGameMinutes;
        public int SelectedGameMinutes
        {
            get { return _selectedGameMinutes; }
            set { _selectedGameMinutes = value; NotifyPropertyChanged("SelectedGameMinutes"); }
        }
       
        public ObservableCollection<int> GameMinutes { get; set; }

        private int _currentGameSpeed;
        public int CurrentGameSpeed
        {
            get { return _currentGameSpeed; }
            set { _currentGameSpeed = value; NotifyPropertyChanged("CurrentGameSpeed"); }
        }

        private DoubleCollection _gameSpeeds;
        public DoubleCollection GameSpeeds 
        {
            get { return _gameSpeeds; }
            set { _gameSpeeds = value; NotifyPropertyChanged("GameSpeeds"); } 
        }
        public List<Language> AllLanguages { get { return Languages.GetLanguages().FindAll(l => l.IsEnabled); } private set { ;} }
        public OptionsMVVM()
        {
            setValues();
                
        }
        //sets the values
        private void setValues()
        {
            this.SelectedLanguage = AppSettings.GetInstance().getLanguage();
            this.SelectedAirportCode = Settings.GetInstance().AirportCodeDisplay;
            this.MailsOnLandings = Settings.GetInstance().MailsOnLandings;
            this.MailsOnBadWeather = Settings.GetInstance().MailsOnBadWeather;
            this.ShortenCurrency = Settings.GetInstance().CurrencyShorten;
            this.HourRoundEnabled = !GameObject.GetInstance().DayRoundEnabled;
            this.SelectedGameMinutes = Settings.GetInstance().MinutesPerTurn;
            this.GameMinutes = new ObservableCollection<int>() { 15, 30, 60 };
            this.CurrentGameSpeed = (int)GameTimer.GetInstance().GameSpeed;

            DoubleCollection cGameSpeeds = new DoubleCollection();
         
            foreach (GeneralHelpers.GameSpeedValue speed in Enum.GetValues(typeof(GeneralHelpers.GameSpeedValue)))
                cGameSpeeds.Insert(0,(double)speed);

            this.GameSpeeds = cGameSpeeds;

        
        }
        //undos the changes
        public void undoChanges()
        {
            setValues();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
         
        }
    }
    public class GameSpeedConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int gameSpeed = System.Convert.ToInt16(value);

            return (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), gameSpeed);
      
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
