namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.GUIModel.PagesModel.AirportPageModel;
    using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageNews.xaml
    /// </summary>
    public partial class PageNews : Page
    {
        #region Constructors and Destructors

        public PageNews()
        {
            this.AllNews = new ObservableCollection<NewsMVVM>();

            foreach (News news in GameObject.GetInstance().NewsBox.GetNews().OrderByDescending(n => n.Date).ToList())
            {
                this.AllNews.Add(new NewsMVVM(news));
            }

            this.SelectedNews = new SelectedNewsMVVM();

            this.SelectedNewsList = new ObservableCollection<NewsMVVM>();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<NewsMVVM> AllNews { get; set; }

        public SelectedNewsMVVM SelectedNews { get; set; }

        public ObservableCollection<NewsMVVM> SelectedNewsList { get; set; }

        #endregion

        #region Methods

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.SelectedNewsList)
            {
                this.AllNews.Remove(news);
                GameObject.GetInstance().NewsBox.RemoveNews(news.News);
            }
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.AllNews)
            {
                news.IsSelected = false;
            }
            //this.SelectedNewsList.Clear();
        }

        private void btnMarkSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.SelectedNewsList)
            {
                news.markAsRead();
            }
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            var news = (News)((Button)sender).Tag;
            this.SelectedNews.SelectedNews = null;

            NewsMVVM newsMVVM = this.AllNews.First(n => n.News == news);
            this.AllNews.Remove(newsMVVM);

            GameObject.GetInstance().NewsBox.RemoveNews(news);
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (NewsMVVM news in this.AllNews)
            {
                if (!this.SelectedNewsList.Contains(news))
                {
                    //this.SelectedNewsList.Add(news);
                    news.IsSelected = true;
                }
            }
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            var news = (News)((Button)sender).Tag;

            news.ExecuteNews();

            this.SelectedNews.SelectedNews = null;

            NewsMVVM newsMVVM = this.AllNews.First(n => n.News == news);
            this.AllNews.Remove(newsMVVM);

            GameObject.GetInstance().NewsBox.RemoveNews(news);
        }

        private void cbNews_Checked(object sender, RoutedEventArgs e)
        {
            var news = (NewsMVVM)((CheckBox)sender).Tag;
            this.SelectedNewsList.Add(news);
        }

        private void cbNews_Unchecked(object sender, RoutedEventArgs e)
        {
            var news = (NewsMVVM)((CheckBox)sender).Tag;
            this.SelectedNewsList.Remove(news);
        }

        private void lnkNews_Click(object sender, RoutedEventArgs e)
        {
            var news = (NewsMVVM)((Hyperlink)sender).Tag;
            news.markAsRead();

            this.SelectedNews.SelectedNews = news.News;
        }

        #endregion
    }

    public class NewsTextConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value.ToString();
       
            var txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;

            char[] delimiterChars = { '[', ']' };
            string[] splittedText = text.Split(delimiterChars);

            foreach (string subText in splittedText)
            {
                if (subText.StartsWith("LI"))
                {
                    txtBlock.Inlines.Add(this.getNewsLink(subText));
                }
                else if (subText.StartsWith("HEAD"))
                {
                    txtBlock.Inlines.Add(this.getNewsHeader(subText));
                }
                else if (subText.StartsWith("BOLD"))
                {
                    txtBlock.Inlines.Add(this.getNewsBold(subText));
                }
                else if (subText.StartsWith("WIDTH"))
                {
                    txtBlock.Inlines.Add(this.getNewsWidthText(subText));
                }
                else
                {
                    string[] newLines = subText.Split(new string[]{"\\n"},StringSplitOptions.RemoveEmptyEntries);

                    if (newLines.Count() > 0)
                    {
                        for (int i = 0; i < newLines.Count(); i++)
                        {
                            txtBlock.Inlines.Add(newLines[i]);

                            if (i < newLines.Count() - 1)
                                txtBlock.Inlines.Add(new LineBreak());
                        }
                       
                    }
                    else
                        txtBlock.Inlines.Add(subText);
                }
            }

         
            return txtBlock;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        //creates a text for a specific width

        //creates the bold text for a news text

        #region Methods

        private TextBlock getNewsBold(string text)
        {
            text = text.Replace("BOLD=", "");

            var txt = new TextBlock();
            txt.Text = text;
            txt.FontWeight = FontWeights.Bold;

            return txt;
        }

        //creates the header for a news text
        private TextBlock getNewsHeader(string text)
        {
            text = text.Replace("HEAD=", "");

            var txtHeader = new TextBlock();
            txtHeader.Text = text;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.FontSize = 24;

            return txtHeader;
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
                    o =
                        Airlines.GetAllAirlines()
                            .SelectMany(a => a.Fleet)
                            .ToList()
                            .Find(f => f.Airliner.TailNumber == linkObject);
                    objectText = ((FleetAirliner)o).Name;
                    break;
            }

            var run = new Run(objectText);

            var hyperLink = new Hyperlink(run);
            hyperLink.Tag = o;
            hyperLink.TextDecorations = TextDecorations.Underline;
            hyperLink.TargetName = linkType;
            hyperLink.Click += this.hyperLink_Click;

            return hyperLink;
        }

        private TextBlock getNewsWidthText(string text)
        {
            int lWidth = 6;
            int space = text.IndexOf(' ');

            string tWidth = text.Substring(lWidth, space - lWidth + 1);

            int width = Int16.Parse(tWidth);

            var txt = new TextBlock();
            txt.Text = text.Substring(space);
            txt.Width = width;

            return txt;
        }

        private void hyperLink_Click(object sender, RoutedEventArgs e)
        {
            object o = ((Hyperlink)sender).Tag;
            string linkType = ((Hyperlink)sender).TargetName;

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

        #endregion
    }
}