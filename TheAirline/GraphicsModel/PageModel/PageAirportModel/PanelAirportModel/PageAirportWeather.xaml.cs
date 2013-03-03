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
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Windows.Threading;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportWeather.xaml
    /// </summary>
    public partial class PageAirportWeather : Page
    {
        private Airport Airport;
        private ContentControl[] ccWeather;
        public PageAirportWeather(Airport airport)
        {
            this.Airport = airport;

            ccWeather = new ContentControl[this.Airport.Weather.Length];

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

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirportWeather_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageAirportWeather_Unloaded);
        }

        private void PageAirportWeather_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirportWeather_OnTimeChanged);
        }

        private void PageAirportWeather_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                
                for (int i = 0; i < this.Airport.Weather.Length; i++)
                {
                    ccWeather[i].Content = this.Airport.Weather[i];
                    
           
                }
                ccWeather[0].Content = this.Airport.Weather[1];
                ccWeather[0].Content = this.Airport.Weather[0];
            }
        
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
                   
            ccWeather[0] = new ContentControl();
            ccWeather[0].ContentTemplate = this.Resources["TodaysWeatherItem"] as DataTemplate;
            ccWeather[0].Content = this.Airport.Weather[0];
            ccWeather[0].Margin = new Thickness(5, 0, 0, 0);
            ccWeather[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelWind.Children.Add(ccWeather[0]);

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
                ccWeather[i] = new ContentControl();
                ccWeather[i].ContentTemplate = this.Resources["WeatherForecastItem"] as DataTemplate;
                ccWeather[i].Content = this.Airport.Weather[i];
                ccWeather[i].Margin = new Thickness(10, 0, 0, 0);

                panelDays.Children.Add(ccWeather[i]);
            }

            panelForecast.Children.Add(panelDays);

            return panelForecast;

        }
    }
    public class CurrentWindConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

            int currentHour;
           
            if (GameObject.GetInstance().DayRoundEnabled)
                currentHour = 12;
            else
                currentHour = GameObject.GetInstance().GameTime.Hour;

            Weather.eWindSpeed windspeed = weather.Temperatures[currentHour].WindSpeed;
            Weather.WindDirection direction = weather.Temperatures[currentHour].Direction;

            return string.Format("{0} {1}", new EnumLanguageConverter().Convert(direction), new WindSpeedToUnitConverter().Convert(windspeed));

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WeatherConditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

            string weatherCondition = "clear";

            if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                weatherCondition = weather.Precip.ToString();
            else
                weatherCondition = weather.Cover.ToString();

            return weatherCondition;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CurrentWeatherConditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

           int currentHour;
            if (GameObject.GetInstance().DayRoundEnabled)
                currentHour = 12;
            else
                currentHour = GameObject.GetInstance().GameTime.Hour;

      
            string weatherCondition = "clear";

            if (weather.Temperatures[currentHour].Cover == Weather.CloudCover.Overcast && weather.Temperatures[currentHour].Precip != Weather.Precipitation.None)
                weatherCondition = weather.Temperatures[currentHour].Precip.ToString();
            else
                weatherCondition = weather.Temperatures[currentHour].Cover.ToString();

            return new TextUnderscoreConverter().Convert(weatherCondition).ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CurrentTemperatureConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

            int currentHour;
            if (GameObject.GetInstance().DayRoundEnabled)
                currentHour = 12;
            else
                currentHour = GameObject.GetInstance().GameTime.Hour;

            return new TemperatureToTextConverter().Convert(weather.Temperatures[currentHour].Temperature);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CurrentWeatherImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

            int currentHour;
            
            if (GameObject.GetInstance().DayRoundEnabled)
                currentHour = 12;
            else
                currentHour= GameObject.GetInstance().GameTime.Hour;

            string weatherCondition = "clear";

            if (weather.Temperatures[currentHour].Cover == Weather.CloudCover.Overcast && weather.Temperatures[currentHour].Precip != Weather.Precipitation.None)
                weatherCondition = weather.Temperatures[currentHour].Precip.ToString();
            else
                weatherCondition = weather.Temperatures[currentHour].Cover.ToString();

          
            if (currentHour < Weather.Sunrise || currentHour > Weather.Sunset)
                weatherCondition += "-night";

            return AppSettings.getDataPath() + "\\graphics\\weather\\" + weatherCondition + ".png";


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WeatherImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather weather = (Weather)value;

            string weatherCondition= "clear";

            if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                weatherCondition= weather.Precip.ToString();
            else
                weatherCondition = weather.Cover.ToString();

            return AppSettings.getDataPath() + "\\graphics\\weather\\" + weatherCondition+ ".png";


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WindSpeedToUnitConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather.eWindSpeed windspeed = (Weather.eWindSpeed)value;

            return string.Format("{0:0} {1}", new NumberToUnitConverter().Convert((int)windspeed), new StringToLanguageConverter().Convert("km/t"));
        }
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
