using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a news box
    [ProtoContract]
    public class NewsBox
    {
        [ProtoMember(1)]
        private List<News> News;
         public NewsBox()
        {
            this.News = new List<News>();
  
        }
        //adds a news to the news box
        public void addNews(News news)
        {
            this.News.Add(news);
        }
        //returns all new
        public List<News> getNews()
        {
            return this.News;
        }
        //returns all news for a specific type
        public List<News> getNews(News.NewsType type)
        {
            return this.News.FindAll((delegate(News n) { return n.Type == type; }));
        }
        //returns all news for a specific period
        public List<News> getNews(DateTime fromDate, DateTime toDate)
        {
            return this.News.FindAll((delegate(News n) { return n.Date >= fromDate && n.Date <= toDate; }));
        }
        //returns all unread news
        public List<News> getUnreadNews()
        {
            return this.News.FindAll((delegate(News n) {return !n.IsRead;}));
        }
        //clears the list of news
        public void clear()
        {
            this.News.Clear();
        }

    }
    //the class for a news
    [ProtoContract]
    public class News
    {
        public enum NewsType { Standard_News,Airport_News, Flight_News, Fleet_News, Airline_News, Alliance_News,Airliner_News}
        [ProtoMember(1)]
        public NewsType Type { get; set; }
        [ProtoMember(2)]
        public DateTime Date { get; set; }
        [ProtoMember(3)]
        public string Subject { get; set; }
        [ProtoMember(4)]
        public string Body { get; set; }
        [ProtoMember(5)]
        public Boolean IsRead { get; set; }
        public Boolean IsUnRead {get{return !this.IsRead;} set{;}}
        public News(NewsType type, DateTime date, string subject, string body)
        {
            this.Type = type;
            this.Date = date;
            this.Subject = subject;
            this.Body = body;
            this.IsRead = false;
        }
    }
}
