namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    using TheAirline.Model.GeneralModel;

    public class OptionsMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _currentGameSpeed;

        private DoubleCollection _gameSpeeds;

        private Boolean _mailsOnAirlineDestinations;

        private Boolean _mailsOnBadWeather;

        private Boolean _mailsOnLanding;

        private Settings.AirportCode _selectedAirportCode;

        private int _selectedGameMinutes;

        private Language _selectedLanguage;

        private Boolean _shortenCurrency;

        #endregion

        #region Constructors and Destructors

        public OptionsMVVM()
        {
            this.setValues();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public List<Language> AllLanguages
        {
            get
            {
                return Languages.GetLanguages().FindAll(l => l.IsEnabled);
            }
            private set
            {
                ;
            }
        }

        public Settings.Intervals AutoSave { get; set; }

        public Settings.Intervals ClearStats { get; set; }

        public int CurrentGameSpeed
        {
            get
            {
                return this._currentGameSpeed;
            }
            set
            {
                this._currentGameSpeed = value;
                this.NotifyPropertyChanged("CurrentGameSpeed");
            }
        }

        public ObservableCollection<int> GameMinutes { get; set; }

        public DoubleCollection GameSpeeds
        {
            get
            {
                return this._gameSpeeds;
            }
            set
            {
                this._gameSpeeds = value;
                this.NotifyPropertyChanged("GameSpeeds");
            }
        }

        public Boolean HourRoundEnabled { get; set; }

        public Boolean MailsOnAirlineDestinations
        {
            get
            {
                return this._mailsOnAirlineDestinations;
            }
            set
            {
                this._mailsOnAirlineDestinations = value;
                this.NotifyPropertyChanged("MailsOnAirlineDestinations");
            }
        }

        public Boolean MailsOnBadWeather
        {
            get
            {
                return this._mailsOnBadWeather;
            }
            set
            {
                this._mailsOnBadWeather = value;
                this.NotifyPropertyChanged("MailsOnBadWeather");
            }
        }

        public Boolean MailsOnLandings
        {
            get
            {
                return this._mailsOnLanding;
            }
            set
            {
                this._mailsOnLanding = value;
                this.NotifyPropertyChanged("MailsOnLandings");
            }
        }

        public Settings.AirportCode SelectedAirportCode
        {
            get
            {
                return this._selectedAirportCode;
            }
            set
            {
                this._selectedAirportCode = value;
                this.NotifyPropertyChanged("SelectedAirportCode");
            }
        }

        public int SelectedGameMinutes
        {
            get
            {
                return this._selectedGameMinutes;
            }
            set
            {
                this._selectedGameMinutes = value;
                this.NotifyPropertyChanged("SelectedGameMinutes");
            }
        }

        public Language SelectedLanguage
        {
            get
            {
                return this._selectedLanguage;
            }
            set
            {
                this._selectedLanguage = value;
                this.NotifyPropertyChanged("SelectedLanguage");
            }
        }

        public Boolean ShortenCurrency
        {
            get
            {
                return this._shortenCurrency;
            }
            set
            {
                this._shortenCurrency = value;
                this.NotifyPropertyChanged("ShortenCurrency");
            }
        }

        #endregion

        //undos the changes

        #region Public Methods and Operators

        public void undoChanges()
        {
            this.setValues();
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void setValues()
        {
            this.SelectedLanguage = AppSettings.GetInstance().getLanguage();
            this.SelectedAirportCode = Settings.GetInstance().AirportCodeDisplay;
            this.MailsOnLandings = Settings.GetInstance().MailsOnLandings;
            this.MailsOnBadWeather = Settings.GetInstance().MailsOnBadWeather;
            this.ShortenCurrency = Settings.GetInstance().CurrencyShorten;
            this.MailsOnAirlineDestinations = Settings.GetInstance().MailsOnAirlineRoutes;
            this.HourRoundEnabled = !GameObject.GetInstance().DayRoundEnabled;
            this.SelectedGameMinutes = Settings.GetInstance().MinutesPerTurn;
            this.GameMinutes = new ObservableCollection<int> { 15, 30, 60 };
            this.CurrentGameSpeed = (int)Settings.GetInstance().GameSpeed;

            this.AutoSave = Settings.GetInstance().AutoSave;
            this.ClearStats = Settings.GetInstance().ClearStats;

            var cGameSpeeds = new DoubleCollection();

            foreach (GeneralHelpers.GameSpeedValue speed in Enum.GetValues(typeof(GeneralHelpers.GameSpeedValue)))
            {
                cGameSpeeds.Insert(0, (double)speed);
            }

            this.GameSpeeds = cGameSpeeds;
        }

        #endregion
    }

    public class GameSpeedConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int gameSpeed = System.Convert.ToInt16(value);

            return (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), gameSpeed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}