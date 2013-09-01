using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    //the mvvm class for an airport
    public class AirportMVVM : INotifyPropertyChanged
    {
        public Airport Airport { get; set; }
        private int _numberOfRoutes;
        public int NumberOfRoutes
        {
            get { return _numberOfRoutes; }
            set { _numberOfRoutes = value; NotifyPropertyChanged("NumberOfRoutes"); }
        }
        private int _numberOfAirlines;
        public int NumberOfAirlines
        {
            get { return _numberOfAirlines; }
            set { _numberOfAirlines = value; NotifyPropertyChanged("NumberOfAirlines"); }
        }
        private int _numberOfFreeGates;
        public int NumberOfFreeGates
        {
            get { return _numberOfFreeGates; }
            set { _numberOfFreeGates = value; NotifyPropertyChanged("NumberOfFreeGates"); }
        }
        private Boolean _isHuman;
        public Boolean IsHuman
        {
            get { return _isHuman; }
            set { _isHuman = value; NotifyPropertyChanged("IsHuman"); }
        }
        public AirportMVVM(Airport airport)
        {
            this.Airport = airport;
            this.IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport);
            this.NumberOfFreeGates = this.Airport.Terminals.NumberOfFreeGates;
            this.NumberOfAirlines = this.Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            this.NumberOfRoutes = AirportHelpers.GetAirportRoutes(this.Airport).Count;
        }
        public void addAirlineContract(AirportContract contract)
        {
            this.Airport.addAirlineContract(contract);
            this.IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport);
            this.NumberOfFreeGates = this.Airport.Terminals.NumberOfFreeGates;
            this.NumberOfAirlines = this.Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            this.NumberOfRoutes = AirportHelpers.GetAirportRoutes(this.Airport).Count;
       
  
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
    //the converter for the airports statistics
    public class AirportStatisticsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Airport airport = (Airport)value;
            StatisticsType statType = StatisticsTypes.GetStatisticsType("Passengers");

            return airport.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year, statType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}
