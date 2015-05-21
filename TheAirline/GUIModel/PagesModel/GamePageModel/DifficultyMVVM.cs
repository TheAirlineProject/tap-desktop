using System;
using System.ComponentModel;
using System.Windows.Media;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    //the mvvm object for a selected new
    public class SelectedNewsMVVM : INotifyPropertyChanged
    {
        #region Fields

        private News _selectedNews;

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public News SelectedNews
        {
            get
            {
                return _selectedNews;
            }
            set
            {
                _selectedNews = value;
                NotifyPropertyChanged("SelectedNews");
            }
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

        #endregion
    }

    //the mvvm object for news
    public class NewsMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isread;

        private Boolean _isselected;

        private Boolean _isunread;

        #endregion

        #region Constructors and Destructors

        public NewsMVVM(News news)
        {
            News = news;
            IsRead = news.IsRead;
            IsUnRead = !news.IsRead;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean IsRead
        {
            get
            {
                return _isread;
            }
            set
            {
                _isread = value;
                IsUnRead = !value;
                NotifyPropertyChanged("IsRead");
            }
        }

        public Boolean IsSelected
        {
            get
            {
                return _isselected;
            }
            set
            {
                _isselected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        public Boolean IsUnRead
        {
            get
            {
                return _isunread;
            }
            set
            {
                _isunread = value;
                NotifyPropertyChanged("IsUnRead");
            }
        }

        public News News { get; set; }

        #endregion

        //sets the news to read

        #region Public Methods and Operators

        public void markAsRead()
        {
            IsRead = true;
            News.IsRead = true;
            GameObject.GetInstance().NewsBox.HasUnreadNews = GameObject.GetInstance().NewsBox.GetUnreadNews().Count > 0;
        }

        public void markAsUnRead()
        {
            IsRead = false;
            News.IsRead = false;
            GameObject.GetInstance().NewsBox.HasUnreadNews = GameObject.GetInstance().NewsBox.GetUnreadNews().Count > 0;
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

        #endregion
    }

    //the mvvm object for difficulty
    public class DifficultyMVVM : INotifyPropertyChanged
    {
        #region Fields

        private double _selectedValue;

        #endregion

        #region Constructors and Destructors

        public DifficultyMVVM(string uid, string name, double minValue, double avgValue, double maxValue)
        {
            Name = name;
            UID = uid;
            MinValue = Math.Min(maxValue, minValue);
            MaxValue = Math.Max(minValue, maxValue);
            SelectedValue = avgValue;
            Ticks = new DoubleCollection();

            double stepValue = (avgValue - Math.Min(maxValue, minValue)) / 3;

            for (double tick = Math.Min(maxValue, minValue); tick < avgValue; tick += stepValue)
            {
                Ticks.Add(tick);
            }

            stepValue = (Math.Max(maxValue, minValue) - avgValue) / 3;

            for (double tick = avgValue; tick <= Math.Max(maxValue, minValue); tick += stepValue)
            {
                Ticks.Add(tick);
            }

            Reversed = minValue > maxValue;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public double MaxValue { get; set; }

        public double MinValue { get; set; }

        public string Name { get; set; }

        public Boolean Reversed { get; set; }

        public double SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                _selectedValue = value;
                NotifyPropertyChanged("SelectedValue");
            }
        }

        public DoubleCollection Ticks { get; set; }

        public string UID { get; set; }

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

        #endregion
    }
}