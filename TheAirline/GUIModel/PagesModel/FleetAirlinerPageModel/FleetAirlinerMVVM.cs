using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    public class FleetAirlinerMVVM
    {
        public FleetAirliner Airliner { get; set; }
        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }
        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.Classes = new ObservableCollection<AirlinerClassMVVM>();

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                this.Classes.Add(new AirlinerClassMVVM(aClass.Type, aClass.SeatingCapacity,aClass.RegularSeatingCapacity));

       
        }
    }
    //the mvvm class for an airliner facility class
    public class AirlinerFacilityMVVM : INotifyPropertyChanged
    {
        private AirlinerClassMVVM AirlinerClass;
        public List<AirlinerFacility> Facilities { get; set; }

        private AirlinerFacility _selectedFacility;
        public AirlinerFacility SelectedFacility
        {
            get { return _selectedFacility; }
            set { _selectedFacility = value; NotifyPropertyChanged("SelectedFacility"); setSeating(); }
        }

        public AirlinerFacility.FacilityType Type { get; set; }
        public AirlinerFacilityMVVM(AirlinerFacility.FacilityType type,AirlinerClassMVVM airlinerClass)
        {
            this.Facilities = new List<AirlinerFacility>();
           
            this.AirlinerClass = airlinerClass;
            this.Type = type;

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
        private void setSeating()
        {
           if (this.Type == AirlinerFacility.FacilityType.Seat && _selectedFacility != null)
               this.AirlinerClass.Seating = Convert.ToInt16(Convert.ToDouble(this.AirlinerClass.RegularSeatingCapacity) / _selectedFacility.SeatUses); 
        }
    }
    //the mvvm class for an airliner class
    public class AirlinerClassMVVM : INotifyPropertyChanged
    {
        public List<AirlinerFacilityMVVM> Facilities { get; set; }

        public AirlinerClass.ClassType Type { get; set; }

        private int _seating;
        public int Seating
        {
            get { return _seating; }
            set { _seating = value; NotifyPropertyChanged("Seating"); }
    
        }
        public int RegularSeatingCapacity { get; set; }
        public AirlinerClassMVVM(AirlinerClass.ClassType type, int seating, int regularSeating)
        {
            this.Type = type;
            this.Seating = seating;
            this.RegularSeatingCapacity = regularSeating;

            this.Facilities = new List<AirlinerFacilityMVVM>();

            foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                AirlinerFacilityMVVM facility = new AirlinerFacilityMVVM(facType,this);

                foreach (AirlinerFacility fac in AirlinerFacilities.GetFacilities(facType))
                    facility.Facilities.Add(fac);
               
                this.Facilities.Add(facility);

            }
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
    public class ValueIsMaxAirlinerClasses : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2)
                return false;
            else
                return (System.Convert.ToInt16(values[0]) == System.Convert.ToInt16(values[1]));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
