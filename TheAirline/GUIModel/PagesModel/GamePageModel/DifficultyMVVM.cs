namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media;

    using TheAirline.Model.GeneralModel;

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
                return this._selectedNews;
            }
            set
            {
                this._selectedNews = value;
                this.NotifyPropertyChanged("SelectedNews");
            }
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
            this.News = news;
            this.IsRead = news.IsRead;
            this.IsUnRead = !news.IsRead;
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
                return this._isread;
            }
            set
            {
                this._isread = value;
                this.IsUnRead = !value;
                this.NotifyPropertyChanged("IsRead");
            }
        }

        public Boolean IsSelected
        {
            get
            {
                return this._isselected;
            }
            set
            {
                this._isselected = value;
                this.NotifyPropertyChanged("IsSelected");
            }
        }

        public Boolean IsUnRead
        {
            get
            {
                return this._isunread;
            }
            set
            {
                this._isunread = value;
                this.NotifyPropertyChanged("IsUnRead");
            }
        }

        public News News { get; set; }

        #endregion

        //sets the news to read

        #region Public Methods and Operators

        public void markAsRead()
        {
            this.IsRead = true;
            this.News.IsRead = true;
            GameObject.GetInstance().NewsBox.HasUnreadNews = GameObject.GetInstance().NewsBox.GetUnreadNews().Count > 0;
        }

        public void markAsUnRead()
        {
            this.IsRead = false;
            this.News.IsRead = false;
            GameObject.GetInstance().NewsBox.HasUnreadNews = GameObject.GetInstance().NewsBox.GetUnreadNews().Count > 0;
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
            this.Name = name;
            this.UID = uid;
            this.MinValue = Math.Min(maxValue, minValue);
            this.MaxValue = Math.Max(minValue, maxValue);
            this.SelectedValue = avgValue;
            this.Ticks = new DoubleCollection();

            double stepValue = (avgValue - Math.Min(maxValue, minValue)) / 3;

            for (double tick = Math.Min(maxValue, minValue); tick < avgValue; tick += stepValue)
            {
                this.Ticks.Add(tick);
            }

            stepValue = (Math.Max(maxValue, minValue) - avgValue) / 3;

            for (double tick = avgValue; tick <= Math.Max(maxValue, minValue); tick += stepValue)
            {
                this.Ticks.Add(tick);
            }

            this.Reversed = minValue > maxValue;
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
                return this._selectedValue;
            }
            set
            {
                this._selectedValue = value;
                this.NotifyPropertyChanged("SelectedValue");
            }
        }

        public DoubleCollection Ticks { get; set; }

        public string UID { get; set; }

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

        #endregion
    }
}