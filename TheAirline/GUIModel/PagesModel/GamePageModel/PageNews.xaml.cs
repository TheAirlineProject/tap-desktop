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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.GUIModel.PagesModel.AirportPageModel;
using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
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
                case "airliner":
                    o = Airlines.GetAllAirlines().SelectMany(a => a.Fleet).ToList().Find(f => f.Airliner.TailNumber == linkObject);
                    objectText = ((FleetAirliner)o).Name;
                    break;
            }


            Run run = new Run(objectText);

            Hyperlink hyperLink = new Hyperlink(run);
            hyperLink.Tag = o;
            hyperLink.TextDecorations = TextDecorations.Underline;
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
                case "airliner":
                    PageNavigator.NavigateTo(new PageFleetAirliner((FleetAirliner)o));
                    break;
            }


        }
    }
}
