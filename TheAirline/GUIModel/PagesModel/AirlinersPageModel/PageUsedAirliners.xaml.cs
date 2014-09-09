namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.FilterableListView;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageUsedAirliners.xaml
    /// </summary>
    public partial class PageUsedAirliners : Page
    {
        #region Constructors and Destructors

        public PageUsedAirliners()
        {
            Boolean isMetric = AppSettings.GetInstance().getLanguage().Unit == TheAirline.Model.GeneralModel.Language.UnitSystem.Metric;
            this.Loaded += this.PageUsedAirliners_Loaded;
            this.Unloaded += this.PageUsedAirliners_Unloaded;
      
            this.RangeRanges = new List<FilterValue>
                               {
                                   new FilterValue("<1500", 0, isMetric ? 1499 : (int)MathHelpers.MilesToKM(1499)),
                                   new FilterValue("1500-2999", isMetric ? 1500 : (int)MathHelpers.MilesToKM(1500), isMetric ? 2999 : (int)MathHelpers.MilesToKM(2999)),
                                   new FilterValue("3000-5999", isMetric ? 3000 : (int)MathHelpers.MilesToKM(3000), isMetric ? 5999 : (int)MathHelpers.MilesToKM(5999)),
                                   new FilterValue("6000+", isMetric ? 600 : (int)MathHelpers.MilesToKM(6000), int.MaxValue)
                               };
            this.SpeedRanges = new List<FilterValue>
                               {
                                   new FilterValue("<400",  0,isMetric ? 399 : (int)MathHelpers.MilesToKM(399)),
                                   new FilterValue("400-599", isMetric ? 400 : (int)MathHelpers.MilesToKM(400), isMetric ? 599 : (int)MathHelpers.MilesToKM(599)),
                                   new FilterValue("600+", isMetric ? 600 : (int)MathHelpers.MilesToKM(600), int.MaxValue)
                               };
            this.RunwayRanges = new List<FilterValue>
                                {
                                    new FilterValue("<5000", 0, isMetric ? 4999 : (int)MathHelpers.FeetToMeter(4999)),
                                    new FilterValue("5000-7999", isMetric ? 5000 : (int)MathHelpers.FeetToMeter(5000), isMetric ? 7999 : (int)MathHelpers.FeetToMeter(7999)),
                                    new FilterValue("8000+", isMetric ? 8000 : (int)MathHelpers.FeetToMeter(8000), int.MaxValue)
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

            this.AllAirliners = new ObservableCollection<AirlinerMVVM>();
            foreach (
                Airliner airliner in Airliners.GetAirlinersForSale().OrderByDescending(a => a.BuiltDate.Year).ToList())
            {
                this.AllAirliners.Add(new AirlinerMVVM(airliner));
            }

            this.SelectedAirliners = new ObservableCollection<AirlinerMVVM>();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerMVVM> AllAirliners { get; set; }

        public List<FilterValue> CapacityRanges { get; set; }

        public List<FilterValue> RangeRanges { get; set; }

        public List<FilterValue> RunwayRanges { get; set; }

        public ObservableCollection<AirlinerMVVM> SelectedAirliners { get; set; }

        public List<FilterValue> SpeedRanges { get; set; }

        #endregion

        #region Methods

        private void PageUsedAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Manufacturer").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }

            Hashtable filters = ((PageAirliners)this.Tag).AirlinersFilters;

            if (filters != null)
            {
                this.lvAirliners.setCurrentFilters(filters);
            }
        }

        private void PageUsedAirliners_Unloaded(object sender, RoutedEventArgs e)
        {
            Hashtable filters = this.lvAirliners.getCurrentFilters();

            var parent = (PageAirliners)this.Tag;

            parent.AirlinersFilters = filters;
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            double totalPrice = this.SelectedAirliners.Sum(a => a.Airliner.getPrice());

            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer =
                    this.SelectedAirliners.FirstOrDefault(
                        a => a.Airliner.Type.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer)
                    == null;

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

            if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2001"),
                    Translator.GetInstance().GetString("MessageBox", "2001", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else

            {
                if (tryOrder)
                {
                    var cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = HorizontalAlignment.Left;
                    cbHomebase.Width = 300;

                    long minRunway = this.SelectedAirliners.Max(a => a.Airliner.Type.MinRunwaylength);

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

                        var selectedAirliners = new List<AirlinerMVVM>(this.SelectedAirliners);
                        foreach (AirlinerMVVM airliner in selectedAirliners)
                        {
                            if (contractedOrder)
                            {
                                AirlineHelpers.BuyAirliner(
                                    GameObject.GetInstance().HumanAirline,
                                    airliner.Airliner,
                                    airport,
                                    GameObject.GetInstance().HumanAirline.Contract.Discount);
                            }
                            else
                            {
                                AirlineHelpers.BuyAirliner(
                                    GameObject.GetInstance().HumanAirline,
                                    airliner.Airliner,
                                    airport);
                            }

                            if (contractedOrder)
                            {
                                GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;
                            }

                            airliner.IsSelected = false;
                            this.SelectedAirliners.Remove(airliner);
                            this.AllAirliners.Remove(airliner);
                        }
                    }
                    else
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2002"),
                            Translator.GetInstance().GetString("MessageBox", "2002", "message"),
                            WPFMessageBoxButtons.Ok);
                    }
                }
            }
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            PopUpCompareAirliners.ShowPopUp(this.SelectedAirliners[0].Airliner, this.SelectedAirliners[1].Airliner);
        }

        private void btnLease_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            double totalLeasingPrice = this.SelectedAirliners.Sum(a => a.Airliner.getLeasingPrice() * 2);

            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer =
                    this.SelectedAirliners.FirstOrDefault(
                        a => a.Airliner.Type.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer)
                    == null;
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

            if (totalLeasingPrice > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2004"),
                    Translator.GetInstance().GetString("MessageBox", "2004", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (tryOrder)
                {
                    var cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = HorizontalAlignment.Left;
                    cbHomebase.Width = 300;

                    long minRunway = this.SelectedAirliners.Max(a => a.Airliner.Type.MinRunwaylength);

                    List<Airport> homebases =
                        GameObject.GetInstance()
                            .HumanAirline.Airports.FindAll(
                                a =>
                                    (a.hasContractType(
                                        GameObject.GetInstance().HumanAirline,
                                        AirportContract.ContractType.Full_Service)
                                     || a.getCurrentAirportFacility(
                                         GameObject.GetInstance().HumanAirline,
                                         AirportFacility.FacilityType.Service).TypeLevel > 0)
                                    && a.getMaxRunwayLength() >= minRunway);
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

                        var selectedAirliners = new List<AirlinerMVVM>(this.SelectedAirliners);
                        
                        foreach (AirlinerMVVM airliner in selectedAirliners)
                        {
                            if (Countries.GetCountryFromTailNumber(airliner.Airliner.TailNumber).Name
                                != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                            {
                                airliner.Airliner.TailNumber =
                                    GameObject.GetInstance()
                                        .HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();
                            }

                            GameObject.GetInstance()
                                .HumanAirline.AddAirliner(
                                    FleetAirliner.PurchasedType.Leased,
                                    airliner.Airliner,
                                    airport);

                            AirlineHelpers.AddAirlineInvoice(
                                GameObject.GetInstance().HumanAirline,
                                GameObject.GetInstance().GameTime,
                                Invoice.InvoiceType.Rents,
                                -airliner.Airliner.LeasingPrice * 2);

                            if (contractedOrder)
                            {
                                GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;
                            }

                            this.SelectedAirliners.Remove(airliner);
                            this.AllAirliners.Remove(airliner);
                        }
                    }
                    else
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2002"),
                            Translator.GetInstance().GetString("MessageBox", "2002", "message"),
                            WPFMessageBoxButtons.Ok);
                    }
                }
            }
        }

        private void cbCompare_Checked(object sender, RoutedEventArgs e)
        {
            var airliner = (AirlinerMVVM)((CheckBox)sender).Tag;
            airliner.IsSelected = true;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbCompare_Unchecked(object sender, RoutedEventArgs e)
        {
            var airliner = (AirlinerMVVM)((CheckBox)sender).Tag;
            airliner.IsSelected = false;

            this.SelectedAirliners.Remove(airliner);
        }

        private void cbPossibleHomebase_Checked(object sender, RoutedEventArgs e)
        {
            //var homebases = AirlineHelpers.GetHomebases(GameObject.GetInstance().HumanAirline,);
            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirlinerMVVM;

                Boolean isPossible =
                    GameObject.GetInstance()
                        .HumanAirline.Airports.FindAll(
                            ai =>
                                ai.getCurrentAirportFacility(
                                    GameObject.GetInstance().HumanAirline,
                                    AirportFacility.FacilityType.Service).TypeLevel > 0
                                && ai.getMaxRunwayLength() >= a.Airliner.Type.MinRunwaylength)
                        .Count > 0;

                return isPossible;
            };

            this.SelectedAirliners.Clear();
        }

        private void cbPossibleHomebase_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirlinerMVVM;
                return true;
            };

            this.SelectedAirliners.Clear();
        }

        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (AirlinerMVVM)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                matchingItem.Header = airliner.Airliner.TailNumber;
                matchingItem.Visibility = Visibility.Visible;
                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageUsedAirliner(airliner.Airliner) { Tag = this.Tag });
            }
        }

        #endregion
    }
}