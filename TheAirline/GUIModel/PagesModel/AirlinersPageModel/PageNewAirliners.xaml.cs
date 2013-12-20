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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageNewAirliners.xaml
    /// </summary>
    public partial class PageNewAirliners : Page
    {
        public List<AirlinerType> AllTypes { get; set; }
        public ObservableCollection<AirlinerType> SelectedAirliners { get; set; }
        public PageNewAirliners()
        {
            this.AllTypes = new List<AirlinerType>();
            this.SelectedAirliners = new ObservableCollection<AirlinerType>();

            this.AllTypes = AirlinerTypes.GetTypes(t => t.Produced.From <= GameObject.GetInstance().GameTime && t.Produced.To > GameObject.GetInstance().GameTime);

            InitializeComponent();
        }

        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {


        }
        private void cbCompare_Checked(object sender, RoutedEventArgs e)
        {
            AirlinerType airliner = (AirlinerType)((CheckBox)sender).Tag;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbCompare_Unchecked(object sender, RoutedEventArgs e)
        {
            AirlinerType airliner = (AirlinerType)((CheckBox)sender).Tag;

            this.SelectedAirliners.Remove(airliner);
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                Boolean sameManufaturer = this.SelectedAirliners.FirstOrDefault(a => a.Manufacturer != GameObject.GetInstance().HumanAirline.Contract.Manufacturer) == null;

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

            if (tryOrder)
            {
                int totalAmount = this.SelectedAirliners.Count;
                double price = this.SelectedAirliners.Sum(a => a.Price);

                double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

                if (contractedOrder)
                    totalPrice = totalPrice * ((100 - GameObject.GetInstance().HumanAirline.Contract.Discount) / 100);



                if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                }
                else
                {
                    ComboBox cbHomebase = new ComboBox();
                    cbHomebase.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbHomebase.ItemTemplate = Application.Current.Resources["AirportCountryItem"] as DataTemplate;
                    cbHomebase.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbHomebase.Width = 200;

                    long minRunway = this.SelectedAirliners.Max(a => a.MinRunwaylength);

                    foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= minRunway))
                    {
                        cbHomebase.Items.Add(airport);
                    }

                    cbHomebase.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageFleetAirlinerInformation", "1014"), cbHomebase) == PopUpSingleElement.ButtonSelected.OK && cbHomebase.SelectedItem != null)
                    {
                        Airport airport = cbHomebase.SelectedItem as Airport;

                        orderAirliners(airport,contractedOrder ? GameObject.GetInstance().HumanAirline.Contract.Discount : 0);

                        if (contractedOrder)
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners += totalAmount;

                 

                    }
                }

            }

        }
        //orders the airliners
        private void orderAirliners(Airport airport, double discount = 0)
        {
            DateTime deliveryDate = getDeliveryDate();

            Guid id = Guid.NewGuid();

            foreach (AirlinerType type in this.SelectedAirliners)
            {

                Airliner airliner = new Airliner(id.ToString(), type, GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);
                Model.AirlinerModel.Airliners.AddAirliner(airliner);

                FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                GameObject.GetInstance().HumanAirline.addAirliner(pType, airliner, airport);

                airliner.clearAirlinerClasses();

                AirlinerHelpers.CreateAirlinerClasses(airliner);

                foreach (AirlinerClass aClass in airliner.Classes)
                {
                    foreach (AirlinerFacility facility in aClass.getFacilities())
                        aClass.setFacility(GameObject.GetInstance().HumanAirline, facility);
                }
             

            }

            int totalAmount = this.SelectedAirliners.Count;
            double price = this.SelectedAirliners.Sum(a => a.Price);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount))) * ((100 - discount) / 100);

            AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);

        }
        //returns a date for delivery based on the aircraft production rate
        private DateTime getDeliveryDate()
        {

            DateTime latestDate = new DateTime(1900, 1, 1);

            foreach (AirlinerType type in this.SelectedAirliners)
            {
                DateTime date = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, GameObject.GetInstance().GameTime.Day);
                int rate = type.ProductionRate;
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
                    latestDate = date;
            }


            return latestDate;


        }
    }

}
