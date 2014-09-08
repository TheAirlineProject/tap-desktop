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
using TheAirline.GUIModel.HelpersModel;
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
        public List<EngineType> Engines { get; set; }
        public EngineType SelectedEngine { get; set; }
        public Boolean CanSetSeats { get; set; }
        private Boolean _canAddNewClass;
        public Boolean CanAddNewClass
        {
            get { return _canAddNewClass; }
            set { _canAddNewClass = value; NotifyPropertyChanged("CanAddNewClass"); }
        }
        public AirlinerType Type { get; set; }

        public static object ShowPopUp(AirlinerType type, List<AirlinerClass> classes, EngineType engine)
        {
            PopUpWindow window = new PopUpAirlinerSeatsConfiguration(type, classes, engine);
            window.ShowDialog();

            return window.Selected;


        }
        public PopUpAirlinerSeatsConfiguration(AirlinerType type, List<AirlinerClass> classes, EngineType engine)
        {
            this.FreeClassTypes = new ObservableCollection<AirlinerClass.ClassType>();
            this.Classes = new ObservableCollection<AirlinerClassMVVM>();
            this.Type = type;
            this.Engines = new List<EngineType>();

            foreach (EngineType e in EngineTypes.GetEngineTypes(type, GameObject.GetInstance().GameTime.Year).OrderBy(t => t.Price))
                this.Engines.Add(e);

            if (this.Engines.Count > 0)
                this.SelectedEngine = engine;

            AirlinerClass economyClass = classes.Find(c => c.Type == AirlinerClass.ClassType.Economy_Class);

            foreach (AirlinerClass aClass in classes)
            {

                int maxseats = aClass.Type == AirlinerClass.ClassType.Economy_Class ? aClass.SeatingCapacity : economyClass.RegularSeatingCapacity - 1;
                AirlinerClassMVVM nClass = new AirlinerClassMVVM(aClass.Type, aClass.SeatingCapacity, maxseats, aClass.Type != AirlinerClass.ClassType.Economy_Class);
                this.Classes.Add(nClass);

                foreach (AirlinerFacility facility in aClass.getFacilities())
                    nClass.Facilities.Where(f => f.Type == facility.Type).First().SelectedFacility = facility;

            }

            this.CanAddNewClass = this.Classes.Count < ((AirlinerPassengerType)this.Type).MaxAirlinerClasses;

            if (this.Classes.Count < 3)
            {
                this.FreeClassTypes.Clear();
                this.FreeClassTypes.Add(AirlinerClass.ClassType.Business_Class);
                this.FreeClassTypes.Add(AirlinerClass.ClassType.First_Class);

            }

            this.Loaded += PopUpAirlinerSeatsConfiguration_Loaded;

            InitializeComponent();
        }

        private void PopUpAirlinerSeatsConfiguration_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null && (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter || this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo))
            {
                TabItem infoItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Engine").FirstOrDefault();

                tab_main.SelectedItem = infoItem;

            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            int seating = Math.Min(5, this.Classes[0].Seating - 1);


            // chs, 2011-11-10 added so seat capacity is correctly calculated
            this.Classes[0].Seating -= seating;
            this.Classes[0].RegularSeating -= seating;
            this.Classes[0].MaxSeats -= seating;
          

            AirlinerClass.ClassType nextType = AirlinerClass.ClassType.Economy_Class;

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                if (!this.Classes.ToList().Exists(c => c.Type == type) && ((int)type <= GameObject.GetInstance().GameTime.Year))
                    nextType = type;
            }

            int maxseats = this.Classes[0].RegularSeating - 1;

            AirlinerClassMVVM newClass = new AirlinerClassMVVM(nextType, seating, maxseats, true);
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
            this.Classes[0].MaxSeats += aClass.RegularSeating;
            this.Classes[0].TotalMaxSeats += aClass.RegularSeating;

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

            AirlinerConfigurationObject aco = new AirlinerConfigurationObject();
            aco.Classes = classes;
            aco.Engine = this.SelectedEngine;


            this.Selected = aco;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void slSeats_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            var tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");
      
            AirlinerClassMVVM aClass = (AirlinerClassMVVM)((Slider)sender).Tag; 
        
            if (aClass.Type != AirlinerClass.ClassType.Economy_Class && tab_main.SelectedIndex == 0)
            {
                int diff = (int)(e.NewValue - e.OldValue);

                double currentUse = aClass.Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility.SeatUses;

                aClass.RegularSeating += (int)(diff * currentUse);            

                this.Classes[0].RegularSeating -= (int)(diff*currentUse);
                this.Classes[0].Seating -= (int)(diff * currentUse);
    
                if (this.Classes.Count == 3)
                {
                    if (this.Classes[1] == aClass)
                    {
                        this.Classes[2].TotalMaxSeats -= diff;

                        var sFacility = this.Classes[2].Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat);

                        AirlinerFacility acFacility = sFacility.SelectedFacility;

                        if (acFacility != null)
                        {
                            this.Classes[2].MaxSeats = (int)(this.Classes[2].TotalMaxSeats / acFacility.SeatUses);
                        }
                    }
                    else
                    {
                        this.Classes[1].TotalMaxSeats -= diff;
                        
                        var sFacility = this.Classes[1].Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat);

                        AirlinerFacility acFacility = sFacility.SelectedFacility;

                        if (acFacility != null)
                        {
                            this.Classes[1].MaxSeats = (int)(this.Classes[1].TotalMaxSeats / acFacility.SeatUses);
                        }
                    }
                }
            }

        }

    }
    public class AirlinerConfigurationObject
    {
        public List<AirlinerClass> Classes { get; set; }
        public EngineType Engine { get; set; }
    }
    public class AirlinerClassFacilityMVVM : INotifyPropertyChanged
    {
        public List<AirlinerFacility> Facilities { get; set; }
        private AirlinerFacility _selectedFacility;
        public AirlinerFacility SelectedFacility
        {
            get { return _selectedFacility; }
            set { _selectedFacility = value; if (this.Type == AirlinerFacility.FacilityType.Seat) setSeats(); NotifyPropertyChanged("SelectedFacility"); }
        }
        public AirlinerFacility.FacilityType Type { get; set; }
        public AirlinerClassMVVM AClass { get; set; }
        public AirlinerClassFacilityMVVM(AirlinerClassMVVM aClass, AirlinerFacility.FacilityType type)
        {
            this.Type = type;
            this.Facilities = new List<AirlinerFacility>();
            this.AClass = aClass;
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
        //sets the seats
        private void setSeats()
        {
            if (this.SelectedFacility != null)
            {
                int seats = (int)(this.AClass.RegularSeating / this.SelectedFacility.SeatUses);
                this.AClass.Seating = seats;
                this.AClass.MaxSeats = (int)(this.AClass.TotalMaxSeats / this.SelectedFacility.SeatUses);

              
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
        public int TotalMaxSeats { get; set; }
        public int RegularSeating { get; set; }
        public AirlinerClassMVVM(AirlinerClass.ClassType type, int seating, int maxseats, Boolean canDelete)
        {
            this.CanDelete = canDelete;
            this.Type = type;
            this.Seating = seating;
            this.MaxSeats = maxseats;
            this.TotalMaxSeats = maxseats;
            this.RegularSeating = seating;
            this.Facilities = new ObservableCollection<AirlinerClassFacilityMVVM>();

            foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                AirlinerClassFacilityMVVM facility = new AirlinerClassFacilityMVVM(this,facType);

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
