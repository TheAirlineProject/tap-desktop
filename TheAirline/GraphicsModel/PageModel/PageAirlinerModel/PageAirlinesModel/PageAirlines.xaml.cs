using System;
using System.Collections.Generic;
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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinesModel
{
    /// <summary>
    /// Interaction logic for PageAirlines.xaml
    /// </summary>
    public partial class PageAirlines : StandardPage
    {
        public PageAirlines()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirlines", this.Uid);

            StackPanel airlinesPanel = new StackPanel();
            airlinesPanel.Margin = new Thickness(10, 0, 10, 0);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["AirlinesHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            //txtHeader.SetResourceReference(Label.BackgroundProperty, "HeaderBackgroundBrush");
         
            airlinesPanel.Children.Add(txtHeader);


            ListBox lbAirlines = new ListBox();
            lbAirlines.ItemTemplate = this.Resources["AirlineItem"] as DataTemplate;
            // chs, 2011-10-10 set max height so scroll bars are enabled
            lbAirlines.MaxHeight=GraphicsHelpers.GetContentHeight() - 75;
            lbAirlines.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            List<Airline> airlines = Airlines.GetAllAirlines();
            //airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            airlines.OrderBy(a => a.Profile.Name);

            airlines.Remove(GameObject.GetInstance().HumanAirline);

            airlines.Insert(0,GameObject.GetInstance().HumanAirline);

            foreach (Airline airline in airlines)
                lbAirlines.Items.Add(airline);

            airlinesPanel.Children.Add(lbAirlines);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airlinesPanel, StandardContentPanel.ContentLocation.Left);


            StackPanel panelSideMenu = new PanelAirlines();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent(this.Title);


            showPage(this);
        }
        private void LnkAirline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));

        }
    }
}
