namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    using AirlinerClassMVVM = TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel.AirlinerClassMVVM;

    /// <summary>
    ///     Interaction logic for PageAirlineEditAirliners.xaml
    /// </summary>
    public partial class PageAirlineEditAirliners : Page
    {
        #region Fields

        private readonly FleetAirliner Airliner;

        private readonly List<FleetAirliner> Airliners;

        #endregion

        #region Constructors and Destructors

        public PageAirlineEditAirliners(List<FleetAirliner> airliners)
        {
            this.Airliners = airliners;

            this.Loaded += this.PageAirlineEditAirliners_Loaded;

            this.DataContext = this.Airliners;

            this.Classes = new ObservableCollection<AirlinerClassMVVM>();
            this.Airliner = this.getMinimumAirliner();

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
            {
                int maxCapacity;

                if (this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                {
                    maxCapacity = ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxSeatingCapacity;
                }
                else
                {
                    maxCapacity = 100;
                }

                Boolean changeable = this.Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats;

                if (this.Airliner.Airliner.Classes.Count == 3)
                {
                    if (this.Airliner.Airliner.Classes.IndexOf(aClass) == 1)
                    {
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[2].RegularSeatingCapacity;
                    }
                    else
                    {
                        maxSeats = maxCapacity - 1 - this.Airliner.Airliner.Classes[1].RegularSeatingCapacity;
                    }
                }
                else
                {
                    maxSeats = maxCapacity - 1;
                }

                var amClass = new AirlinerClassMVVM(
                    aClass,
                    aClass.SeatingCapacity,
                    aClass.RegularSeatingCapacity,
                    maxSeats,
                    changeable);
                this.Classes.Add(amClass);
            }

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }

        #endregion

        //returns the minimum of the selected airliners

        #region Methods

        private void PageAirlineEditAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tcMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliners").FirstOrDefault();

                matchingItem.Visibility = Visibility.Visible;

                matchingItem.IsSelected = true;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2128"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2128", "message")),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (FleetAirliner airliner in this.Airliners)
                {
                    airliner.Airliner.ClearAirlinerClasses();

                    foreach (AirlinerClassMVVM aClass in this.Classes)
                    {
                        var newClass = new AirlinerClass(aClass.Type, aClass.Seating);

                        airliner.Airliner.AddAirlinerClass(newClass);

                        foreach (AirlinerFacilityMVVM facility in aClass.Facilities)
                        {
                            newClass.ForceSetFacility(facility.SelectedFacility);
                        }
                    }

                    int totalSeats = this.Classes.Sum(c => c.RegularSeatingCapacity);

                    int seatingDiff = ((AirlinerPassengerType)airliner.Airliner.Type).MaxSeatingCapacity - totalSeats;

                    AirlinerClass economyClass =
                        airliner.Airliner.Classes.Find(c => c.Type == AirlinerClass.ClassType.EconomyClass);
                    economyClass.RegularSeatingCapacity += seatingDiff;

                    AirlinerFacility seatingFacility = economyClass.GetFacility(AirlinerFacility.FacilityType.Seat);

                    var extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                    economyClass.SeatingCapacity += extraSeats;
                }
            }
        }

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (
                AirlinerConfiguration confItem in
                    Configurations.GetConfigurations(Configuration.ConfigurationType.Airliner)
                        .Where(
                            a =>
                                ((AirlinerConfiguration)a).MinimumSeats <= this.Airliner.Airliner.GetTotalSeatCapacity()
                                && ((AirlinerConfiguration)a).Classes.Count
                                <= ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxAirlinerClasses))
            {
                cbConfigurations.Items.Add(confItem);
            }

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineWages", "1013"),
                cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                this.Classes.Clear();

                var configuration = (AirlinerConfiguration)cbConfigurations.SelectedItem;

                foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                {
                    var nClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                    var tClass = new AirlinerClassMVVM(
                        nClass,
                        aClass.SeatingCapacity,
                        aClass.RegularSeatingCapacity,
                        aClass.RegularSeatingCapacity);

                    foreach (AirlinerFacility facility in aClass.GetFacilities())
                    {
                        tClass.Facilities.First(f => f.Type == facility.Type).SelectedFacility = facility;
                    }

                    foreach (
                        AirlinerFacility.FacilityType fType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                    {
                        if (!aClass.Facilities.Exists(f => f.Type == fType))
                        {
                            tClass.Facilities.First(f => f.Type == fType).SelectedFacility =
                                AirlinerFacilities.GetBasicFacility(fType);
                        }
                    }

                    this.Classes.Add(tClass);
                }
                int seatingDiff = ((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxSeatingCapacity
                                  - configuration.MinimumSeats;

                AirlinerClassMVVM economyClass = this.Classes.First(
                    c => c.Type == AirlinerClass.ClassType.EconomyClass);

                economyClass.RegularSeatingCapacity += seatingDiff;

                AirlinerFacility seatingFacility =
                    economyClass.Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility;

                var extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                economyClass.Seating += extraSeats;
            }
        }

        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            int totalServiceLevel = this.Classes.Sum(c => c.Facilities.Sum(f => f.SelectedFacility.ServiceLevel));
            var txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("Custom configuration ({0} classes)", this.Classes.Count);
            txtName.HorizontalAlignment = HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineEditAirliners", "1002"),
                txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                var configuration = new AirlinerConfiguration(
                    name,
                    this.getMinimumAirliner().Airliner.GetTotalSeatCapacity(),
                    false);

                foreach (AirlinerClassMVVM type in this.Classes)
                {
                    var classConfiguration = new AirlinerClassConfiguration(
                        type.Type,
                        type.Seating,
                        type.RegularSeatingCapacity);

                    foreach (AirlinerFacilityMVVM facility in type.Facilities)
                    {
                        classConfiguration.AddFacility(facility.SelectedFacility);
                    }

                    configuration.AddClassConfiguration(classConfiguration);
                }

                Configurations.AddConfiguration(configuration);
            }
        }

        private FleetAirliner getMinimumAirliner()
        {
            FleetAirliner minAirliner = this.Airliners[0];

            foreach (FleetAirliner airliner in this.Airliners)
            {
                if (((AirlinerPassengerType)airliner.Airliner.Type).MaxAirlinerClasses
                    < ((AirlinerPassengerType)minAirliner.Airliner.Type).MaxAirlinerClasses)
                {
                    minAirliner = airliner;
                }
                else
                {
                    if (((AirlinerPassengerType)airliner.Airliner.Type).MaxAirlinerClasses
                        == ((AirlinerPassengerType)minAirliner.Airliner.Type).MaxAirlinerClasses
                        && airliner.Airliner.Classes[0].SeatingCapacity
                        < minAirliner.Airliner.Classes[0].SeatingCapacity)
                    {
                        minAirliner = airliner;
                    }
                }
            }

            return minAirliner;
        }

        #endregion
    }
}