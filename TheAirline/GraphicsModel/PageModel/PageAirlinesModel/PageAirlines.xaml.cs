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
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.StatisticsModel;

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
            lbAirlines.MaxHeight=GraphicsHelpers.GetContentHeight() - 100;
            lbAirlines.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            List<Airline> airlines = Airlines.GetAllAirlines().FindAll(a=>!a.IsSubsidiary);
            //airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            airlines = airlines.OrderBy(a => a.Profile.Name).ToList();

            airlines.Remove(GameObject.GetInstance().MainAirline);
            airlines.Insert(0, GameObject.GetInstance().MainAirline);
            
            foreach (Airline airline in airlines)
            {
                lbAirlines.Items.Add(airline);
                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                    lbAirlines.Items.Add(sAirline);
            }

            airlinesPanel.Children.Add(lbAirlines);

            airlinesPanel.Children.Add(createSymbolsPanel());

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

            //PageNavigator.NavigateTo(new PageAirline(airline));
            PageNavigator.NavigateTo(new GUIModel.PagesModel.AirlinePageModel.PageAirline(airline));
        }
        //creates the panel for the symbols
        private WrapPanel createSymbolsPanel()
        {
            WrapPanel panelSymbols = new WrapPanel();
            panelSymbols.Margin = new Thickness(0, 5, 0, 0);

            Image imgHuman = new Image();
            imgHuman.Source = new BitmapImage(new Uri(@"/Data/images/human.png", UriKind.RelativeOrAbsolute));
            imgHuman.Width = 20;
            RenderOptions.SetBitmapScalingMode(imgHuman, BitmapScalingMode.HighQuality);

            panelSymbols.Children.Add(imgHuman);

            TextBlock txtHuman = new TextBlock();
            txtHuman.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtHuman.Text = string.Format(" = {0}", Translator.GetInstance().GetString("PageAirlines","1001"));

            panelSymbols.Children.Add(txtHuman);

            Image imgSubsidiary = new Image();
            imgSubsidiary.Source = new BitmapImage(new Uri(@"/Data/images/airplane.png", UriKind.RelativeOrAbsolute));
            imgSubsidiary.Width = 20;
            imgSubsidiary.Margin = new Thickness(5, 0, 0, 0);
            RenderOptions.SetBitmapScalingMode(imgSubsidiary, BitmapScalingMode.HighQuality);

            panelSymbols.Children.Add(imgSubsidiary);

            TextBlock txtSubsidiary = new TextBlock();
            txtSubsidiary.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtSubsidiary.Text = string.Format(" = {0}", Translator.GetInstance().GetString("PageAirlines","1002"));

            panelSymbols.Children.Add(txtSubsidiary);

            return panelSymbols;
        }
    }
    public class AirlineRatingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Airline airline = (Airline)value;

            return Ratings.GetCustomerHappiness(airline);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
