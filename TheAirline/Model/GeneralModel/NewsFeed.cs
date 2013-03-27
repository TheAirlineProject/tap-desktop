using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.GeneralModel
{
    //the class for a news feed
    public class NewsFeed
    {
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public NewsFeed(DateTime date, string text)
        {
            this.Text = text;
            this.Date = date;
        }
    }
    //the list of news feeds
    public class NewsFeeds
    {
        private static List<NewsFeed> feeds = new List<NewsFeed>();
        //adds a news feed to the list
        public static void AddNewsFeed(NewsFeed feed)
        {
            lock (feeds)
            {
                feeds.Add(feed);
            }
        }
        //returns the list of feeds
        public static List<NewsFeed> GetNewsFeeds()
        {
            lock (feeds)
            {
                return feeds;
            }
        }
        //clears the list of news feeds
        public static void ClearNewsFeeds()
        {
            lock (feeds)
            {
                feeds.Clear();
            }
        }
        //returns the number of news feeds
        public static int Count()
        {
            lock (feeds)
            {
                return feeds.Count;
            }
        }
    }
}
