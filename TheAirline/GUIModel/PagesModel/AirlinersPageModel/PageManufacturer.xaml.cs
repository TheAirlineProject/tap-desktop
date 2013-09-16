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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageManufacturer.xaml
    /// </summary>
    public partial class PageManufacturer : Page
    {
        public Manufacturer Manufacturer { get; set; }
        public ObservableCollection<AirlinerType> Airliners { get; set; }
        public AirlinerOrdersMVVM Orders { get; set; }
        public ManufacturerContractMVVM Contract { get; set; }
        public List<int> NumberOfAirliners { get; set; }
        public PageManufacturer(Manufacturer manufacturer)
        {
            this.NumberOfAirliners = new List<int>();

            for (int i = 1; i <= 50; i++)
                this.NumberOfAirliners.Add(i);

            this.Manufacturer = manufacturer;

            this.Orders = new AirlinerOrdersMVVM();

            this.Airliners = new ObservableCollection<AirlinerType>();
            AirlinerTypes.GetTypes(a => a.Manufacturer == manufacturer && a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime).ForEach(t => this.Airliners.Add(t));
            this.DataContext = this.Airliners;

            this.Contract = new ManufacturerContractMVVM(this.Manufacturer, GameObject.GetInstance().HumanAirline.Contract != null ? GameObject.GetInstance().HumanAirline.Contract.Manufacturer : null);
           
            InitializeComponent();

            lvAirliners.ItemsSource = this.Airliners;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvAirliners.ItemsSource);
            view.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("AirlinerFamily");
            view.GroupDescriptions.Add(groupDescription);

      

        }

        private void btnAddType_Click(object sender, RoutedEventArgs e)
        {
            AirlinerType type = (AirlinerType)((Button)sender).Tag;

            this.Orders.addOrder(new AirlinerOrderMVVM(type));
        }
        /*
        private void btnAddToOrder_Click(object sender, RoutedEventArgs e)
        {
            AirlinerOrderMVVM order = (AirlinerOrderMVVM)((Button)sender).Tag;

            order.Amount++;
        }
        */
        private void btnRemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            AirlinerOrderMVVM order = (AirlinerOrderMVVM)((Button)sender).Tag;

            this.Orders.Orders.Remove(order);
        }
        private void btnEquipped_Click(object sender, RoutedEventArgs e)
        {
            AirlinerOrderMVVM order = (AirlinerOrderMVVM)((Button)sender).Tag;

            List<AirlinerClass> classes = (List<AirlinerClass>)PopUpAirlinerConfiguration.ShowPopUp(order.Type, order.Classes);
            //seats + Video/audio + 
            if (classes != null)
            {
                order.Classes = classes;


            }
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            Boolean hasHomebases = true;
            foreach (AirlinerOrderMVVM order in this.Orders.Orders)
            {
                if (order.Homebase == null)
                    hasHomebases = false;
            }
            Boolean contractedOrder = false;
            Boolean tryOrder = true;

            DateTime deliveryDate = this.Orders.getDeliveryDate();

            if (!hasHomebases)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (GameObject.GetInstance().HumanAirline.Contract != null)
                {
                    if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Manufacturer)
                        contractedOrder = true;
                    else
                    {
                        double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                        WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                        if (result == WPFMessageBoxResult.Yes)
                        {
                            AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);
                            GameObject.GetInstance().HumanAirline.Contract = null;

                            this.Contract.Contracted = null;

                        }
                        tryOrder = result == WPFMessageBoxResult.Yes;
                    }
                }

                if (tryOrder)
                {
                    int totalAmount = this.Orders.Orders.Sum(o => o.Amount);
                    double price = this.Orders.Orders.Sum(o => o.Type.Price * o.Amount);

                    double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

                    if (contractedOrder)
                        totalPrice = totalPrice * ((100 - GameObject.GetInstance().HumanAirline.Contract.Discount) / 100);

                    double downpaymentPrice = 0;

                    downpaymentPrice = totalPrice * (GameObject.GetInstance().Difficulty.PriceLevel / 10);

                    if (cbDownPayment.IsChecked.Value)
                    {

                        if (downpaymentPrice > GameObject.GetInstance().HumanAirline.Money)
                        {
                            WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                        }
                        else
                        {
                            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2009"), string.Format(Translator.GetInstance().GetString("MessageBox", "2009", "message"), totalPrice, downpaymentPrice), WPFMessageBoxButtons.YesNo);

                            if (result == WPFMessageBoxResult.Yes)
                            {
                                foreach (AirlinerOrderMVVM order in this.Orders.Orders)
                                {
                                    for (int i = 0; i < order.Amount; i++)
                                    {
                                        Guid id = Guid.NewGuid();

                                        Airliner airliner = new Airliner(id.ToString(), order.Type, GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);

                                        airliner.clearAirlinerClasses();

                                        foreach (AirlinerClass aClass in order.Classes)
                                        {
                                            AirlinerClass tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                                            tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                                            foreach (AirlinerFacility facility in aClass.getFacilities())
                                                tClass.setFacility(GameObject.GetInstance().HumanAirline, facility);

                                            airliner.addAirlinerClass(tClass);
                                        }


                                        Model.AirlinerModel.Airliners.AddAirliner(airliner);

                                        FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.BoughtDownPayment;
                                        GameObject.GetInstance().HumanAirline.addAirliner(pType, airliner, order.Homebase);


                                    }




                                }
                                if (contractedOrder)
                                    GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners += this.Orders.Orders.Sum(o => o.Amount);
                                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -downpaymentPrice);

                                TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                                if (tab_main != null)
                                {
                                    var matchingItem =
                         tab_main.Items.Cast<TabItem>()
                           .Where(item => item.Tag.ToString() == "Order")
                           .FirstOrDefault();

                                    tab_main.SelectedItem = matchingItem;
                                }
                            }
                        }
                    }
                    else
                    {

                        if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
                        {
                            WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                        }
                        else
                        {
                            if (this.Orders.Orders.Sum(o => o.Amount) > 0)
                            {
                                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2008"), string.Format(Translator.GetInstance().GetString("MessageBox", "2008", "message"), totalPrice), WPFMessageBoxButtons.YesNo);

                                if (result == WPFMessageBoxResult.Yes)
                                {
                                    orderAirliners(contractedOrder ? GameObject.GetInstance().HumanAirline.Contract.Discount : 0);

                                    TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                                    if (tab_main != null)
                                    {
                                        var matchingItem =
                             tab_main.Items.Cast<TabItem>()
                               .Where(item => item.Tag.ToString() == "Order")
                               .FirstOrDefault();

                                        tab_main.SelectedItem = matchingItem;
                                    }
                                }

                                if (contractedOrder)
                                    GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners += this.Orders.Orders.Sum(o => o.Amount);

                            }
                        }
                    }
                }

            }

        }
        //orders the airliners
        private void orderAirliners(double discount = 0)
        {
            DateTime deliveryDate = this.Orders.getDeliveryDate();

            Guid id = Guid.NewGuid();

            foreach (AirlinerOrderMVVM order in this.Orders.Orders)
            {
                for (int i = 0; i < order.Amount; i++)
                {
                    Airliner airliner = new Airliner(id.ToString(), order.Type, GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber(), deliveryDate);
                    Model.AirlinerModel.Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    GameObject.GetInstance().HumanAirline.addAirliner(pType, airliner, order.Homebase);

                    airliner.clearAirlinerClasses();

                    foreach (AirlinerClass aClass in order.Classes)
                    {
                        AirlinerClass tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            tClass.setFacility(GameObject.GetInstance().HumanAirline, facility);

                        airliner.addAirlinerClass(tClass);
                    }


                }



            }

            int totalAmount = this.Orders.Orders.Sum(o => o.Amount);
            double price = this.Orders.Orders.Sum(o => o.Type.Price * o.Amount);

            double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount))) * ((100 - discount) / 100);

            AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);

        }

        private void btnSignContract_Click(object sender, RoutedEventArgs e)
        {
            Boolean newContract = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {

                double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);
                    GameObject.GetInstance().HumanAirline.Contract = null;
                }
                else
                    newContract = false;

            }
            else
            {
                  WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2013"), string.Format(Translator.GetInstance().GetString("MessageBox", "2013", "message"), this.Manufacturer.Name), WPFMessageBoxButtons.YesNo);

                  if (result == WPFMessageBoxResult.No)
                  {
                      newContract = false;
                  }
            }

            if (newContract)
            {

                int length = Convert.ToInt16(((ComboBoxItem)cbContractLenth.SelectedItem).Tag);

                double discount = AirlineHelpers.GetAirlineManufactorerDiscountFactor(GameObject.GetInstance().HumanAirline, length, true);

                ManufacturerContract contract = new ManufacturerContract(this.Manufacturer, GameObject.GetInstance().GameTime, length, discount);
                GameObject.GetInstance().HumanAirline.Contract = contract;

                this.Contract.Contracted = contract.Manufacturer;
            }
  
        }

        private void btnExtendContract_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2011"), string.Format(Translator.GetInstance().GetString("MessageBox", "2011", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {

                int nLength = Convert.ToInt16(((ComboBoxItem)cbContractLenth.SelectedItem).Tag);

                int length = GameObject.GetInstance().HumanAirline.Contract.Length + nLength;

                double discount = AirlineHelpers.GetAirlineManufactorerDiscountFactor(GameObject.GetInstance().HumanAirline, length, true);

                GameObject.GetInstance().HumanAirline.Contract.Length = length;
                GameObject.GetInstance().HumanAirline.Contract.Discount = discount;
                GameObject.GetInstance().HumanAirline.Contract.Airliners = length;
                GameObject.GetInstance().HumanAirline.Contract.ExpireDate = GameObject.GetInstance().HumanAirline.Contract.ExpireDate.AddYears(nLength);

            }
        }

        private void cbNumberOfAirliners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbAmount = (ComboBox)sender;

            AirlinerOrderMVVM order = (AirlinerOrderMVVM)cbAmount.Tag;

            order.Amount = (int)cbAmount.SelectedItem ;
        }
    }
}
