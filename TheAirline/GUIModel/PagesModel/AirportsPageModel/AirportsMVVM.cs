using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    //the mvvm class for an airport
    public class AirportMVVM : INotifyPropertyChanged
    {
        public Airport Airport { get; set; }
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

        }
        public void addAirlineContract(AirportContract contract)
        {
            this.Airport.addAirlineContract(contract);
            this.IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport);
            this.NumberOfFreeGates = this.Airport.Terminals.NumberOfFreeGates;

  
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
