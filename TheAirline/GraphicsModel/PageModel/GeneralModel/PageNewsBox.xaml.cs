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
            StackPanel newsPanel = new StackPanel();
            newsPanel.Margin = new Thickness(10, 0, 10, 0);

            // airportPanel.Children.Add(createQuickInfoPanel());

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
            lbNews.Height = 500;

            
          

            newsPanel.Children.Add(lbNews);

            
            base.setContent(panelContent);

            base.setHeaderContent("News");



            showPage(this);

            showNews(true);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageNewsBox_OnTimeChanged);
        }

        private void PageNewsBox_OnTimeChanged()
        {
            if (this.IsLoaded)
                showNews(false);
        }
        private void showNews(Boolean forceShow)
        {
            
            List<News> lNews = GameObject.GetInstance().NewsBox.getNews();
            lNews.Sort((delegate(News n1, News n2) { if (n2.Date.CompareTo(n1.Date) !=0) return n2.Date.CompareTo(n1.Date); else return n2.Body.CompareTo(n1.Body); }));


            if (lNews.Count != lbNews.Items.Count || forceShow)
            {

                lbNews.Items.Clear();


                foreach (News news in lNews)
                {
                    lbNews.Items.Add(news);
                }
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
}
