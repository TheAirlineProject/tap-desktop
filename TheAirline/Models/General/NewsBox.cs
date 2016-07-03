using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General
{
    //the class for a news box
    [Serializable]
    public class NewsBox : BaseModel, INotifyPropertyChanged
    {
        #region Fields

        [Versioning("news")] private readonly List<News> _news;

        [Versioning("hasunread")] private bool _hasunreadnews;

        #endregion

        #region Constructors and Destructors

        public NewsBox()
        {
            _news = new List<News>();
        }

        private NewsBox(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Events

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public bool HasUnreadNews
        {
            get { return _hasunreadnews; }
            set
            {
                _hasunreadnews = value;
                NotifyPropertyChanged("HasUnreadNews");
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds a news to the news box
        public void AddNews(News news)
        {
            HasUnreadNews = true;
            _news.Add(news);
        }

        public void Clear()
        {
            _news.Clear();
        }

        //removes a news from the news box

        //returns all new
        public List<News> GetNews()
        {
            return _news;
        }

        //returns all news for a specific type
        public List<News> GetNews(News.NewsType type)
        {
            return _news.FindAll((n => n.Type == type));
        }

        //returns all news for a specific period
        public List<News> GetNews(DateTime fromDate, DateTime toDate)
        {
            return _news.FindAll((n => n.Date >= fromDate && n.Date <= toDate));
        }

        //returns all unread news
        public List<News> GetUnreadNews()
        {
            return _news.FindAll((n => !n.IsRead));
        }

        public void RemoveNews(News news)
        {
            _news.Remove(news);
            HasUnreadNews = _news.Exists(n => n.IsUnRead);
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        //clears the list of news
    }

    //the class for a news
    [Serializable]
    public class News : BaseModel
    {
        #region Constructors and Destructors

        public News(NewsType type, DateTime date, string subject, string body, bool isactionnews = false)
        {
            Type = type;
            Date = date;
            Subject = subject;
            Body = body;
            IsRead = false;
            IsActionNews = isactionnews;
        }

        private News(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                IsActionNews = false;
            }
        }

        #endregion

        #region Public Events

        public event Action<object> Action;

        #endregion

        #region Enums

        public enum NewsType
        {
            StandardNews,

            AirportNews,

            FlightNews,

            FleetNews,

            AirlineNews,

            AllianceNews,

            AirlinerNews
        }

        #endregion

        #region Public Properties

        [Versioning("actionobject", Version = 2)]
        public object ActionObject { get; set; }

        [Versioning("body")]
        public string Body { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("actionnews", Version = 2)]
        public bool IsActionNews { get; set; }

        [Versioning("isread")]
        public bool IsRead { get; set; }

        public bool IsUnRead => !IsRead;

        [Versioning("subject")]
        public string Subject { get; set; }

        [Versioning("type")]
        public NewsType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        public void ExecuteNews()
        {
            Action?.Invoke(ActionObject);
        }

        #endregion
    }
}