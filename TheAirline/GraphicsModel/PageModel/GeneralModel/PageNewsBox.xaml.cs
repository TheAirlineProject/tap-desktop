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
using TheAirline.Model.GeneralModel;
using System.Text.RegularExpressions;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageNewsBox.xaml
    /// </summary>
    public partial class PageNewsBox : StandardPage
    {
        private ContentControl ccNews;
        private ListBox lbNews;
        public PageNewsBox()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageNewsBox", this.Uid);

            StackPanel newsPanel = new StackPanel();
            newsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(newsPanel, StandardContentPanel.ContentLocation.Left);

            Panel panelSideMenu = new StackPanel();

            ccNews = new ContentControl();
            ccNews.ContentTemplate = this.Resources["NewsShowItem"] as DataTemplate;
            ccNews.Visibility = System.Windows.Visibility.Collapsed;

            panelSideMenu.Children.Add(ccNews);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);
            
            ContentControl ccNewsHeader = new ContentControl();
            ccNewsHeader.ContentTemplate = this.Resources["NewsHeader"] as DataTemplate;
            newsPanel.Children.Add(ccNewsHeader);

            
            lbNews = new ListBox();
            lbNews.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbNews.ItemTemplate = this.Resources["NewsItem"] as DataTemplate;
            lbNews.MaxHeight = GraphicsHelpers.GetContentHeight()-50;

            
          

            newsPanel.Children.Add(lbNews);

            
            base.setContent(panelContent);

            base.setHeaderContent(this.Title);



            showPage(this);

            showNews(true);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageNewsBox_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageNewsBox_Unloaded);
        }

        private void PageNewsBox_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageNewsBox_OnTimeChanged);

        }

        private void PageNewsBox_OnTimeChanged()
        {
            if (this.IsLoaded)
                showNews(false);
        }
        private void showNews(Boolean forceShow)
        {
            
            List<News> lNews = GameObject.GetInstance().NewsBox.getNews();

            lNews = (from n in lNews orderby n.Date descending select n).ToList();
                   
            if (lNews.Count != lbNews.Items.Count || forceShow)
            {

                lbNews.Items.Clear();

                lNews.ForEach(n => lbNews.Items.Add(n));
               
            }
        }
        private void LnkNews_Click(object sender, RoutedEventArgs e)
        {
            News news = (News)((Hyperlink)sender).Tag;
            news.IsRead = true;
            
            ccNews.Content = news;
            ccNews.Visibility = System.Windows.Visibility.Visible;

            showNews(true);
        }
    }
    public class NewsTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = value.ToString();
            
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;


            char[] delimiterChars = { '[', ']' };
            string[] splittedText = text.Split(delimiterChars);

            foreach (string subText in splittedText)
            {
                if (subText.StartsWith("LI"))
                    txtBlock.Inlines.Add(getNewsLink(subText));
                else
                    txtBlock.Inlines.Add(subText);
            }

            return txtBlock;
         
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        //creates the link for a news link
        private Hyperlink getNewsLink(string text)
        {
            string linkType = text.Substring(3).Split('=')[0];
            string linkObject = text.Split('=')[1];
            object o = null;
            
            string objectText = "";
            switch (linkType)
            {
                case "airline":
                    o = Airlines.GetAirline(linkObject);
                    objectText = ((Airline)o).Profile.Name;
                    break;
                case "airport":
                    o = Airports.GetAirport(linkObject);
                    objectText = ((Airport)o).Profile.Name;
                    break;
                     
            }

            
            Run run = new Run(objectText);

            Hyperlink hyperLink = new Hyperlink(run);
            hyperLink.Tag = o;
            hyperLink.TargetName = linkType;
            hyperLink.Click += new RoutedEventHandler(hyperLink_Click);

            return hyperLink;
        }

        private void hyperLink_Click(object sender, RoutedEventArgs e)
        {
            object o = ((Hyperlink)sender).Tag;
            string linkType = (string)((Hyperlink)sender).TargetName;

            switch (linkType)
            {
                case "airline":
                    PageNavigator.NavigateTo(new PageAirline((Airline)o));
                    break;
                case "airport":
                    PageNavigator.NavigateTo(new PageAirport((Airport)o));
                    break;
            }


        }
    }
}
