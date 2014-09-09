using System;
using System.Collections.Generic;

namespace TheAirline.Model.GeneralModel
{
    //the class for a news feed
    public class NewsFeed
    {
        #region Constructors and Destructors

        public NewsFeed(DateTime date, string text)
        {
            Text = text;
            Date = date;
        }

        #endregion

        #region Public Properties

        public DateTime Date { get; set; }

        public string Text { get; set; }

        #endregion
    }

    //the list of news feeds
    public class NewsFeeds
    {
        #region Static Fields

        private static readonly List<NewsFeed> Feeds = new List<NewsFeed>();

        #endregion

        #region Public Methods and Operators

        public static void AddNewsFeed(NewsFeed feed)
        {
            lock (Feeds)
            {
                Feeds.Add(feed);
            }
        }

        //returns the list of feeds

        //clears the list of news feeds
        public static void ClearNewsFeeds()
        {
            lock (Feeds)
            {
                Feeds.Clear();
            }
        }

        //returns the number of news feeds
        public static int Count()
        {
            int count;

            lock (Feeds) count = Feeds.Count;

            return count;
        }

        public static List<NewsFeed> GetNewsFeeds()
        {
            List<NewsFeed> tFeeds;
            lock (Feeds)
            {
                tFeeds = new List<NewsFeed>(Feeds);
            }
            return tFeeds;
        }

        #endregion

        //adds a news feed to the list
    }
}