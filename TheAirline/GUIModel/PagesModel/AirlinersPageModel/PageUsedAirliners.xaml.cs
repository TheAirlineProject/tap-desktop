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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageUsedAirliners.xaml
    /// </summary>
    public partial class PageUsedAirliners : Page
    {
        public ObservableCollection<Airliner> AllAirliners { get; set; }
        public ObservableCollection<Airliner> SelectedAirliners { get; set; }
        public PageUsedAirliners()
        {
            this.Loaded += PageUsedAirliners_Loaded;
            this.Unloaded += PageUsedAirliners_Unloaded;

            this.AllAirliners = new ObservableCollection<Airliner>();
            foreach (Airliner airliner in Airliners.GetAirlinersForSale().OrderByDescending(a => a.BuiltDate.Year).ToList())
                this.AllAirliners.Add(airliner);

            this.SelectedAirliners = new ObservableCollection<Airliner>();

            InitializeComponent();



        }

        private void PageUsedAirliners_Unloaded(object sender, RoutedEventArgs e)
        {
            var filters = this.lvAirliners.getCurrentFilters();

            PageAirliners parent = (PageAirliners)this.Tag;

            parent.AirlinersFilters = filters;
        }

        private void PageUsedAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Manufacturer")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;

                matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }

            var filters = ((PageAirliners)this.Tag).AirlinersFilters;

            if (filters != null)
                this.lvAirliners.setCurrentFilters(filters);

        }

        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((Hyperlink)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                matchingItem.Header = airliner.TailNumber;
                matchingItem.Visibility = System.Windows.Visibility.Visible;
                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageUsedAirliner(airliner) { Tag = this.Tag });
        }

        private void cbPossibleHomebase_Checked(object sender, RoutedEventArgs e)
        {

            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                Airliner a = o as Airliner;

                Boolean isPossible = GameObject.GetInstance().HumanAirline.Airports.FindAll(ai => ai.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && ai.getMaxRunwayLength() >= a.Type.MinRunwaylength).Count > 0;

                return isPossible;
            };

            this.SelectedAirliners.Clear();
        }

        private void cbPossibleHomebase_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                Airliner a = o as Airliner;
                return true;
            };

            this.SelectedAirliners.Clear();
        }

        private void cbCompare_Checked(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbCompare_Unchecked(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Remove(airliner);
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            PopUpCompareAirliners.ShowPopUp(this.SelectedAirliners[0], this.SelectedAirliners[1]);

        }
        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            double totalPrice = this.SelectedAirliners.Sum(a => a.getPrice());


            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer = this.SelectedAirliners.FirstOrDefault(a => a.Type.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer) == null;
                
                if (sameManufaturer)
                    contractedOrder = true;
                else
                {
                    double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);
                        GameObject.GetInstance().HumanAirline.Contract = null;


                    }
                    tryOrder = result == WPFMessageBoxResult.Yes;
                }
            }


           if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
                        {
                            WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                        }
                        else
                        
            {
                if (tryOrder)
                {
                    ComboBox cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbHomebase.Width = 200;

                    long minRunway = this.SelectedAirliners.Max(a => a.Type.MinRunwaylength);

                    foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= minRunway))
                    {
                        cbHomebase.Items.Add(airport);
                    }

                    cbHomebase.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1014"), cbHomebase) == PopUpSingleElement.ButtonSelected.OK && cbHomebase.SelectedItem != null)
                    {
                        Airport airport = cbHomebase.SelectedItem as Airport;

                        var selectedAirliners = new List<Airliner>(this.SelectedAirliners);
                        foreach (Airliner airliner in selectedAirliners)
                        {
                            if (contractedOrder)
                                AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, airliner, airport, GameObject.GetInstance().HumanAirline.Contract.Discount);
                            else
                                AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, airliner, airport);

                            if (contractedOrder)
                                GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;

                            this.SelectedAirliners.Remove(airliner);
                            this.AllAirliners.Remove(airliner);

                        }
                    }
                    else
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

                }

            }
        }

        private void btnLease_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            double totalLeasingPrice = this.SelectedAirliners.Sum(a => a.getLeasingPrice() * 2);


            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer = this.SelectedAirliners.FirstOrDefault(a => a.Type.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer) == null;
                if (sameManufaturer)
                    contractedOrder = true;
                else
                {
                    double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);
                        GameObject.GetInstance().HumanAirline.Contract = null;


                    }
                    tryOrder = result == WPFMessageBoxResult.Yes;
                }
            }


            if (totalLeasingPrice > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2004"), Translator.GetInstance().GetString("MessageBox", "2004", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                if (tryOrder)
                {
                    ComboBox cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbHomebase.Width = 200;

                    long minRunway = this.SelectedAirliners.Max(a => a.Type.MinRunwaylength);

                    foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= minRunway))
                    {
                        cbHomebase.Items.Add(airport);
                    }

                    cbHomebase.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1014"), cbHomebase) == PopUpSingleElement.ButtonSelected.OK && cbHomebase.SelectedItem != null)
                    {
                        Airport airport = cbHomebase.SelectedItem as Airport;

                        var selectedAirliners = new List<Airliner>(this.SelectedAirliners);
                        foreach (Airliner airliner in selectedAirliners)
                        {
                            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                                airliner.TailNumber = GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();

                            GameObject.GetInstance().HumanAirline.addAirliner(FleetAirliner.PurchasedType.Leased, airliner, airport);

                            AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.LeasingPrice * 2);

                            if (contractedOrder)
                                GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;

                            this.SelectedAirliners.Remove(airliner);
                            this.AllAirliners.Remove(airliner);
                        }
                    }
                    else
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

                }



            }


        }
    }
}
