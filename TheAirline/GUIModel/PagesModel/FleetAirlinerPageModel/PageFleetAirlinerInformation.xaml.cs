namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.PilotModel;

    /// <summary>
    ///     Interaction logic for PageFleetAirlinerInformation.xaml
    /// </summary>
    public partial class PageFleetAirlinerInformation : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerInformation(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;

            this.InRoute = this.Airliner.Airliner.Status != FleetAirliner.AirlinerStatus.Stopped;

            this.DataContext = this.Airliner;
            this.Loaded += this.PageFleetAirlinerInformation_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public Boolean InRoute { get; set; }

        #endregion

        #region Methods

        private void PageFleetAirlinerInformation_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    AirlinerFacility facility =
                        this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).getFacility(aFacility.Type);
                    aFacility.SelectedFacility = facility;
                }
            }
        }

        private void btnAddClass_Click(object sender, RoutedEventArgs e)
        {
            var cbClasses = new ComboBox();
            cbClasses.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbClasses.ItemTemplate = this.Resources["AirlinerClassItem"] as DataTemplate;
            cbClasses.HorizontalAlignment = HorizontalAlignment.Left;
            cbClasses.Width = 200;

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                Boolean hasClass = this.Airliner.Classes.ToList().Exists(c => c.Type == type);
                if ((int)type <= GameObject.GetInstance().GameTime.Year && !hasClass)
                {
                    cbClasses.Items.Add(type);
                }
            }

            cbClasses.SelectedIndex = 0;

            AirlinerClassMVVM tClass = this.Airliner.Classes[0];

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1011"),
                cbClasses) == PopUpSingleElement.ButtonSelected.OK && cbClasses.SelectedItem != null)
            {
                int maxseats;

                int maxCapacity;

                if (this.Airliner.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                {
                    maxCapacity = ((AirlinerPassengerType)this.Airliner.Airliner.Airliner.Type).MaxSeatingCapacity;
                }
                else
                {
                    maxCapacity = tClass.RegularSeatingCapacity;
                }

                if (this.Airliner.Classes.Count == 2)
                {
                    maxseats = maxCapacity - 1 - this.Airliner.Classes[1].RegularSeatingCapacity;
                }
                else
                {
                    maxseats = maxCapacity - 1;
                }

                var aClass = new AirlinerClassMVVM(
                    new AirlinerClass((AirlinerClass.ClassType)cbClasses.SelectedItem, 0),
                    1,
                    1,
                    maxseats,
                    true);

                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    AirlinerFacility facility = AirlinerFacilities.GetBasicFacility(aFacility.Type);
                    aFacility.SelectedFacility = facility;
                }

                this.Airliner.Classes.Add(aClass);

                tClass.RegularSeatingCapacity -= aClass.RegularSeatingCapacity;

                tClass.Seating =
                    Convert.ToInt16(
                        Convert.ToDouble(tClass.RegularSeatingCapacity)
                        / tClass.Facilities.Where(f => f.Type == AirlinerFacility.FacilityType.Seat)
                            .First()
                            .SelectedFacility.SeatUses);
            }
        }

        private void btnAddPilot_Click(object sender, RoutedEventArgs e)
        {
            var cbPilots = new ComboBox();
            cbPilots.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbPilots.DisplayMemberPath = "Profile.Name";
            cbPilots.SelectedValuePath = "Profile.Name";
            cbPilots.HorizontalAlignment = HorizontalAlignment.Left;
            cbPilots.Width = 200;

            foreach (
                Pilot pilot in
                    this.Airliner.Airliner.Airliner.Airline.Pilots.Where(
                        p =>
                            p.Airliner == null
                            && p.Aircrafts.Contains(this.Airliner.Airliner.Airliner.Type.AirlinerFamily)))
            {
                cbPilots.Items.Add(pilot);
            }

            cbPilots.SelectedIndex = 0;

            AirlinerClassMVVM tClass = this.Airliner.Classes[0];

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1013"),
                cbPilots) == PopUpSingleElement.ButtonSelected.OK && cbPilots.SelectedItem != null)
            {
                var pilot = (Pilot)cbPilots.SelectedItem;
                this.Airliner.addPilot(pilot);
            }
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            if (this.Airliner.Airliner.Airliner.getPrice() > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2006"),
                    Translator.GetInstance().GetString("MessageBox", "2006", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2007"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2007", "message"),
                        new ValueCurrencyConverter().Convert(this.Airliner.Airliner.Airliner.getPrice())),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airliner.buyAirliner();
                }
            }
        }
        private void btnOutlease_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2016"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2016", "message"),
                        new ValueCurrencyConverter().Convert(this.Airliner.Airliner.Name)),
                    WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airliner.Airliner.Airliner.Status = Model.AirlinerModel.Airliner.StatusTypes.Leasing;

                PageNavigator.NavigateTo(new PageAirline(this.Airliner.Airliner.Airliner.Airline));
            }
        }
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            double price =
                AirlinerHelpers.GetCargoConvertingPrice(this.Airliner.Airliner.Airliner.Type as AirlinerPassengerType);

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2015"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2015", "message")),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2014"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2014", "message"),
                        new ValueCurrencyConverter().Convert(price)),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airliner.convertToCargo();

                    AirlineHelpers.AddAirlineInvoice(
                        this.Airliner.Airliner.Airliner.Airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -price);
                }
            }
        }

        private void btnDeletePilot_Click(object sender, RoutedEventArgs e)
        {
            var pilot = (Pilot)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2125"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2125", "message"), pilot.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airliner.removePilot(pilot);
            }
        }

        private void btnEditHomebase_Click(object sender, RoutedEventArgs e)
        {
            var cbHomebase = new ComboBox();
            cbHomebase.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
            cbHomebase.HorizontalAlignment = HorizontalAlignment.Left;
            cbHomebase.Width = 200;

            foreach (
                Airport airport in
                    AirlineHelpers.GetHomebases(
                        GameObject.GetInstance().HumanAirline,
                        this.Airliner.Airliner.Airliner.Type))
            {
                cbHomebase.Items.Add(airport);
            }

            cbHomebase.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1014"),
                cbHomebase) == PopUpSingleElement.ButtonSelected.OK && cbHomebase.SelectedItem != null)
            {
                var homebase = cbHomebase.SelectedItem as Airport;
                this.Airliner.Homebase = homebase;
            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            this.Airliner.Airliner.Airliner.clearAirlinerClasses();

            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                var nClass = new AirlinerClass(aClass.Type, aClass.RegularSeatingCapacity);
                nClass.SeatingCapacity = aClass.Seating;

                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    nClass.forceSetFacility(aFacility.SelectedFacility);
                }

                this.Airliner.Airliner.Airliner.addAirlinerClass(nClass);
            }
        }

        private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            var allclasses = new List<AirlinerClassMVVM>(this.Airliner.Classes);

            foreach (
                AirlinerClassMVVM aClass in
                    allclasses.Where(c => !this.Airliner.Airliner.Airliner.Classes.Exists(ac => ac.Type == c.Type)))
            {
                this.Airliner.Classes.Remove(aClass);
            }

            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                aClass.RegularSeatingCapacity =
                    this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).RegularSeatingCapacity;
                aClass.Seating = this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).SeatingCapacity;

                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    AirlinerFacility facility =
                        this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).getFacility(aFacility.Type);
                    aFacility.SelectedFacility = facility;
                }
            }
        }

        private void slSeats_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var aClass = (AirlinerClassMVVM)((Slider)sender).Tag;

            if (aClass.Type != AirlinerClass.ClassType.Economy_Class && !aClass.ChangedFacility)
            {
                var diff = (int)(e.NewValue - e.OldValue);

                this.Airliner.Classes[0].RegularSeatingCapacity -= diff;
                this.Airliner.Classes[0].Seating =
                    Convert.ToInt16(
                        Convert.ToDouble(this.Airliner.Classes[0].RegularSeatingCapacity)
                        / this.Airliner.Classes[0].Facilities.Where(f => f.Type == AirlinerFacility.FacilityType.Seat)
                            .First()
                            .SelectedFacility.SeatUses);

                if (this.Airliner.Classes.Count == 3)
                {
                    if (this.Airliner.Classes[1] == aClass)
                    {
                        //this.Airliner.Classes[2].RegularSeatingCapacity -= diff;
                        this.Airliner.Classes[2].MaxSeats -=
                            Convert.ToInt16(
                                Convert.ToDouble(diff)
                                / this.Airliner.Classes[2].Facilities.Where(
                                    f => f.Type == AirlinerFacility.FacilityType.Seat).First().SelectedFacility.SeatUses);
                    }
                    else
                    {
                        //this.Airliner.Classes[1].RegularSeatingCapacity -= diff;
                        this.Airliner.Classes[1].MaxSeats -=
                            Convert.ToInt16(
                                Convert.ToDouble(diff)
                                / this.Airliner.Classes[2].Facilities.Where(
                                    f => f.Type == AirlinerFacility.FacilityType.Seat).First().SelectedFacility.SeatUses);
                    }
                }
            }
            /*
            if (aClass.Type != AirlinerClass.ClassType.Economy_Class)
            {
                double diff = (e.NewValue - e.OldValue);// *aClass.Facilities.Find(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility.SeatUses;

              
                if (this.Airliner.Classes.Count == 3)
                {
                    if (this.Airliner.Classes[1] == aClass)
                    {
                       // this.Airliner.Classes[2].RegularSeatingCapacity -= Convert.ToInt16(diff);
                        this.Airliner.Classes[2].MaxSeats = Convert.ToInt16(Convert.ToDouble(this.Airliner.Classes[2].RegularSeatingCapacity) / this.Airliner.Classes[2].Facilities.Where(f => f.Type == AirlinerFacility.FacilityType.Seat).First().SelectedFacility.SeatUses);
           
                    }
                    
                    if (this.Airliner.Classes[2] == aClass)
                    {
                       // this.Airliner.Classes[1].RegularSeatingCapacity -= Convert.ToInt16(diff);
                        this.Airliner.Classes[1].MaxSeats = Convert.ToInt16(Convert.ToDouble(this.Airliner.Classes[1].RegularSeatingCapacity) / this.Airliner.Classes[1].Facilities.Where(f => f.Type == AirlinerFacility.FacilityType.Seat).First().SelectedFacility.SeatUses);
                    }
                }

                AirlinerClassMVVM tClass = this.Airliner.Classes[0];

                tClass.RegularSeatingCapacity -= Convert.ToInt16(diff);

                tClass.Seating = Convert.ToInt16(Convert.ToDouble(tClass.RegularSeatingCapacity) / tClass.Facilities.Where(f => f.Type == AirlinerFacility.FacilityType.Seat).First().SelectedFacility.SeatUses);
            }*/
        }

        #endregion
    }
}