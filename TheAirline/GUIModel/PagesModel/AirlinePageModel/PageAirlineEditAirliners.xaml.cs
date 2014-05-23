using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineEditAirliners.xaml
    /// </summary>
    public partial class PageAirlineEditAirliners : Page
    {
        private List<FleetAirliner> Airliners;
        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }
        private FleetAirliner Airliner;
        public PageAirlineEditAirliners(List<FleetAirliner> airliners)
        {
            this.Airliners = airliners;

            this.Loaded += PageAirlineEditAirliners_Loaded;

            this.DataContext = this.Airliners;

            this.Classes = new ObservableCollection<AirlinerClassMVVM>();
            this.Airliner = getMinimumAirliner();

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
            {
                int maxCapacity;

                if (this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                    maxCapacity = ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxSeatingCapacity;
                else
                    maxCapacity = 100;

                Boolean changeable = this.Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats;

                if (this.Airliner.Airliner.Classes.Count == 3)
                {
                    if (this.Airliner.Airliner.Classes.IndexOf(aClass) == 1)
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[2].RegularSeatingCapacity;
                    else
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[1].RegularSeatingCapacity;
                }
                else
                    maxSeats = maxCapacity - 1;


                AirlinerClassMVVM amClass = new AirlinerClassMVVM(aClass, aClass.SeatingCapacity, aClass.RegularSeatingCapacity, maxSeats, changeable);
                this.Classes.Add(amClass);
            }

            InitializeComponent();


        }
        //returns the minimum of the selected airliners
        private FleetAirliner getMinimumAirliner()
        {
            FleetAirliner minAirliner = this.Airliners[0];

            foreach (FleetAirliner airliner in this.Airliners)
            {
                if (((AirlinerPassengerType)airliner.Airliner.Type).MaxAirlinerClasses < ((AirlinerPassengerType)minAirliner.Airliner.Type).MaxAirlinerClasses)
                    minAirliner = airliner;
                else
                {
                    if (((AirlinerPassengerType)airliner.Airliner.Type).MaxAirlinerClasses == ((AirlinerPassengerType)minAirliner.Airliner.Type).MaxAirlinerClasses && airliner.Airliner.Classes[0].SeatingCapacity < minAirliner.Airliner.Classes[0].SeatingCapacity)
                        minAirliner = airliner;
                }
            }

            return minAirliner;


        }
        private void PageAirlineEditAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliners")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Visible;

                matchingItem.IsSelected = true;
            }


        }
        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            int totalServiceLevel = this.Classes.Sum(c => c.Facilities.Sum(f => f.SelectedFacility.ServiceLevel));
            TextBox txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("Custom configuration ({0} classes)", this.Classes.Count);
            txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            if (TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineEditAirliners", "1002"), txtName) == TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                AirlinerConfiguration configuration = new AirlinerConfiguration(name,getMinimumAirliner().Airliner.getTotalSeatCapacity(),false);

                foreach (AirlinerClassMVVM type in this.Classes)
                {
                    AirlinerClassConfiguration classConfiguration = new AirlinerClassConfiguration(type.Type,type.Seating,type.RegularSeatingCapacity);

                    foreach (AirlinerFacilityMVVM facility in type.Facilities)
                        classConfiguration.addFacility(facility.SelectedFacility);

                    configuration.addClassConfiguration(classConfiguration);
                }

                Configurations.AddConfiguration(configuration);

      
            }
        }
        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {

            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (AirlinerConfiguration confItem in Configurations.GetConfigurations(Configuration.ConfigurationType.Airliner).Where(a => ((AirlinerConfiguration)a).MinimumSeats <= this.Airliner.Airliner.getTotalSeatCapacity() && ((AirlinerConfiguration)a).Classes.Count <= ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxAirlinerClasses))
                cbConfigurations.Items.Add(confItem);

            cbConfigurations.SelectedIndex = 0;


            if (TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineWages", "1013"), cbConfigurations) == TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
            
                this.Classes.Clear();

                AirlinerConfiguration configuration = (AirlinerConfiguration)cbConfigurations.SelectedItem;

                foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                {
                    AirlinerClass nClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                    AirlinerClassMVVM tClass = new AirlinerClassMVVM(nClass, aClass.SeatingCapacity, aClass.RegularSeatingCapacity, aClass.RegularSeatingCapacity);

                    foreach (AirlinerFacility facility in aClass.getFacilities())
                        tClass.Facilities.First(f => f.Type == facility.Type).SelectedFacility = facility;

                    foreach (AirlinerFacility.FacilityType fType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                    {
                        if (!aClass.Facilities.Exists(f => f.Type == fType))
                        {
                            tClass.Facilities.First(f => f.Type == fType).SelectedFacility = AirlinerFacilities.GetBasicFacility(fType);

                        }
                    }

                    this.Classes.Add(tClass);
                }
                int seatingDiff = ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxSeatingCapacity - configuration.MinimumSeats;

                AirlinerClassMVVM economyClass = this.Classes.First(c => c.Type == AirlinerClass.ClassType.Economy_Class);

                economyClass.RegularSeatingCapacity += seatingDiff;

                AirlinerFacility seatingFacility = economyClass.Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility;

                int extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                economyClass.Seating += extraSeats;
            }


        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2128"), string.Format(Translator.GetInstance().GetString("MessageBox", "2128", "message")), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (FleetAirliner airliner in this.Airliners)
                {
                    airliner.Airliner.clearAirlinerClasses();

                    foreach (AirlinerClassMVVM aClass in this.Classes)
                    {
                        AirlinerClass newClass = new AirlinerClass(aClass.Type, aClass.Seating);

                        airliner.Airliner.addAirlinerClass(newClass);

                        foreach (AirlinerFacilityMVVM facility in aClass.Facilities)
                            newClass.forceSetFacility(facility.SelectedFacility);
                    }

                    int totalSeats = this.Classes.Sum(c => c.RegularSeatingCapacity);

                    int seatingDiff = ((AirlinerPassengerType)airliner.Airliner.Type).MaxSeatingCapacity - totalSeats;

                    AirlinerClass economyClass = airliner.Airliner.Classes.Find(c => c.Type == AirlinerClass.ClassType.Economy_Class);
                    economyClass.RegularSeatingCapacity += seatingDiff;

                    AirlinerFacility seatingFacility = economyClass.getFacility(AirlinerFacility.FacilityType.Seat);

                    int extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                    economyClass.SeatingCapacity += extraSeats;

                }
            }

        }
    }
}
