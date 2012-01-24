using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a news box
    public class NewsBox
    {
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
    public class News
    {
        public enum NewsType { Standard_News,Airport_News, Flight_News, Fleet_News }
        public NewsType Type { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
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
