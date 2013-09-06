using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    
    public class FleetAirlinerMVVM : INotifyPropertyChanged
    {
        private DateTime _SchedCMaintenance;
        public DateTime SchedCMaintenance
        {
            get { return _SchedCMaintenance; }
            set { _SchedCMaintenance = value; NotifyPropertyChanged("SchedCMaintenance"); }
        }
        private DateTime _SchedDMaintenance;
        public DateTime SchedDMaintenance
        {
            get { return _SchedDMaintenance; }
            set { _SchedDMaintenance = value; NotifyPropertyChanged("SchedDMaintenance"); }
        }
        private int _AMaintenanceInterval;
        public int AMaintenanceInterval
        {
            get { return _AMaintenanceInterval; }
            set { _AMaintenanceInterval = value; NotifyPropertyChanged("AMaintenanceInterval"); }
        }
        private int _BMaintenanceInterval;
        public int BMaintenanceInterval
        {
            get { return _BMaintenanceInterval; }
            set { _BMaintenanceInterval = value; NotifyPropertyChanged("BMaintenanceInterval"); }
        }
        private int _CMaintenanceInterval;
        public int CMaintenanceInterval
        {
            get { return _CMaintenanceInterval; }
            set { _CMaintenanceInterval = value; NotifyPropertyChanged("CMaintenanceInterval"); }
        }
        private int _DMaintenanceInterval;
        public int DMaintenanceInterval
        {
            get { return _DMaintenanceInterval; }
            set { _DMaintenanceInterval = value; NotifyPropertyChanged("DMaintenanceInterval"); }
        }

        public FleetAirliner Airliner { get; set; }
        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }
        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.Classes = new ObservableCollection<AirlinerClassMVVM>();

            AirlinerClass tClass = this.Airliner.Airliner.Classes[0];
               
            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
            {
                Boolean changeable = this.Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats =tClass.RegularSeatingCapacity -1;

                this.Classes.Add(new AirlinerClassMVVM(aClass.Type, aClass.SeatingCapacity, aClass.RegularSeatingCapacity,maxSeats,changeable));
            }

            this.AMaintenanceInterval = this.Airliner.AMaintenanceInterval;
            this.BMaintenanceInterval = this.Airliner.BMaintenanceInterval;
            this.CMaintenanceInterval = this.Airliner.CMaintenanceInterval;
            this.DMaintenanceInterval = this.Airliner.DMaintenanceInterval;

            this.SchedCMaintenance = this.Airliner.SchedCMaintenance;
            this.SchedDMaintenance = this.Airliner.SchedDMaintenance;
       
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
            {
                this.AirlinerClass.Seating = Convert.ToInt16(Convert.ToDouble(this.AirlinerClass.RegularSeatingCapacity) / _selectedFacility.SeatUses);
                this.AirlinerClass.MaxSeats = Convert.ToInt16(Convert.ToDouble(this.AirlinerClass.MaxSeatsCapacity) / _selectedFacility.SeatUses);
            }
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
            set { _seating = value;  NotifyPropertyChanged("Seating"); }
    
        }
        private int _maxseats;
        public int MaxSeats
        {
            get { return _maxseats; }
            set { _maxseats = value; NotifyPropertyChanged("MaxSeats"); }

        }
        public Boolean ChangeableSeats { get; set; }
        public int RegularSeatingCapacity { get; set; }
        public int MaxSeatsCapacity { get; set; }
        public AirlinerClassMVVM(AirlinerClass.ClassType type, int seating, int regularSeating, int maxseats, Boolean changeableSeats = false)
        {
            this.Type = type;
            this.Seating = seating;
            this.RegularSeatingCapacity = regularSeating;
            this.ChangeableSeats = changeableSeats;
            this.MaxSeats = maxseats;
            this.MaxSeatsCapacity = maxseats;

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
    //the mvvm object for airline statistics
    public class FleetAirlinerStatisticsMVVM
    {
        public StatisticsType Type { get; set; }
        public double LastYear { get { return getLastYear(); } private set { ;} }
        public double CurrentYear { get { return getCurrentYear(); } private set { ;} }
        public double Change { get { return getChange(); } private set { ;} }
        public FleetAirliner Airliner { get; set; }
        public FleetAirlinerStatisticsMVVM(FleetAirliner airliner, StatisticsType type)
        {
            this.Type = type;
            this.Airliner = airliner;
        }
        //returns the value for the last year
        private double getLastYear()
        {
            int year = GameObject.GetInstance().GameTime.Year - 1;

            return this.Airliner.Statistics.getStatisticsValue(year, this.Type);
        }
        //returns the value for the current year
        private double getCurrentYear()
        {
            int year = GameObject.GetInstance().GameTime.Year;

            return this.Airliner.Statistics.getStatisticsValue(year, this.Type);

        }
        //returns the change in %
        private double getChange()
        {
            double currentYear = getCurrentYear();
            double lastYear = getLastYear();

            if (lastYear == 0)
                return 1;

            double changePercent = System.Convert.ToDouble(currentYear - lastYear) / lastYear;

            if (double.IsInfinity(changePercent))
                return 1;
            if (double.IsNaN(changePercent))
                return 0;

            return changePercent;


        }

    }
    public class ValueIsMaxAirlinerClasses : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                 return (System.Convert.ToInt16(values[0]) == System.Convert.ToInt16(values[1]));
            }
            catch (Exception)
            {
                return false;
            }
           
               
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
