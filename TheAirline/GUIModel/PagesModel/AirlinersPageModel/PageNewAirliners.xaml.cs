namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.FilterableListView;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageNewAirliners.xaml
    /// </summary>
    public partial class PageNewAirliners : Page
    {
        #region Constructors and Destructors

        public PageNewAirliners()
        {
            this.AllTypes = new List<AirlinerTypeMVVM>();
            this.SelectedAirliners = new ObservableCollection<AirlinerTypeMVVM>();

            this.RangeRanges = new List<FilterValue>
                               {
                                   new FilterValue("<1500", 0, 1499),
                                   new FilterValue("1500-2999", 1500, 2999),
                                   new FilterValue("3000-5999", 3000, 5999),
                                   new FilterValue("6000+", 6000, int.MaxValue)
                               };
            this.SpeedRanges = new List<FilterValue>
                               {
                                   new FilterValue("<400", 0, 399),
                                   new FilterValue("400-599", 400, 599),
                                   new FilterValue("600+", 600, int.MaxValue)
                               };
            this.RunwayRanges = new List<FilterValue>
                                {
                                    new FilterValue("<5000", 0, 4999),
                                    new FilterValue("5000-7999", 5000, 7999),
                                    new FilterValue("8000+", 8000, int.MaxValue)
                                };
            this.CapacityRanges = new List<FilterValue>
                                  {
                                      new FilterValue("<100", 0, 99),
                                      new FilterValue("100-199", 100, 199),
                                      new FilterValue("200-299", 200, 299),
                                      new FilterValue("300-399", 300, 399),
                                      new FilterValue("400-499", 400, 499),
                                      new FilterValue("500+", 500, int.MaxValue)
                                  };

            this.AllTypes = new List<AirlinerTypeMVVM>();
            AirlinerTypes.GetTypes(
                t =>
                    t.Produced.From <= GameObject.GetInstance().GameTime
                    && t.Produced.To > GameObject.GetInstance().GameTime)
                .ForEach(t => this.AllTypes.Add(new AirlinerTypeMVVM(t)));

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<AirlinerTypeMVVM> AllTypes { get; set; }

        public List<FilterValue> CapacityRanges { get; set; }

        public List<FilterValue> RangeRanges { get; set; }

        public List<FilterValue> RunwayRanges { get; set; }

        public ObservableCollection<AirlinerTypeMVVM> SelectedAirliners { get; set; }

        public List<FilterValue> SpeedRanges { get; set; }

        #endregion

        #region Methods

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer =
                    this.SelectedAirliners.FirstOrDefault(
                        a => a.Type.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer) == null;

                if (sameManufaturer)
                {
                    contractedOrder = true;
                }
                else
                {
                    double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2010"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2010", "message"),
                                GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name,
                                terminationFee),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            GameObject.GetInstance().HumanAirline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -terminationFee);
                        GameObject.GetInstance().HumanAirline.Contract = null;
                    }
                    tryOrder = result == WPFMessageBoxResult.Yes;
                }
            }

            if (tryOrder)
            {
                int totalAmount = this.SelectedAirliners.Count;
                double price = this.SelectedAirliners.Sum(a => a.Type.Price);

                double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

                if (contractedOrder)
                {
                    totalPrice = totalPrice * ((100 - GameObject.GetInstance().HumanAirline.Contract.Discount) / 100);
                }

                if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2001"),
                        Translator.GetInstance().GetString("MessageBox", "2001", "message"),
                        WPFMessageBoxButtons.Ok);
                }
                else
                {
                    var cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = HorizontalAlignment.Left;
                    cbHomebase.Width = 200;

                    long minRunway = this.SelectedAirliners.Max(a => a.Type.MinRunwaylength);

                    List<Airport> homebases = AirlineHelpers.GetHomebases(
                        GameObject.GetInstance().HumanAirline,
                        minRunway);

                    foreach (Airport airport in homebases)
                    {
                        cbHomebase.Items.Add(airport);
                    }

                    cbHomebase.SelectedIndex = 0;

                    if (
                        PopUpSingleElement.ShowPopUp(
                            Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1014"),
                            cbHomebase) == PopUpSingleElement.ButtonSelected.OK && cbHomebase.SelectedItem != null)
                    {
                        var airport = cbHomebase.SelectedItem as Airport;

                        this.orderAirliners(
                            airport,
                            contractedOrder ? GameObject.GetInstance().HumanAirline.Contract.Discount : 0);

                        if (contractedOrder)
                        {
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners += totalAmount;
                        }

                        var sList = new List<AirlinerTypeMVVM>(this.SelectedAirliners);
                        foreach (AirlinerTypeMVVM type in sList)
                        {
                            type.IsSelected = false;
                        }

                        this.SelectedAirliners.Clear();
                    }
                }
            }
        }

        private void cbCompare_Checked(object sender, RoutedEventArgs e)
        {
            var airliner = (AirlinerTypeMVVM)((CheckBox)sender).Tag;
            airliner.IsSelected = true;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbCompare_Unchecked(object sender, RoutedEventArgs e)
        {
            var airliner = (AirlinerTypeMVVM)((CheckBox)sender).Tag;
            airliner.IsSelected = false;

            this.SelectedAirliners.Remove(airliner);
        }

        //returns a date for delivery based on the aircraft production rate
        private DateTime getDeliveryDate()
        {
            var latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerTypeMVVM type in this.SelectedAirliners)
            {
                var date = new DateTime(
                    GameObject.GetInstance().GameTime.Year,
                    GameObject.GetInstance().GameTime.Month,
                    GameObject.GetInstance().GameTime.Day);
                int rate = type.Type.ProductionRate;
                if (1 <= (rate / 4))
                {
                    date = date.AddMonths(3);
                }
                else
                {
                    for (int i = (rate / 4) + 1; i <= 1; i++)
                    {
                        double iRate = 365 / rate;
                        date = date.AddDays(Math.Round(iRate, 0, MidpointRounding.AwayFromZero));
                    }
                }

                if (date > latestDate)
                {
                    latestDate = date;
                }
            }

            return latestDate;
        }

        private void orderAirliners(Airport airport, double discount = 0)
        {
            DateTime deliveryDate = this.getDeliveryDate();

            Guid id = Guid.NewGuid();

            foreach (AirlinerTypeMVVM type in this.SelectedAirliners)
            {
                var airliner = new Airliner(
                    id.ToString(),
                    type.Type,
                    GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber(),
                    deliveryDate);

                EngineType engine = EngineTypes.GetStandardEngineType(type.Type,GameObject.GetInstance().GameTime.Year);

                if (engine != null)
                    airliner.EngineType = engine;

                Airliners.AddAirliner(airliner);

                var pType = FleetAirliner.PurchasedType.Bought;
                GameObject.GetInstance().HumanAirline.addAirliner(pType, airliner, airport);

                airliner.clearAirlinerClasses();

                AirlinerHelpers.CreateAirlinerClasses(airliner);

                foreach (AirlinerClass aClass in airliner.Classes)
                {
                    foreach (AirlinerFacility facility in aClass.getFacilities())
                    {
                        aClass.setFacility(GameObject.GetInstance().HumanAirline, facility);
                    }
                }
         
            }

            int totalAmount = this.SelectedAirliners.Count;
            double price = this.SelectedAirliners.Sum(a => a.Type.Price);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)))
                                * ((100 - discount) / 100);

            AirlineHelpers.AddAirlineInvoice(
                GameObject.GetInstance().HumanAirline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Purchases,
                -totalPrice);
        }

        #endregion
    }
}