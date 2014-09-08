namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections.Generic;

    //the class for a news feed
    public class NewsFeed
    {
        #region Constructors and Destructors

        public NewsFeed(DateTime date, string text)
        {
            this.Text = text;
            this.Date = date;
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

        private static readonly List<NewsFeed> feeds = new List<NewsFeed>();

        #endregion

        //adds a news feed to the list

        #region Public Methods and Operators

        public static void AddNewsFeed(NewsFeed feed)
        {
            lock (feeds)
            {
                feeds.Add(feed);
            }
        }

        //returns the list of feeds

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
            int count;

            lock (feeds) count = feeds.Count;

            return count;
        }

        public static List<NewsFeed> GetNewsFeeds()
        {
            List<NewsFeed> tFeeds;
            lock (feeds)
            {
                tFeeds = new List<NewsFeed>(feeds);
            }
            return tFeeds;
        }

        #endregion
    }
}