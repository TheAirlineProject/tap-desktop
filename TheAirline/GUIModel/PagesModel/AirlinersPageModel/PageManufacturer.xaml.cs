using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageManufacturer.xaml
    /// </summary>
    public partial class PageManufacturer : Page
    {
        #region Constructors and Destructors

        public PageManufacturer(Manufacturer manufacturer)
        {
            this.NumberOfAirliners = new List<int>();

            for (int i = 1; i <= 50; i++)
            {
                this.NumberOfAirliners.Add(i);
            }

            this.Manufacturer = manufacturer;

            this.Orders = new AirlinerOrdersMVVM();

            this.Airliners = new ObservableCollection<AirlinerType>();
            AirlinerTypes.GetTypes(
                a =>
                    a.Manufacturer == manufacturer && a.Produced.From <= GameObject.GetInstance().GameTime
                    && a.Produced.To >= GameObject.GetInstance().GameTime).ForEach(t => this.Airliners.Add(t));
            this.DataContext = this.Airliners;

            this.Contract = new ManufacturerContractMVVM(
                this.Manufacturer,
                GameObject.GetInstance().HumanAirline.Contract != null
                    ? GameObject.GetInstance().HumanAirline.Contract.Manufacturer
                    : null);

            this.InitializeComponent();

            this.lvAirliners.ItemsSource = this.Airliners;

            var view = (CollectionView)CollectionViewSource.GetDefaultView(this.lvAirliners.ItemsSource);
            view.GroupDescriptions.Clear();

            var groupDescription = new PropertyGroupDescription("AirlinerFamily");
            view.GroupDescriptions.Add(groupDescription);
        }

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinerType> Airliners { get; set; }

        public ManufacturerContractMVVM Contract { get; set; }

        public Manufacturer Manufacturer { get; set; }

        public List<int> NumberOfAirliners { get; set; }

        public AirlinerOrdersMVVM Orders { get; set; }

        #endregion

        #region Methods

        private void btnAddType_Click(object sender, RoutedEventArgs e)
        {
            var type = (AirlinerType)((Button)sender).Tag;

            this.Orders.addOrder(new AirlinerOrderMVVM(type, this.Orders));
        }

        private void btnEquipped_Click(object sender, RoutedEventArgs e)
        {
            var order = (AirlinerOrderMVVM)((Button)sender).Tag;

            var confObject = (AirlinerConfigurationObject)PopUpAirlinerSeatsConfiguration.ShowPopUp(order.Type, order.Classes,order.Engine);

            if (confObject != null)
            {
                order.Classes = confObject.Classes;
                order.Engine = confObject.Engine;
            }
        }

        private void btnExtendContract_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2011"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2011", "message"),
                    GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                int nLength = Convert.ToInt16(((ComboBoxItem)this.cbContractLenth.SelectedItem).Tag);

                int length = GameObject.GetInstance().HumanAirline.Contract.Length + nLength;

                double discount =
                    AirlineHelpers.GetAirlineManufactorerDiscountFactor(
                        GameObject.GetInstance().HumanAirline,
                        length,
                        true);

                GameObject.GetInstance().HumanAirline.Contract.Length = length;
                GameObject.GetInstance().HumanAirline.Contract.Discount = discount;
                GameObject.GetInstance().HumanAirline.Contract.Airliners = length;
                GameObject.GetInstance().HumanAirline.Contract.ExpireDate =
                    GameObject.GetInstance().HumanAirline.Contract.ExpireDate.AddYears(nLength);
            }
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            Boolean hasHomebases = true;
            foreach (AirlinerOrderMVVM order in this.Orders.Orders)
            {
                if (order.Homebase == null)
                {
                    hasHomebases = false;
                }
            }
            Boolean contractedOrder = false;
            Boolean tryOrder = true;
             DateTime deliveryDate = this.dpDeliveryDate.SelectedDate.HasValue
                ? this.dpDeliveryDate.SelectedDate.Value
                : this.Orders.DeliveryDate;

            if (!hasHomebases)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2002"),
                    Translator.GetInstance().GetString("MessageBox", "2002", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (GameObject.GetInstance().HumanAirline.Contract != null)
                {
                    if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Manufacturer)
                    {
                        contractedOrder = true;
                    }
                    else
                    {
                        double terminationFee = GameObject.GetInstance().HumanAirline.Contract.GetTerminationFee();
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

                            this.Contract.Contracted = null;
                        }
                        tryOrder = result == WPFMessageBoxResult.Yes;
                    }
                }

                if (tryOrder)
                {
                    int totalAmount = this.Orders.Orders.Sum(o => o.Amount);
                    double price = this.Orders.Orders.Sum(o => o.getOrderPrice());

                    double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));

                    if (contractedOrder)
                    {
                        totalPrice = totalPrice
                                     * ((100 - GameObject.GetInstance().HumanAirline.Contract.Discount) / 100);
                    }

                    double downpaymentPrice = 0;

                    downpaymentPrice = totalPrice * (GameObject.GetInstance().Difficulty.PriceLevel / 10);

                    if (this.cbDownPayment.IsChecked.Value)
                    {
                        if (downpaymentPrice > GameObject.GetInstance().HumanAirline.Money)
                        {
                            WPFMessageBox.Show(
                                Translator.GetInstance().GetString("MessageBox", "2001"),
                                Translator.GetInstance().GetString("MessageBox", "2001", "message"),
                                WPFMessageBoxButtons.Ok);
                        }
                        else
                        {
                            WPFMessageBoxResult result =
                                WPFMessageBox.Show(
                                    Translator.GetInstance().GetString("MessageBox", "2009"),
                                    string.Format(
                                        Translator.GetInstance().GetString("MessageBox", "2009", "message"),
                                        totalPrice,
                                        downpaymentPrice),
                                    WPFMessageBoxButtons.YesNo);

                            if (result == WPFMessageBoxResult.Yes)
                            {
                                foreach (AirlinerOrderMVVM order in this.Orders.Orders)
                                {
                                    for (int i = 0; i < order.Amount; i++)
                                    {
                                        Guid id = Guid.NewGuid();

                                        var airliner = new Airliner(
                                            id.ToString(),
                                            order.Type,
                                            GameObject.GetInstance()
                                                .HumanAirline.Profile.Country.TailNumbers.GetNextTailNumber(),
                                            deliveryDate);

                                        airliner.ClearAirlinerClasses();

                                        foreach (AirlinerClass aClass in order.Classes)
                                        {
                                            var tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                                            tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                                            foreach (AirlinerFacility facility in aClass.GetFacilities())
                                            {
                                                tClass.SetFacility(GameObject.GetInstance().HumanAirline, facility);
                                            }

                                            airliner.AddAirlinerClass(tClass);
                                        }

                                        airliner.EngineType = order.Engine;

                                        Models.Airliners.Airliners.AddAirliner(airliner);

                                        var pType = FleetAirliner.PurchasedType.BoughtDownPayment;
                                        GameObject.GetInstance()
                                            .HumanAirline.AddAirliner(pType, airliner, order.Homebase);

                                       
                                    }
                                }
                                if (contractedOrder)
                                {
                                    GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners +=
                                        this.Orders.Orders.Sum(o => o.Amount);
                                }

                                AirlineHelpers.AddAirlineInvoice(
                                    GameObject.GetInstance().HumanAirline,
                                    GameObject.GetInstance().GameTime,
                                    Invoice.InvoiceType.Purchases,
                                    -downpaymentPrice);

                                var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                                if (tab_main != null)
                                {
                                    TabItem matchingItem =
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
                            WPFMessageBox.Show(
                                Translator.GetInstance().GetString("MessageBox", "2001"),
                                Translator.GetInstance().GetString("MessageBox", "2001", "message"),
                                WPFMessageBoxButtons.Ok);
                        }
                        else
                        {
                            if (this.Orders.Orders.Sum(o => o.Amount) > 0)
                            {
                                WPFMessageBoxResult result =
                                    WPFMessageBox.Show(
                                        Translator.GetInstance().GetString("MessageBox", "2008"),
                                        string.Format(
                                            Translator.GetInstance().GetString("MessageBox", "2008", "message"),
                                            totalPrice),
                                        WPFMessageBoxButtons.YesNo);

                                if (result == WPFMessageBoxResult.Yes)
                                {
                                    this.orderAirliners(
                                        contractedOrder ? GameObject.GetInstance().HumanAirline.Contract.Discount : 0);

                                    var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                                    if (tab_main != null)
                                    {
                                        TabItem matchingItem =
                                            tab_main.Items.Cast<TabItem>()
                                                .Where(item => item.Tag.ToString() == "Order")
                                                .FirstOrDefault();

                                        tab_main.SelectedItem = matchingItem;
                                    }
                                }

                                if (contractedOrder)
                                {
                                    GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners +=
                                        this.Orders.Orders.Sum(o => o.Amount);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnRemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = (AirlinerOrderMVVM)((Button)sender).Tag;

            this.Orders.Orders.Remove(order);
        }

        //orders the airliners

        private void btnSignContract_Click(object sender, RoutedEventArgs e)
        {
            Boolean newContract = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                double terminationFee = GameObject.GetInstance().HumanAirline.Contract.GetTerminationFee();
                WPFMessageBoxResult result = WPFMessageBox.Show(
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
                else
                {
                    newContract = false;
                }
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2013"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2013", "message"),
                        this.Manufacturer.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.No)
                {
                    newContract = false;
                }
            }

            if (newContract)
            {
                int length = Convert.ToInt16(((ComboBoxItem)this.cbContractLenth.SelectedItem).Tag);

                double discount =
                    AirlineHelpers.GetAirlineManufactorerDiscountFactor(
                        GameObject.GetInstance().HumanAirline,
                        length,
                        true);

                var contract = new ManufacturerContract(
                    this.Manufacturer,
                    GameObject.GetInstance().GameTime,
                    length,
                    discount);
                GameObject.GetInstance().HumanAirline.Contract = contract;

                this.Contract.Contracted = contract.Manufacturer;
            }
        }

        private void cbNumberOfAirliners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbAmount = (ComboBox)sender;

            var order = (AirlinerOrderMVVM)cbAmount.Tag;

            order.Amount = (int)cbAmount.SelectedItem;
        }

        private void orderAirliners(double discount = 0)
        {
            DateTime deliveryDate = this.dpDeliveryDate.SelectedDate.HasValue
                ? this.dpDeliveryDate.SelectedDate.Value
                : this.Orders.DeliveryDate;

            Guid id = Guid.NewGuid();

            foreach (AirlinerOrderMVVM order in this.Orders.Orders)
            {
                for (int i = 0; i < order.Amount; i++)
                {
                    var airliner = new Airliner(
                        id.ToString(),
                        order.Type,
                        GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.GetNextTailNumber(),
                        deliveryDate);
                    Models.Airliners.Airliners.AddAirliner(airliner);

                    var pType = FleetAirliner.PurchasedType.Bought;
                    GameObject.GetInstance().HumanAirline.AddAirliner(pType, airliner, order.Homebase);

                    airliner.ClearAirlinerClasses();

                    foreach (AirlinerClass aClass in order.Classes)
                    {
                        var tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.GetFacilities())
                        {
                            tClass.SetFacility(GameObject.GetInstance().HumanAirline, facility);
                        }

                        airliner.AddAirlinerClass(tClass);
                    }

                    airliner.EngineType = order.Engine;
             
                }
            }

            int totalAmount = this.Orders.Orders.Sum(o => o.Amount);
            double price = this.Orders.Orders.Sum(o => o.getOrderPrice());

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