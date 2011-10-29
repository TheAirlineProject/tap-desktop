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

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFleet.xaml
    /// </summary>
    public partial class PageAirlineFleet : Page
    {
        private Airline Airline;
        private ListView lvFleet, lvRouteFleet;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        private StackPanel panelOverview, panelDetailed;
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
            panelFleet.Margin = new Thickness(0, 10, 50, 0);

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

            panelOverview = createOverviewPanel();

            panelDetailed = createDetailedPanel();
            panelDetailed.Visibility = System.Windows.Visibility.Collapsed;

            panelFleet.Children.Add(panelOverview);
            panelFleet.Children.Add(panelDetailed);

            this.Content = panelFleet;

           // GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlineFleet_OnTimeChanged);
        }

        private void PageAirlineFleet_OnTimeChanged()
        {
            if (this.IsLoaded)
                showFleet();
        }

        private void sbOverview_Click(object sender, RoutedEventArgs e)
        {
            panelDetailed.Visibility = System.Windows.Visibility.Collapsed;
            panelOverview.Visibility = System.Windows.Visibility.Visible;
        }

        private void sbDetailed_Click(object sender, RoutedEventArgs e)
        {
            panelOverview.Visibility = System.Windows.Visibility.Collapsed;
            panelDetailed.Visibility = System.Windows.Visibility.Visible;
        }

        //creates the overview of the fleet
        private StackPanel createOverviewPanel()
        {
            StackPanel panelOverview = new StackPanel();

            TextBlock txtFleetHeader = new TextBlock();
            txtFleetHeader.Uid = "1003";
            txtFleetHeader.Margin = new Thickness(0, 0, 0, 0);
            txtFleetHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFleetHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFleetHeader.FontWeight = FontWeights.Bold;
            txtFleetHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtFleetHeader.Uid);

            panelOverview.Children.Add(txtFleetHeader);

            lvFleet = new ListView();
            lvFleet.Background = Brushes.Transparent;
            lvFleet.SetResourceReference(ListView.ItemContainerStyleProperty, "ListViewItemStyle");
            lvFleet.MaxHeight = 400;
            lvFleet.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(FleetHeaderClickedHandler), true);
            lvFleet.BorderThickness = new Thickness(0);
            lvFleet.View = this.Resources["FleetViewItem"] as GridView;

            panelOverview.Children.Add(lvFleet);

            lvFleet.ItemsSource = this.FleetDelivered;

            TextBlock txtInOrderHeader = new TextBlock();
            txtInOrderHeader.Uid = "1004";
            txtInOrderHeader.Margin = new Thickness(0, 5, 0, 0);
            txtInOrderHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtInOrderHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtInOrderHeader.FontWeight = FontWeights.Bold;
            txtInOrderHeader.Text = Translator.GetInstance().GetString("PageAirlineFleet", txtInOrderHeader.Uid);

            panelOverview.Children.Add(txtInOrderHeader);

            ListBox lbInOrder = new ListBox();
            lbInOrder.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInOrder.ItemTemplate = this.Resources["InOrderItem"] as DataTemplate;
            lbInOrder.MaxHeight = 400;

            panelOverview.Children.Add(lbInOrder);

            List<FleetAirliner> airliners = this.Airline.Fleet;

            foreach (FleetAirliner airliner in airliners.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate > GameObject.GetInstance().GameTime; })))
                lbInOrder.Items.Add(airliner);

            return panelOverview;
        }

        //creates the fleet route details
        private StackPanel createDetailedPanel()
        {
            StackPanel panelDetailed = new StackPanel();

            TextBlock txtFleetHeader = new TextBlock();
            txtFleetHeader.Uid = "1101";
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
            //lvRouteFleet.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(FleetHeaderClickedHandler), true);
            lvRouteFleet.BorderThickness = new Thickness(0);
            lvRouteFleet.View = this.Resources["FleetRouteViewItem"] as GridView;

            panelDetailed.Children.Add(lvRouteFleet);

            //lvFleet.ItemsSource = Airports.GetAirports();

            List<FleetAirliner> fAirliners = this.Airline.Fleet;//.FindAll((delegate(FleetAirliner a) { return a.HasRoute; }));

            lvRouteFleet.ItemsSource = this.FleetDelivered;//fAirliners;

            return panelDetailed;
        }

        //shows the fleet for the airline
        private void showFleet()
        {
            ICollectionView dataViewFleet = CollectionViewSource.GetDefaultView(lvFleet.ItemsSource);
            dataViewFleet.Refresh();

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

            // PageNavigator.NavigateTo(new PagePlayerProfile(player));
        }

        private void HyperlinkAirline_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageFleetAirliner(airliner));
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Sell airliner", string.Format("Are you sure you want to sell {0}?", airliner.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    if (airliner.RouteAirliner != null)
                        airliner.RouteAirliner.Route.Airliner = null;

                    this.Airline.removeAirliner(airliner);

                    this.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airliner.Airliner.getPrice()));

                    _FleetDelivered.Remove(airliner);

                    showFleet();
                }
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Terminate leasing", string.Format("Are you sure you want to terminate the leasing of {0}?", airliner.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    if (airliner.RouteAirliner != null)
                        airliner.RouteAirliner.Route.Airliner = null;

                    this.Airline.removeAirliner(airliner);

                    _FleetDelivered.Remove(airliner);

                    showFleet();
                }
            }
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
