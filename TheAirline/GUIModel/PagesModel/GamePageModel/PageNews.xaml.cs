using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageNews.xaml
    /// </summary>
    public partial class PageNews : Page
    {
        public ObservableCollection<NewsMVVM> AllNews { get; set; }
        public SelectedNewsMVVM SelectedNews { get; set; }
        public ObservableCollection<NewsMVVM> SelectedNewsList { get; set; }
        public PageNews()
        {
            this.AllNews = new ObservableCollection<NewsMVVM>();
           
            foreach (News news in GameObject.GetInstance().NewsBox.getNews().OrderByDescending(n => n.Date).ToList())
                this.AllNews.Add(new NewsMVVM(news));

            this.SelectedNews = new SelectedNewsMVVM();

            this.SelectedNewsList = new ObservableCollection<NewsMVVM>();

            InitializeComponent();

        }

        private void lnkNews_Click(object sender, RoutedEventArgs e)
        {
            NewsMVVM news = (NewsMVVM)((Hyperlink)sender).Tag;
            news.markAsRead();

            this.SelectedNews.SelectedNews = news.News;
     
        }
         

        private void cbNews_Checked(object sender, RoutedEventArgs e)
        {
            NewsMVVM news = (NewsMVVM)((CheckBox)sender).Tag;
            this.SelectedNewsList.Add(news);
        }

        private void cbNews_Unchecked(object sender, RoutedEventArgs e)
        {
            NewsMVVM news = (NewsMVVM)((CheckBox)sender).Tag;
            this.SelectedNewsList.Remove(news);
        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.SelectedNewsList)
            {
                this.AllNews.Remove(news);
                GameObject.GetInstance().NewsBox.removeNews(news.News);
            }
        }

        private void btnMarkSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.SelectedNewsList)
                news.markAsRead();
       
        }
    }
}
