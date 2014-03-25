using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerSeatsConfiguration.xaml
    /// </summary>
    public partial class PopUpAirlinerSeatsConfiguration : PopUpWindow, INotifyPropertyChanged
    {
        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }
        public ObservableCollection<AirlinerClass.ClassType> FreeClassTypes { get; set; }
        private Boolean _canAddNewClass;
        public Boolean CanAddNewClass
        {
            get { return _canAddNewClass; }
            set { _canAddNewClass = value; NotifyPropertyChanged("CanAddNewClass"); }
        }
        public AirlinerType Type { get; set; }

        public static object ShowPopUp(AirlinerType type, List<AirlinerClass> classes)
        {
            PopUpWindow window = new PopUpAirlinerSeatsConfiguration(type, classes);
            window.ShowDialog();
            
            return window.Selected;


        }
        public PopUpAirlinerSeatsConfiguration(AirlinerType type, List<AirlinerClass> classes)
        {
            this.FreeClassTypes = new ObservableCollection<AirlinerClass.ClassType>();
            this.Classes = new ObservableCollection<AirlinerClassMVVM>();
            this.Type = type;

            AirlinerClass economyClass = classes.Find(c => c.Type == AirlinerClass.ClassType.Economy_Class);

            foreach (AirlinerClass aClass in classes)
            {
               
                int maxseats = aClass.Type == AirlinerClass.ClassType.Economy_Class ? aClass.SeatingCapacity : economyClass.RegularSeatingCapacity - 1;
                AirlinerClassMVVM nClass = new AirlinerClassMVVM(aClass.Type, aClass.SeatingCapacity,maxseats, aClass.Type != AirlinerClass.ClassType.Economy_Class);
                this.Classes.Add(nClass);

                foreach (AirlinerFacility facility in aClass.getFacilities())
                    nClass.Facilities.Where(f=>f.Type == facility.Type).First().SelectedFacility = facility;

            }

            this.CanAddNewClass = this.Classes.Count < ((AirlinerPassengerType)this.Type).MaxAirlinerClasses;

            if (this.Classes.Count < 3)
            {
                this.FreeClassTypes.Clear();
                this.FreeClassTypes.Add(AirlinerClass.ClassType.Business_Class);
                this.FreeClassTypes.Add(AirlinerClass.ClassType.First_Class);

            }

            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            int seating = Math.Min(5, this.Classes[0].Seating - 1);


            // chs, 2011-11-10 added so seat capacity is correctly calculated
            this.Classes[0].Seating -= seating;
            this.Classes[0].RegularSeating -= seating;

            AirlinerClass.ClassType nextType = AirlinerClass.ClassType.Economy_Class;

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                if (!this.Classes.ToList().Exists(c => c.Type == type) && ((int)type <= GameObject.GetInstance().GameTime.Year))
                    nextType = type;
            }

            int maxseats = this.Classes[0].RegularSeating - 1;

            AirlinerClassMVVM newClass = new AirlinerClassMVVM(nextType, seating,maxseats, true);
            this.Classes.Add(newClass);

            this.CanAddNewClass = this.Classes.Count < ((AirlinerPassengerType)this.Type).MaxAirlinerClasses;

            if (this.Classes.Count < 3)
            {
                this.FreeClassTypes.Clear();
                this.FreeClassTypes.Add(AirlinerClass.ClassType.Business_Class);
                this.FreeClassTypes.Add(AirlinerClass.ClassType.First_Class);

            }
            else
                this.FreeClassTypes.Clear();


        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            AirlinerClassMVVM aClass = (AirlinerClassMVVM)((Button)sender).Tag;
            this.Classes.Remove(aClass);

            this.Classes.Remove(aClass);

            this.Classes[0].RegularSeating += aClass.RegularSeating;
            this.Classes[0].Seating += aClass.RegularSeating;

            this.CanAddNewClass = true;

            this.FreeClassTypes.Clear();

            this.FreeClassTypes.Add(AirlinerClass.ClassType.Business_Class);
            this.FreeClassTypes.Add(AirlinerClass.ClassType.First_Class);
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

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (AirlinerClassMVVM aClass in this.Classes)
            {
                foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                {
                    AirlinerFacility facility = aClass.Facilities.Where(f => f.Type == facType).First().SelectedFacility;

                    if (facility == null)
                        facility = AirlinerFacilities.GetBasicFacility(facType);

                    aClass.Facilities.Where(f => f.Type == facType).First().SelectedFacility = facility;
                }
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<AirlinerClass> classes = new List<AirlinerClass>();

            foreach (AirlinerClassMVVM mvvmClass in this.Classes)
            {
                AirlinerClass aClass = new AirlinerClass(mvvmClass.Type, mvvmClass.Seating);

                foreach (AirlinerClassFacilityMVVM facility in mvvmClass.Facilities)
                    aClass.forceSetFacility(facility.SelectedFacility);

                classes.Add(aClass);
            }

            this.Selected = classes;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void slSeats_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AirlinerClassMVVM aClass = (AirlinerClassMVVM)((Slider)sender).Tag;

            if (aClass.Type != AirlinerClass.ClassType.Economy_Class)
            {
                int diff = (int)(e.NewValue - e.OldValue);
                this.Classes[0].RegularSeating -= diff;
                this.Classes[0].Seating -= diff;

                if (this.Classes.Count == 3)
                {
                    if (this.Classes[1] == aClass)
                    {
                        this.Classes[2].MaxSeats -= diff;
                    }
                    else
                    {
                        this.Classes[1].MaxSeats -= diff;
                    }
                }
            }
          
        }

    }
    public class AirlinerClassFacilityMVVM : INotifyPropertyChanged
    {
        public List<AirlinerFacility> Facilities { get; set; }
        private AirlinerFacility _selectedFacility;
        public AirlinerFacility SelectedFacility
        {
            get { return _selectedFacility; }
            set { _selectedFacility = value; NotifyPropertyChanged("SelectedFacility"); }
        }
        public AirlinerFacility.FacilityType Type { get; set; }
        public AirlinerClassFacilityMVVM(AirlinerFacility.FacilityType type)
        {
            this.Type = type;
            this.Facilities = new List<AirlinerFacility>();
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
    public class AirlinerClassMVVM : INotifyPropertyChanged
    {
        public ObservableCollection<AirlinerClassFacilityMVVM> Facilities { get; set; }

        public Boolean CanDelete { get; set; }
        private AirlinerClass.ClassType _type;
        public AirlinerClass.ClassType Type
        {
            get { return _type; }
            set { _type = value; NotifyPropertyChanged("Type"); }
        }
        private int _seating;
        public int Seating
        {
            get { return _seating; }
            set { _seating = value; NotifyPropertyChanged("Seating"); }
        }
        private int _maxseats;
        public int MaxSeats
        {
            get { return _maxseats; }
            set { _maxseats = value; NotifyPropertyChanged("MaxSeats"); }
        }
        public int RegularSeating { get; set; }
        public AirlinerClassMVVM(AirlinerClass.ClassType type, int seating, int maxseats, Boolean canDelete)
        {
            this.CanDelete = canDelete;
            this.Type = type;
            this.Seating = seating;
            this.MaxSeats = maxseats;
            this.RegularSeating = seating;
            this.Facilities = new ObservableCollection<AirlinerClassFacilityMVVM>();

            foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                AirlinerClassFacilityMVVM facility = new AirlinerClassFacilityMVVM(facType);

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
}
