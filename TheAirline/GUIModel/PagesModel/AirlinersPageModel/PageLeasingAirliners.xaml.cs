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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.FilterableListView;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PassengerModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageLeasingAirliners.xaml
    /// </summary>
    public partial class PageLeasingAirliners : Page
    {
        #region Public Properties

        public ObservableCollection<AirlinerMVVM> AllAirliners { get; set; }

        public List<FilterValue> CapacityRanges { get; set; }

        public List<FilterValue> RangeRanges { get; set; }

        public List<FilterValue> RunwayRanges { get; set; }

        public ObservableCollection<AirlinerMVVM> SelectedAirliners { get; set; }

        public List<FilterValue> SpeedRanges { get; set; }

        #endregion
        public PageLeasingAirliners()
        {
            this.Loaded += this.PageLeasingAirliners_Loaded;
            Boolean isMetric = AppSettings.GetInstance().getLanguage().Unit == TheAirline.Model.GeneralModel.Language.UnitSystem.Metric;
           
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
           if (isMetric)
            {
                this.RunwayRanges = new List<FilterValue>
                                {
                                    new FilterValue("<1500", 0, 1500),
                                    new FilterValue("1500-3000",1500,3000),
                                    new FilterValue("3000+", 3000, int.MaxValue) 
                                };
            }
            else
            {
                this.RunwayRanges = new List<FilterValue>
                                {
                                    new FilterValue("<5000", 0, (int)MathHelpers.FeetToMeter(4999)),
                                    new FilterValue("5000-7999", (int)MathHelpers.FeetToMeter(5000), (int)MathHelpers.FeetToMeter(7999)),
                                    new FilterValue("8000+", (int)MathHelpers.FeetToMeter(8000), int.MaxValue) 
                                };
            }
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
                Airliner airliner in Airliners.GetAirlinersForLeasing().Where(a => a.Owner == null || !a.Owner.IsHuman && !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline, a.Type, GameObject.GetInstance().GameTime)).OrderByDescending(a => a.BuiltDate.Year).ToList())
            {
                this.AllAirliners.Add(new AirlinerMVVM(airliner));
            }

            this.SelectedAirliners = new ObservableCollection<AirlinerMVVM>();

            InitializeComponent();
        }
        #region Methods
        private void PageLeasingAirliners_Loaded(object sender, RoutedEventArgs e)
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
                            Country country = Countries.GetCountryFromTailNumber(airliner.Airliner.TailNumber);
                            if (country==null || country.Name 
                                != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                            {
                                airliner.Airliner.TailNumber =
                                    GameObject.GetInstance()
                                        .HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();
                            }

                            GameObject.GetInstance()
                                .HumanAirline.addAirliner(
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
                            airliner.Airliner.Status = Airliner.StatusTypes.Normal;
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
        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            PopUpCompareAirliners.ShowPopUp(this.SelectedAirliners[0].Airliner, this.SelectedAirliners[1].Airliner);
        }
        #endregion
    }
}
