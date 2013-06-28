using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFleet.xaml
    /// </summary>
    public partial class PageAirlineFleet : Page
    {
        private Airline Airline;
        private ListView lvBoughtFleet, lvRouteFleet, lvLeasedFleet;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        private StackPanel panelOverview, panelDetailed, panelOrdered;
        private ObservableCollection<FleetAirliner> _FleetDelivered = new ObservableCollection<FleetAirliner>();
        public ObservableCollection<FleetAirliner> FleetDelivered
        { get { return _FleetDelivered; } }

        public PageAirlineFleet(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            foreach (FleetAirliner fa in this.Airline.DeliveredFleet)
                _FleetDelivered.Add(fa);

            StackPanel panelFleet = new StackPanel();
            
            WrapPanel panelMenuButtons = new WrapPanel();
            panelFleet.Children.Add(panelMenuButtons);

            ucSelectButton sbOverview = new ucSelectButton();
            sbOverview.Uid = "1001";
            sbOverview.Content = Translator.GetInstance().GetString("PageAirlineFleet", sbOverview.Uid);
            sbOverview.Click += new RoutedEventHandler(sbOverview_Click);
            sbOverview.IsSelected = true;
            panelMenuButtons.Children.Add(sbOverview);

            ucSelectButton sbDetailed = new ucSelectButton();
            sbDetailed.Uid = "1002";
            sbDetailed.Content = Translator.GetInstance().GetString("PageAirlineFleet", sbDetailed.Uid);
            sbDetailed.Click += new RoutedEventHandler(sbDetailed_Click);
            panelMenuButtons.Children.Add(sbDetailed);

            ucSelectButton sbOrdered = new ucSelectButton();
            sbOrdered.Uid = "1009";
            sbOrdered.Content = Translator.GetInstance().GetString("PageAirlineFleet", sbOrdered.Uid);
            sbOrdered.Click += new RoutedEventHandler(sbOrdered_Click);
            panelMenuButtons.Children.Add(sbOrdered);


            panelOverview = createOverviewPanel();

            panelDetailed = createDetailedPanel();
            panelDetailed.Visibility = System.Windows.Visibility.Collapsed;
            panelDetailed.ToolTip = UICreator.CreateToolTip("View detailed information for this aircraft");

            panelOrdered = createOrderedPanel();
            panelOrdered.Visibility = System.Windows.Visibility.Collapsed;
            panelOrdered.ToolTip = UICreator.CreateToolTip("View ordered airliners");

            panelFleet.Children.Add(panelOverview);
            panelFleet.Children.Add(panelDetailed);
            panelFleet.Children.Add(panelOrdered);



            this.Content = panelFleet;

         }

     
        private void PageAirlineFleet_OnTimeChanged()
        {
            if (this.IsLoaded)
                showFleet();
        }
        private void sbOrdered_Click(object sender, RoutedEventArgs e)
        {
            panelDetailed.Visibility = System.Windows.Visibility.Collapsed;
            panelOverview.Visibility = System.Windows.Visibility.Collapsed;
            panelOrdered.Visibility = System.Windows.Visibility.Visible;
        }

        private void sbOverview_Click(object sender, RoutedEventArgs e)
        {
            panelDetailed.Visibility = System.Windows.Visibility.Collapsed;
            panelOverview.Visibility = System.Windows.Visibility.Visible;
            panelOrdered.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void sbDetailed_Click(object sender, RoutedEventArgs e)
        {
            panelOverview.Visibility = System.Windows.Visibility.Collapsed;
            panelDetailed.Visibility = System.Windows.Visibility.Visible;
            panelOrdered.Visibility = System.Windows.Visibility.Collapsed;
   
        }
        //creates the panel for the airliners in order
        private StackPanel createOrderedPanel()
        {
            StackPanel panelInOrder = new StackPanel();

            TextBlock txtFleetHeader = new TextBlock();
            txtFleetHeader.Uid = "1003";
            txtFleetHeader.Margin = new Thickness(0, 0, 0, 0);
            txtFleetHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFleetHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFleetHeader.FontWeight = FontWeights.Bold;
            txtFleetHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtFleetHeader.Uid);

            panelInOrder.Children.Add(txtFleetHeader);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["OrderedHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            panelInOrder.Children.Add(txtHeader);

            ScrollViewer svOrdered = new ScrollViewer();
            svOrdered.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svOrdered.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svOrdered.MaxHeight = 400;
       
            ListBox lbInOrder = new ListBox();
            lbInOrder.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInOrder.ItemTemplate = this.Resources["InOrderItem"] as DataTemplate;

            svOrdered.Content = lbInOrder;

            panelInOrder.Children.Add(svOrdered);

            List<FleetAirliner> airliners = this.Airline.Fleet;

            foreach (FleetAirliner airliner in airliners.FindAll((a=>a.Airliner.BuiltDate>GameObject.GetInstance().GameTime)))
                lbInOrder.Items.Add(airliner);

            return panelInOrder;
        }
        //creates the overview of the fleet
        private StackPanel createOverviewPanel()
        {
            StackPanel panelOverview = new StackPanel();

            TextBlock txtFleetBoughtHeader = new TextBlock();
            txtFleetBoughtHeader.Uid = "1003";
            txtFleetBoughtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtFleetBoughtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFleetBoughtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFleetBoughtHeader.FontWeight = FontWeights.Bold;
            txtFleetBoughtHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtFleetBoughtHeader.Uid);

            panelOverview.Children.Add(txtFleetBoughtHeader);

            lvBoughtFleet = new ListView();
            lvBoughtFleet.Background = Brushes.Transparent;
            lvBoughtFleet.SetResourceReference(ListView.ItemContainerStyleProperty, "ListViewItemStyle");
            lvBoughtFleet.MaxHeight = (GraphicsHelpers.GetContentHeight()-100) / 2;
            lvBoughtFleet.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(FleetHeaderClickedHandler), true);
            lvBoughtFleet.BorderThickness = new Thickness(0);
            lvBoughtFleet.View = this.Resources["FleetViewBoughtItem"] as GridView;
           
            panelOverview.Children.Add(lvBoughtFleet);

            lvBoughtFleet.ItemsSource = this.FleetDelivered.Where(f=>f.Purchased == FleetAirliner.PurchasedType.Bought || f.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment);

            TextBlock txtFleetLeasedHeader = new TextBlock();
            txtFleetLeasedHeader.Uid = "1012";
            txtFleetLeasedHeader.Margin = new Thickness(0, 5, 0, 0);
            txtFleetLeasedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFleetLeasedHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFleetLeasedHeader.FontWeight = FontWeights.Bold;
            txtFleetLeasedHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtFleetLeasedHeader.Uid);

            panelOverview.Children.Add(txtFleetLeasedHeader);

            lvLeasedFleet = new ListView();
            lvLeasedFleet.Background = Brushes.Transparent;
            lvLeasedFleet.SetResourceReference(ListView.ItemContainerStyleProperty, "ListViewItemStyle");
            lvLeasedFleet.MaxHeight = (GraphicsHelpers.GetContentHeight()-100) / 2;
            lvLeasedFleet.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(FleetHeaderClickedHandler), true);
            lvLeasedFleet.View = this.Resources["FleetViewLeasedItem"] as GridView;

            panelOverview.Children.Add(lvLeasedFleet);

            lvLeasedFleet.ItemsSource = this.FleetDelivered.Where(f => f.Purchased == FleetAirliner.PurchasedType.Leased);

            return panelOverview;
        }

        //creates the fleet route details
        private StackPanel createDetailedPanel()
        {
            StackPanel panelDetailed = new StackPanel();

            TextBlock txtFleetHeader = new TextBlock();
            txtFleetHeader.Uid = "1011";
            txtFleetHeader.Margin = new Thickness(0, 0, 0, 0);
            txtFleetHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFleetHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFleetHeader.FontWeight = FontWeights.Bold;
            txtFleetHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtFleetHeader.Uid);

            panelDetailed.Children.Add(txtFleetHeader);

            lvRouteFleet = new ListView();
            lvRouteFleet.Background = Brushes.Transparent;
            lvRouteFleet.SetResourceReference(ListView.ItemContainerStyleProperty, "ListViewItemStyle");
            lvRouteFleet.MaxHeight = 400;
            lvRouteFleet.BorderThickness = new Thickness(0);
            lvRouteFleet.View = this.Resources["FleetRouteViewItem"] as GridView;

            panelDetailed.Children.Add(lvRouteFleet);

            List<FleetAirliner> fAirliners = this.Airline.Fleet;

            lvRouteFleet.ItemsSource = this.FleetDelivered;

            return panelDetailed;
        }

        //shows the fleet for the airline
        private void showFleet()
        {
            ICollectionView dataViewFleet = CollectionViewSource.GetDefaultView(lvBoughtFleet.ItemsSource);
            dataViewFleet.Refresh();

            ICollectionView dataViewLeased = CollectionViewSource.GetDefaultView(lvLeasedFleet.ItemsSource);
            dataViewLeased.Refresh();

            ICollectionView dataViewFleetRoute = CollectionViewSource.GetDefaultView(lvRouteFleet.ItemsSource);
            dataViewFleetRoute.Refresh();
        }

        private void FleetHeaderClickedHandler(object sender,
                                    RoutedEventArgs e)
        {
           //sort route 
            if (e.OriginalSource is GridViewColumnHeader)
            {
                string name = "Airliner.Type.Name";

                if (((GridViewColumnHeader)e.OriginalSource).Content != null)
                {
                    switch (((GridViewColumnHeader)e.OriginalSource).Content.ToString())
                    {
                        case "Name":
                            name = "Name";
                            break;
                        case "Type":
                            name = "Airliner.Type.Name";
                            break;
                        case "Purchased":
                            name = "Purchased";
                            break;
                        case "Route":
                            name = "HasRoute";
                            break;
                    }
                }

                sortDirection = sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(name, sortDirection);
                dataView.SortDescriptions.Add(sd);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));

        }

        private void HyperlinkAirline_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageFleetAirliner(airliner));
        }
        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            ComboBox cbAirlines = new ComboBox();
            cbAirlines.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirlines.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbAirlines.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            cbAirlines.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbAirlines.Items.Add(airline);

            cbAirlines.Items.Remove(airliner.Airliner.Airline);

            cbAirlines.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineFleet", "1010"), cbAirlines) == PopUpSingleElement.ButtonSelected.OK && cbAirlines.SelectedItem != null)
            {
                Airline airline = (Airline)cbAirlines.SelectedItem;
                airliner.Airliner.Airline.removeAirliner(airliner);

                airliner.Airliner.Airline = airline;
                airline.addAirliner(airliner);

                _FleetDelivered.Remove(airliner);

                showFleet();

            }
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2122"), string.Format(Translator.GetInstance().GetString("MessageBox", "2122", "message")), WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2106"), string.Format(Translator.GetInstance().GetString("MessageBox", "2106", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }

                        this.Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airliner.Airliner.getPrice());


                        _FleetDelivered.Remove(airliner);

                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();

                        showFleet();

                    }
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2107"), string.Format(Translator.GetInstance().GetString("MessageBox", "2107", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }


                        this.Airline.removeAirliner(airliner);

                        _FleetDelivered.Remove(airliner);


                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();

                        showFleet();

                    }
                }
            }
        }
    }
    //the converter for transfering airliners between subsidairy
    public class TransferAirlinerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FleetAirliner airliner = (FleetAirliner)value;

            Boolean isTransferable = airliner.Airliner.Airline.IsHuman && airliner.Status == FleetAirliner.AirlinerStatus.Stopped && GameObject.GetInstance().MainAirline.Subsidiaries.Count > 0;

            return isTransferable ? Visibility.Visible : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a filling degree to color
   public class FillingDegreeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double amount = (double)value;

                if (amount < 0.30)
                    return Brushes.DarkRed;
                else if (amount >= 0.30 && amount < 0.70)
                    return Brushes.White;
                else
                    return Brushes.DarkGreen;
            }
            catch
            {
                return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
