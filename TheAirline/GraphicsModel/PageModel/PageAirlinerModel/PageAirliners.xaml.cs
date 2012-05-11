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
        private ListBox lbUsedAirliners;//, lbNewAirliners;
        private Comparison<Airliner> sortCriteriaUsed;
        private Frame sideFrame;
        public PageAirliners()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirliners", this.Uid);

            sortCriteriaUsed = delegate(Airliner a1, Airliner a2) { return a2.BuiltDate.CompareTo(a1.BuiltDate); };

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

            ListBox lbManufacturers = new ListBox();
            lbManufacturers.ItemTemplate = this.Resources["ManufacturerItem"] as DataTemplate;
            lbManufacturers.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            lbManufacturers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            (from a in AirlinerTypes.GetTypes() where a.Produced.From <= GameObject.GetInstance().GameTime.Year && a.Produced.To >= GameObject.GetInstance().GameTime.Year orderby a.Manufacturer.Name select a.Manufacturer).Distinct().ToList().ForEach(m => lbManufacturers.Items.Add(m));
            panelScroller.Children.Add(lbManufacturers);

            TextBlock txtUsedHeader = new TextBlock();
            txtUsedHeader.Uid = "1002";
            txtUsedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtUsedHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtUsedHeader.FontWeight = FontWeights.Bold;
            txtUsedHeader.Margin = new Thickness(0, 10, 0, 0);
            txtUsedHeader.Text = Translator.GetInstance().GetString("PageAirliners", txtUsedHeader.Uid);

            airlinersPanel.Children.Add(txtUsedHeader);

            ContentControl lblUsedHeader = new ContentControl();
            lblUsedHeader.ContentTemplate = this.Resources["AirlinersUsedHeader"] as DataTemplate;
            lblUsedHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            airlinersPanel.Children.Add(lblUsedHeader);

            lbUsedAirliners = new ListBox();
            lbUsedAirliners.ItemTemplate = this.Resources["AirlinerUsedItem"] as DataTemplate;
            lbUsedAirliners.Height = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            lbUsedAirliners.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            airlinersPanel.Children.Add(lbUsedAirliners);

            showUsedAirliners();


            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airlinersPanel, StandardContentPanel.ContentLocation.Left);

            sideFrame = new Frame();

            panelContent.setContentPage(sideFrame, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);


            showPage(this);
        }



        //shows the list of used airliners for sale
        public void showUsedAirliners()
        {
            lbUsedAirliners.Items.Clear();

            List<Airliner> airliners = Airliners.GetAirlinersForSale();

            airliners.Sort(sortCriteriaUsed);

            foreach (Airliner airliner in airliners)
                lbUsedAirliners.Items.Add(airliner);
        }

        private void lnkManufacturer_Click(object sender, RoutedEventArgs e)
        {
            Manufacturer manufacturer = (Manufacturer)((Hyperlink)sender).Tag;
            sideFrame.Content = new PageOrderAirliners(manufacturer);
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

            string type = ((Hyperlink)sender).TargetName;

            if (GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; })).Count == 0)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2000"), Translator.GetInstance().GetString("MessageBox", "2000", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                sideFrame.Content = new PanelUsedAirliner(this, (Airliner)((Hyperlink)sender).Tag);
            }


        }

        private void HeaderUsed_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            switch (type)
            {
                case "Built":
                    sortCriteriaUsed = delegate(Airliner a1, Airliner a2) { return a2.BuiltDate.CompareTo(a1.BuiltDate); };
                    showUsedAirliners();
                    break;
                case "Price":
                    sortCriteriaUsed = delegate(Airliner a1, Airliner a2) { return a2.Price.CompareTo(a1.Price); };
                    showUsedAirliners();
                    break;
                case "Type":
                    sortCriteriaUsed = delegate(Airliner a1, Airliner a2) { return a1.Type.Name.CompareTo(a2.Type.Name); };
                    showUsedAirliners();
                    break;
            }
        }

    }
}
