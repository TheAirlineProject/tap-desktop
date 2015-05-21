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
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using AirlinerClassMVVM = TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel.AirlinerClassMVVM;

namespace TheAirline.Views.Airline
{
    /// <summary>
    ///     Interaction logic for PageAirlineEditAirliners.xaml
    /// </summary>
    public partial class PageAirlineEditAirliners
    {
        #region Constructors and Destructors

        public PageAirlineEditAirliners(List<FleetAirliner> airliners)
        {
            Airliners = airliners;

            Loaded += PageAirlineEditAirliners_Loaded;

            DataContext = Airliners;

            Classes = new ObservableCollection<AirlinerClassMVVM>();
            Airliner = getMinimumAirliner();

            foreach (var aClass in Airliner.Airliner.Classes)
            {
                var maxCapacity = Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger ? ((AirlinerPassengerType) Airliner.Airliner.Type).MaxSeatingCapacity : 100;

                var changeable = Airliner.Airliner.Classes.IndexOf(aClass) > 0;

                int maxSeats;

                if (Airliner.Airliner.Classes.Count == 3)
                {
                    if (Airliner.Airliner.Classes.IndexOf(aClass) == 1)
                    {
                        maxSeats = maxCapacity - 1 - Airliner.Airliner.Classes[2].RegularSeatingCapacity;
                    }
                    else
                    {
                        maxSeats = maxCapacity - 1 - Airliner.Airliner.Classes[1].RegularSeatingCapacity;
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
                Classes.Add(amClass);
            }

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerClassMVVM> Classes { get; set; }

        #endregion

        #region Fields

        private readonly FleetAirliner Airliner;

        private readonly List<FleetAirliner> Airliners;

        #endregion

        //returns the minimum of the selected airliners

        #region Methods

        private void PageAirlineEditAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
                    tab_main.Items.Cast<TabItem>().FirstOrDefault(item => item.Tag.ToString() == "Airliners");

                matchingItem.Visibility = Visibility.Visible;

                matchingItem.IsSelected = true;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2128"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2128", "message")),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (var airliner in Airliners)
                {
                    airliner.Airliner.ClearAirlinerClasses();

                    foreach (var aClass in Classes)
                    {
                        var newClass = new AirlinerClass(aClass.Type, aClass.Seating);

                        airliner.Airliner.AddAirlinerClass(newClass);

                        foreach (var facility in aClass.Facilities)
                        {
                            newClass.ForceSetFacility(facility.SelectedFacility);
                        }
                    }

                    var totalSeats = Classes.Sum(c => c.RegularSeatingCapacity);

                    var seatingDiff = ((AirlinerPassengerType) airliner.Airliner.Type).MaxSeatingCapacity - totalSeats;

                    var economyClass =
                        airliner.Airliner.Classes.Find(c => c.Type == AirlinerClass.ClassType.EconomyClass);
                    economyClass.RegularSeatingCapacity += seatingDiff;

                    var seatingFacility = economyClass.GetFacility(AirlinerFacility.FacilityType.Seat);

                    var extraSeats = (int) (seatingDiff/seatingFacility.SeatUses);

                    economyClass.SeatingCapacity += extraSeats;
                }
            }
        }

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var cbConfigurations = new ComboBox();
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
                                ((AirlinerConfiguration) a).MinimumSeats <= Airliner.Airliner.GetTotalSeatCapacity()
                                && ((AirlinerConfiguration) a).Classes.Count
                                <= ((AirlinerPassengerType) Airliner.Airliner.Type).MaxAirlinerClasses))
            {
                cbConfigurations.Items.Add(confItem);
            }

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineWages", "1013"),
                cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                Classes.Clear();

                var configuration = (AirlinerConfiguration) cbConfigurations.SelectedItem;

                foreach (var aClass in configuration.Classes)
                {
                    var nClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                    var tClass = new AirlinerClassMVVM(
                        nClass,
                        aClass.SeatingCapacity,
                        aClass.RegularSeatingCapacity,
                        aClass.RegularSeatingCapacity);

                    foreach (var facility in aClass.GetFacilities())
                    {
                        tClass.Facilities.First(f => f.Type == facility.Type).SelectedFacility = facility;
                    }

                    foreach (
                        AirlinerFacility.FacilityType fType in Enum.GetValues(typeof (AirlinerFacility.FacilityType)))
                    {
                        if (!aClass.Facilities.Exists(f => f.Type == fType))
                        {
                            tClass.Facilities.First(f => f.Type == fType).SelectedFacility =
                                AirlinerFacilities.GetBasicFacility(fType);
                        }
                    }

                    Classes.Add(tClass);
                }
                var seatingDiff = ((AirlinerPassengerType) Airliner.Airliner.Type).MaxSeatingCapacity
                                  - configuration.MinimumSeats;

                var economyClass = Classes.First(
                    c => c.Type == AirlinerClass.ClassType.EconomyClass);

                economyClass.RegularSeatingCapacity += seatingDiff;

                var seatingFacility =
                    economyClass.Facilities.First(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility;

                var extraSeats = (int) (seatingDiff/seatingFacility.SeatUses);

                economyClass.Seating += extraSeats;
            }
        }

        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var totalServiceLevel = Classes.Sum(c => c.Facilities.Sum(f => f.SelectedFacility.ServiceLevel));
            var txtName = new TextBox
            {
                Width = 200,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                Text = $"Custom configuration ({Classes.Count} classes)",
                HorizontalAlignment = HorizontalAlignment.Left
            };

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineEditAirliners", "1002"),
                txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                var name = txtName.Text.Trim();
                var configuration = new AirlinerConfiguration(
                    name,
                    getMinimumAirliner().Airliner.GetTotalSeatCapacity(),
                    false);

                foreach (var type in Classes)
                {
                    var classConfiguration = new AirlinerClassConfiguration(
                        type.Type,
                        type.Seating,
                        type.RegularSeatingCapacity);

                    foreach (var facility in type.Facilities)
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
            var minAirliner = Airliners[0];

            foreach (var airliner in Airliners)
            {
                if (((AirlinerPassengerType) airliner.Airliner.Type).MaxAirlinerClasses
                    < ((AirlinerPassengerType) minAirliner.Airliner.Type).MaxAirlinerClasses)
                {
                    minAirliner = airliner;
                }
                else
                {
                    if (((AirlinerPassengerType) airliner.Airliner.Type).MaxAirlinerClasses
                        == ((AirlinerPassengerType) minAirliner.Airliner.Type).MaxAirlinerClasses
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