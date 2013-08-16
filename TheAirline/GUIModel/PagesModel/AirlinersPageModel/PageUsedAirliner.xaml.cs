using System;
using System.Collections.Generic;
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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageUsedAirliner.xaml
    /// </summary>
    public partial class PageUsedAirliner : Page
    {
        public Airliner Airliner { get; set; }
        public List<Airport> Homebases { get; set; }
        public PageUsedAirliner(Airliner airliner)
        {
            
            this.Airliner = airliner;
            this.Homebases = GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= this.Airliner.Type.MinRunwaylength);
         
            this.DataContext = this.Airliner;

            InitializeComponent();

 
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Airliner.Type.Manufacturer)
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

            Airport airport = (Airport)cbHomebase.SelectedItem;

            if (this.Airliner.getPrice() > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2006"), Translator.GetInstance().GetString("MessageBox", "2006", "message"), WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

            else
            {
                
                if (tryOrder)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2007"), string.Format(Translator.GetInstance().GetString("MessageBox", "2007", "message"), this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (contractedOrder)
                            AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, this.Airliner, airport, GameObject.GetInstance().HumanAirline.Contract.Discount);
                        else
                            AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, this.Airliner, airport);


                        if (contractedOrder)
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;

                         TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                         if (tab_main != null)
                         {
                             var matchingItem =
                  tab_main.Items.Cast<TabItem>()
                    .Where(item => item.Tag.ToString() == "Used")
                    .FirstOrDefault();

                             tab_main.SelectedItem = matchingItem;
                         }
                    }
                }
            }
        }

        private void btnLease_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Airliner.Type.Manufacturer)
                {
                    contractedOrder = true;

                }
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

            Airport airport = (Airport)cbHomebase.SelectedItem;

            if (this.Airliner.getLeasingPrice() * 2 > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2004"), Translator.GetInstance().GetString("MessageBox", "2004", "message"), WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

            else
            {
                if (tryOrder)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2005"), string.Format(Translator.GetInstance().GetString("MessageBox", "2005", "message"), this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (Countries.GetCountryFromTailNumber(this.Airliner.TailNumber).Name != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                            this.Airliner.TailNumber = GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();

                        GameObject.GetInstance().HumanAirline.addAirliner(FleetAirliner.PurchasedType.Leased, this.Airliner, airport);

                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -this.Airliner.LeasingPrice * 2);

                        if (contractedOrder)
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;
                       
                        TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");
                                          
                        if (tab_main != null)
                        {
                            var matchingItem =
                 tab_main.Items.Cast<TabItem>()
                   .Where(item => item.Tag.ToString() == "Used")
                   .FirstOrDefault();

                            tab_main.SelectedItem = matchingItem;
                        }


                    }
                }
            }
        }
    }
}
