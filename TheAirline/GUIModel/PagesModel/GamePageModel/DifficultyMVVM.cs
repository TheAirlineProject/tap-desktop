using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
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
