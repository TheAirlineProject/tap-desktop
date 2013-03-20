using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageAirliner.xaml
    /// </summary>
    public partial class PageAirliners : StandardPage
    {
        private ListBox lbUsedAirliners, lbManufacturers;
        private Func<Airliner,object> sortCriteriaUsed;
        private Boolean sortDescending;
        private Frame sideFrame;
        private List<Airliner> airliners;
        private AirlinerType.TypeOfAirliner airlinerType;
        public PageAirliners()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirliners", this.Uid);
           
            
            airlinerType = AirlinerType.TypeOfAirliner.Passenger;
            sortCriteriaUsed = a => a.BuiltDate;
            sortDescending = true;

            StackPanel airlinersPanel = new StackPanel();
            airlinersPanel.Margin = new Thickness(10, 0, 10, 0);

            TextBlock txtNewHeader = new TextBlock();
            txtNewHeader.Uid = "1001";
            txtNewHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtNewHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtNewHeader.FontWeight = FontWeights.Bold;
            txtNewHeader.Text = Translator.GetInstance().GetString("PageAirliners", txtNewHeader.Uid);

            airlinersPanel.Children.Add(txtNewHeader);

            // chs, 2011-11-10 added a scroller so all elements are viewable

            ScrollViewer scroller = new ScrollViewer();
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            airlinersPanel.Children.Add(scroller);

            StackPanel panelScroller = new StackPanel();
            panelScroller.Orientation = Orientation.Vertical;

            scroller.Content = panelScroller;

            ContentControl ccManufacturerHeader = new ContentControl();
            ccManufacturerHeader.ContentTemplate = this.Resources["ManufacturerHeader"] as DataTemplate;
            ccManufacturerHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            panelScroller.Children.Add(ccManufacturerHeader);

            lbManufacturers = new ListBox();
            lbManufacturers.ItemTemplate = this.Resources["ManufacturerItem"] as DataTemplate;
            lbManufacturers.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            lbManufacturers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

             panelScroller.Children.Add(lbManufacturers);

            TextBlock txtUsedHeader = new TextBlock();
            txtUsedHeader.Uid = "1002";
            txtUsedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtUsedHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtUsedHeader.FontWeight = FontWeights.Bold;
            txtUsedHeader.Margin = new Thickness(0, 10, 0, 0);
            txtUsedHeader.Text = Translator.GetInstance().GetString("PageAirliners", txtUsedHeader.Uid);

            airlinersPanel.Children.Add(txtUsedHeader);

            WrapPanel airlinerTypePanel = new WrapPanel();
            airlinersPanel.Children.Add(airlinerTypePanel);

            RadioButton rbPassengerAirliner = new RadioButton();
            rbPassengerAirliner.GroupName = "AirlinerRouteType";
            rbPassengerAirliner.IsChecked = true;
            rbPassengerAirliner.Tag = AirlinerType.TypeOfAirliner.Passenger;
            rbPassengerAirliner.Checked += rbRouteType_Checked;
            rbPassengerAirliner.Content = Translator.GetInstance().GetString("PageAirliners", "1003");

            airlinerTypePanel.Children.Add(rbPassengerAirliner);

            RadioButton rbCargoAirliner = new RadioButton();
            rbCargoAirliner.GroupName = "AirlinerRouteType";
            rbCargoAirliner.Margin = new Thickness(5, 0, 0, 0);
            rbCargoAirliner.Tag = AirlinerType.TypeOfAirliner.Cargo;
            rbCargoAirliner.Checked += rbRouteType_Checked;
            rbCargoAirliner.Content = Translator.GetInstance().GetString("PageAirliners", "1004");

            airlinerTypePanel.Children.Add(rbCargoAirliner);
            
            ContentControl lblUsedHeader = new ContentControl();
            lblUsedHeader.ContentTemplate = this.Resources["AirlinersUsedHeader"] as DataTemplate;
            lblUsedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            airlinersPanel.Children.Add(lblUsedHeader);
            
            lbUsedAirliners = new ListBox();
            lbUsedAirliners.ItemTemplate = this.Resources["AirlinerUsedItem"] as DataTemplate;
            lbUsedAirliners.Height = (GraphicsHelpers.GetContentHeight() - 150) / 2;
            lbUsedAirliners.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            airlinersPanel.Children.Add(lbUsedAirliners);

            Button btnSearch = new Button();
            btnSearch.Uid = "109";
            btnSearch.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSearch.Height = Double.NaN;
            btnSearch.Width = Double.NaN;
            btnSearch.IsDefault = true;
            btnSearch.Content = Translator.GetInstance().GetString("General", btnSearch.Uid);
            btnSearch.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSearch.Margin = new Thickness(0, 5, 0, 0);
            btnSearch.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);

            airlinersPanel.Children.Add(btnSearch);

            showUsedAirliners(Airliners.GetAirlinersForSale());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airlinersPanel, StandardContentPanel.ContentLocation.Left);

            sideFrame = new Frame();

            panelContent.setContentPage(sideFrame, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);


            showPage(this);

            showManufacturers();
        }

        private void rbRouteType_Checked(object sender, RoutedEventArgs e)
        {
            
            airlinerType = (AirlinerType.TypeOfAirliner)((RadioButton)sender).Tag;

            showUsedAirliners();
          
        

        }

     //shows the list of manufacturers
        private void showManufacturers()
        {
            lbManufacturers.Items.Clear();
            (from a in AirlinerTypes.GetAllTypes() where a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime orderby a.Manufacturer.Name select a.Manufacturer).Distinct().ToList().ForEach(m => lbManufacturers.Items.Add(m));
         
        }

     
        //shows the list of used airliners for sale
        private void showUsedAirliners()
        {
            lbUsedAirliners.Items.Clear();

            var lAirliners = new List<Airliner>(airliners.FindAll(a => a.Type.TypeAirliner == airlinerType));

            if (sortDescending)
                lAirliners = lAirliners.OrderByDescending(sortCriteriaUsed).ThenBy(a => a.TailNumber).ToList();
            else
                lAirliners = lAirliners.OrderBy(sortCriteriaUsed).ThenBy(a => a.TailNumber).ToList();

            foreach (Airliner airliner in lAirliners)
                lbUsedAirliners.Items.Add(airliner);
        }
        public void showUsedAirliners(List<Airliner> airlinersList)
        {
            airliners = airlinersList;
            showUsedAirliners();
        }
        //removes an airliner from the used list
        public void removeUsedAirliner(Airliner airliner)
        {
            lbUsedAirliners.Items.Remove(airliner);
        }
        private void lnkManufacturer_Click(object sender, RoutedEventArgs e)
        {
            Manufacturer manufacturer = (Manufacturer)((Hyperlink)sender).Tag;
            sideFrame.Content = new PageOrderAirliners(this,manufacturer);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            sideFrame.Content = new PageSearchAirliners(this);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

            string type = ((Hyperlink)sender).TargetName;

            if (GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; })).Count == 0)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2000"), Translator.GetInstance().GetString("MessageBox", "2000", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                sideFrame.Content = new PanelUsedAirliner(this, (Airliner)((Hyperlink)sender).Tag);
            }


        }

        private void HeaderUsed_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            Func<Airliner, object> oSortCriteria = sortCriteriaUsed;

            switch (type)
            {
                case "Built":
                    sortCriteriaUsed = a => a.BuiltDate;

                    if (sortCriteriaUsed == oSortCriteria)
                        sortDescending = !sortDescending;

                    showUsedAirliners();
                    break;
                case "Price":
                    sortCriteriaUsed = a => a.Price;

                    if (sortCriteriaUsed == oSortCriteria)
                        sortDescending = !sortDescending;

                    showUsedAirliners();
                    break;
                case "Type":
                    sortCriteriaUsed = a => a.Type.Name;

                    if (sortCriteriaUsed == oSortCriteria)
                        sortDescending = !sortDescending;

                    showUsedAirliners();
                    break;
                case "Range":
                    sortCriteriaUsed = a => a.Type.RangeType;

                    if (sortCriteriaUsed == oSortCriteria)
                        sortDescending = !sortDescending;

                    showUsedAirliners();
                    break;
            }
        }
        public override void updatePage()
        {
            showManufacturers();
        }

    }
    public class HasContractConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Manufacturer manufacturer = (Manufacturer)value;

            if (GameObject.GetInstance().HumanAirline.Contract != null && GameObject.GetInstance().HumanAirline.Contract.Manufacturer == manufacturer)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
