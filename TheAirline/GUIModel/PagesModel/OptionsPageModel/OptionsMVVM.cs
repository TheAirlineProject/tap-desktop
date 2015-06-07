using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.General.Enums;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.General;
using Settings = TheAirline.Properties.Settings;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    public class OptionsMVVM : INotifyPropertyChanged
    {
        #region Fields

        private int _currentGameSpeed;

        private DoubleCollection _gameSpeeds;

        private Boolean _mailsOnAirlineDestinations;

        private Boolean _mailsOnBadWeather;

        private Boolean _mailsOnLanding;

        private AirportCode _selectedAirportCode;

        private int _selectedGameMinutes;

        private Language _selectedLanguage;

        private Boolean _shortenCurrency;

        #endregion

        #region Constructors and Destructors

        public OptionsMVVM()
        {
            setValues();
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

        public Intervals AutoSave { get; set; }

        public Intervals ClearStats { get; set; }

        public int CurrentGameSpeed
        {
            get
            {
                return _currentGameSpeed;
            }
            set
            {
                _currentGameSpeed = value;
                NotifyPropertyChanged("CurrentGameSpeed");
            }
        }

        public ObservableCollection<int> GameMinutes { get; set; }

        public DoubleCollection GameSpeeds
        {
            get
            {
                return _gameSpeeds;
            }
            set
            {
                _gameSpeeds = value;
                NotifyPropertyChanged("GameSpeeds");
            }
        }

        public Boolean HourRoundEnabled { get; set; }

        public Boolean MailsOnAirlineDestinations
        {
            get
            {
                return _mailsOnAirlineDestinations;
            }
            set
            {
                _mailsOnAirlineDestinations = value;
                NotifyPropertyChanged("MailsOnAirlineDestinations");
            }
        }

        public Boolean MailsOnBadWeather
        {
            get
            {
                return _mailsOnBadWeather;
            }
            set
            {
                _mailsOnBadWeather = value;
                NotifyPropertyChanged("MailsOnBadWeather");
            }
        }

        public Boolean MailsOnLandings
        {
            get
            {
                return _mailsOnLanding;
            }
            set
            {
                _mailsOnLanding = value;
                NotifyPropertyChanged("MailsOnLandings");
            }
        }

        public AirportCode SelectedAirportCode
        {
            get
            {
                return _selectedAirportCode;
            }
            set
            {
                _selectedAirportCode = value;
                NotifyPropertyChanged("SelectedAirportCode");
            }
        }

        public int SelectedGameMinutes
        {
            get
            {
                return _selectedGameMinutes;
            }
            set
            {
                _selectedGameMinutes = value;
                NotifyPropertyChanged("SelectedGameMinutes");
            }
        }

        public Language SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                _selectedLanguage = value;
                NotifyPropertyChanged("SelectedLanguage");
            }
        }

        public Boolean ShortenCurrency
        {
            get
            {
                return _shortenCurrency;
            }
            set
            {
                _shortenCurrency = value;
                NotifyPropertyChanged("ShortenCurrency");
            }
        }

        #endregion

        //undos the changes

        #region Public Methods and Operators

        public void undoChanges()
        {
            setValues();
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void setValues()
        {
            SelectedLanguage = AppSettings.GetInstance().GetLanguage();
            SelectedAirportCode = Infrastructure.Settings.GetInstance().AirportCodeDisplay;
            MailsOnLandings = Infrastructure.Settings.GetInstance().MailsOnLandings;
            MailsOnBadWeather = Infrastructure.Settings.GetInstance().MailsOnBadWeather;
            ShortenCurrency = Infrastructure.Settings.GetInstance().CurrencyShorten;
            MailsOnAirlineDestinations = Infrastructure.Settings.GetInstance().MailsOnAirlineRoutes;
            HourRoundEnabled = !GameObject.GetInstance().DayRoundEnabled;
            SelectedGameMinutes = Infrastructure.Settings.GetInstance().MinutesPerTurn;
            GameMinutes = new ObservableCollection<int> { 15, 30, 60 };
            CurrentGameSpeed = (int)Infrastructure.Settings.GetInstance().GameSpeed;

            AutoSave = Infrastructure.Settings.GetInstance().AutoSave;
            ClearStats = Infrastructure.Settings.GetInstance().ClearStats;

            var cGameSpeeds = new DoubleCollection();

            foreach (GeneralHelpers.GameSpeedValue speed in Enum.GetValues(typeof(GeneralHelpers.GameSpeedValue)))
            {
                cGameSpeeds.Insert(0, (double)speed);
            }

            GameSpeeds = cGameSpeeds;
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