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
        private StackPanel panelAirliner;
        private ListBox lbUsedAirliners, lbNewAirliners;
        private Comparison<AirlinerType> sortCriteriaNew;
        private Comparison<Airliner> sortCriteriaUsed;

        public PageAirliners()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirliners", this.Uid);

            sortCriteriaNew = delegate(AirlinerType t1, AirlinerType t2) { return t2.Price.CompareTo(t1.Price); };
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

            ContentControl lblNewHeader = new ContentControl();
            lblNewHeader.ContentTemplate = this.Resources["AirlinersNewHeader"] as DataTemplate;
            lblNewHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            panelScroller.Children.Add(lblNewHeader);

            lbNewAirliners = new ListBox();
            lbNewAirliners.ItemTemplate = this.Resources["AirlinerNewItem"] as DataTemplate;
            lbNewAirliners.Height = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            lbNewAirliners.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            showNewAirliners();

            panelScroller.Children.Add(lbNewAirliners);



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

            panelAirliner = new StackPanel();

            panelContent.setContentPage(panelAirliner, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);


            showPage(this);
        }



        //shows the list of new airliners for order
        private void showNewAirliners()
        {
            lbNewAirliners.Items.Clear();

            List<AirlinerType> types = AirlinerTypes.GetTypes().FindAll(delegate(AirlinerType t) { return t.Produced.To >= GameObject.GetInstance().GameTime.Year && t.Produced.From <= GameObject.GetInstance().GameTime.Year; });

            types.Sort(sortCriteriaNew);

            foreach (AirlinerType airliner in types)
                lbNewAirliners.Items.Add(airliner);
        }

        //shows the list of used airliners for sale
        public void showUsedAirliners()
        {
            //    Sort ved Click på header

            lbUsedAirliners.Items.Clear();

            List<Airliner> airliners = Airliners.GetAirlinersForSale();

            airliners.Sort(sortCriteriaUsed);

            foreach (Airliner airliner in airliners)
                lbUsedAirliners.Items.Add(airliner);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            panelAirliner.Children.Clear();

            string type = ((Hyperlink)sender).TargetName;

            if (GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; })).Count == 0)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2000"), Translator.GetInstance().GetString("MessageBox", "2000", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                if (type == "Used")
                    panelAirliner.Children.Add(new PanelUsedAirliner(this, (Airliner)((Hyperlink)sender).Tag));

                if (type == "New")
                    panelAirliner.Children.Add(new PanelNewAirliner(this, (AirlinerType)((Hyperlink)sender).Tag));
            }


        }
       
        private void HeaderNew_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            switch (type)
            {
                case "Manufacturer":
                    sortCriteriaNew = delegate(AirlinerType a1, AirlinerType a2) { return a1.Manufacturer.Name.CompareTo(a2.Manufacturer.Name); };
                    showNewAirliners();
                    break;
                case "Price":
                    sortCriteriaNew = delegate(AirlinerType a1, AirlinerType a2) { return a2.Price.CompareTo(a1.Price); };
                    showNewAirliners();
                    break;
                case "Type":
                    sortCriteriaNew = delegate(AirlinerType a1, AirlinerType a2) { return a1.Name.CompareTo(a2.Name); };
                    showNewAirliners();
                    break;
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
