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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportWeather.xaml
    /// </summary>
    public partial class PageAirportWeather : Page
    {
        private Airport Airport;
        public PageAirportWeather(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            StackPanel panelWeather = new StackPanel();
            panelWeather.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportWeather", txtHeader.Uid);

            panelWeather.Children.Add(txtHeader);

            panelWeather.Children.Add(createCurrentWeatherPanel());
            panelWeather.Children.Add(createWeatherForecastPanel());

            this.Content = panelWeather;
        }
        //creates the panel for the current weather
        private StackPanel createCurrentWeatherPanel()
        {
            StackPanel panelWeather = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1002";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text =Translator.GetInstance().GetString("PageAirportWeather", txtHeader.Uid);

            panelWeather.Children.Add(txtHeader);

            WrapPanel panelWind = new WrapPanel();
            panelWeather.Children.Add(panelWind);

            Image imgWind = new Image();
            imgWind.Source = new BitmapImage(new Uri(@"/Data/images/wind.png", UriKind.RelativeOrAbsolute));
            imgWind.Height = 24;
            imgWind.Width = 24;
            RenderOptions.SetBitmapScalingMode(imgWind, BitmapScalingMode.HighQuality);

            panelWind.Children.Add(imgWind);

            TextBlock txtWind = UICreator.CreateTextBlock(string.Format("{0} ({1:0.##} {2} in {3} direction)", new Converters.TextUnderscoreConverter().Convert(this.Airport.Weather[0].WindSpeed, null, null, null), new NumberToUnitConverter().Convert((int)this.Airport.Weather[0].WindSpeed), new StringToLanguageConverter().Convert("km/t"), this.Airport.Weather[0].Direction));//string.Format("{0} ({1} km/h) in {2} direction", new Converters.TextUnderscoreConverter().Convert(this.Airport.Weather.WindSpeed, null, null, null), (int)this.Airport.Weather.WindSpeed, this.Airport.Weather.Direction));
            txtWind.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtWind.Margin = new Thickness(10, 0, 0, 0);

            ContentControl ccWeather = new ContentControl();
            ccWeather.ContentTemplate = this.Resources["TodaysWeatherItem"] as DataTemplate;
            ccWeather.Content = this.Airport.Weather[0];
            ccWeather.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelWind.Children.Add(ccWeather);

            return panelWeather;
        }
        //creates the weather forecast panel
        private StackPanel createWeatherForecastPanel()
        {
            StackPanel panelForecast = new StackPanel();
            panelForecast.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1003";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportWeather", txtHeader.Uid);

            panelForecast.Children.Add(txtHeader);

            WrapPanel panelDays = new WrapPanel();

            for (int i = 1; i < this.Airport.Weather.Length; i++)
            {
                ContentControl ccWeather = new ContentControl();
                ccWeather.ContentTemplate = this.Resources["WeatherForecastItem"] as DataTemplate;
                ccWeather.Content = this.Airport.Weather[i];
                ccWeather.Margin = new Thickness(10, 0, 0, 0);

                panelDays.Children.Add(ccWeather);
            }

            panelForecast.Children.Add(panelDays);

            return panelForecast;

        }
    }
    public class WindSpeedToUnitConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather.eWindSpeed windspeed = (Weather.eWindSpeed)value;

            return string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert((int)windspeed), new StringToLanguageConverter().Convert("km/t"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
