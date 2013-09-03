using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    //the mvvm object for a selected new
    public class SelectedNewsMVVM : INotifyPropertyChanged
    {
        private News _selectedNews;
        public News SelectedNews
        {
            get { return _selectedNews; }
            set { _selectedNews = value; NotifyPropertyChanged("SelectedNews"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm object for news
    public class NewsMVVM : INotifyPropertyChanged
    {
        public News News { get; set; }
        private Boolean _isread;
        public Boolean IsRead
        {
            get { return _isread; }
            set { _isread = value; this.IsUnRead = !value; NotifyPropertyChanged("IsRead"); }
        }
        private Boolean _isunread;
        public Boolean IsUnRead
        {
            get { return _isunread; }
            set { _isunread = value; NotifyPropertyChanged("IsUnRead"); }
        }
        public NewsMVVM(News news)
        {
            this.News = news;
            this.IsRead = news.IsRead;
            this.IsUnRead = !news.IsRead;
        }
        //sets the news to read
        public void markAsRead()
        {
            this.IsRead = true;
            this.News.IsRead = true;
            GameObject.GetInstance().NewsBox.HasUnreadNews = GameObject.GetInstance().NewsBox.getUnreadNews().Count > 0;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm object for difficulty
    public class DifficultyMVVM : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string UID { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public Boolean Reversed { get; set; }
        public DoubleCollection Ticks { get; set; }
        private double _selectedValue;
        public double SelectedValue
        {
            get { return _selectedValue; }
            set { _selectedValue = value; NotifyPropertyChanged("SelectedValue"); }
        }
        public DifficultyMVVM(string uid, string name, double minValue, double avgValue, double maxValue)
        {
            this.Name = name;
            this.UID = uid;
            this.MinValue = Math.Min(maxValue,minValue);
            this.MaxValue = Math.Max(minValue,maxValue);
            this.SelectedValue = avgValue;
            this.Ticks = new DoubleCollection();

             double stepValue = (avgValue - Math.Min(maxValue, minValue)) / 3;

             for (double tick = Math.Min(maxValue, minValue); tick < avgValue; tick += stepValue)
                this.Ticks.Add(tick);

            stepValue = (Math.Max(maxValue, minValue) - avgValue) / 3;

            for (double tick = avgValue; tick <= Math.Max(maxValue, minValue); tick += stepValue)
                this.Ticks.Add(tick);

            this.Reversed = minValue > maxValue;
            
        }
       
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
